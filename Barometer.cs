using System;
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
        protected ClientSocket tcpClient;
        

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

        /// <summary>
        /// creates a directory on the C drive and the I drive for the data to go into.
        /// according to year and month and lab name...
        /// </summary>
        /// <returns>True if successfuly, or False if a problem</returns>
        public void SetDirectory()
        {

            //get the date component of the directory string.  Use the current time and date for this
            DateTime date = System.DateTime.Now;
            int year = date.Year;     //the year i.e 2013
            int month = date.Month;   //1-12 for which month we are in
          
            //The default directory is on C & I:  Each measurement in written to C when it arrives 
            directory = @"C:\Pressure Monitoring Data\" + location + @"\" + year.ToString() + @"\" + year.ToString() + "-" + month.ToString() + @"\";
            directory2 = @"I:\MSL\Private\LENGTH\Pressure Monitoring Data\" + location + @"\" + year.ToString() + @"\" + year.ToString() + "-" + month.ToString() + @"\";

            //create the directories if they don't exist already
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            if (!System.IO.Directory.Exists(directory2))
            {
                System.IO.Directory.CreateDirectory(directory2);
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
        public void Close()
        {
            tcpClient.CloseConnection();
        }
    }
}
