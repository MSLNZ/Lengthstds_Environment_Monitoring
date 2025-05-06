using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Temperature_Monitor
{
    public abstract class Barometer
    {
        protected static short num_connected_loggers = 0;
        protected bool appenditure;
        protected bool appenditure2;
        protected double pressure = 50.00;
        protected string hostname;
        protected string report_number;
        protected string report_date;
        protected string equipment_id;
        protected string equip_type;
        protected double[,] corrections;  //2d array holds rising and falling pressures at 10hpa intervals over the range 950 to 1050
        protected string location;
        protected string directory;
        protected string directory2;
        protected string filename;
        protected bool on = false;
        protected int year = System.DateTime.Now.Year;
        protected int month = System.DateTime.Now.Month;
        protected TcpClient tcpClient;
        

        public Barometer()
        {
            pressure = 1014.00;
            corrections = new double[11,2];
        }

        protected abstract void SetPressure(double pressure_);

        public abstract double GetPressure();

        public void StoreCorrectionTable(string table)
        {

        }
        public string HostName
        {
            get { return hostname; }
            set { hostname = value; }
        }

        public bool OpState
        {
            get { return on; }
            set { on = value; }
        }

        public string Filename
        {
            set { filename = value; }
            get { return filename; }
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
          
            //The default directory is on C & G:  Each measurement in written to C when it arrives 
            directory = @"C:\Pressure Monitoring Data\" + location + @"\" + current_year.ToString() + @"\" + current_year.ToString() + "-" + current_month.ToString() + @"\";
            directory2 = @"L:\Pressure Monitoring Data\" + location + @"\" + current_year.ToString() + @"\" + current_year.ToString() + "-" + current_month.ToString() + @"\";

            if (!((year == current_year) && (month == current_month)))
            {
                directory_change_expected = true;
            }

            year = current_year;
            month = current_month;

            //create the directories if they don't exist already
            if (!System.IO.Directory.Exists(directory))
            {
                //we need to determine the reason why Directory.Exists returned false.
                if (directory_change_expected)
                {
                    try { System.IO.Directory.CreateDirectory(directory); }
                    catch (System.IO.IOException) { }
                }
            }

            if (!System.IO.Directory.Exists(directory2))
            {
                //we need to determine the reason why Directory.Exists returned false.
                if (directory_change_expected)
                {
                    try { System.IO.Directory.CreateDirectory(directory2); }
                    catch (System.IO.IOException) { }
                }
            }
        }

        public string ReportNumber
        {
            set { report_number = value; }
            get { return report_number; }
        }
        public string ReportDate
        {
            set { report_date = value; }
            get { return report_date; }
        }
        public string EquipID
        {
            set { equipment_id = value; }
            get { return equipment_id; }
        }
        public string EquipType
        {
            set { equip_type = value; }
            get { return equip_type; }
        }
        public string Location
        {
            set { location = value; }
            get { return location; }
        }
    }
}
