using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NCalc;

namespace Temperature_Monitor
{
    abstract class ResistanceBridge : GPIBOverLANCommands
    {

        //private const int F26_bridge_adr = 15;
        protected MUX multi;
        protected Object thislock = new Object();
        protected string equation1;
        protected string equation2;
        protected string equation3;
        protected string location;
        protected double internal_r;
        protected double tinsley_r;
        protected string eq_id;


        protected short current_channel_in_use;

        public ResistanceBridge(int GPIB_Address_, string SICL_,ref MUX multi_)
        {
            base.GPIB_adr = GPIB_Address_;
            base.SICL_interface_id = SICL_;
            multi = multi_;
        }

        public ResistanceBridge(int GPIB_Address_, string SICL_)
        {
            base.GPIB_adr = GPIB_Address_;
            base.SICL_interface_id = SICL_;
         
        }

        protected abstract void SetRemoteMode();
        

        /// <summary>
        /// - Current must be between 0 and 3 which equates to 0.1mA, 0.3mA, 1mA and 3mA.
        /// </summary>
        /// <param name="current">A value betweem 0 and 3</param>
        protected abstract void SetCurrent(short current);
        

        /// <summary>
        /// -Unit must be between 0 and 3 which equates to 0.1mA, 0.3mA, 1mA and 3mA.
        /// </summary>
        /// <param name="unit">A value betweem 0 and 3</param>
        protected abstract void SetUnits(short unit);


        protected abstract void Init();

        /// <summary>
        /// -Returns the current temperature in degrees C
        /// </summary>
        /// <param name="multiplexor_channel">channel number is a value between 1 and 9</param>
        public abstract double GetTemperature(PRT probe_type, short channel_number, bool probe_has_changed);
        
        public void SetMUX(MUX mux){
            multi = mux;
        }
        /// <summary>
        /// -Gets a probe with the specified channel type
        /// </summary>
        /// <param name="multiplexor_channel">A channel type</param>
        protected PRT GetProbe(string probe_name)
        {
            return multi.getProbe(probe_name);
        }
        protected short GetCurrentChannel()
        {
            return current_channel_in_use;
        }

        public double Tinsley
        {
            set { tinsley_r = value; }
            get { return tinsley_r; }
        }

        public double InternalResistance
        {
            get { return internal_r; }
            set { internal_r = value; }
        }

        public string Equation1
        {
            get { return equation1; }
            set { equation1 = value; }
        }
        public string Equation2
        {
            get { return equation2; }
            set { equation2 = value; }
        }
        public string Equation3
        {
            get { return equation3; }
            set { equation3 = value; }
            }
       
        public int Addr
        {
            get { return GPIB_adr; }
            set { GPIB_adr = value; }

        }

        public string Location
        {
            set { location = value; }
            get { return location; }
        }

        public string EqId
        {
            set { eq_id = value; }
            get { return eq_id;}
        }

        public double CalculateCorrectedBridgereading(double bridge_reading,string equation)
        {
            equation = equation.Replace("pow", "Pow");
            var expr = new Expression(equation);

            // Bind variable
            expr.Parameters["R"] = bridge_reading;

            object result = expr.Evaluate();
            return Convert.ToDouble(result);

        }

        public void SetCurrentChannel(short channel)
        {
            lock (thislock)
            {
                multi.setChannel(channel);
            }
        }

        /// <summary>
        /// -Removes the bad stuff out of the string so that it can be converted to a double
        /// </summary>
        /// <param name="multiplexor_channel">A channel type</param>
        protected string ParseResistanceString(string resistance)
        {
            if (resistance.Contains('+'))
            {
                int index = resistance.IndexOf('+');
                resistance.Remove(index, 1);
            }
            return resistance;
        }
        public void Close()
        {
            CloseConnection();
        }

    }
}
