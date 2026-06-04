using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Length_Stds_Environmental_Monitoring
{
    public delegate void PrintTemperatureData(double temperature, string msg, long index);
    public delegate void PrintPressureData(double pressure, string msg, long index);
    public delegate void PrintHumidityData(double humidity, string msg, long index);


    public partial class LaboratoryTempMonitor : Form
    {

        
        private readonly MeasurementScheduler _bridgeScheduler;
        private readonly MeasurementScheduler _envScheduler;
        private readonly DeviceFactory deviceFactory;
        private List<FileServerUpdater> _serverUpdaters = new();

        private readonly Dictionary<string, IMeasurementDevice> _devices =
            new Dictionary<string, IMeasurementDevice>();

        private TemperatureMeasurement[] measurement_list;   //the current temperature measurement list
        private Thread[] humidity_threads;
        private string lab_name = "";
        private short h_threads;
        private short barometer_index = 0;
        private short hygrometer_index = 0;
        private List<MUX> multiplexors;
        private MUX multiplexor;
        private List<ResistanceBridge> bridges;
        private ResistanceBridge bridge;
        private bool server_update;
        private EquipmentRegister register;
        private Sensor[] prts;
        private bool force_update_server;
        private bool _isStopping = false;
        private double OA_date;
        private long interval = 6;
        private short current_channel;
        private Thread serverUpdate;
        private TextWriter configs;
        private string saved_configs_filename = "C:\\Temperature Configuration\\Saved Configs.txt";
        PrintTemperatureData prTemp;
        PrintPressureData prPres;
        PrintHumidityData prHumty;
        private VaisalaPTU300SeriesDevice _ptuDevice;
        private VaisalaIndigo500SeriesDevice _indigoDevice;





        public LaboratoryTempMonitor()
        {
            InitializeComponent();
            _bridgeScheduler = new MeasurementScheduler(TimeSpan.FromSeconds(5));
            _envScheduler = new MeasurementScheduler(TimeSpan.FromSeconds(5));


            //Microsoft doesn't directly support .ini files a
            //Conversion from .ini to .xml is required
            register = new EquipmentRegister();
            deviceFactory = new DeviceFactory(register);

            bridges = new List<ResistanceBridge>();
            multiplexors = new List<MUX>();

            

            //populate GUI drop down boxes with equipment from the equipment register
            PopulatePRTMenu_();
            PopulateBridgeMenu_();
            PopulateMuxMenu_();
            PopulateLaboratoryMenu();
            PopulatePressureComboBox_();
            PopulateHumidityComboBox_();

            //Can have up to 100 PRTs
            prts = new Sensor[100];
            prTemp = new PrintTemperatureData(ShowTemperatureData);
            prPres = new PrintPressureData(ShowPressureData);
            prHumty = new PrintHumidityData(ShowHumidityData);
            measurement_list = new TemperatureMeasurement[1];
            h_threads = 0;


            //select default values from the drop down menus.

            PRTName.Text = "T2088";
            Resistance_Bridge_Type.Text = "Hilger_Isotech";
            Laboratory.Text = "Hilger";
            Channel_Select.Text = "1";
            Time.Value = System.Convert.ToDateTime("5:00:00 pm");
            Date.Value = System.DateTime.Now;
            server_update = true;

            if (File.Exists("C:\\Temperature Configuration\\Saved Configs.txt")) LoadsavedMeasurements();

            FormClosing += LaboratoryTempMonitor_FormClosing;
        }


        private IMeasurementDevice GetOrCreateDevice(
            string eq_id,
            Func<IMeasurementDevice> factory)
        {
            if (!_devices.TryGetValue(eq_id, out var device))
            {
                device = factory();
                _devices[eq_id] = device;
            }

            return device;
        }


        private void Channel_Select_SelectedIndexChanged(object sender, EventArgs e)
        {
            current_channel = System.Convert.ToInt16(Channel_Select.Text);
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {

            interval = (long) numericUpDown1.Value;

            if (_bridgeScheduler != null)
            {
                _bridgeScheduler.Interval =
                    TimeSpan.FromSeconds(interval);
            }

        }

        /// <summary>
        /// Populates the resistance bridge menu
        /// </summary>
        private void PopulateBridgeMenu_()
        {
            //get Resistance Bridge inventory
            List<InventoryItem> items = register.WildCardInventory("Bridge");

            foreach (InventoryItem item in items)
            {
                Resistance_Bridge_Type.Items.Add(item.Id + "_" + item.Model);
            }
        }

        /// <summary>
        /// Populates the multiplexor menu from metadata found in the equipment register
        /// </summary>
        private void PopulateMuxMenu_()
        {
            //get Resistance Bridge inventory
            List<InventoryItem> items = register.WildCardInventory("Bridge");

            foreach (InventoryItem item in items)
            {
                Multiplexor_Type.Items.Add(item.Id + "_" + item.Model);
            }
        }

        /// <summary>
        /// Populates the PRT menu from metadata found in the equipment register
        /// </summary>
        private void PopulatePRTMenu_()
        {
            //get PRT inventory
            List<InventoryItem> items = register.WildCardInventory("PRT");

            foreach (InventoryItem item in items)
            {
                PRTName.Items.Add(item.Serial);
            }
        }

        /// <summary>
        /// Populates the barometer menu from metadata found in the equipment register
        /// </summary>
        private void PopulatePressureComboBox_()
        {
            //get barometer inventory
            List<InventoryItem> items = register.WildCardInventory("Barometer");

            foreach (InventoryItem item in items)
            {
                if (register.Loggable(item.Id))
                {
                    Pressure_barometers.AppendText(string.Concat(item.Id, " (", item.Model, ")\n"));
                }
            }
        }

        /// <summary>
        /// Populates the RH menu from metadata found in the equipment register
        /// </summary>
        private void PopulateHumidityComboBox_()
        {
            //get barometer inventory
            List<InventoryItem> items = register.WildCardInventory("Hygrometer");

            foreach (InventoryItem item in items)
            {
                if (register.Loggable(item.Id))
                {
                    HumidityHygrometers.AppendText(string.Concat(item.Id, " (", item.Model, ")\n"));
                }
            }
        }

        /// <summary>
        /// Get the information about the given PRT, create a new PRT and returns it
        /// </summary>
        /// <param name="prt_name_">The name of the PRT</param>
        private TemperatureSensor FindPRT(string prt_name_)
        {
            string id = "";
            string report_n = "";
            DateTime? report_date = DateTime.Now;
            string component_name = "prt";
            CalibrationType cal_type = CalibrationType.Equation;
            TemperatureSensor prtSensor = null;

            CalibrationRecord calibrationRecord;


            List<InventoryItem> items = register.WildCardInventory("PRT");
            foreach (InventoryItem item in items)
            {
                if (item.Serial == (prt_name_))
                {

                    calibrationRecord = register.GetLatestCalibration(item.Id, "prt");
                    report_n = calibrationRecord.ReportId;
                    id = item.Id;
                    report_date = calibrationRecord.ReportIssueDate;
                    component_name = calibrationRecord.ComponentName;
                    cal_type = calibrationRecord.CalibrationType;
                    break;
                }
            }
            switch(cal_type)
            {
                case CalibrationType.Equation:
                    EquationCalibration equation = new EquationCalibration(register.GetLatestEquationValue(id, "prt"), "t");
                    prtSensor = new TemperatureSensor(prt_name_, report_n, (DateTime) report_date, component_name, equation, Convert.ToInt32(Channel_Select.Text),SensorType.prt);
                    break;
                case CalibrationType.Table:
                    TableCalibration table = new TableCalibration(register.GetLatestCalibrationTable(id, "prt"));
                    prtSensor = new TemperatureSensor(prt_name_, report_n, (DateTime)report_date, component_name, table, Convert.ToInt32(Channel_Select.Text),SensorType.prt);

                    break;
                case CalibrationType.File:
                    MessageBox.Show("PRT calibration type not supported. Currently, only equations are supported");
                    break;
                default:
                    MessageBox.Show("PRT calibration type not supported. Currently, only equations are supported");
                    break;
            }
            return prtSensor;
        }

        private void PopulateLaboratoryMenu()
        {
            List<string> labs = register.GetUniqueEquipmentLocations();

            foreach (string lab in labs)
            {
                Laboratory.Items.Add(lab);
            }
        }

        private void Laboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
            lab_name = Laboratory.Text.ToString();
        }
        /// <summary>
        /// selectedText contains the id of the resistance bridge selected
        /// resistance bridge are ofter associated with network gateways by their location
        /// This means the location of both the gateway and bridge need to be specified in the equipment register.
        /// </summary>
        private void Resistance_Bridge_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = Resistance_Bridge_Type.Text;
            
        }

        private void Multiplexor_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = Multiplexor_Type.Text;
            
        }


        //function takes a string holding the value and 
        private void ShowTemperatureData(double temperature, string msg, long index)
        {

            if (!this.InvokeRequired)
            {
                Progress_Window.Text = temperature.ToString() + "   " + msg + "\n";
                //Progress_Window.AppendText(temperature.ToString()+"   "+msg+"\n");
                Progress_Window.ScrollToCaret();
            }
            else
            {
                object[] textobj = { temperature, msg, index };
                this.BeginInvoke(prTemp, textobj);
            }
        }

        //function takes a string holding the value and 
        private void ShowPressureData(double pressure, string msg, long errortype)
        {

            if (!this.InvokeRequired)
            {
                //buildChart(measurement_list[index].X.ToString(), measurement_list[index].Y.ToString(), (int) index);
                PressureOutputWindow.Text = (pressure.ToString() + "   " + msg + "\n");
                PressureOutputWindow.ScrollToCaret();
            }
            else
            {
                object[] textobj = { pressure, msg, errortype };
                this.BeginInvoke(prPres, textobj);
            }
        }

        //function takes a string holding the value and 
        private void ShowHumidityData(double humidity, string msg, long errortype)
        {

            if (!this.InvokeRequired)
            {
                //buildChart(measurement_list[index].X.ToString(), measurement_list[index].Y.ToString(), (int) index);
                HumidityOutputWindow.Text = (humidity.ToString() + "   " + msg + "\n");
                HumidityOutputWindow.ScrollToCaret();
            }
            else
            {
                object[] textobj = { humidity, msg, errortype };
                this.BeginInvoke(prHumty, textobj);
            }
        }

        //THESE NEXT TWO FUNCTIONS HAVE A PROBLEM.  FINISH DATE DOESN'T QUITE WORK
        private void Date_ValueChanged(object sender, EventArgs e)
        {
            OA_date = (Time.Value.ToOADate() - Math.Floor(Time.Value.ToOADate()))
                      + Date.Value.ToOADate();

        }

        private void Time_ValueChanged(object sender, EventArgs e)
        {
            //Get time part of the day by subtracting of the floor of the date time
            //set the new date.
            OA_date = (Time.Value.ToOADate() - Math.Floor(Time.Value.ToOADate()))
                      + Date.Value.ToOADate();

        }


        private void SaveCurrentMeasurementToConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (measurement_list == null || measurement_list.Length == 0)
            {
                MessageBox.Show(
                    "No Measurements are added so you can't create a config file.\n" +
                    "A config file can be created only when measurements are active.");
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                StringBuilder sb = new StringBuilder();
                int i = 1;

                foreach (TemperatureMeasurement meas in measurement_list)
                {
                    sb.AppendLine($"MEASUREMENT {i}");
                    sb.AppendLine($"LOCATION IN LAB:{meas.Filename}");
                    sb.AppendLine($"CHANNEL:{meas.Channel}");
                    TemperatureSensor s = (TemperatureSensor) meas.Sensor;
                    sb.AppendLine($"PRT:{s.SensorId}");
                    sb.AppendLine($"LAB NAME:{meas.LabLocation}");
                    sb.AppendLine($"BRIDGE NAME:{meas.BridgeName}");
                    sb.AppendLine($"MUX_TYPE:{meas.MUXName}");
                    i++;
                }

                sb.AppendLine("END");

                File.WriteAllText(saveFileDialog.FileName, sb.ToString());
            }
        }

        private void LoadMeasurementsFromConfig_Click(object sender, EventArgs e)
        {
            int size;
            string read_file = "";
            string text;
            bool okay = true;
            //Open a config file for reading
            DialogResult result = openConfigFile.ShowDialog(); // Show the dialog and get result.
            if (result == DialogResult.OK) // Test result.
            {
                read_file = openConfigFile.FileName;
                try
                {
                    text = File.ReadAllText(read_file);
                    size = text.Length;
                }
                catch (IOException)
                {
                    MessageBox.Show("Could not Open config file");
                }
            }
            else
            {
                okay = false;
            }
            if (okay)
            {
                string message = "Before the configuration can be loaded a new end date and time\n"
                                  + "for this measurement configuration should be set.  Okay to use\n"
                                  + "the Date and time you have selected below?\n"
                                  + "Also it is a good idea to click Stop All Measurements prior to\n"
                                  + "loading a configuration";

                var selected = MessageBox.Show(message, "Do you want to proceed?", MessageBoxButtons.YesNo);


                // Show testDialog as a modal dialog and determine if DialogResult = OK.
                if (selected == DialogResult.Yes)
                {
                    FileStream fs;
                    try
                    {
                        fs = new FileStream(saved_configs_filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        Directory.CreateDirectory("C:\\Temperature Configuration\\");
                        fs = new FileStream(saved_configs_filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    }
                    configs = new StreamWriter(fs);  //a list of saved configuration file names (these filenames are saved when the users load configurations)



                    //add the read file path to the list of saved configurations
                    configs.WriteLine(read_file);
                    configs.Close();


                    //Stream reader to parse the file
                    StreamReader file_reader = new StreamReader(read_file);

                    while (true)
                    {

                        string line_read = file_reader.ReadLine();


                        if (line_read.Contains("MEASUREMENT "))
                        {
                            continue;
                        }
                        else if (line_read.Contains("LOCATION IN LAB:"))
                        {
                            line_read = line_read.Remove(0, 16);
                            Location_String.Text = line_read;
                            continue;
                        }
                        else if (line_read.Contains("CHANNEL:"))
                        {
                            line_read = line_read.Remove(0, 8);
                            Channel_Select.Text = line_read;
                            continue;
                        }
                        else if (line_read.Contains("PRT:"))
                        {
                            line_read = line_read.Remove(0, 4);
                            PRTName.Text = line_read;
                            continue;
                        }
                        else if (line_read.Contains("LAB NAME:"))
                        {
                            line_read = line_read.Remove(0, 9);
                            Laboratory.Text = line_read;
                            continue;
                        }
                        else if (line_read.Contains("BRIDGE NAME:"))
                        {
                            line_read = line_read.Remove(0, 12);
                            Resistance_Bridge_Type.Text = line_read;
                            continue;
                        }
                        else if (line_read.Contains("MUX_TYPE:"))
                        {
                            line_read = line_read.Remove(0, 9);
                            Multiplexor_Type.Text = line_read;
                            RegisterTemperature(SensorType.prt);
                            continue;
                        }
                        else if (line_read.Contains("END"))
                        {
                            break;
                        }
                        else break;

                    }
                    file_reader.Close();

                }
            }
        }
        private void LoadsavedMeasurements()
        {
            if (!File.Exists(saved_configs_filename))
                return;

            string[] filepaths = File.ReadAllLines(saved_configs_filename);

            foreach (string filepath in filepaths)
            {
                if (string.IsNullOrWhiteSpace(filepath))
                    continue;

                try
                {
                    using (StreamReader file_reader = new StreamReader(filepath))
                    {
                        while (true)
                        {
                            string line = file_reader.ReadLine();
                            if (line == null)
                                break;

                            // ---- Parse config ----

                            if (line.StartsWith("MEASUREMENT"))
                            {
                                continue;
                            }
                            else if (line.StartsWith("LOCATION IN LAB:"))
                            {
                                Location_String.Text = line.Substring(16);
                            }
                            else if (line.StartsWith("CHANNEL:"))
                            {
                                Channel_Select.Text = line.Substring(8);
                            }
                            else if (line.StartsWith("PRT:"))
                            {
                                PRTName.Text = line.Substring(4);
                            }
                            else if (line.StartsWith("LAB NAME:"))
                            {
                                Laboratory.Text = line.Substring(9);
                            }
                            else if (line.StartsWith("BRIDGE NAME:"))
                            {
                                Resistance_Bridge_Type.Text = line.Substring(12);
                            }
                            else if (line.StartsWith("MUX_TYPE:"))
                            {
                                Multiplexor_Type.Text = line.Substring(9);

                                // ✅ This is the trigger point
                                RegisterTemperature(SensorType.prt);
                            }
                            else if (line.StartsWith("END"))
                            {
                                break;
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show($"Could not read config file: {filepath}");
                }
            }

            // ✅ Register environmental measurements (PTU / Indigo / Omega)
            StartLogging();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }

        private async void StopAllMeasurements_Click(object sender, EventArgs e)
        {

            await _bridgeScheduler.StopAsync();
            await _envScheduler.StopAsync();

        }

        

        private void AddCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegisterTemperature(SensorType.prt);
        }



        private void RemoveCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            Sensor selectedPrt = FindPRT(PRTName.Text);
            if (selectedPrt == null)
                return;

            IMeasurementTask toRemove = null;

            foreach (var task in _bridgeScheduler.Tasks)
            {
                if (task is TemperatureMeasurement tm &&
                    tm.Sensor is TemperatureSensor s &&
                    s.ReportNumber == selectedPrt.ReportNumber)
                {
                    toRemove = task;
                    break;
                }
            }

            if (toRemove != null)
            {
                _bridgeScheduler.Remove(toRemove);
            }
        }

        private void Force_Server_Update_Click(object sender, EventArgs e)
        {
            force_update_server = true;

        }


        private void StartLogging()
        {
            // ---- Pressure devices (PTU / Indigo) ----
            foreach (string line in Pressure_barometers.Lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string eq_id = line.Substring(0, 10);

                var calRecord = register.GetLatestCalibration(eq_id,"barometer");
                var inventoryItem = register.EquipmentDetails(eq_id);

                // ---- PTU ----
                if (line.Contains("PTU"))
                {
                    var device = GetOrCreateDevice(eq_id, () =>
                        new VaisalaPTU300SeriesDevice(
                            register.Address(eq_id, "ip"), 
                            Convert.ToInt32(register.Access(eq_id, "port"))));

                    RegisterPressure(device, eq_id, calRecord, inventoryItem);
                    calRecord = register.GetLatestCalibration(eq_id, "hygrometer");
                    RegisterHumidity(device, eq_id, calRecord, inventoryItem);
                    calRecord = register.GetLatestCalibration(eq_id, "thermometer");
                    RegisterTemperature(device, eq_id, calRecord, inventoryItem,SensorType.integrated_logger);
                }

                // ---- Indigo ----
                else if (line.Contains("Indigo"))
                {
                    var device = GetOrCreateDevice(eq_id, () =>
                        new VaisalaIndigo500SeriesDevice(
                            register.Address(eq_id, "ip"),
                            Convert.ToInt32(register.Access(eq_id, "port"))));

                    RegisterPressure(device, eq_id, calRecord, inventoryItem);
                    calRecord = register.GetLatestCalibration(eq_id, "hygrometer");
                    RegisterHumidity(device, eq_id, calRecord, inventoryItem);
                    calRecord = register.GetLatestCalibration(eq_id, "thermometer");
                    RegisterTemperature(device, eq_id, calRecord, inventoryItem, SensorType.integrated_logger);
                }
            }

            // ---- Humidity-only devices (Omega) ----
            foreach (string line in HumidityHygrometers.Lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string eq_id = line.Substring(0, 10);

                var calRecord = register.GetLatestCalibration(eq_id, "hygrometer");
                var inventoryItem = register.EquipmentDetails(eq_id);

                if (line.Contains("Omega"))
                {
                    var device = GetOrCreateDevice(eq_id, () =>
                        new OmegaTHDevice(register.Address(eq_id, "ip"),
                        Convert.ToInt32(register.Access(eq_id, "port"))));

                    RegisterHumidity(device, eq_id, calRecord, inventoryItem);
                }
            }
        }

        private void RegisterPressure(IMeasurementDevice device, string eq_id, CalibrationRecord calRecord, InventoryItem inventoryItem)
        {
            
            ICalibration cal = CreateCalibration(calRecord, "p");

            var sensor = new PressureSensor(
                    calRecord.EquipmentId,
                    calRecord.ReportId,
                    (DateTime)calRecord.ReportIssueDate,
                    cal,
                    calRecord.ComponentName);

            var measurement = new PressureMeasurement(
                device,
                sensor,
                calRecord.EquipmentId,
                inventoryItem.Location,
                ShowPressureData)
            {
                Enabled = register.Loggable(eq_id)
            };
            measurement.Interval = TimeSpan.FromSeconds(5);
            measurement.NextRun = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            _envScheduler.Add(measurement);
        }

        private void RegisterHumidity(IMeasurementDevice device, string eq_id, CalibrationRecord calRecord, InventoryItem inventoryItem)
        {
            ICalibration cal = CreateCalibration(calRecord, "h");
            var sensor = new HumiditySensor(
                eq_id,
                calRecord.ReportId,
                (DateTime)calRecord.ReportIssueDate,
                calRecord.ComponentName, cal);

            var measurement = new HumidityMeasurement(
                device,
                sensor,
                calRecord.EquipmentId,
                inventoryItem.Location,
                ShowHumidityData)
            {
                Log = register.Loggable(eq_id)
            };
            measurement.Interval = TimeSpan.FromSeconds(5);
            measurement.NextRun = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            _envScheduler.Add(measurement);
        }

        private void RegisterTemperature(SensorType sensor_type)
        {
            TemperatureSensor sensor = null;
            // ✅ 1. Build the PRT from the register
            if (sensor_type == SensorType.prt)
            {
                sensor = FindPRT(PRTName.Text);
                // ✅ 2. Prevent duplicates
                bool exists = _bridgeScheduler.Tasks.Any(t => t is TemperatureMeasurement tm &&
                    tm.Sensor is TemperatureSensor s &&
                    s.ReportNumber == sensor.ReportNumber &&
                    tm.Channel == sensor.Channel);

                if (exists)
                    return;

                // ✅ 3. Resolve selected bridge equipment
                if (Resistance_Bridge_Type.Text.Length < 10)
                    return;

                string eq_id = Resistance_Bridge_Type.Text.Substring(0, 10);

                InventoryItem item = register.EquipmentDetails(eq_id);
                if (item == null)
                    return;

                // ✅ 4. Create or reuse BridgeDevice
                var device = GetOrCreateDevice(
                    $"BRIDGE_{eq_id}",
                    () => deviceFactory.CreateBridgeDevice(item));

                // ✅ 5. Create measurement
                var measurement = new TemperatureMeasurement(
                    sensor: sensor,
                    device: device,
                    labLocation: Laboratory.Text,
                    fileName: Location_String.Text,
                    uiCallback: ShowTemperatureData)
                {
                    BridgeName = Resistance_Bridge_Type.Text,
                    MUXName = Multiplexor_Type.Text
                };
                
                measurement.Interval = TimeSpan.FromSeconds(Convert.ToInt32(numericUpDown1.Text));
                measurement.NextRun = DateTime.UtcNow + TimeSpan.FromSeconds(1);
                // ✅ 6. Register with scheduler
                _bridgeScheduler.Add(measurement);
            }
            else if (sensor_type == SensorType.integrated_logger)
            {
                
            }
            else if (sensor_type == SensorType.thermocouple) return;
            else if (sensor_type == SensorType.thermister) return;

            if (sensor == null)
                return;  
        }

        private void RegisterTemperature(IMeasurementDevice device, string eq_id, CalibrationRecord calRecord, InventoryItem inventoryItem, SensorType sensor_type)
        {
            ICalibration cal = CreateCalibration(calRecord, "t");
            var sensor = new TemperatureSensor(
                eq_id,
                calRecord.ReportId,
                (DateTime)calRecord.ReportIssueDate,
                calRecord.ComponentName, cal, 1, sensor_type);

            var measurement = new TemperatureMeasurement(
                sensor,
                device,
                inventoryItem.Location,
                calRecord.EquipmentId,
                ShowTemperatureData)
            {
                Log = register.Loggable(eq_id)
            };
            measurement.Interval = TimeSpan.FromSeconds(5);
            measurement.NextRun = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            _envScheduler.Add(measurement);
        }

        private ICalibration CreateCalibration (CalibrationRecord cal_record, string parameter)
        {
            ICalibration cal = null;
            switch (cal_record.CalibrationType)
            {
                case CalibrationType.Equation:
                    cal = new EquationCalibration(register.GetLatestEquationValue(cal_record.EquipmentId, cal_record.ComponentName), parameter);
                    break;
                case CalibrationType.Table:
                    //return a table calibration (can be either a hysterisis table or a regular table depending on the calibration record)
                    cal = register.GetLatestCalibrationTableObject(cal_record.EquipmentId, cal_record.ComponentName);
                    break;
                case CalibrationType.File:
                    MessageBox.Show("Calibration type not supported. Currently, only equations and tables are supported");
                    break;
                default:
                    MessageBox.Show("Calibration type not supported. Currently, only equations and tables are supported");
                    break;
            }
            return cal;

        }





        /// <summary>
        /// -Implements a graceful exit of all threads running
        /// </summary>
        private async void LaboratoryTempMonitor_FormClosing(Object sender, FormClosingEventArgs e)
        {

            e.Cancel = true;

            // Stop measurements first
            await StopLogging();

            // Stop server updater

            if (_serverUpdaters != null)
            {
                foreach (var updater in _serverUpdaters)
                {
                    await updater.StopAsync();
                }
            }


            e.Cancel = false;
            Close();

        }

        private async Task StopLogging()
        {
            if(_isStopping) return;
            _isStopping = true;

            try
            {
                // ✅ Stop schedulers
                if (_bridgeScheduler != null)
                {
                    await _bridgeScheduler.StopAsync();
                }

                if (_envScheduler != null)
                {
                    await _envScheduler.StopAsync();
                }

                // ✅ Clear tasks (bridge)
                foreach (var task in _bridgeScheduler.Tasks.ToArray())
                {
                    _bridgeScheduler.Remove(task);
                }

                // ✅ Clear tasks (env)
                foreach (var task in _envScheduler.Tasks.ToArray())
                {
                    _envScheduler.Remove(task);
                }

                // ✅ Dispose devices
                foreach (var device in _devices.Values)
                {
                    try
                    {
                        if (device is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Device dispose error: {ex.Message}");
                    }
                }

                _devices.Clear();
            }
            finally
            {
                _isStopping = false;
            }
        }

        private void LaboratoryTempMonitor_Load(object sender, EventArgs e)
        {
            

            _serverUpdaters.Add(new FileServerUpdater(
                @"C:\Temperature Monitoring Data",
                @"L:\Temperature Monitoring Data",
                TimeSpan.FromMinutes(1)));

            _serverUpdaters.Add(new FileServerUpdater(
                @"C:\Pressure Monitoring Data",
                @"L:\Pressure Monitoring Data",
                TimeSpan.FromMinutes(1)));

            _serverUpdaters.Add(new FileServerUpdater(
                @"C:\Humidity Monitoring Data",
                @"L:\Humidity Monitoring Data",
                TimeSpan.FromMinutes(1)));

            foreach (var updater in _serverUpdaters)
            {
                updater.Start();
            }

            // ✅ Schedulers starts here too
            _bridgeScheduler.Start();
            _envScheduler.Start();

        }
    }
}
