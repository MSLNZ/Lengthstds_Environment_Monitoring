namespace Length_Stds_Environmental_Monitoring
{
    public enum SensorType
    {
        prt,
        thermocouple,
        thermister,
        integrated_logger
    }
    /// <summary>
    /// Describes a temperature sensor connected to a device.
    /// </summary>
    public sealed class TemperatureSensor:Sensor
    {

        private readonly int channel;
        private readonly SensorType sensor_type;
        
        //PRT temperature sensors plugged into a bridge
        public TemperatureSensor(string sensorId, string reportNumber, DateTime reportDate, string componentName, ICalibration calibration, int channel_,SensorType sensor_type_) : base(sensorId, reportNumber, reportDate, componentName, calibration)
        {
            channel = channel_;
            sensor_type = sensor_type_;

        }

        public int Channel
        {
            get { return channel; }
        }

        /// <summary>
        /// -Returns the resistance for a given temperature
        /// </summary>
        /// <param name="correctedResistance">The resistance value that has had a calibration corrected already applied</param>
        public double ConvertResistanceToTemperature(double correctedResistance)
        {
            return SolveForTemperatureBisection(correctedResistance, -1, 50, 1e-6);
        }

        /// <summary>
        /// -Returns the corrected temperature in degrees C
        /// </summary>
        /// <param name="measuredR">Measured Resistance</param>
        /// <param name="tMin">The lower temperature bound</param>
        /// <param name="tMax">The upper temperature bound</param>
        /// <param name="tolerance">The precision/tolerance of the measurement</param>
        private double SolveForTemperatureBisection(double measuredR, double tMin, double tMax, double tolerance = 1e-6)
        {
            if (_calibration is EquationCalibration)
            {
                while (Math.Abs(tMax - tMin) > tolerance)
                {
                    double tMid = (tMin + tMax) / 2.0;
                    double rMid = EvaluateResistance(tMid);

                    if (rMid > measuredR)
                        tMax = tMid;
                    else
                        tMin = tMid;
                }
                return (tMin + tMax) / 2.0;
            }
            return double.NaN;
        }

        /// <summary>
        /// -Returns the resistance for a given temperature
        /// </summary>
        /// <param name="equation">The equation R(t)</param>
        /// <param name="t">The temperature reading</param>
        private double EvaluateResistance(double t)
        {
            return _calibration.Apply(t);
            
        }
        

    }
}
