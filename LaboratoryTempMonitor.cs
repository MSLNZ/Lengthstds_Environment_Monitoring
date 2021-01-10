using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Threading;
using System.Web;
//using Microsoft.Office.Interop.Excel;
//using AxMicrosoft.Office.Interop.Owc11;
//using Microsoft.Office.Interop.OWC;


namespace Temperature_Monitor
{
    public delegate void PrintTemperatureData(double temperature,string msg,long index);
    public delegate void PrintPressureData(double pressure,string msg,long index);
    public delegate void PrintHumidityData(double humidity, string msg, long index);
    

    public partial class LaboratoryTempMonitor : Form
    {
        
        private XmlTextReader xmlreader;
        private XmlReaderSettings settings;
        private TemperatureMeasurement[] measurement_list;   //the current measurement list
        private Thread[] Threads;
        private Thread[] pressure_threads;
        private Thread[] humidity_threads;
        private short p_threads;
        private short h_threads;
        private static short measurement_index = 0;
        private short barometer_index = 0;
        private short hygrometer_index = 0;
        private MUX multiplexor;
        private IsotechMux h_plexor;
        private AgilentMUX a_plexor;
        private ResistanceBridge bridge;
        private IsotechMicro isotech_bridge;
        private AgilentBridge a_agilent;
        private AgilentBridge b_agilent;
        private AgilentBridge c_agilent;
        private bool server_update;
    
        private Barometer[] barometer_list;
        private Hygrometer[] hygrometer_list;
        private PRT[] prts;
        private bool force_update_server;
        private short isotech_gpib_address = 1;
        //private DateTime endDateTime;
        private double OA_date;
        private long interval = 10;
        //private long num_samples = 3;
        //private long averaging_points = 1;
        private short current_channel;
        private string ipaddress = "131.203.8.237";
        private string equiptype = "GPIB NETWORK GATEWAY";
        private string gatewaytype = "E5810A";
        private Thread serverUpdate;
        string xmlfilename;
      
        



        public LaboratoryTempMonitor()
        {
            InitializeComponent();
            
            //Microsoft doesn't directly support .ini files a
            //Conversion from .ini to .xml is required
            DoIni2XmlConversion();
            PopulatePRTMenu();
            PopulateBridgeMenu();
            PopulateLaboratoryMenu();
            PopulatePressureComboBox();
            PopulateHumidityComboBox();
            
            //Can have up to 100 PRTs
            prts = new PRT[100];

            
            measurement_list = new TemperatureMeasurement[1];
            barometer_list = new Barometer[1];
            hygrometer_list = new Hygrometer[1];
            Threads = new Thread[1];
            pressure_threads = new Thread[1];
            humidity_threads = new Thread[1];
            p_threads = 0;
            h_threads = 0;

            //select default values from the drop down menus.
            Multiplexor_Type.Text = "Hilger Lab Multiplexor";
            PRTName.Text = "T2088";
            Resistance_Bridge_Type.Text = "Hilger_Isotech";
            Laboratory.Text = "Hilger";
            Channel_Select.Text = "1";
            Time.Value = System.Convert.ToDateTime("5:00:00 pm");
            Date.Value = System.DateTime.Now;
            server_update = true;
            StartPressureLogging();
            StartHumidityLogging();

            FormClosing += LaboratoryTempMonitor_FormClosing;
        }
        


        private void Channel_Select_SelectedIndexChanged(object sender, EventArgs e)
        {
                current_channel = System.Convert.ToInt16(Channel_Select.Text);
        }

        private DateTime GetDT()
        {
            
            return DateTime.FromOADate(OA_date);
            
            
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            interval = System.Convert.ToInt64(numericUpDown1.Text);
        }
        //private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        //{
        //    averaging_points = System.Convert.ToInt64(numericUpDown2.Text);
       // }

        //Does an INI2XML conversion       
        private void DoIni2XmlConversion()
        {
            Progress_Window.AppendText("Attempting .ini to .xml conversion\n");
            //calibration data file is better accessed off the C drive, so parse .ini file
            //is saved to the C drive.
            xmlfilename = @"I:\MSL\Private\LENGTH\EQUIPREG\cal_data.xml";
            string inifilename = @"I:\MSL\Private\LENGTH\EQUIPREG\cal_data.ini";

            if (INI2XML.Convert(inifilename, ref xmlfilename))
            {
                TextReader tr = new StreamReader(xmlfilename);
                tr.Close();

                Progress_Window.AppendText("Successfully converted\n");
            }
            else
            {
                Progress_Window.AppendText("Problem converting: file in use .... new version created\n...proceeding");
               
            }
        }
        /// <summary>
        /// Loads the xml file located on the C drive
        /// <summary>
        private void LoadXML()
        {
            //create a new xml reader setting object incase we need to change settings on the fly
            settings = new XmlReaderSettings();
           
            //create a new xml doc
            xmlreader = new XmlTextReader(xmlfilename);

        }

