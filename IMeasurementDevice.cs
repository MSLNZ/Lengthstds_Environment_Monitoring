
namespace Length_Stds_Environmental_Monitoring
{
    /// <summary>
    /// Represents a physical device that can produce measurements.
    /// Owns communication, locking, and hardware semantics.
    /// </summary>
    public interface IMeasurementDevice
    {
        /// <summary>
        /// Prepare the device to read the given sensor
        /// (e.g. select channel, configure mode).
        /// </summary>
        void PrepareSensor(object sensor);

        /// <summary>
        /// Read a value from the given sensor.
        /// </summary>
        double ReadSensor(object sensor);

        void ResetCycle();
    }
}

