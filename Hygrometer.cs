using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Temperature_Monitor
{
    public abstract class Hygrometer
    {
        protected static short num_connected_loggers = 0;
        protected PrintHumidityData h_update;
        protected string hostname;
        protected bool appenditure;
        protected string ip_address;
        protected double rh;
        protected string report_number;
        protected string report_date;
        protected string equipment_id;
        protected string equip_type;
        protected string correction_equation;  
        protected string location;
        protected string directory;
        protected string directory2;
        protected string filename;
        protected double correction;
        protected bool on = false;
        protected ClientSocket TcpClient;
        protected double humidity_result = 50.00;


        public Hygrometer(string correction_eq, string hostname_, ref PrintHumidityData h_update_)
        {
            rh = 50.00;
            correction_equation = correction_eq;
            hostname = hostname_;
            h_update = h_update_;
        }

        public abstract void SetHumidity(double hty);

        public abstract double GetHumidity();

        public string HLoggerEq
        {
            set { correction_equation = value; }
            get { return correction_equation; }
        }
        
        public string IP
        {
            set { ip_address = value; }
            get { return ip_address; }
        }

        public void CalculateCorrection()
        {
            
            bool remove = true;
            int pos_of_R = 0;
            string a = "";
            string b = "";
            string c = "";
            string d = "";
            string remainder;

            char a_signbit = correction_equation[0];

            if ((a_signbit == '-') || (a_signbit == '+'))
            {

                remove = true;
            }
            else
            {
                a_signbit = '+';
                remove = false;
            }

            if (remove == true)
            {
                remainder = correction_equation.Substring(1);
            }
            else remainder = correction_equation;

            pos_of_R = remainder.IndexOf('R');
            if (remainder.IndexOf('+') < pos_of_R)
            {

                a = remainder.Remove(remainder.IndexOf('+'));
                remainder = remainder.Substring(remainder.IndexOf('+'));
            }
            else if (remainder.IndexOf('-') < pos_of_R)
            {

                a = remainder.Remove(remainder.IndexOf('-'));
                remainder = remainder.Substring(remainder.IndexOf('-'));
            }

            try
            {
                b = remainder.Remove(remainder.IndexOf('R'));
                remainder = remainder.Substring(remainder.IndexOf('R') + 1);

                c = remainder.Remove(remainder.IndexOf('R'));
                remainder = remainder.Substring(remainder.IndexOf('R') + 3);

                d = remainder.Remove(remainder.IndexOf('R'));

            }
            catch (ArgumentOutOfRangeException)
            {
                h_update(-1,"",ProcNameHumidity.EQUATION_FORMAT);
            }

            a = a_signbit + a;

            try
            {
                double a_ = Convert.ToDouble(a);
                double b_ = Convert.ToDouble(b);
                double c_ = Convert.ToDouble(c);
                double d_ = Convert.ToDouble(d);
                double currentH = GetHumidity();

                correction = a_ + b_ * currentH + c_ * Math.Pow(currentH, 2) + d_ * Math.Pow(currentH, 3);

            }
            catch (FormatException)
            {
                return;
            }
        }
        public string Directory1
        {
            get { return directory; }
        }
        public string Directory2
        {
            get { return directory2; }
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
            directory = @"C:\Humidity Monitoring Data\" + location + @"\" + year.ToString() + @"\" + year.ToString() + "-" + month.ToString() + @"\";
            directory2 = @"I:\MSL\Private\LENGTH\Humidity Monitoring Data\" + location + @"\" + year.ToString() + @"\" + year.ToString() + "-" + month.ToString() + @"\";

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

        public string HostName
        {
            get { return hostname; }
            set { hostname = value;}
        }
        public void Close()
        {

        }
    }
}