        /// <summary>
        /// Populates the resistance bridge menu
        /// </summary>
        private void PopulateBridgeMenu()
        {

            //set the reader to point at the start of the file
            LoadXML();

            xmlreader.ResetState();
            //read the first node
            xmlreader.ReadStartElement();

            while (!xmlreader.EOF)
            {
                while (xmlreader.Name.Contains("RESISTANCEBRIDGE"))
                {
                    xmlreader.Read();
                    while (xmlreader.LocalName.Contains("resistance"))
                    {
                        string res_bridge = xmlreader.LocalName;
                        res_bridge = res_bridge.Remove(0, 16);          //remove the resistancebridge prefix off the start (makes viewing in the menu nicer)
                        Resistance_Bridge_Type.Items.Add(res_bridge);
                        xmlreader.Skip();
                    }
                }
                xmlreader.Skip();
            }
        }


        /// <summary>
        /// Loads the PRTS from the xml file and populates the menu in the GUI
        /// </summary>
        private void PopulatePRTMenu()
        {
            //set the reader to point at the start of the file
            LoadXML();

            xmlreader.ResetState();
            //read the first node
            xmlreader.ReadStartElement();

            //parse the rest of the xml file
            while (!xmlreader.EOF)
            {
                while (xmlreader.Name.Contains("PRT"))
                {
                    xmlreader.Read();
                    while (xmlreader.LocalName.Contains("prt"))
                    {
                        string prt_name = xmlreader.LocalName;
                        prt_name = prt_name.Remove(0, 3);          //remove the prt prefix off the start (makes viewing in the menu nicer)
                        PRTName.Items.Add(prt_name);
                        xmlreader.Skip();     
                    }     
                }
                xmlreader.Skip();
            }
        }

        /// <summary>
        /// Loads the barometers from the xml file and populates the menu in the GUI
        /// </summary>
        private void PopulatePressureComboBox()
        {
            //set the reader to point at the start of the file
            LoadXML();

            xmlreader.ResetState();

            //read the first node
            xmlreader.ReadStartElement();

            //parse the rest of the xml file
            while (!xmlreader.EOF)
            {
                while (xmlreader.Name.Contains("BAROMETER"))
                {
                    xmlreader.Read();
                    while (xmlreader.LocalName.Contains("barometer"))
                    {
                        string barometer_name = xmlreader.LocalName;
                        barometer_name = barometer_name.Remove(0, 9);          //remove the prt prefix off the start (makes viewing in the menu nicer)
                        Pressure_barometers.AppendText(String.Concat(barometer_name,"\n"));
                        xmlreader.Skip();
                    }
                }
                xmlreader.Skip();
            }
        }

        /// <summary>
        /// Loads the relative humidity devices from the xml file and populates the menu in the GUI
        /// </summary>
        private void PopulateHumidityComboBox()
        {
            //set the reader to point at the start of the file
            LoadXML();

            xmlreader.ResetState();

            //read the first node
            xmlreader.ReadStartElement();

            //parse the rest of the xml file
            while (!xmlreader.EOF)
            {
                while (xmlreader.Name.Contains("HUMIDITY"))
                {
                    xmlreader.Read();
                    while (xmlreader.LocalName.Contains("humidity"))
                    {
                        string rh_name = xmlreader.LocalName;
                        rh_name = rh_name.Remove(0, 8);          //remove the prt prefix off the start (makes viewing in the menu nicer)
                        HumidityHygrometers.AppendText(String.Concat(rh_name, "\n"));
                        xmlreader.Skip();
                    }
                }
                xmlreader.Skip();
            }
        }


