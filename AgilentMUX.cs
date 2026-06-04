using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{
    public class AgilentMUX : MUX
    {
        //Nothing much happens here
        public AgilentMUX()
        {
        }

        /// <summary>
        /// The agilient MUX and bridge are actually one unit so channel selection is done in the AgilentBridge object via the ReadCommand
        /// </summary>
        /// <param name="channel_number">channel number is a value between 1 and 30</param>
        public override void SelectChannel(int channel_number)
        {
        }

    }
}
