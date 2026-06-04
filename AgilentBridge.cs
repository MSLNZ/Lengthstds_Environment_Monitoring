using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Length_Stds_Environmental_Monitoring
{


    public sealed class AgilentBridge : ResistanceBridge
    {
        private readonly ITransport _transport;
        private readonly ICalibration _calibration1;
        private readonly ICalibration _calibration2;
        private readonly ICalibration _calibration3;
        private bool _initialised = false;
        private string append_string;
        private int _channel = 1;

        public AgilentBridge(ITransport transport, ICalibration calibration1_, ICalibration calibration2_, ICalibration calibration3_)
            : base() { 

            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _calibration1 = calibration1_;
            _calibration2 = calibration2_;
            _calibration3 = calibration3_;

            Initialise(); // ✅ run once at creation
        }

        private void Initialise()
        {
            if (_initialised)
                return;

            _transport.SendCommand("FORM:READ:TIME:TYPE ABS\r\n");
            Sleep(50);

            _transport.SendCommand("FORM:READ:TIME ON\r\n");
            Sleep(50);

            _transport.SendCommand("FORM:READ:CHAN OFF\r\n");
            Sleep(50);

            _transport.SendCommand("FORM:READ:ALAR OFF\r\n");
            Sleep(100);

            _initialised = true;
        }

        /// <summary>
        /// -Current must be between 0 and 3 which equates to 0.1mA, 0.3mA, 1mA and 3mA.
        /// </summary>
        /// <param name="current">A value betweem 0 and 3</param>
        private void SetCurrent(short current)
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);

            string command = String.Concat("I", current.ToString(), "\r\n");
            _transport.SendCommand(command);
            Sleep(50);
        }



        private void SetRemoteMode()
        {
            _transport.SendCommand("R1\n\r");
            Sleep(50);
        }

        private static void Sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        public override double ReadResistance(int channel)
        {
            
            DetermineAppendString(channel);
            _transport.SendCommand("MEAS:FRES? 100, 0.0001, "+append_string);
            string response = _transport.ReadResponse();
            double raw = double.Parse(response, CultureInfo.InvariantCulture);

            

            ICalibration calibration = GetCalibrationForChannel(channel);
            return calibration != null
                ? calibration.Apply(raw)
                : raw;
        }


        private ICalibration GetCalibrationForChannel(int channel)
        {
            if (channel >= 1 && channel <= 10)
                return _calibration1;

            if (channel >= 11 && channel <= 20)
                return _calibration2;

            if (channel >= 21 && channel <= 30)
                return _calibration3;

            return null;
        }


        /// <summary>
        /// Sets what channel the multiplexor is switched to
        /// creates a string to append to the Bridge send command
        /// </summary>
        /// <param name="channel_number">channel number is a value between 1 and 30</param>
        private void DetermineAppendString(int channel_number)
        {

            if ((channel_number > 0) && (channel_number <= 10))
            {
                if (channel_number < 10)
                {
                    append_string = string.Concat("(@10", channel_number.ToString(), ")\r\n");
                }
                else append_string = "(@110)\r\n";
            }
            else if ((channel_number > 10) && (channel_number <= 20))
            {
                if (channel_number < 20)
                {
                    append_string = string.Concat("(@20", (channel_number - 10).ToString(), ")\r\n");
                }
                else append_string = "(@210)\r\n";
            }
            else if ((channel_number > 20) && (channel_number <= 30))
            {
                if (channel_number < 30)
                {

                    append_string = string.Concat("(@30", (channel_number - 20).ToString(), ")\r\n");

                }
                else append_string = "(@310)\r\n";

            }
            _channel = channel_number;

        }
        /// <summary>
        /// -Unit must be between 0 and 3 which equates to 0.1mA, 0.3mA, 1mA and 3mA.
        /// </summary>
        /// <param name="unit">A value betweem 0 and 3</param>
        private void SetUnits(short unit)
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);

            string command = String.Concat("U", unit.ToString(), "\r\n");
            _transport.SendCommand(command);
            Thread.CurrentThread.Join(500);
        }
        private string GetAppendString()
        {
           
            return append_string;
        }
    }
}