        /// <summary>
        /// Get the information about the given PRT, create a new PRT and returns it
        /// </summary>
        /// <param name="prt_name_">The name of the PRT</param>
        private PRT FindPRT(string prt_name_){
            //set the reader to point at the start of the file
            LoadXML();

            xmlreader.ResetState();
            //read the first node
            xmlreader.ReadStartElement();

            xmlreader.ReadToDescendant(string.Concat("prt",prt_name_));

            xmlreader.ReadToFollowing("reportnumber");
            string report_n = xmlreader.ReadElementString();
            xmlreader.ReadToFollowing("r0");
            double r0_ = System.Convert.ToDouble(xmlreader.ReadElementString());
            //xmlreader.readToFollowing("a");
            double a_ = System.Convert.ToDouble(xmlreader.ReadElementString());
            //xmlreader.ReadToFollowing("b");
            double b_ = System.Convert.ToDouble(xmlreader.ReadElementString());
            PRT selected_prt = new PRT(report_n, a_, b_,r0_);
            return selected_prt;
        }
        private void PopulateLaboratoryMenu()
        {
            //set the reader to point at the start of the file
            LoadXML();
            

            //read the first node
            xmlreader.ReadStartElement();

            //parse the rest of the xml file
            while (!xmlreader.EOF)
            {
                while (xmlreader.Name.Contains("LABORATORY"))
                {
                    xmlreader.Read();
                    while (xmlreader.LocalName.Contains("laboratory"))
                    {
                        string lab_name = xmlreader.LocalName;
                        lab_name = lab_name.Remove(0, 10);          //remove the prt suffix off the start (makes viewing in the menu nicer)
                        Laboratory.Items.Add(lab_name);
                        xmlreader.Skip();
                    }
                }
                xmlreader.Skip();
            }
        }
        private void GetBridgeCorrection(string bridge_name_,ref double A1_1,ref double A2_1, ref double A3_1, ref double A1_2, ref double A2_2, ref double A3_2, ref double A1_3, ref double A2_3, ref double A3_3, ref double ir, ref double tinsley)
        {
            //set the reader to point at the start of the file
            LoadXML();

            xmlreader.ResetState();
            //read the first node
            xmlreader.ReadStartElement();
            xmlreader.ReadToNextSibling("RESISTANCEBRIDGE");
            xmlreader.ReadToDescendant(string.Concat("resistancebridge", bridge_name_));
            xmlreader.ReadToFollowing("A1_1");
            A1_1 = System.Convert.ToDouble(xmlreader.ReadElementString());
            A2_1 = System.Convert.ToDouble(xmlreader.ReadElementString());
            A3_1 = System.Convert.ToDouble(xmlreader.ReadElementString());
            A1_2 = System.Convert.ToDouble(xmlreader.ReadElementString());
            A2_2 = System.Convert.ToDouble(xmlreader.ReadElementString());
            A3_2 = System.Convert.ToDouble(xmlreader.ReadElementString());
            A1_3 = System.Convert.ToDouble(xmlreader.ReadElementString());
            A2_3 = System.Convert.ToDouble(xmlreader.ReadElementString());
            A3_3 = System.Convert.ToDouble(xmlreader.ReadElementString());

            if(bridge_name_.Equals("Hilger_Isotech"))
            {
                ir = System.Convert.ToDouble(xmlreader.ReadElementString());
                isotech_gpib_address = (short) System.Convert.ToInt32(xmlreader.ReadElementString());


                //read the first node
                xmlreader.ReadToNextSibling("RESISTOR");
                xmlreader.ReadToDescendant("tinsleystandardresistor");
                xmlreader.ReadToFollowing("R");
                tinsley = System.Convert.ToDouble(xmlreader.ReadElementString());
            }
                 

        }

        private void Laboratory_SelectedIndexChanged(object sender, EventArgs e)
        {

            try
            {
                //find the details of the selected lab from the xml file
                LoadXML();

                //read the first node
                xmlreader.ReadStartElement();

                //parse the rest of the xml file
                while (!xmlreader.EOF)
                {
                    while (xmlreader.Name.Contains("LABORATORY"))
                    {
                        xmlreader.Read();
                        while (xmlreader.LocalName.Contains("laboratory"))
                        {
                            //if we are at the Node we selected then get the IP address of the LAB
                            if (xmlreader.LocalName.Contains(Laboratory.SelectedItem.ToString()))
                            {
                                xmlreader.Read();
                                if (xmlreader.LocalName.Contains("ipaddress"))
                                {
                                    ipaddress = xmlreader.ReadElementString();
                                    xmlreader.ReadToFollowing("equiptype");
                                    equiptype = xmlreader.ReadElementString();
                                    xmlreader.ReadToFollowing("gatewaytype");
                                    gatewaytype = xmlreader.ReadElementString();
                                }

                            }
                            xmlreader.Skip();
                        }
                    }
                    xmlreader.Skip();
                    
                }
                xmlreader.Close();
            }
            catch (XmlException)
            {
                xmlreader.Close();
            }
        }

        private void Resistance_Bridge_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = Resistance_Bridge_Type.Text;

            double A1 = 0;
            double A2 = 0;
            double A3 = 0;
            double A1_2 = 0;
            double A2_2 = 0;
            double A3_2 = 0;
            double A1_3 = 0;
            double A2_3 = 0;
            double A3_3 = 0;
            double internal_resistor = 100;
            double tinsley = 100;

