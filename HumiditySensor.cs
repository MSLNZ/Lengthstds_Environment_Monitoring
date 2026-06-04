using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{

    public sealed class HumiditySensor:Sensor
    {

        public ICalibration Calibration { get; }

        public HumiditySensor(string sensorId,string reportNumber,DateTime reportDate, string componentName, ICalibration calibration) : base(sensorId, reportNumber, reportDate, componentName, calibration)
        {
            Calibration = calibration;
        }

    }

}
