using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NCalc;

namespace Temperature_Monitor
{
    public class PRT
    {
        //note the report number is also used for prt identification
        private string prt_name;
        private string REPORT_NUMBER;
        private string equation;

        public PRT(string Report_number,string equation_)
        {
            REPORT_NUMBER = Report_number;
            equation = equation_;
        }
        public string getReportNumber()
        {
            return REPORT_NUMBER;
        }
        public string Equation
        {
            set { equation = value; }
            get { return equation;  }
        }
        public string PRTName
        {
            set {prt_name = value;}
            get {return prt_name;}
        }

        /// <summary>
        /// -Returns the resistance for a given temperature
        /// </summary>
        /// <param name="equation">The equation R(t)</param>
        /// <param name="t">The temperature reading</param>
        private double EvaluateResistance(string equation, double t)
        {
            equation = equation.Replace("pow", "Pow");
            var expr = new Expression(equation);
            expr.Parameters["t"] = t;
            return Convert.ToDouble(expr.Evaluate());
        }

        /// <summary>
        /// -Returns the corrected temperature in degrees C
        /// </summary>
        /// <param name="equation">The correction equation expressing R(t) in terms of R0 and t</param>
        /// <param name="measuredR">Measured Resistance</param>
        /// <param name="tMin">The lower temperature bound</param>
        /// <param name="tMax">The upper temperature bound</param>
        /// <param name="tolerance">The precision/tolerance of the measurement</param>
        public double SolveForTemperatureBisection(string equation,double measuredR,double tMin,double tMax,double tolerance = 1e-6)
        {
            while (Math.Abs(tMax - tMin) > tolerance)
            {
                double tMid = (tMin + tMax) / 2.0;
                double rMid = EvaluateResistance(equation, tMid);

                if (rMid > measuredR)
                    tMax = tMid;
                else
                    tMin = tMid;
            }
            return (tMin + tMax) / 2.0;
        }

    }
}
