using System;
using System.Globalization;

namespace Length_Stds_Environmental_Monitoring
{
    /// <summary>
    /// Concrete implementation of a resistance bridge using Isotech MicroK hardware.
    /// Responsible ONLY for reading resistance via injected transport.
    /// </summary>
    public sealed class IsotechMicroBridge : ResistanceBridge
    {
        private readonly ITransport _transport;
        private readonly ICalibration _calibration1;
        private readonly ICalibration _calibration2;
        private readonly ICalibration _calibration3;
        private readonly double _internal_r;
        private bool _initialised = false;
        private string append_string;
        private int _channel = 1;
        public IsotechMicroBridge(ITransport transport, ICalibration calibration1_, ICalibration calibration2_, ICalibration calibration3_, double internal_r)
            : base()
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _calibration1 = calibration1_;
            _calibration2 = calibration2_;
            _calibration3 = calibration3_;
            Initialise();
            _internal_r = internal_r;
        }

        private void Initialise()
        {
            if (_initialised)
                return;

            //measure resistor in ratio mode, ratioed with the internal resistor
            _transport.SendCommand("SENSE:FUNCTION RATIO\n\r"); //ratio mode
            Sleep(1000);
            _transport.SendCommand("SENSE:RATIO:REFERENCE 204\n\r"); //internal 100 ohm resistor
            Sleep(1000);
            _transport.SendCommand("SENSE:RATIO:RANGE 110, 1/r/n"); //set the range according to the maximum expected prt resistance, say 110 ohm
            Sleep(1000);
            _transport.SendCommand("CURRENT 1\n\r");  //use 1 mA
            Sleep(1000);
            _transport.SendCommand("INITIATE\r\n");  //set the above conditions
            Sleep(1000);

            _initialised = true;
        }

        /// <summary>
        /// Reads resistance from the currently selected channel.
        /// </summary>
        public override double ReadResistance(int channel)
        {
            _transport.SendCommand("READ?\r\n");
            string ratio = _transport.ReadResponse();
            double bridge_reading = 0.0;
            try
            {
                double raw_ratio = double.Parse(ratio, CultureInfo.InvariantCulture);
                bridge_reading = raw_ratio * _internal_r;
            }
            catch (FormatException)
            {
                return -1;
            }


            ICalibration calibration = GetCalibrationForChannel(channel);
            return calibration != null
                ? calibration.Apply(bridge_reading)
                : bridge_reading;
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

        private static void Sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }
        private string ParseResistanceString(string resistance)
        {
            if (resistance.Contains('+'))
            {
                int index = resistance.IndexOf('+');
                resistance.Remove(index, 1);
            }
            return resistance;
        }
    }
}