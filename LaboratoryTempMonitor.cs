using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Length_Stds_Environmental_Monitoring
{
    public delegate void PrintTemperatureData(double temperature, string msg, long index);
    public delegate void PrintPressureData(double pressure, string msg, long index);
    public delegate void PrintHumidityData(double humidity, string msg, long index);


    public partial class LaboratoryTempMonitor : Form
    {

        private readonly SeqeuencedTemperatureMeasurementManager _measurementManager;
        private TemperatureServerUpdater _serverUpdater;

        private TemperatureMeasurement[] measurement_list;   //the current temperature measurement list
        private Barometer[] barometer_list;
        private Hygrometer[] hygrometer_list;
        private Thread[] Threads;
        private Thread[] pressure_threads;
        private Thread[] humidity_threads;
        private short p_threads;
        private string lab_name = "";
        private short h_threads;
        private static short measurement_index = 0;
        private short barometer_index = 0;
        private short hygrometer_index = 0;
        private List<MUX> multiplexors;
        private MUX multiplexor;
        private List<ResistanceBridge> bridges;
        private ResistanceBridge bridge;
        private bool server_update;
        private EquipmentRegister register;
        private PRT[] prts;
        private bool force_update_server;
        private double OA_date;
        private long interval = 6;
        private short current_channel;
        private Thread serverUpdate;
        private TextWriter configs;
        private string saved_configs_filename = "C:\\Temperature Configuration\\Saved Configs.txt";
        PrintTemperatureData prTemp;
        PrintPressureData prPres;
        PrintHumidityData prHumty;





        public LaboratoryTempMonitor()
        {
            InitializeComponent();
            _measurementManager = new SeqeuencedTemperatureMeasurementManager(TimeSpan.FromSeconds(5));
            //Microsoft doesn't directly support .ini files a
            //Conversion from .ini to .xml is required
            register = new EquipmentRegister();
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
            prts = new PRT[100];
            prTemp = new PrintTemperatureData(ShowTemperatureData);
            prPres = new PrintPressureData(ShowPressureData);
            prHumty = new PrintHumidityData(ShowHumidityData);
            measurement_list = new TemperatureMeasurement[1];
            barometer_list = new Barometer[1];
            hygrometer_list = new Hygrometer[1];
            Threads = new Thread[1];
            pressure_threads = new Thread[1];
            humidity_threads = new Thread[1];
            p_threads = 0;
            h_threads = 0;


            //select default values from the drop down menus.

            PRTName.Text = "T2088";
            Resistance_Bridge_Type.Text = "Hilger_Isotech";
            Laboratory.Text = "Hilger";
            Channel_Select.Text = "1";
            Time.Value = System.Convert.ToDateTime("5:00:00 pm");
            Date.Value = System.DateTime.Now;


            server_update = true;
            StartPressureLogging();
            StartHumidityLogging();

            if (File.Exists("C:\\Temperature Configuration\\Saved Configs.txt")) LoadsavedMeasurements();

            FormClosing += LaboratoryTempMonitor_FormClosing;
        }



        private void Channel_Select_SelectedIndexChanged(object sender, EventArgs e)
        {
            current_channel = System.Convert.ToInt16(Channel_Select.Text);
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {

            interval = (long) numericUpDown1.Value;

            if (_measurementManager != null)
            {
                _measurementManager.Interval =
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
        private PRT FindPRT(string prt_name_)
        {
            string report_n = "";

            CalibrationRecord calibrationRecord = new CalibrationRecord();
            string id = "";
            List<InventoryItem> items = register.WildCardInventory("PRT");
            foreach (InventoryItem item in items)
            {
                if (item.Serial == (prt_name_))
                {

                    calibrationRecord = register.GetLatestCalibrationMetadata(item.Id);
                    report_n = calibrationRecord.ReportId;
                    id = item.Id;
                    break;
                }
            }
            string equation = register.GetLatestEquationValue(id, "prt");
            PRT selected_prt = new PRT(report_n, equation);
            return selected_prt;
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
            string SICL = "";
            string ipaddr = "";
            string gpibaddr = "";
            bool microK = false;
            bool agilent_3497__ = false;
            double internal_resistor = double.NaN;
            double tinsley_R = double.NaN;


            if (selectedText.Length > 10) selectedText = selectedText.Substring(0, 10);
            else return;

            InventoryItem item = register.EquipmentDetails(selectedText);

            foreach (ResistanceBridge b in bridges) //we have already added this bridge, so don't do it again
            {
                if (b.EqId == item.Id) return;
            }

            string location = item.Location;
            string equation1 = register.GetLatestEquationValuePartialMatch(selectedText, "Bridge 1");
            string equation2 = register.GetLatestEquationValuePartialMatch(selectedText, "Bridge 2");
            string equation3 = register.GetLatestEquationValuePartialMatch(selectedText, "Bridge 3");
            if (equation1 == null) equation1 = "r+0";
            if (equation2 == null) equation2 = "r+0";
            if (equation3 == null) equation3 = "r+0";

            if (item.Model.Contains("Micro")) microK = true;
            else if (item.Model.Contains("3497")) agilent_3497__ = true;
            else
            {
                MessageBox.Show("Equipment not supported by software");
                return;
            }
            string s = register.SpecificationElement(item.Id, "sicl");

            if (s != null && s.Contains("GPIB"))
            {
                //this must be a 34972A which has a gateway itegrated into the bridge/DAQ
                //It has a GPIB address and an IP address that we need
                SICL = register.SpecificationElement(item.Id, "sicl");
                ipaddr = register.Address(item.Id, "ip");
                gpibaddr = register.Address(item.Id, "gpib");
            }

            else
            {
                List<InventoryItem> inventoryList = register.WildCardInventory("Gateway");

                string gateway_id = "";
                foreach (InventoryItem item1 in inventoryList)
                {
                    if (item1.Location == location)
                    {
                        gateway_id = item1.Id;
                    }
                }
                SICL = register.SpecificationElement(gateway_id, "siclInterfaceID");
                ipaddr = register.Address(gateway_id, "ip");
                gpibaddr = register.Address(item.Id, "gpib");
            }

            if (SICL == "")
            {
                MessageBox.Show("Equipment Register data entry error");
                return;
            }

            string r = register.SpecificationElement(item.Id, "internalResistor");
            try
            {
                internal_resistor = Convert.ToDouble(r);
            }
            catch (FormatException)
            {
                internal_resistor = 0.0;
            }

            r = register.GetLatestEquationValue("MSLE.L.012", "100 ohm resistance");
            tinsley_R = Convert.ToDouble(r);

            if (agilent_3497__)
            {
                bridge = new AgilentBridge(Convert.ToInt16(gpibaddr), SICL);
                bridge.EqId = item.Id;
                bridge.Location = location;
                bridge.Equation1 = equation1;
                bridge.Equation2 = equation2;
                bridge.Equation3 = equation3;
                bridge.InternalResistance = internal_resistor;
                bridge.Tinsley = tinsley_R;
                bridges.Add(bridge); //multiplexors are a 1:1 association with bridges i.e the association is at the same index in the lists "Bridges" and "Multiplexors"
            }
            else if (microK)
            {
                bridge = new IsotechMicro(Convert.ToInt16(gpibaddr), SICL);
                bridge.EqId = item.Id;
                bridge.Location = location;
                bridge.Equation1 = equation1;
                bridge.Equation2 = equation2;
                bridge.Equation3 = equation3;
                bridge.InternalResistance = internal_resistor;
                bridge.Tinsley = tinsley_R;
                bridges.Add(bridge);
            }
        }

        private void Multiplexor_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = Multiplexor_Type.Text;
            string SICL = "";
            string ipaddr = "";
            string gpibaddr = "";
            bool microK = false;
            bool agilent_3497__ = false;

            if (selectedText.Length > 10) selectedText = selectedText.Substring(0, 10);
            else return;

            InventoryItem item = register.EquipmentDetails(selectedText);


            foreach (MUX m in multiplexors) //we have already added this mux, so don't do it again
            {
                if (m.EqId == item.Id) return;
            }


            string location = item.Location;

            if (item.Model.Contains("Micro")) microK = true;
            else if (item.Model.Contains("3497")) agilent_3497__ = true;
            else
            {
                MessageBox.Show("Equipment not supported by software");
                return;
            }

            string s = register.SpecificationElement(item.Id, "sicl");

            if (s != null && s.Contains("GPIB"))
            {
                //this must be a 34972A which has a gateway itegrated into the bridge/DAQ
                //It has a GPIB address and an IP address that we need
                SICL = register.SpecificationElement(item.Id, "sicl");
                ipaddr = register.Address(item.Id, "ip");
                gpibaddr = register.Address(item.Id, "gpib");

            }

            else
            {
                List<InventoryItem> inventoryList = register.WildCardInventory("Gateway");

                string gateway_id = "";
                foreach (InventoryItem item1 in inventoryList)
                {
                    if (item1.Location == location)
                    {
                        gateway_id = item1.Id;
                    }
                }

                SICL = register.SpecificationElement(gateway_id, "siclInterfaceID");
                ipaddr = register.Address(gateway_id, "ip");
                gpibaddr = register.Address(item.Id, "gpib");
            }

            if (agilent_3497__)
            {
                multiplexor = new AgilentMUX(ref prts);
                multiplexor.EqId = item.Id;
                multiplexors.Add(multiplexor);
                int count = multiplexors.Count;
                bridges[count - 1].SetMUX(multiplexor); //multiplexors are a 1:1 association with bridges i.e the association is at the same index in the lists "Bridges" and "Multiplexors"

            }
            else if (microK)
            {
                multiplexor = new IsotechMux(Convert.ToInt16(gpibaddr), SICL, ref prts);
                multiplexor.EqId = item.Id;
                multiplexors.Add(multiplexor);
                int count = multiplexors.Count;
                bridges[count - 1].SetMUX(multiplexor); //multiplexors are a 1:1 association with bridges i.e the association is at the same index in the lists "Bridges" and "Multiplexors"
            }
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
                    sb.AppendLine($"PRT:{meas.Probe.PRTName}");
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
                            AddMeasurement();
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

            //add the read file path to the list of saved configurations
            string[] filepaths = File.ReadAllLines(saved_configs_filename);

            foreach (string filepath in filepaths)
            {
                //Stream reader to parse the file
                StreamReader file_reader = new StreamReader(filepath);

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
                        AddMeasurement();
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


        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }

        private async void StopAllMeasurements_Click(object sender, EventArgs e)
        {

            await _measurementManager.StopAsync();

        }

        

        private void AddCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddMeasurement();
        }



        private void RemoveCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            PRT selectedPrt = FindPRT(PRTName.Text);
            if (selectedPrt == null)
                return;

            TemperatureMeasurement toRemove = null;

            foreach (var measurement in _measurementManager.Measurements)
            {
                if (measurement.Probe.getReportNumber() ==
                    selectedPrt.getReportNumber())
                {
                    toRemove = measurement;
                    break;
                }
            }

            if (toRemove != null)
            {
                _measurementManager.Remove(toRemove);
            }
        }


        private void AddMeasurement()
        {


            // 1. Build the PRT from the register
            PRT prt = FindPRT(PRTName.Text);
            prt.PRTName = PRTName.Text;

            // 2. Associate probe with MUX channel (legacy behavior preserved)
            multiplexor.setProbe(prt, current_channel);

            // 3. Create the measurement
            var measurement = new TemperatureMeasurement(
                measurementIndex: 0, // index no longer matters yet
                prt: prt,
                bridge: bridge,
                channel: current_channel,
                labLocation: Laboratory.Text,
                fileName: Location_String.Text,
                uiCallback: ShowTemperatureData
            )
            {
                BridgeName = Resistance_Bridge_Type.Text,
                MUXName = Multiplexor_Type.Text
            };

            // 4. Hand off responsibility to the manager

            _measurementManager.Add(measurement);
            _measurementManager.Start(); // safe to call multiple times

        }






        private void Force_Server_Update_Click(object sender, EventArgs e)
        {
            force_update_server = true;

        }
        private void StartPressureLogging()
        {

            barometer_index = 1;
            int i = 0;
            foreach (string line in Pressure_barometers.Lines)
            {
                if (line.Equals("")) break;
                string eq_id = line.Substring(0, 10);
                CalibrationRecord calRecord = new CalibrationRecord();
                calRecord = register.GetLatestCalibrationMetadata(eq_id);
                InventoryItem inventoryItem = new InventoryItem();
                inventoryItem = register.EquipmentDetails(eq_id);

                if (line.Contains("PTB220A"))
                {
                    //create a delegate to wait for the pressure data to arrive
                    //PrintPressureData pdel = new PrintPressureData(showPressureData);
                    //barometer_list[i] = new VaisalaPTU300Barometer("", 80, ref pdel); this should change to a PTB220A object when implemented
                    Array.Resize(ref barometer_list, barometer_index + 1);
                }
                else if (line.Contains("PTU303"))
                {
                    //create a delegate to wait for the pressure data to arrive
                    PrintPressureData pdel2 = new PrintPressureData(ShowPressureData);

                    //instantiate the object, if required.
                    barometer_list[i] = new VaisalaPTU300Barometer("", 23, ref pdel2);
                    VaisalaPTU300Barometer ptu303 = (VaisalaPTU300Barometer)barometer_list[i];
                    barometer_index++;
                    ptu303.OpState = true;
                    Array.Resize(ref barometer_list, barometer_index + 1);

                    string[][] table = register.GetLatestCalibrationTable(eq_id, "Barometer");

                    ptu303.ReportNumber = calRecord.ReportId;
                    ptu303.ReportDate = calRecord.ReportIssueDate.ToString();
                    ptu303.EquipID = calRecord.EquipmentId;
                    ptu303.EquipType = calRecord.ComponentName;
                    ptu303.IP = register.Address(eq_id, "ip");
                    ptu303.Location = inventoryItem.Location;
                    ptu303.Filename = ptu303.EquipID + ".txt";
                    ptu303.P950 = "0.0:0.0";
                    ptu303.P960 = "0.0:0.0";
                    ptu303.P970 = "0.0:0.0";
                    ptu303.P980 = "0.0:0.0";
                    ptu303.P990 = "0.0:0.0";
                    ptu303.P1000 = "0.0:0.0";
                    ptu303.P1010 = "0.0:0.0";
                    ptu303.P1020 = "0.0:0.0";
                    ptu303.P1030 = "0.0:0.0";
                    ptu303.P1040 = "0.0:0.0";
                    ptu303.P1050 = "0.0:0.0";
                    try
                    {
                        ptu303.P950 = string.Concat(table[3][1], ":", table[3][2]);
                        ptu303.P960 = string.Concat(table[4][1], ":", table[4][2]);
                        ptu303.P970 = string.Concat(table[5][1], ":", table[5][2]);
                        ptu303.P980 = string.Concat(table[6][1], ":", table[6][2]);
                        ptu303.P990 = string.Concat(table[7][1], ":", table[7][2]);
                        ptu303.P1000 = string.Concat(table[8][1], ":", table[8][2]);
                        ptu303.P1010 = string.Concat(table[9][1], ":", table[9][2]);
                        ptu303.P1020 = string.Concat(table[10][1], ":", table[10][2]);
                        ptu303.P1030 = string.Concat(table[11][1], ":", table[11][2]);
                        ptu303.P1040 = string.Concat(table[12][1], ":", table[12][2]);
                        ptu303.P1050 = string.Concat(table[13][1], ":", table[13][2]);
                    }
                    catch (IndexOutOfRangeException) { }

                    //create a thread whose job is to querry a PTU300
                    Thread newthread = new Thread(new ParameterizedThreadStart(ptu303.Measure));
                    newthread.Priority = ThreadPriority.Normal;
                    newthread.IsBackground = true;
                    newthread.Start(ptu303);
                }

                else if (line.Contains("Indigo500"))
                {
                    //create a delegate to wait for the pressure data to arrive
                    PrintPressureData pdel3 = new PrintPressureData(ShowPressureData);

                    //instantiate the object, if required.
                    barometer_list[i] = new VaisalaIndigo500SeriesBarometer("", 502, ref pdel3);
                    VaisalaIndigo500SeriesBarometer indigo500 = (VaisalaIndigo500SeriesBarometer)barometer_list[i];
                    barometer_index++;
                    indigo500.OpState = true;
                    Array.Resize(ref barometer_list, barometer_index + 1);

                    string[][] table = register.GetLatestCalibrationTable(eq_id, "Barometer");
                    indigo500.ReportNumber = calRecord.ReportId;
                    indigo500.ReportDate = calRecord.ReportIssueDate.ToString();
                    indigo500.EquipID = calRecord.EquipmentId;
                    indigo500.EquipType = calRecord.ComponentName;
                    indigo500.IP = register.Address(eq_id, "ip");
                    indigo500.Location = inventoryItem.Location;
                    indigo500.Filename = indigo500.EquipID + ".txt";
                    indigo500.P950 = "0.0:0.0";
                    indigo500.P960 = "0.0:0.0";
                    indigo500.P970 = "0.0:0.0";
                    indigo500.P980 = "0.0:0.0";
                    indigo500.P990 = "0.0:0.0";
                    indigo500.P1000 = "0.0:0.0";
                    indigo500.P1010 = "0.0:0.0";
                    indigo500.P1020 = "0.0:0.0";
                    indigo500.P1030 = "0.0:0.0";
                    indigo500.P1040 = "0.0:0.0";
                    indigo500.P1050 = "0.0:0.0";
                    try
                    {
                        indigo500.P950 = string.Concat(table[3][1], ":", table[3][2]);
                        indigo500.P960 = string.Concat(table[4][1], ":", table[4][2]);
                        indigo500.P970 = string.Concat(table[5][1], ":", table[5][2]);
                        indigo500.P980 = string.Concat(table[6][1], ":", table[6][2]);
                        indigo500.P990 = string.Concat(table[7][1], ":", table[7][2]);
                        indigo500.P1000 = string.Concat(table[8][1], ":", table[8][2]);
                        indigo500.P1010 = string.Concat(table[9][1], ":", table[9][2]);
                        indigo500.P1020 = string.Concat(table[10][1], ":", table[10][2]);
                        indigo500.P1030 = string.Concat(table[11][1], ":", table[11][2]);
                        indigo500.P1040 = string.Concat(table[12][1], ":", table[12][2]);
                        indigo500.P1050 = string.Concat(table[13][1], ":", table[13][2]);
                    }
                    catch (IndexOutOfRangeException) { }

                    //create a thread whose job is to querry the indigo500
                    Thread newthread2 = new Thread(new ParameterizedThreadStart(indigo500.Measure));
                    newthread2.Priority = ThreadPriority.Normal;
                    newthread2.IsBackground = true;
                    newthread2.Start(indigo500);
                }



                i++;
            }
            //create a new thread to update the server with pressure data
            Thread P_ServerUpdate = new Thread(new ParameterizedThreadStart(PressureServerUpdater));
            P_ServerUpdate.Start(barometer_list);
            return;
        }

        private void PressureServerUpdater(object stateInfo)
        {
            Barometer[] b_list = ((Barometer[])stateInfo);


            //update the server every minute
            DateTime current_time;
            int stored_hour = (System.DateTime.Now).Hour;   //store this hour
            int stored_month = (System.DateTime.Now).Month;  //store this month
            int stored_minute = DateTime.Now.Minute; //store this minute
            int hour;
            int month;
            int minute;


            while (server_update)
            {
                Thread.CurrentThread.Join(2000);
                current_time = System.DateTime.Now;  //the time stamp now
                hour = current_time.Hour;  //the hour now
                month = current_time.Month;   //The month now
                minute = current_time.Minute;

                if (stored_minute != minute || force_update_server)
                {
                    //turn off force update server
                    force_update_server = false;

                    //do server update
                    stored_minute = (System.DateTime.Now).Minute;   //get the new MINUTE we are in
                    int i = 0;
                    string di = "";
                    string dc = "";
                    while (i < b_list.Count())
                    {
                        if (b_list[i] != null)
                        {
                            b_list[i].GetDirectories(ref di, ref dc);

                            //try and do a file copy until we find a way that works
                            while (true)
                            {
                                try
                                {
                                    //append any recent data to the file that exists on the server
                                    using (Stream local = File.OpenRead(dc + b_list[i].Filename))
                                    using (FileStream server = File.Open(di + b_list[i].Filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                                    {
                                        local.CopyTo(server);
                                    }
                                    //we have a successful write to the server so we can delete the current local copy (as we don't want to write duplicate data to the server
                                    File.Delete(dc + b_list[i].Filename);
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    //this has probably occured because someone has opened the file on the server and is looking at it, allow them to
                                    //do so.  When they finally close it we can do the copy
                                    Thread.CurrentThread.Join(10000);  //sleep for 10 seconds and try again
                                    continue;
                                }
                                catch (DirectoryNotFoundException)
                                {
                                    //This will have occured because someone deleted the directory laid down originally by the measurement thread
                                    //To overcome this we will rebuild the directory
                                    try
                                    {
                                        System.IO.Directory.CreateDirectory(di);
                                    }
                                    catch (System.IO.DirectoryNotFoundException)
                                    {
                                        Thread.CurrentThread.Join(10000);  //sleep for 10 seconds and try again
                                        continue;
                                    }
                                }
                                catch (FileNotFoundException)
                                {

                                    //this means the file does not exist on c:  we can't write a file that doesn't exist
                                    Thread.CurrentThread.Join(10000);  //sleep for 10 seconds and try again
                                    break;
                                }
                                catch (IOException)
                                {
                                    //This means we can't talk to the server, not much we can do but keep trying
                                    Thread.CurrentThread.Join(10000);
                                    continue;
                                }
                            }
                            Thread.CurrentThread.Join(10000);  //sleep for 10 seconds and try again.

                        }
                        i++;
                    }

                }

                //if we have changed month we need to reset the directory folder
                if (stored_month != month)
                {
                    stored_month = (System.DateTime.Now).Month;   //store the new month we are in

                    for (int i = 0; i < barometer_index; i++)
                    {
                        if (barometer_list[i] != null) barometer_list[i].SetDirectory();
                    }

                }
            }

        }

        private void StartHumidityLogging()
        {
            for (int i = 0; i <= HumidityHygrometers.Lines.Count(); i++)
            {

                string eq_id = "";
                CalibrationRecord calRecord = new CalibrationRecord();
                InventoryItem inventoryItem = new InventoryItem();

                string line = HumidityHygrometers.Lines.ElementAt(i);

                if (!line.Equals(""))
                {
                    eq_id = line.Substring(0, 10);
                    calRecord = register.GetLatestCalibrationMetadata(eq_id);
                    inventoryItem = register.EquipmentDetails(eq_id);
                }
                if (line.Contains("Omega") || line.Contains("omega")) line = "Omega";
                if (line.Contains("ptu") || line.Contains("PTU")) line = "PTU";
                if (line.Contains("indigo") || line.Contains("Indigo")) line = "VaisalaIndigo";

                switch (line)
                {

                    case "PTU":
                        //create a delegate to wait for the humidity data to arrive
                        PrintHumidityData hdel1 = new PrintHumidityData(ShowHumidityData);

                        //add a new humidity device to the device list
                        hygrometer_list[hygrometer_index] = new VaisalaPTU300Hygrometer("", "", ref hdel1);

                        //get a handle on it
                        VaisalaPTU300Hygrometer ptu303 = (VaisalaPTU300Hygrometer)hygrometer_list[hygrometer_index];

                        //resize the array.
                        hygrometer_index++;
                        Array.Resize(ref hygrometer_list, hygrometer_index + 1);

                        ptu303.OpState = true;
                        ptu303.ReportNumber = calRecord.ReportId;
                        ptu303.ReportDate = calRecord.ReportIssueDate.ToString();
                        ptu303.EquipID = calRecord.EquipmentId;
                        ptu303.EquipType = calRecord.ComponentName;
                        ptu303.IP = register.Address(eq_id, "ip");
                        ptu303.Location = inventoryItem.Location;
                        ptu303.HLoggerEq = register.GetLatestEquationValue(eq_id, "Hygrometer");
                        ptu303.Filename = ptu303.EquipID + ".txt";

                        foreach (Barometer b in barometer_list)
                        {
                            if (b != null)
                            {
                                if (b.GetType() == typeof(VaisalaPTU300Barometer))
                                {
                                    VaisalaPTU300Barometer b_ = (VaisalaPTU300Barometer)b;

                                    if (b_.EquipID == ptu303.EquipID)
                                    {
                                        b_.HumidityTransducer = ptu303;
                                    }
                                }
                            }
                        }
                        break;
                    case "VaisalaIndigo":

                        //create a delegate to wait for the humidity data to arrive
                        PrintHumidityData hdel2 = new PrintHumidityData(ShowHumidityData);

                        //add a new humidity device to the device list
                        hygrometer_list[hygrometer_index] = new VaisalaIndigo500SeriesHygrometer("", "", ref hdel2);

                        //get a handle on it
                        VaisalaIndigo500SeriesHygrometer indigo500 = (VaisalaIndigo500SeriesHygrometer)hygrometer_list[hygrometer_index];

                        //resize the array.
                        hygrometer_index++;
                        Array.Resize(ref hygrometer_list, hygrometer_index + 1);

                        indigo500.OpState = true;
                        indigo500.ReportNumber = calRecord.ReportId;
                        indigo500.ReportDate = calRecord.ReportIssueDate.ToString();
                        indigo500.EquipID = calRecord.EquipmentId;
                        indigo500.EquipType = calRecord.ComponentName;
                        indigo500.IP = register.Address(eq_id, "ip");
                        indigo500.Location = inventoryItem.Location;
                        indigo500.HLoggerEq = register.GetLatestEquationValue(eq_id, "Hygrometer");
                        indigo500.Filename = indigo500.EquipID + ".txt";

                        foreach (Barometer b in barometer_list)
                        {
                            if (b != null)
                            {
                                if (b.GetType() == typeof(VaisalaIndigo500SeriesBarometer))
                                {

                                    VaisalaIndigo500SeriesBarometer b_ = (VaisalaIndigo500SeriesBarometer)b;

                                    if (b_.EquipID == indigo500.EquipID)
                                    {
                                        b_.HumidityTransducer = indigo500;
                                    }
                                }
                            }
                        }
                        break;
                    case "Omega":
                        //create a delegate to wait for the humidity data to arrive
                        PrintHumidityData hdel3 = new PrintHumidityData(ShowHumidityData);

                        //instantiate the object
                        hygrometer_list[hygrometer_index] = new OmegaTHLogger("", "", ref hdel3);

                        //increase the size of the hydrometer array so we can fit the next entry
                        hygrometer_index++;
                        Array.Resize(ref hygrometer_list, hygrometer_index + 1);

                        //get a handle of the object at the top of the list
                        OmegaTHLogger omega = (OmegaTHLogger)hygrometer_list[i];
                        omega.Log = register.Loggable(eq_id);
                        omega.OpState = true;
                        omega.ReportNumber = calRecord.ReportId;
                        omega.ReportDate = calRecord.ReportIssueDate.ToString();
                        omega.EquipID = calRecord.EquipmentId;
                        omega.EquipType = calRecord.ComponentName;
                        omega.IP = register.Address(eq_id, "ip");
                        omega.Location = inventoryItem.Location;
                        omega.HLoggerEq = register.GetLatestEquationValue(eq_id, "Hygrometer");
                        omega.Filename = omega.EquipID + ".txt";

                        if (omega.Log)
                        {
                            //create a thread whose job is to querry the omega logger
                            Thread newthread = new Thread(new ParameterizedThreadStart(omega.HLoggerQuery));
                            newthread.Priority = ThreadPriority.Normal;
                            newthread.IsBackground = true;
                            newthread.Start(omega);
                            humidity_threads[h_threads] = newthread;
                            h_threads++;
                            Array.Resize(ref humidity_threads, h_threads + 1);
                        }
                        break;
                    default:
                        //create a new thread to update the server with pressure data
                        Thread H_ServerUpdate = new Thread(new ParameterizedThreadStart(HumidityServerUpdater));
                        H_ServerUpdate.Start(hygrometer_list);
                        return;
                }
            }


        }

        private void HumidityServerUpdater(object stateInfo)
        {
            Hygrometer[] h_list = ((Hygrometer[])stateInfo);


            //update the server every minute
            DateTime current_time;
            int stored_hour = (System.DateTime.Now).Hour;   //store this hour
            int stored_month = (System.DateTime.Now).Month;  //store this month
            int stored_minute = DateTime.Now.Minute; //store this minute
            int hour;
            int month;
            int minute;


            while (server_update)
            {
                Thread.CurrentThread.Join(2000);
                current_time = System.DateTime.Now;  //the time stamp now
                hour = current_time.Hour;  //the hour now
                month = current_time.Month;   //The month now
                minute = current_time.Minute;

                if (stored_minute != minute || force_update_server)
                {
                    //turn off force update server
                    force_update_server = false;

                    //do server update
                    stored_minute = (System.DateTime.Now).Minute;   //get the new hour we are in
                    int i = 0;
                    string di = "";
                    string dc = "";
                    bool error_reported_ = false;
                    while (i < h_list.Count())
                    {
                        if (h_list[i] != null)
                        {
                            h_list[i].GetDirectories(ref di, ref dc);

                            //try and do a file copy until we find a way that works
                            while (true)
                            {
                                try
                                {
                                    //append any recent data to the file that exists on the server
                                    using (Stream local = File.OpenRead(dc + h_list[i].Filename))
                                    using (FileStream server = File.Open(di + h_list[i].Filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                                    {
                                        local.CopyTo(server);
                                    }
                                    //we have a successful write to the server so we can delete the current local copy (as we don't want to write duplicate data to the server
                                    File.Delete(dc + h_list[i].Filename);

                                    error_reported_ = false;
                                    break;
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    //this has probably occured because someone has opened the file on the server and is looking at it, allow them to
                                    //do so.  When they finally close it we can do the copy
                                    Thread.CurrentThread.Join(10000);  //sleep for 10 seconds and try again
                                    break;
                                }
                                catch (DirectoryNotFoundException)
                                {
                                    //This will have occured because someone deleted the directory laid down originally by the measurement thread
                                    //To overcome this we will rebuild the directory
                                    try
                                    {
                                        System.IO.Directory.CreateDirectory(di);
                                    }
                                    catch (System.IO.DirectoryNotFoundException)
                                    {
                                        Thread.CurrentThread.Join(10000);  //sleep for 10 seconds and try again
                                        continue;
                                    }
                                }
                                catch (FileNotFoundException)
                                {
                                    Thread.CurrentThread.Join(10000);  //sleep for 10 seconds and try again
                                    //this means the file does not exist on c:  we can't write a file that doesn't exist
                                    break;
                                }
                                catch (IOException)
                                {
                                    Thread.CurrentThread.Join(10000);  //sleep for 10 seconds and try again
                                    //This means we can't talk to the server, not much we can do but keep trying
                                    break;
                                }
                            }
                            Thread.CurrentThread.Join(10000);  //sleep for 10 seconds and try again.


                        }
                        i++;
                    }
                }

                //if we have changed month we need to reset the directory folder
                if (stored_month != month)
                {
                    stored_month = (System.DateTime.Now).Month;   //store the new month we are in

                    for (int i = 0; i < hygrometer_index; i++)
                    {
                        hygrometer_list[i].SetDirectory();
                    }

                }
            }

        }



        /// <summary>
        /// -Implements a graceful exit of all threads running
        /// </summary>
        private async void LaboratoryTempMonitor_FormClosing(Object sender, FormClosingEventArgs e)
        {

            e.Cancel = true;

            // Stop measurements first
            if (_measurementManager != null)
                await _measurementManager.StopAsync();

            // Stop server updater
            if (_serverUpdater != null)
                await _serverUpdater.StopAsync();

            e.Cancel = false;
            Close();

        }



        private void LaboratoryTempMonitor_Load(object sender, EventArgs e)
        {

            _serverUpdater = new TemperatureServerUpdater(
                localRoot: @"C:\Temperature Monitoring Data",
                serverRoot: @"L:\Temperature Monitoring Data",
                syncInterval: TimeSpan.FromMinutes(1));

            _serverUpdater.Start();

        }
    }
}
