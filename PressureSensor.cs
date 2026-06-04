using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{
    public sealed class PressureSensor : Sensor
    {
        
        private bool isRising = false;

        public PressureSensor(string sensorId,string reportNumber,DateTime reportDate,ICalibration calibration, string componentName) : base(sensorId, reportNumber, reportDate, componentName, calibration)
        {
        }
        public override double ApplyCalibration(double rawPressure)
        {

            double result;
            if (_calibration is HysterisisTableCalibration hCal)
            {
                result = _calibration.Apply(rawPressure, isRising);
            }
            else
            {
                result = _calibration.Apply(rawPressure);
            }
         

            return result;
        }

        public bool IsRising
        {
            set { isRising = value; }
            get { return isRising; }
        }


    }

}
