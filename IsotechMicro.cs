using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NCalc;

namespace Temperature_Monitor
{
    class IsotechMicro:ResistanceBridge
    {

        private bool initialised;


        public IsotechMicro(short address, string gatewaystring,ref MUX multi):base(address,gatewaystring,ref multi){
            string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            InitIO(init_string);
            internal_r = 100;
            tinsley_r = 100;
            initialised = false;
        }

        public IsotechMicro(short address, string gatewaystring) : base(address, gatewaystring)
        {
            string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            InitIO(init_string);
            internal_r = 100;
            tinsley_r = 100;
            initialised = false;
        }

        /// <summary>
        /// - Current must be between 0 and 3 which equates to 0.1mA, 0.3mA, 1mA and 3mA.
        /// </summary>
        /// <param name="current">A value betweem 0 and 3</param>
        protected override void SetCurrent(short current)
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);
            string command = String.Concat("I", current.ToString(), "\r\n");
            sendcommand(command);
            Thread.CurrentThread.Join(500);
        }

        protected override void SetRemoteMode()
        {
           // string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
           // InitIO(init_string);
            sendcommand("R1\n\r");
            Thread.CurrentThread.Join(500);
        }

        protected override void Init()
        {
            //string init_string = String.Concat(SICL_interface_id, Convert.ToString(GPIB_adr));
            //InitIO(init_string);

            //measure resistor in ratio mode, ratioed with the internal resistor
            sendcommand("SENSE:FUNCTION RATIO\n\r"); //ratio mode
            Thread.CurrentThread.Join(1000);
            sendcommand("SENSE:RATIO:REFERENCE 204\n\r"); //internal 100 ohm resistor
            Thread.CurrentThread.Join(1000);
            sendcommand("SENSE:RATIO:RANGE 110, 1/r/n"); //set the range according to the maximum expected prt resistance, say 110 ohm
            Thread.CurrentThread.Join(1000);
            sendcommand("CURRENT 1\n\r");  //use 1 mA
            Thread.CurrentThread.Join(1000);
            sendcommand("INITIATE\r\n");  //set the above conditions
            Thread.CurrentThread.Join(1000);
        }

        /// <summary>
        /// -Returns the current temperature in degrees C
        /// </summary>
        /// <param name="probe">The PRT to take a measurement with</param>
        /// <param name="channel_number">channel number is a value between 1 and 9</param>
        /// <param name="probe_has_changed">a flag indicating if the probe has changed</param>
        public override double GetTemperature(PRT probe, short channel_number, bool probe_has_changed)
        {
            lock (thislock)
            {
                double resistance_ = 0.0;
                string ratio = "";
                double ratio_ = 0.0;
                double bridge_reading = 0.0;
                string eq = probe.Equation;

                if (!initialised) {
                    Init();
                    initialised = true;
                }

                sendcommand("READ?\r\n");
                //Thread.CurrentThread.Join(1000); 
                ReadResponse(ref ratio);

                try
                {
                    ratio = ParseResistanceString(ratio);
                    ratio_ = Convert.ToDouble(ratio);
                    bridge_reading = ratio_ * internal_r;
                }
                catch (FormatException)
                {
                    return -1;
                }

                //Apply the bridge correction equations
                resistance_ = CalculateCorrectedBridgereading(bridge_reading,equation1);
                
                 
                if (probe.PRTName.Equals("StdResistor")) return resistance_;
                else
                {
                    double t = probe.SolveForTemperatureBisection(eq, resistance_, -30, 110, 1E-6);
                    return t;
                }
            }
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

        
    }
}
