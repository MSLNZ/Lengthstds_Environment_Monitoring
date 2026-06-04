using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{

    public abstract class MUX
    {

        protected MUX()
        {
          
        }
        public abstract void SelectChannel(int channel);
    }

}
