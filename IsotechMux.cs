using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{
    public class IsotechMux : MUX
    {
        private readonly ITransport _transport;
        bool _initialised = false;
        public IsotechMux(ITransport transport_)
        {
            _transport = transport_;
            Initialise();
        }

        private void Initialise()
        {
            if (_initialised)
                return;

            //measure resistor in ratio mode, ratioed with the internal resistor
            _transport.SendCommand("SENSE:FUNCTION RATIO\n\r"); //ratio mode
            Sleep(10);
            _transport.SendCommand("SENSE:RATIO:REFERENCE 204\n\r"); //internal 100 ohm resistor
            Sleep(10);
            _transport.SendCommand("SENSE:RATIO:RANGE 110, 1/r/n"); //set the range according to the maximum expected prt resistance, say 110 ohm
            Sleep(10);
            _transport.SendCommand("CURRENT 1\n\r");  //use 1 mA
            Sleep(10);
            _transport.SendCommand("INITIATE\r\n");  //set the above conditions
            Sleep(10);

            _initialised = true;
        }

        /// <summary>
        /// Sets what channel the multiplexor is switched to
        /// </summary>
        /// <param name="channel_number">channel number is a value between 10 and 19 inclusive</param>
        public override void SelectChannel(int channel_number)
        { 
              _transport.SendCommand(String.Concat("SENSE:CHANNEL ", channel_number.ToString(), "\r\n"));
        }

        private static void Sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }
    }
}
