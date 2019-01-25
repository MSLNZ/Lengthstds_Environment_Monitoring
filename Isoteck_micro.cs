using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Temperature_Monitor
{
    class IsotechMicro:ResistanceBridge
    {

        private double R_internal;
        private double R_tinsley;
        private bool initialised;


        public IsotechMicro(short address, string gatewaystring,ref MUX multi):base(address,gatewaystring,ref multi){
            string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            InitIO(init_string);
            R_internal = 100;
            R_tinsley = 100;
            initialised = false;
        }
        
        /// <summary>
        /// - Current must be between 0 and 3 which equates to 0.1mA, 0.3mA, 1mA and 3mA.
        /// </summary>
        /// <param name="current">A value betweem 0 and 3</param>
        protected override void setCurrent(short current)
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);
            string command = String.Concat("I", current.ToString(), "\r\n");
            sendcommand(command);
            Thread.Sleep(500);
        }

        protected override void setRemoteMode()
        {
           // string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
           // InitIO(init_string);
            sendcommand("R1\n\r");
            Thread.Sleep(500);
        }

        protected override void Init()
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);

            //measure resistor in ratio mode, ratioed with the internal resistor
            sendcommand("SENSE:FUNCTION RATIO\n\r"); //ratio mode
            sendcommand("SENSE:RATIO:REFERENCE 204\n\r"); //internal 100 ohm resistor
            sendcommand("SENSE:RATIO:RANGE 110, 1/r/n"); //set the range according to the maximum expected prt resistance, say 110 ohm
            sendcommand("CURRENT 1\n\r");  //use 1 mA
            sendcommand("INITIATE\r\n");  //set the above conditions
            Thread.Sleep(1000);
        }

        /// <summary>
        /// -Returns the current temperature in degrees C
        /// </summary>
        /// <param name="multiplexor_channel">channel number is a value between 1 and 9</param>
        public override double getTemperature(PRT probe_type, short channel_number, bool probe_has_changed)
        {
            lock (thislock)
            {
                
                double resistance_ = 0.0;
                string ratio = "";
                double ratio_ = 0.0;
                double A = probe_type.getA();
                double B = probe_type.getB();
                double R0 = probe_type.getR0();

                // string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
                // InitIO(init_string);

                if (!initialised) {
                    Init();
                    initialised = true;
                }

               // if (probe_has_changed)
               // {

                //Thread.Sleep(10000); //wait 10 seconds for the bridge to settle after the channel change  
               // }
               // else
               // {
               //     Thread.Sleep(1000);  //change this back to 1 s if it is not required
              //  }
                sendcommand("READ?\r\n");
                //Thread.Sleep(1000); 
                ReadResponse(ref ratio);

                try
                {
                    ratio = ParseResistanceString(ratio);
                    ratio_ = Convert.ToDouble(ratio);
                }
                catch (FormatException)
                {
                    return -1;
                }

                ratio_ = ratio_ + base.A1 + ratio_ * base.A2 + ratio_ * ratio_ * base.A3;
                resistance_ = R_internal * ratio_;
                return (-A + Math.Sqrt(A * A - 4 * B * (1 - (resistance_ / R0)))) / (2 * B);
            }
        }
        /// <summary>
        /// -Unit must be between 0 and 3 which equates to 0.1mA, 0.3mA, 1mA and 3mA.
        /// </summary>
        /// <param name="unit">A value betweem 0 and 3</param>
        protected override void setUnits(short unit)
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);

            string command = String.Concat("U", unit.ToString(), "\r\n");
            sendcommand(command);
            Thread.Sleep(500);
        }

        public double Tir
        {
            set { R_internal = value; }
            get { return R_internal; }
        }

        public double Tinsley
        {
            set { R_tinsley = value; }
            get { return R_tinsley; }
        }
    }
}