            switch (selectedText)
            {
                case "Hilger_Isotech":
                    //if we haven't yet allocated an F26 bridge then do it now
                    if (isotech_bridge == null)
                    {
                        GetBridgeCorrection(selectedText, ref A1, ref A2, ref A3, ref A1_2, ref A2_2, ref A3_2, ref A1_3, ref A2_3, ref A3_3, ref internal_resistor, ref tinsley);
                        isotech_bridge = new IsotechMicro(isotech_gpib_address, "GPIB2::", ref multiplexor);
                        isotech_bridge.A1 = A1;
                        isotech_bridge.A2 = A2;
                        isotech_bridge.A3 = A3;
                        isotech_bridge.Tir = internal_resistor;
                        isotech_bridge.Tinsley = tinsley;
                        isotech_bridge.Addr = isotech_gpib_address;
                    }
                    bridge = isotech_bridge;
                    break;
                case "CMM_34970A_1":
                    if(!Multiplexor_Type.Text.Contains("Agilent")){
                        Multiplexor_Type.Text = "Agilent Multiplexor";
                    }
                    //if we haven't yet allocated agilent scanner A do it now
                    if (a_agilent == null)
                    {
                        GetBridgeCorrection(selectedText, ref A1, ref A2, ref A3, ref A1_2, ref A2_2, ref A3_2, ref A1_3, ref A2_3, ref A3_3, ref internal_resistor, ref tinsley);
                        a_agilent = new AgilentBridge(1, "GPIB1::", ref multiplexor);
                        a_agilent.A1 = A1;
                        a_agilent.A2 = A2;
                        a_agilent.A3 = A3;
                        a_agilent.A1_2 = A1_2;
                        a_agilent.A2_2 = A2_2;
                        a_agilent.A3_2 = A3_2;
                        a_agilent.A1_3 = A1_3;
                        a_agilent.A2_3 = A2_3;
                        a_agilent.A3_3 = A3_3;

                    }
                    bridge = a_agilent;
                    break;
                case "Long_34970A_2":
                    if (!Multiplexor_Type.Text.Contains("Agilent"))
                    {
                        Multiplexor_Type.Text = "Agilent Multiplexor";
                    }
                    //if we haven't yet allocated agilent scanner B do it now
                    if (b_agilent == null)
                    {
                        GetBridgeCorrection(selectedText, ref A1, ref A2, ref A3, ref A1_2, ref A2_2, ref A3_2, ref A1_3, ref A2_3, ref A3_3, ref internal_resistor, ref tinsley);
                        b_agilent = new AgilentBridge(2, "GPIB2::", ref multiplexor);
                        b_agilent.A1 = A1;
                        b_agilent.A2 = A2;
                        b_agilent.A3 = A3;
                        b_agilent.A1_2 = A1_2;
                        b_agilent.A2_2 = A2_2;
                        b_agilent.A3_2 = A3_2;
                        b_agilent.A1_3 = A1_3;
                        b_agilent.A2_3 = A2_3;
                        b_agilent.A3_3 = A3_3;
                    
                    }
                    bridge = b_agilent;
                    break;
                case "Laser_34970A_3":
                    if (!Multiplexor_Type.Text.Contains("Agilent"))
                    {
                        Multiplexor_Type.Text = "Agilent Multiplexor";
                    }
                    //if we haven't yet allocated agilent scanner C do it now
                    if (c_agilent == null)
                    {
                        GetBridgeCorrection(selectedText, ref A1, ref A2, ref A3, ref A1_2, ref A2_2, ref A3_2, ref A1_3, ref A2_3, ref A3_3, ref internal_resistor, ref tinsley);
                        c_agilent = new AgilentBridge(9, "GPIB0::", ref multiplexor);
                        c_agilent.A1 = A1;
                        c_agilent.A2 = A2;
                        c_agilent.A3 = A3;
                        c_agilent.A1_2 = A1_2;
                        c_agilent.A2_2 = A2_2;
                        c_agilent.A3_2 = A3_2;
                        c_agilent.A1_3 = A1_3;
                        c_agilent.A2_3 = A2_3;
                        c_agilent.A3_3 = A3_3;
                    }
                    bridge = c_agilent;
                    break;

                case "Tunnel_34970A_4":
                    if (!Multiplexor_Type.Text.Contains("Agilent"))
                    {
                        Multiplexor_Type.Text = "Agilent Multiplexor";
                    }
                    //if we haven't yet allocated agilent scanner C do it now
                    if (c_agilent == null)
                    {
                        GetBridgeCorrection(selectedText, ref A1, ref A2, ref A3, ref A1_2, ref A2_2, ref A3_2, ref A1_3, ref A2_3, ref A3_3, ref internal_resistor, ref tinsley);
                        c_agilent = new AgilentBridge(3, "GPIB3::", ref multiplexor);
                        c_agilent.A1 = A1;
                        c_agilent.A2 = A2;
                        c_agilent.A3 = A3;
                        c_agilent.A1_2 = A1_2;
                        c_agilent.A2_2 = A2_2;
                        c_agilent.A3_2 = A3_2;
                        c_agilent.A1_3 = A1_3;
                        c_agilent.A2_3 = A2_3;
                        c_agilent.A3_3 = A3_3;
                    }
                    bridge = c_agilent;
                    break;

                    
                default:
                    if (isotech_bridge == null)
                    {
                        isotech_bridge = new IsotechMicro(15, "GPIB2::", ref multiplexor);
                    }
                    bridge = isotech_bridge;
                    break;
            }
        }

        private void Multiplexor_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = Multiplexor_Type.Text;

