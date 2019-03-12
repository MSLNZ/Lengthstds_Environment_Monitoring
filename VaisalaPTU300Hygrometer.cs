using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

namespace Temperature_Monitor
{
    public class VaisalaPTU300Hygrometer:Hygrometer
    {
        private int timer_zero1;
        private int timer_zero2;
        private int timer_1;
        private int timer_2;
        private string barometer_ip = "";
       
        private bool error_reported = false;
        private bool isactive = false;
        
        private short dev_id = 255;
        protected static readonly int port = 23;

        public VaisalaPTU300Hygrometer(string correction_eq, string hostname_, ref PrintHumidityData h_update_):base(correction_eq, hostname_, ref h_update_)
        {
            h_update = h_update_;
        }
     
        public PrintHumidityData HUpdate
        {
            get { return h_update; }
        }

        public override void SetHumidity(double hty)
        {
            humidity_result = hty;
        }

        public ClientSocket Sockt
        {
            set { TcpClient = value; }
            get { return TcpClient; }
        }
        public override double GetHumidity()
        {
            return humidity_result + correction;
        }

        public double Correction
        {
            get { return correction; }
            set { correction = value; }
        }
        public short DevID
        {
            get { return dev_id; }
            set { dev_id = value; }
        }

        


        
        public void HLoggerQuery(object stateinfo)
        {

           

        }

        

       
        

        public static short NumConnectedLoggers
        {
            get { return num_connected_loggers; }
        }
    }
}
