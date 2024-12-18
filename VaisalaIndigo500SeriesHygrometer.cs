using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temperature_Monitor
{
    public class VaisalaIndigo500SeriesHygrometer:Hygrometer
    {
        private int timer_zero1;
        private int timer_zero2;
        private int timer_1;
        private int timer_2;
        private string barometer_ip = "";

        private bool error_reported = false;
        private bool isactive = false;

        private short unit_id = 0xF1;
        protected static readonly int port = 502;

        public VaisalaIndigo500SeriesHygrometer(string correction_eq, string hostname_, ref PrintHumidityData h_update_) : base(correction_eq, hostname_, ref h_update_)
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
            get { return unit_id; }
            set { unit_id = value; }
        }

        public void HLoggerQuery(object stateinfo)
        {
            //because this is a joint sensor device this is logged via 


        }






        public static short NumConnectedLoggers
        {
            get { return num_connected_loggers; }
        }
    }
}