            switch (selectedText)
            {
                case "Hilger Lab Multiplexor":

                    //if we haven't yet allocated a hilger mux, do it now
                    if (h_plexor == null)
                    {
                        h_plexor = new IsotechMux(15, "GPIB2::", ref prts);
                    }
                    multiplexor = h_plexor;
                    break;
                    
                case "Agilent Multiplexor":

                    //check that the selected bridge is also agilent
                    if (!Resistance_Bridge_Type.Text.Contains("3497"))
                    {
                        MessageBox.Show("Cannot Set the multiplexor to Agilent because the bridge is not set to agilent");
                        goto default;
                    }
                    else
                    {
                        //if we haven't yet allocated an agilent mux, do it now
                        if (a_plexor == null)
                        {
                            a_plexor = new AgilentMUX(ref prts);
                        }
                        multiplexor = a_plexor;
                        break;
                    }

                default:
                    //if we haven't yet allocated a hilger mux, do it now
                    if (h_plexor == null)
                    {
                        h_plexor = new IsotechMux(15, "GPIB2::", ref prts);
                    }
                    multiplexor = h_plexor;
                    break;
            }
        }
    
        private void AddMeasurement()
        {
            //Make a new PRT from the selected PRT drop down box based on the stuff in the xml file
            PRT got = FindPRT(PRTName.Text);

            //if the server updater thread is running stop its execution and restart it so that it has the full temperature measurent list to work on.
            try
            {
                if (serverUpdate.IsAlive)
                {

                    serverUpdate.Abort();
                    serverUpdate = new Thread(new ParameterizedThreadStart(TemperatureServerUpdater));
                }

                else
                {
                    serverUpdate = new Thread(new ParameterizedThreadStart(TemperatureServerUpdater));
                    
                }
            }
            catch (NullReferenceException)
            {
                serverUpdate = new Thread(new ParameterizedThreadStart(TemperatureServerUpdater));
            }
            //remember to store the name of the PRT associated with this measurement
            //this is so that we can easily load a prt from the config file
            got.PRTName = PRTName.Text;
            multiplexor.setProbe(got, current_channel);      //associates a probe with a channel

            //create a delegate to wait for the temperature data to come in
            PrintTemperatureData msgDelegate = new PrintTemperatureData(ShowTemperatureData);
            TemperatureMeasurement to_add = new TemperatureMeasurement(ref got, ref multiplexor, ref bridge, current_channel, ref msgDelegate, measurement_index);

            //set this from the gui later
            to_add.Inverval = interval;
            to_add.Date = GetDT();
            to_add.LabLocation = Laboratory.Text;
            to_add.Filename = Location_String.Text;
            to_add.MUXName = Multiplexor_Type.Text;
            to_add.BridgeName = Resistance_Bridge_Type.Text;

            //increase the measurement list to fit the next measurement that might be added
            Array.Resize(ref measurement_list, measurement_index + 1);
            measurement_list[measurement_index] = to_add;

            //create a thread to run the measurement and log the data to C:
            Thread newthread = new Thread(new ParameterizedThreadStart(TemperatureMeasurement.SingleMeasurement));
            newthread.Priority = ThreadPriority.Normal;
            newthread.IsBackground = false;
            newthread.SetApartmentState(ApartmentState.MTA);
            Threads[measurement_index] = newthread;
            Array.Resize(ref Threads, measurement_index + 2);
            to_add.SetThreads(Threads);
            to_add.SetDirectory();   //set the directories for this measurement

            //start the new measurement
            newthread.Start(to_add);  //start the new thread and give it the measurement object
            measurement_index++;
            serverUpdate.Start(measurement_list);                //run the server updater with the latest measurement list

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
                object[] textobj = { temperature,msg,index};
                this.BeginInvoke(new PrintTemperatureData(ShowTemperatureData), textobj);
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
                this.BeginInvoke(new PrintPressureData(ShowPressureData), textobj);
            }
        }

        //function takes a string holding the value and 
        private void ShowHumidityData(double humidity,string msg, long errortype)
        {

            if (!this.InvokeRequired)
            {
                //buildChart(measurement_list[index].X.ToString(), measurement_list[index].Y.ToString(), (int) index);
                HumidityOutputWindow.Text = (humidity.ToString() + "   " + msg + "\n");
                HumidityOutputWindow.ScrollToCaret();
            }
            else
            {
                object[] textobj = { humidity, msg, errortype};
                this.BeginInvoke(new PrintHumidityData(ShowHumidityData), textobj);
            }
        }



        private void Clear_window_button_Click(object sender, EventArgs e)
        {
            Progress_Window.Text = "";
            Progress_Window.Clear();
        }

        private void MonthCalendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            
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
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            bool can_save = true;
            
            //prepare the text to write to the file
            StringBuilder sb = new StringBuilder();

            try
            {
                //build a string that will get written to the file
                int i = 1;
                
                foreach (TemperatureMeasurement meas_writer in measurement_list)
                {
                    sb.AppendLine("MEASUREMENT "+i.ToString());
                    sb.AppendLine("LOCATION IN LAB:" + meas_writer.Filename);
                    sb.AppendLine("CHANNEL:" + (meas_writer.GetMUXChannel()).ToString());
                    sb.AppendLine("PRT:" + meas_writer.PRT.PRTName);
                    sb.AppendLine("LAB NAME:" + meas_writer.LabLocation);
                    sb.AppendLine("BRIDGE NAME:" + meas_writer.BridgeName);
                    sb.AppendLine("MUX_TYPE:" + meas_writer.MUXName);
                    i++;
                }
                sb.AppendLine("END");

            }

            catch (NullReferenceException)
            {
                MessageBox.Show("No Measurements are added so you can't create a config file\n"
                                 +"A config file can be created only when measurements are active");

                can_save = false;
            }

            if (can_save)
            {
                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    //check the file is open and valid
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        TextWriter w = new System.IO.StreamWriter(myStream);
                        w.Write(sb.ToString());
                        w.Flush();
                        w.Close();
                    }
                }

            }
        }
        private void LoadMeasurementsFromConfig_Click(object sender, EventArgs e)
        {
            int size;
            string file = "";
            string text;
            bool okay = true;
            //Oprn a config file for reading
            DialogResult result = openConfigFile.ShowDialog(); // Show the dialog and get result.
            if (result == DialogResult.OK) // Test result.
            {
                file = openConfigFile.FileName;
                try
                {
                    text = File.ReadAllText(file);
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
                Form2 testDialog = new Form2();

                testDialog.setlabel("Before the configuration can be loaded a new end date and time\n"
                                  + "for this measurement configuration should be set.  Okay to use\n"
                                  + "the Date and time you have selected below?\n"
                                  + "Also it is a good idea to click Stop All Measurements prior to\n"
                                  + "loading a configuration");
                // Show testDialog as a modal dialog and determine if DialogResult = OK.
                if (testDialog.ShowDialog(this) == DialogResult.OK)
                {
                    // parse the file
                    StreamReader file_reader = new StreamReader(file);

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

                }
                testDialog.Dispose();
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void StopAllMeasurements_Click(object sender, EventArgs e)
        {

            StopAllMeasurements_();

        }

        private void StopAllMeasurements_()
        {
            //stop the thread executing associated with this measurement
            TemperatureMeasurement.Execute = false;
            while (TemperatureMeasurement.ThreadCount > 0) ; //wait here until threads have finished their processes

            while ((measurement_index - 1) >= 0)
            {
                
                
                measurement_list[measurement_index - 1] = null;
                measurement_index--;
            }


            //create fresh lists
            measurement_list = new TemperatureMeasurement[1];
            Threads = new Thread[1];
        }

        private void AddCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddMeasurement();
        }

        private void RemoveCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //get the selected probe name
            PRT prt_found = FindPRT(PRTName.Text);
            string report = prt_found.getReportNumber();

            //search the measurement list for the prt with the selected name
            for (int i = 0; i < measurement_index; i++)
            {

                string report_ = measurement_list[i].PRT.getReportNumber();

                //if we find the measurement using the given PRT then delete the measurement and remove the chart series
                if (string.Compare(report, report) == 0)
                {
                    if (TemperatureMeasurement.AbortThread(i))   //stop the threads execution
                    {
                        //modify the measurement list
                        RemoveAt(i);
                        
                      
                        break;  //successfully aborted thread execution of probe
                    }
                }
            }
        }

        //Remove a measurement from the measurement list.
        private void RemoveAt(int index)
        {
            
        
            for (int i = 0; i < measurement_list.Length; i++)
            {
                //if it's less than the remove index then leave the array alone
                if (i < index) continue;

                else if (i == index)
                {
                    measurement_list[i].MeasurementRemoved = true;
                    measurement_list[i].MeasurementRemovalIndex = index;
                }

                //if it's greater than the remove index then left shift 
                else if (i > index)
                {

                    measurement_list[i - 1] = measurement_list[i];

                }
            }
            Array.Resize<TemperatureMeasurement>(ref measurement_list,measurement_index-1);
            measurement_index--;
            TemperatureMeasurement.ThreadCount--;
        }

        private void TemperatureServerUpdater(object stateInfo)
        {

            TemperatureMeasurement[] measurement_list_copy = ((TemperatureMeasurement[]) stateInfo);
            
            
            //update the server every minute
            DateTime current_time;
            int stored_hour = (System.DateTime.Now).Hour;   //store this hour
            int stored_month = (System.DateTime.Now).Month;  //store this month
            int stored_minute = DateTime.Now.Minute; //store this minute
            int hour;
            int month;
            int minute;
            

            while(server_update){
                Thread.Sleep(2000);
                current_time = System.DateTime.Now;  //the time stamp now
                hour = current_time.Hour;  //the hour now
                month = current_time.Month;   //The month now
                minute = current_time.Minute;

                if (stored_minute != minute || force_update_server)
                {
                    //turn off force update server
                    force_update_server = false;

                    //do server update
                    stored_minute = (System.DateTime.Now).Hour;   //get the new hour we are in
                    int i = 0;
                    string di = "";
                    string dc = "";
                    while (i < measurement_index)
                    {
                        measurement_list_copy[i].GetDirectories(ref di, ref dc);

                        //try and do a file copy until we find a way that works
                        while (true)
                        {
                            try
                            {
                                File.Copy(dc + measurement_list_copy[i].Filename + ".txt", di + measurement_list_copy[i].Filename + ".txt", true);
                                break;
                            }
                            catch (UnauthorizedAccessException)
                            {
                                //this has probably occured because someone has opened the file on the server and is looking at it, allow them to
                                //do so.  When they finally close it we can do the copy
                                continue;
                            }
                            catch (DirectoryNotFoundException)
                            {
                                //This will have occured because someone deleted the directory laid down originally by the measurement thread
                                //To overcome this we will rebuild the directory
                                System.IO.Directory.CreateDirectory(di);
                            }
                            catch (FileNotFoundException){
                                
                                //this means the file does not exist on c:  we can't write a file that doesn't exist
                                break;
                            }
                            catch (IOException)
                            {
                                //This means we can't talk to the server, not much we can do but keep trying
                                continue;
                            }
                        }
                        Thread.Sleep(10000);  //sleep for 10 seconds and try again.
                        i++;
                    }

                }

                //if we have changed month we need to reset the directory folder
                if (stored_month != month)
                {
                    stored_month = (System.DateTime.Now).Month;   //store the new month we are in

                    for (int i = 0; i < measurement_index; i++)
                    {
                        measurement_list[i].SetDirectory();
                    }

                }
            }
        }

        private void PressureServerUpdater(object stateInfo)
        {

            
        }

        private void Force_Server_Update_Click(object sender, EventArgs e)
        {
            force_update_server = true;

        }
        private void StartPressureLogging()
        {
            barometer_index = 1;
            for (int i = 0; i <= Pressure_barometers.Lines.Count(); i++)
            {
                switch (Pressure_barometers.Lines.ElementAt(i))
                {
                    case "PTB220A":
                        //create a delegate to wait for the pressure data to arrive
                        //PrintPressureData pdel = new PrintPressureData(showPressureData);
                        //barometer_list[i] = new VaisalaPTU300Barometer("", 80, ref pdel); this should change to a PTB220A object when implemented
                        Array.Resize(ref barometer_list, barometer_index + 1);
                        break;
                    case "PTU303":

                        //create a delegate to wait for the pressure data to arrive
                        PrintPressureData pdel2 = new PrintPressureData(ShowPressureData);

                        //instantiate the object, if required.
                        barometer_list[i] = new VaisalaPTU300Barometer("", 23, ref pdel2);
                        VaisalaPTU300Barometer ptu303 = (VaisalaPTU300Barometer) barometer_list[i];
                        barometer_index++;
                        ptu303.OpState = true;
                        Array.Resize(ref barometer_list, barometer_index + 1);
                        LoadXML();
                        xmlreader.ResetState();

                        //read the first node
                        xmlreader.ReadStartElement();

                        //parse the rest of the xml file
                        while (!xmlreader.EOF)
                        {
                            while (xmlreader.Name.Contains("BAROMETER"))
                            {
                                xmlreader.Read();
                                while (xmlreader.LocalName.Contains("barometer"))
                                {
                                    //check to see if we are at the correct node
                                    if (xmlreader.LocalName.Contains("PTU303"))
                                    {

                                        ptu303.ReportNumber = xmlreader.ReadElementString();
                                        ptu303.ReportDate = xmlreader.ReadElementString();
                                        ptu303.EquipID = xmlreader.ReadElementString();
                                        ptu303.EquipType = xmlreader.ReadElementString();
                                        ptu303.IP = xmlreader.ReadElementString();
                                        ptu303.Location = xmlreader.ReadElementString();
                                        ptu303.P950 = xmlreader.ReadElementString();
                                        ptu303.P960 = xmlreader.ReadElementString();
                                        ptu303.P970 = xmlreader.ReadElementString();
                                        ptu303.P980 = xmlreader.ReadElementString();
                                        ptu303.P990 = xmlreader.ReadElementString();
                                        ptu303.P1000 = xmlreader.ReadElementString();
                                        ptu303.P1010 = xmlreader.ReadElementString();
                                        ptu303.P1020 = xmlreader.ReadElementString();
                                        ptu303.P1030 = xmlreader.ReadElementString();
                                        ptu303.P1040 = xmlreader.ReadElementString();
                                        ptu303.P1050 = xmlreader.ReadElementString();

                                    }
                                    xmlreader.Skip();
                                }
                            }
                            xmlreader.Skip();
                        }
                        

                        //create a thread whose job is to querry a PTU300
                        Thread newthread = new Thread(new ParameterizedThreadStart(ptu303.Measure));
                        newthread.Priority = ThreadPriority.Normal;
                        newthread.IsBackground = true;
                        newthread.Start(ptu303);
                        break;
                    default: return;
                }
            } 
        }

        private void StartHumidityLogging()
        {
            
            short num_of_ptus = 0;
            short num_of_omegas = 0;
            short iterator1 = 0;
            short iterator2 = 0;
            for (int i = 0; i <= HumidityHygrometers.Lines.Count(); i++)
            {
                iterator1 = 0;
                iterator2 = 0;
                string line = HumidityHygrometers.Lines.ElementAt(i);
                if (line.Contains("Omega") || line.Contains("omega")) line = "Omega";
                if (line.Contains("ptu") || line.Contains("PTU")) line = "PTU";

                switch (line)
                {
                    
                    case "PTU":
                        num_of_ptus++;
                        //create a delegate to wait for the humidity data to arrive
                        PrintHumidityData hdel1 = new PrintHumidityData(ShowHumidityData);

                        //add a new humidity device to the device list
                        hygrometer_list[hygrometer_index] = new VaisalaPTU300Hygrometer("", "", ref hdel1);

                        //get a handle on it
                        VaisalaPTU300Hygrometer ptu303 = (VaisalaPTU300Hygrometer) hygrometer_list[hygrometer_index];
                        
                        //resize the array.
                        hygrometer_index++;
                        Array.Resize(ref hygrometer_list, hygrometer_index + 1);

                        //load the xml and reset its state
                        LoadXML();
                        xmlreader.ResetState();

                        //read the first node
                        xmlreader.ReadStartElement();

                        //parse the rest of the xml file
                        while (!xmlreader.EOF)
                        {
                            while (xmlreader.Name.Contains("HUMIDITY"))
                            {
                                
                                xmlreader.Read();
                                while (xmlreader.LocalName.Contains("humidity"))
                                {
                                    //check to see if we are at the correct node
                                    if (xmlreader.LocalName.Contains("PTU303"))
                                    {
                                        iterator1++;
                                        if (num_of_ptus == iterator1)
                                        {
                                            ptu303.OpState = true;
                                            ptu303.ReportNumber = xmlreader.ReadElementString();
                                            ptu303.ReportDate = xmlreader.ReadElementString();
                                            ptu303.EquipID = xmlreader.ReadElementString();
                                            ptu303.EquipType = xmlreader.ReadElementString();
                                            ptu303.IP = xmlreader.ReadElementString();
                                            ptu303.Location = xmlreader.ReadElementString();
                                            ptu303.HLoggerEq = xmlreader.ReadElementString();

                                            foreach(VaisalaPTU300Barometer b in barometer_list)
                                            {
                                                if (b != null)
                                                {
                                                    if (b.IP == ptu303.IP)
                                                    {
                                                        b.HumidityTransducer = ptu303;
                                                        //ptu300.setHUpdate(ref hdel1);
                                                    }
                                                }
                                            }
                                            
                                        }
                                    }
                                    xmlreader.Skip();
                                }
                            }
                            xmlreader.Skip();
                        }
                        
                        break;
                    case "Omega":
                        
                        num_of_omegas++;
                        //create a delegate to wait for the humidity data to arrive
                        PrintHumidityData hdel2 = new PrintHumidityData(ShowHumidityData);

                        //instantiate the object
                        hygrometer_list[hygrometer_index] = new OmegaTHLogger("", "", ref hdel2);

                        //increase the size of the hydrometer array so we can fit the next entry
                        hygrometer_index++;
                        Array.Resize(ref hygrometer_list, hygrometer_index+1);

                        //get a handle of the object at the top of the list
                        OmegaTHLogger omega = (OmegaTHLogger) hygrometer_list[i];
                        omega.OpState = true;
                        LoadXML();
                        xmlreader.ResetState();

                        //read the first node
                        xmlreader.ReadStartElement();

                        //parse the rest of the xml file
                        while (!xmlreader.EOF)
                        {
                            while (xmlreader.Name.Contains("HUMIDITY"))
                            {
                                xmlreader.Read();
                                while (xmlreader.LocalName.Contains("humidity"))
                                {
                                    //check to see if we are at the correct node (remembering to ignore devices in the tunnel)
                                    if (xmlreader.LocalName.Contains("Omega"))
                                    {
                                        
                                        iterator2++;
                                        if (num_of_omegas == iterator2)
                                        {
                                            omega.ReportNumber = xmlreader.ReadElementString();
                                            omega.ReportDate = xmlreader.ReadElementString();
                                            omega.EquipID = xmlreader.ReadElementString();
                                            omega.EquipType = xmlreader.ReadElementString();
                                            omega.IP = xmlreader.ReadElementString();
                                            omega.Location = xmlreader.ReadElementString();
                                            omega.HLoggerEq = xmlreader.ReadElementString();
                                            omega.Log = Convert.ToBoolean(xmlreader.ReadElementString());

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
                                        }
                                        
                                    }

                                    xmlreader.Skip();
                                }
                            }
                            xmlreader.Skip();
                        }
                        
                        break;
                    default: return;
                }
            }
           
        }
       
       

        /// <summary>
        /// -Implements a graceful exit of all threads running
        /// </summary>
        private void LaboratoryTempMonitor_FormClosing(Object sender, FormClosingEventArgs e)
        {
            //Exiting all processes will take a while, so this will need to run in a seperate thread.
            Thread closing_thread = new Thread(new ThreadStart(CloseProcesses));
            
            //disable form controls,so the user can't cause any mischief while the processes are ending.
            //foreach(Control c in this.Controls)
            //{
            //    c.Enabled = false;
            //}
            
            closing_thread.Start();

        }

        private void CloseProcesses()
        {
            //First tell the temperature measurements it's time to finish
            StopAllMeasurements_();
            TemperatureMeasurement.Execute = false;

            for (int i = 0; i < hygrometer_list.Length; i++)
            {
                //for each object in the list
                if (hygrometer_list[i] != null)
                {
                    hygrometer_list[i].OpState = false;
                    //hygrometer_list[i].Close();
                    //hygrometer_list[i] = null;


                }
            }

            for (int i = 0; i < barometer_list.Length; i++)
            {
                //for each object in the list
                if (barometer_list[i] != null)
                {
                    barometer_list[i].OpState = false;
                    //barometer_list[i].Close();
                    //barometer_list[i] = null;
                }
            }
            server_update = false;
            Thread.Sleep(3000);
            Application.Exit();
        }
    }
    
}
