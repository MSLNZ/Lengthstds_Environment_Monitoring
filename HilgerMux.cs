using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Temperature_Monitor
{
    public class HilgerMux:MUX
    {

        private Object thislock = new Object();

        public HilgerMux(int GPIB_Address_,string SICL_, ref PRT[] prts_connected):base(GPIB_Address_, SICL_, ref prts_connected)
        {
            string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            InitIO(init_string);
        }

        /// <summary>
        /// Sets what channel the multiplexor is switched to
        /// </summary>
        /// <param name="channel_number">channel number is a value between 10 and 19 inclusive</param>
        public override void setChannel(short channel_number)
        {
            lock (thislock)
            {
                
                sendcommand(String.Concat("SENSE:CHANNEL ",channel_number.ToString(),"\r\n"));
                selected_channel = channel_number;
            }
        }

        /// <summary>
        /// -Gets the channel the mux is set to
        /// </summary>
        /// <param name="probe_name">A channel type</param>
        public override short getCurrentChannel()
        {
            return selected_channel;

        }

        /// <summary>
        /// -Gets the probe that is plugged into a given channel
        /// </summary>
        /// <param name="probe_name">The channel</param>
        public override PRT getProbe(string channel_)
        {
            PRT probe_on_this_channel = null;
            switch (channel_)
            {
                case "CH10":
                    probe_on_this_channel = prts[0];
                    break;
                case "CH11":
                    probe_on_this_channel = prts[1];
                    break;
                case "CH12":
                    probe_on_this_channel = prts[2];
                    break;
                case "CH13":
                    probe_on_this_channel = prts[3];
                    break;
                case "CH14":
                    probe_on_this_channel = prts[4];
                    break;
                case "CH15":
                    probe_on_this_channel = prts[5];
                    break;
                case "CH16":
                    probe_on_this_channel = prts[6];
                    break;
                case "CH17":
                    probe_on_this_channel = prts[7];
                    break;
                case "CH18":
                    probe_on_this_channel = prts[8];
                    break;
                case "CH19":
                    probe_on_this_channel = prts[9];
                    break;
            }
            return probe_on_this_channel;
        }
        public override void setProbe(PRT new_PRT,short channel_is_on)
        {
            try
            {
                prts[channel_is_on - 10] = new_PRT;
              
            }
            catch (IndexOutOfRangeException)
            {
                channel_is_on = 10;
                prts[channel_is_on - 10] = new_PRT;
                
            }
        }
    }
}
