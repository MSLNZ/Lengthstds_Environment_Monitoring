using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{   
    public abstract class Sensor
    {
        protected readonly ICalibration _calibration;
        public string SensorId { get; }
        public string ComponentName { get; }
        public string ReportNumber { get; }
        public DateTime ReportDate { get; }

        protected Sensor(string sensorId, string reportNumber, DateTime reportDate, string componentName, ICalibration calibration)
        {
            SensorId = sensorId;
            ReportNumber = reportNumber;
            ReportDate = reportDate;
            ComponentName = componentName;
            _calibration = calibration;
        }

        // ✅ default behaviour (can be overridden)
        public virtual double ApplyCalibration(double rawValue)
        {
            return _calibration != null
                ? _calibration.Apply(rawValue)
                : rawValue;
        }

        public ICalibration Calibration
        {
            get { return _calibration; }
        }

    }

    public interface ICalibration
    {
        public double Apply(double rawValue);
        double Apply(double rawValue, bool isRising);
    }

    public sealed class EquationCalibration : ICalibration
    {
        private readonly string _equation;
        private readonly string _parameter;
        public EquationCalibration(string equation, string paramenter)
        {
            _equation = equation;
            _parameter = paramenter;
        }
        public double Apply(double rawValue)
        {
            var expr = new NCalc.Expression(_equation.Replace("pow", "Pow"));
            expr.Parameters[_parameter] = rawValue;
            return Convert.ToDouble(expr.Evaluate());
        }
       
        public double Apply(double rawValue, bool isRising)
        {
            throw new NotSupportedException(
                "Use Apply(input) for non-hysteresis calibration.");
        }
    }

    public sealed class TableCalibration : ICalibration
    {
        private readonly string[][] _table;
        
        public TableCalibration(string[][] table)
        {
            _table = table;
        }

        public double Apply(double rawValue)
        {
            int rows = _table.GetLength(0);

            // ✅ Choose correct column
            int correctionCol = 1;

            // ✅ Handle out-of-range (clamp to nearest endpoint)
            if (rawValue <= Convert.ToDouble(_table[3][0]))
            {
                return rawValue + Convert.ToDouble(_table[3][correctionCol]);
            }

            if (rawValue >= Convert.ToDouble(_table[rows - 2][0]))
            {
                return rawValue + Convert.ToDouble(_table[rows - 2][correctionCol]);
            }

            // ✅ Find bounding points
            for (int i = 3; i < rows - 2; i++)
            {
                double p1 = Convert.ToDouble(_table[i][0]);
                double p2 = Convert.ToDouble(_table[i + 1][0]);

                if (rawValue >= p1 && rawValue < p2)
                {
                    double c1 = Convert.ToDouble(_table[i][correctionCol]);
                    double c2 = Convert.ToDouble(_table[i + 1][correctionCol]);

                    // ✅ Linear interpolation
                    double fraction = (rawValue - p1) / (p2 - p1);
                    double correction = c1 + fraction * (c2 - c1);

                    return rawValue + correction;
                }
            }

            // ✅ Fallback (should never happen if table is valid)
            return double.NaN;
        }
        public double Apply(double rawValue, bool isRising)
        {
            throw new NotSupportedException(
                "Use Apply(input) for non-hysteresis calibration.");
        }
    }

    public sealed class HysterisisTableCalibration : ICalibration
    {
        private readonly string[][] _table;

        public HysterisisTableCalibration(string[][] table)
        {
            _table = table;
        }


        public double Apply(double input)
        {
            throw new NotSupportedException(
                "Use Apply(input, isRising) for hysteresis calibration.");
          
        }


        public double Apply(double rawValue, bool isRising)
        {
            int rows = _table.GetLength(0);

            // ✅ Choose correct column
            int correctionCol = isRising ? 1 : 2;

            // ✅ Handle out-of-range (clamp to nearest endpoint)
            if (rawValue <= Convert.ToDouble(_table[3][0]))
            {
                return rawValue + Convert.ToDouble(_table[3][correctionCol]);
            }

            if (rawValue >= Convert.ToDouble(_table[rows - 2][0]))
            {
                return rawValue + Convert.ToDouble(_table[rows - 2][correctionCol]);
            }

            // ✅ Find bounding points
            for (int i = 3; i < rows - 2; i++)
            {
                double p1 = Convert.ToDouble(_table[i][0]);
                double p2 = Convert.ToDouble(_table[i + 1][0]);

                if (rawValue >= p1 && rawValue < p2)
                {
                    double c1 = Convert.ToDouble(_table[i][correctionCol]);
                    double c2 = Convert.ToDouble(_table[i + 1][correctionCol]);

                    // ✅ Linear interpolation
                    double fraction = (rawValue - p1) / (p2 - p1);
                    double correction = c1 + fraction * (c2 - c1);

                    return rawValue + correction;
                }
            }

            // ✅ Fallback (should never happen if table is valid)
            return double.NaN;

        }



    }



}
