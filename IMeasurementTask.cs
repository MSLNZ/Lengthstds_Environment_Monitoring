
namespace Length_Stds_Environmental_Monitoring
{

    public interface IMeasurementTask
    {
        IMeasurementDevice Device { get; }
        object Sensor { get; }

        void MeasureOnce();


        TimeSpan Interval { get; set; }           
        DateTime NextRun { get; set; }       

    }

}