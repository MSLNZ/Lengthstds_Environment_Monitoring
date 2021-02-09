using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace Temperature_Monitor
{
    //Contains all the stuff relevant to a measurement
    //The single measurement method uses thread synchronization - hence the inherit from contectboundobject
    //[synchronization]
    class TemperatureMeasurement
    {
        private PRT prt;
        private static Thread[] threads_running = new Thread[1];
        private static TemperatureMeasurement[] current_measurements = new TemperatureMeasurement[1];
        private static long thread_count;
        private volatile MUX mux;
        private volatile ResistanceBridge bridge;
        private PrintTemperatureData data;
        private DateTime date;
        private string lab_location;
        private static int measurement_anomalies;
        private string mux_name;
        private string bridge_name;
        private string directory;
        private string directory2;
        private string filename;
        private long assigned_thread_priority;
        protected int year = System.DateTime.Now.Year;
        protected int month = System.DateTime.Now.Month;
        private static bool measurement_removed = false;  //set to true if a measurement has just been removed
        private static long removal_index = 0;
        private double result;
        private static bool execute;
        private static long interval;
        private short channel_for_measurement;
        private long measurement_index_;
        private System.DateTime date_time;
        private StringBuilder x_data;
        private StringBuilder y_data;
        private static Mutex measurementMutex = new Mutex(false);
        private static Object lockthis = new Object();
        public static Random random = new Random();
        public static bool active = true; 
     
        
        /// <summary>
        /// Builds a measurement
        /// </summary>
        /// <param name="PRT_m">The PRT being used in the measurement</param>
        /// <param name="MUX_m">The MUX type that the PRT is pluged into</param>
        /// <param name="bridge_m">The type of resistance bridge the PRT is plugged into</param>
        /// <param name="bridge_m">The channel for the measurement</param>
        /// <param name="bridge_m">A delegate to be called when temperature data becomes available</param>
        public TemperatureMeasurement(ref PRT PRT_m,ref MUX MUX_m,ref ResistanceBridge bridge_m,short channel,ref PrintTemperatureData msgDelegate,long measurement_index)
        {
            prt = PRT_m;    
            mux = MUX_m;
            data = msgDelegate;
            bridge = bridge_m;
            lab_location = "";
            filename = "";
            channel_for_measurement = channel;
            result = 0.0;
            measurement_index_ = measurement_index;
            thread_count = measurement_index+1;
            x_data = new StringBuilder("");
            y_data = new StringBuilder("");
            date = DateTime.Now;
            assigned_thread_priority = 1;  //make the fresh measurement added have the highest execution priority.

            //everytime we add a new measurement change the size of the thread array
            Array.Resize(ref threads_running,(int) measurement_index);
            Array.Resize(ref current_measurements, (int) measurement_index+1);
            current_measurements[measurement_index] = this;

            execute = true;

            //Array.Resize(ref threadexecution, (int)measurement_index);

            //threadexecution[measurement_index] = true;
        }

        public bool MeasurementRemoved
        {
            get
            {
                return measurement_removed;
            }

            set
            {
                measurement_removed = value;
            }
            
        }

        public long MeasurementRemovalIndex
        {
            get
            {
                return removal_index;
            }

            set
            {
                removal_index  = value;
            }

        }
        public static bool Execute
        {
            set { execute = value; }
            get { return execute; }
        }

        public void SetThreads(Thread[] add_thread)
        {
            threads_running = add_thread;  //update the threads running array
        }
        public static bool AbortThread(int index)
        {
            if (threads_running[index].IsAlive)
            {
                threads_running[index].Abort();
                return true;
            }
            else return false;
        }
        public double Measure()
        {
            result = bridge.GetTemperature(prt, channel_for_measurement, false);
            return result;
        }

        public void GetDirectories(ref string i_dir, ref string c_dir)
        {
            i_dir = directory2;
            c_dir = directory;

        }


        /// <summary>
        /// creates a directory on the C drive and the I drive for the data to go into.
        /// according to year and month and lab name...
        /// </summary>
        /// <returns>True if successfuly, or False if a problem</returns>
        public void SetDirectory()
        {
            bool directory_change_expected = false;
            //get the date component of the directory string.  Use the current time and date for this
            DateTime date = System.DateTime.Now;
            int current_year = date.Year;     //the year i.e 2013
            int current_month = date.Month;   //1-12 for which month we are in
            string lb;
            switch (lab_location)
            {
                case "HILGER":
                    lb = "Hilger Lab";
                    break;
                case "LONGROOM":
                    lb = "Long Room";
                    break;
                case "LASER":
                    lb = "Laser Lab";
                    break;
                case "TUNNEL":
                    lb = "Tunnel";
                    break;
                case "LEITZ":
                    lb = "Leitz Room";
                    break;
                default:
                    lb = "MISC";
                    break;
            }

            if (!((Year == current_year) && (Month == current_month)))
            {
                directory_change_expected = true;
            }

            Year = current_year;
            Month = current_month;

            //The default directory is on C & G:  Each measurement in written to C when it arrives 
            directory = @"C:\Temperature Monitoring Data\" + lb + @"\" + current_year.ToString() + @"\" + current_year.ToString() + "-" + current_month.ToString() + @"\";
            directory2 = @"G:\Shared drives\MSL - Length\Length\Temperature Monitoring Data\" + lb + @"\" + current_year.ToString() + @"\" + current_year.ToString() + "-" + current_month.ToString() + @"\";

            //create the directories if they don't exist already
            if (!System.IO.Directory.Exists(directory)) //it is possible for this to return false when the directory actually exists.  This can occur if there's an error for any other possible reason i.e temporary failure of the network.
            {
                //we need to determine the reason why Directory.Exists returned false.
                if (directory_change_expected)
                {
                    try { System.IO.Directory.CreateDirectory(directory); }
                    catch (System.IO.IOException) { }
                }
            }

            if (!System.IO.Directory.Exists(directory2)) //it is possible for this to return false when the directory actually exists.  This can occur if there's an error for any other possible reason i.e temporary failure of the network.
            {
                //we need to determine the reason why Directory.Exists returned false.
                if (directory_change_expected)
                {
                    try { System.IO.Directory.CreateDirectory(directory2); }
                    catch (System.IO.IOException) { }
                }
            }
        }

        public int Year
        {
            set { year = value; }
            get { return year; }
        }

        public int Month
        {
            set { month = value; }
            get { return month; }
        }
        public double Result
        {
            get
            {
                return result;
            }
        }

        public long AssignedThreadPriority
        {
            get
            {
                return assigned_thread_priority;
            }
            set
            {
                assigned_thread_priority = value;
            }
        }

        public StringBuilder X
        {
            get
            {
                return x_data;
            }
        }

        public StringBuilder Y
        {
            get
            {
                return y_data;
            }
        }

        public PRT PRT
        {
            get { return prt; }
        }
        /// <summary>
        /// Gets or sets the number of measurements to do.
        /// </summary>
        public DateTime Date
        {
            set { date = value; }
            get { return date; }
        }
        /// <summary>
        /// Gets or sets the interval between each measurement
        /// </summary>
        public long Inverval
        {
            set { interval = value; }
            get { return interval; }
        }
       
        public string Filename
        {
            set { filename = value; }
            get { return filename; }
        }
        public string LabLocation
        {
            set { lab_location = value; }
            get { return lab_location; }
        }

        public long MeasurementIndex
        {
            get { return measurement_index_; }
            set { measurement_index_ = value; }
        }
        public string MUXName
        {
            set { mux_name = value; }
            get { return mux_name; }
        }
        public string BridgeName
        {
            set { bridge_name = value; }
            get { return bridge_name; }
        }

        public MUX MUX
        {
            get { return mux; }
        }
        public void SetMUXChannel()
        {
           bridge.SetCurrentChannel(channel_for_measurement);
        }
        public short GetMUXChannel()
        {
            return channel_for_measurement;
        }

        public Thread MeasurementThread
        {
            get { return threads_running[measurement_index_]; }
        }
        public Thread[] MeasurementThreads
        {
            get { return threads_running; }
        }
        public static long ThreadCount
        {
            get { return thread_count; }
            set { thread_count = value; }
        }

        public static void SingleMeasurement(object stateInfo)
        {

            //System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            //get the measurement object into this thread
            TemperatureMeasurement measuring = (TemperatureMeasurement)stateInfo;
            
            //The file paths
            string path;
            string path2;

            Execute = true;

            //create a file stream writer to put the data into
            System.IO.StreamWriter writer=null;

            //record the month we are in
            //int month_ = System.DateTime.Now.Month;

            while (Execute)
            {

                bool appenditure = false;
                path = measuring.directory + measuring.Filename + ".txt";
                path2 = measuring.directory2 + measuring.Filename + ".txt";


                try
                {
                    //if the file exists append to it otherwise create a new file. We write to the c: here.  ServerUpdater() will then periodically attempt to upload to secure backup
                    if (System.IO.File.Exists(path))
                    {
                        appenditure = true;

                        writer = System.IO.File.AppendText(path);
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(measuring.directory);
                        writer = System.IO.File.CreateText(path);
                    }

                }
                catch (System.IO.IOException)
                {
                    //try closing this instance of the file writer and creating a new instance.. maybe that might fix it
                    if (writer != null)
                    {
                        writer.Close();
                        writer.Dispose();
                        Thread.CurrentThread.Join(10000);
                    }
                    try
                    {
                        //if the file exists append to it otherwise create a new file
                        if (System.IO.File.Exists(measuring.directory + measuring.Filename + ".txt"))
                        {

                            writer = System.IO.File.AppendText(path);
                        }
                        else
                        {
                            System.IO.Directory.CreateDirectory(measuring.directory);
                            writer = System.IO.File.CreateText(path);
                        }
                    }
                    catch (System.IO.IOException e)
                    {
                        continue; //just ignore the issues and hope the connectivity resolves by itself.
                    }
                }

                //if appending we don't need this again
                if (!appenditure)
                {
                    writer.WriteLine("Automatically Generated File!\n");
                }


                
               

                //set the current (incoming) measurements priority to be the lowest (biggest number)
                measuring.AssignedThreadPriority = thread_count;


                //make the current thread wait until its priority reaches 1
                lock (lockthis) while (measuring.AssignedThreadPriority != 1)
                    {
                        Monitor.Wait(lockthis);
                    }

//---------------------------------------------------------------------START OF CRITICAL SECTION-------------------------------------------------------------
                lock(lockthis)  //only one thread at a time is allowed to execute this code
                {
                    //make sure the channel is correct (it may have been changed by another thread)
                    measuring.SetMUXChannel();

                    //sleep the thread for the specified dead time
                    Thread.CurrentThread.Join((int)(measuring.Inverval * 1000));

                    //take the measurement
                    double measurement_result = measuring.Measure();
                    measuring.y_data.Append(measurement_result.ToString() + ",");

                    //record the time of the measurement
                    measuring.date_time = System.DateTime.Now;
                    double ole_date = measuring.date_time.ToOADate();
                    measuring.x_data.Append(ole_date.ToString() + ",");

                    //invoke the GUI to print the temperature data
                    measuring.data(measurement_result
                        , measuring.filename + " on CH" + measuring.channel_for_measurement.ToString() + " in " + measuring.lab_location + "\n"
                        , measuring.MeasurementIndex);

                    try
                    {
                        if ((measurement_result < 22.0) || (measurement_result > 18.0))
                        {
                            measurement_anomalies++;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    try
                    {
                        //write the measurement to file
                        writer.WriteLine(string.Concat(measurement_result.ToString() + ", " + measuring.MUX.getCurrentChannel().ToString()
                            , "," + measuring.date_time.ToString() + ", " + measuring.lab_location
                            , ", " + measuring.Filename));
                        writer.Flush();
                    }
                    catch (System.IO.IOException)
                    {
                        MessageBox.Show("Issue writing to file - Check Drive - in the mean time the data will be written to C:");
                        writer.Close();
                        writer = System.IO.File.CreateText("c:" + measuring.Filename);

                    }

                    //let the exiting thread decrement all the measurement priorities 
                    for (int i = 0; i < ThreadCount; i++)
                    {
                        current_measurements[i].AssignedThreadPriority--;
                        Monitor.PulseAll(lockthis);
                    }
                    if (!execute)
                    {
                        int index_of_exiting_measurement = 0;
                        for(int i = 0; i < thread_count; i++)
                        {
                            if (current_measurements[i].AssignedThreadPriority == 0)
                            {
                                index_of_exiting_measurement = i;
                            }
                        }
                        //remove the measurement which has just finished
                        for(long i = index_of_exiting_measurement; i < ThreadCount; i++)
                        {
                            if (i == ThreadCount - 1)
                            {
                                //delete the last place in the array
                                Array.Resize(ref current_measurements, current_measurements.Length - 1);
                                break;
                            }
                            //shuffle all measurements to fill in the space
                            current_measurements[i] = current_measurements[i + 1];
                        }
                    }
                    //if we have removed an item then we need to reorder the priorities
                    if (measuring.MeasurementRemoved)
                    {
                        measuring.MeasurementRemoved = false;
                        for (long i = measuring.MeasurementRemovalIndex; i < ThreadCount; i++)
                        {
                            current_measurements[i].AssignedThreadPriority--;
                            Monitor.PulseAll(lockthis);

                        }
                    }
                }
                //--------------------------------------------------------------END OF CRITICAL SECTION------------------------------------------------------------------------------
                writer.Close();
            }
            
                thread_count--;

            //Close the TCP connection
            //measuring.bridge.Close();
            //if there is no threads left then set 
            if(thread_count == 0)
            {
                active = false;
            }
            
           
        }
    }
}
