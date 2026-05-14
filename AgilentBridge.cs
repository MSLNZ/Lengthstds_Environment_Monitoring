using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Length_Stds_Environmental_Monitoring
{
    class AgilentBridge : ResistanceBridge
    {


        /// <summary>
        /// Creates a new Agilent Bridge
        /// </summary>
        /// <param name="address">The GPIB Address of the Bridge/MUX</param>
        /// <param name="gatewaystring">The SICL interface ID of the gateway</param>
        /// <param name="multi">The multiplexor associated with this device.  Each bridge created must have a multiplexor object even if it is integral to the bridge</param>
        public AgilentBridge(short address, string gatewaystring, ref MUX multi_) : base(address, gatewaystring, ref multi_)
        {
            string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            InitIO(init_string);
        }

        /// <summary>
        /// Creates a new Agilent Bridge
        /// </summary>
        /// <param name="address">The GPIB Address of the Bridge/MUX</param>
        /// <param name="gatewaystring">The SICL interface ID of the gateway</param>
        public AgilentBridge(short address, string gatewaystring) : base(address, gatewaystring)
        {
            string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            InitIO(init_string);
        }

        /// <summary>
        /// -Current must be between 0 and 3 which equates to 0.1mA, 0.3mA, 1mA and 3mA.
        /// </summary>
        /// <param name="current">A value betweem 0 and 3</param>
        protected override void SetCurrent(short current)
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);

            string command = String.Concat("I", current.ToString(), "\r\n");
            sendcommand(command);
            Thread.CurrentThread.Join(50);
        }



        protected override void SetRemoteMode()
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);
            sendcommand("R1\n\r");
            Thread.CurrentThread.Join(50);
        }

        protected override void Init()
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);

            sendcommand("FORM:READ:TIME:TYPE ABS\r\n");
            Thread.CurrentThread.Join(50);
            sendcommand("FORM:READ:TIME ON\r\n");
            Thread.CurrentThread.Join(50);
            sendcommand("FORM:READ:CHAN OFF\r\n");
            Thread.CurrentThread.Join(50);
            sendcommand("FORM:READ:ALAR OFF\r\n");
            Thread.CurrentThread.Join(100);
        }

        /// <summary>
        /// -Returns the current temperature in degrees C
        /// </summary>
        /// <param name="probe">The PRT to take a measurement with</param>
        /// <param name="channel_number">channel number is a value between 1 and 30</param>
        /// <param name="probe_has_changed">a flag indicating if the probe has changed</param>
        public override double GetTemperature(PRT probe, short channel_number, bool probe_has_changed)
        {
            string resistance = "";
            double resistance_ = 0.0;
            string eq = probe.Equation;

            Init();

            if (probe_has_changed)
            {
                Thread.CurrentThread.Join(100);   //wait 1 seconds for the bridge to settle after the channel change
            }

            //Do a measurement (MEAS) with four wire FRES
            string to_send = string.Concat("MEAS:FRES? 100, 0.0001, ", GetAppendString());
            sendcommand(to_send);
            ReadResponse(ref resistance);

            try
            {
                resistance = ParseResistanceString(resistance);
                resistance_ = Convert.ToDouble(resistance);
            }
            catch (FormatException)
            {
                return -1;
            }

            if ((channel_number > 0) && (channel_number <= 10))
            {
                resistance_ = CalculateCorrectedBridgereading(resistance_, equation1);
            }
            else if ((channel_number > 10) && (channel_number <= 20))
            {
                resistance_ = CalculateCorrectedBridgereading(resistance_, equation2);
            }
            else if ((channel_number > 20) && (channel_number <= 30))
            {
                resistance_ = CalculateCorrectedBridgereading(resistance_, equation3);
            }

            double t = probe.SolveForTemperatureBisection(eq, resistance_, -30, 110, 1E-6);
            return t;
        }
        /// <summary>
        /// -Unit must be between 0 and 3 which equates to 0.1mA, 0.3mA, 1mA and 3mA.
        /// </summary>
        /// <param name="unit">A value betweem 0 and 3</param>
        protected override void SetUnits(short unit)
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);

            string command = String.Concat("U", unit.ToString(), "\r\n");
            sendcommand(command);
            Thread.CurrentThread.Join(500);
        }

        private string GetAppendString()
        {
            AgilentMUX agilent_bridge_mux = (AgilentMUX)multi;
            return agilent_bridge_mux.AppendString;
        }

        private DateTime ReadDateTime()
        {
            string strDate = "";
            string strTime = "";

            sendcommand("SYST:DATE?\r\n");
            Thread.CurrentThread.Join(50);
            ReadResponse(ref strDate);

            sendcommand("SYST:TIME?\r\n");
            Thread.CurrentThread.Join(50);
            ReadResponse(ref strTime);

            return (DateTime)System.Convert.ToDateTime(string.Concat(strDate, strTime));
        }

        private void WriteDateTime()
        {
            DateTime datetime;
            datetime = DateTime.Now;

            string strDate = datetime.Date.ToString();
            string strTime = datetime.TimeOfDay.ToString();

            sendcommand(string.Concat("SYST:DATE ", strDate, "\r\n"));
            Thread.CurrentThread.Join(50);

            sendcommand(string.Concat("SYST:TIME ", strTime, "\r\n"));
            Thread.CurrentThread.Join(50);
        }



    }
}
