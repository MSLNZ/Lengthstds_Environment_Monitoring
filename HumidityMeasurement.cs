using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{
    public sealed class HumidityMeasurement : IMeasurementTask
    {
        private readonly IMeasurementDevice _device;
        private readonly HumiditySensor _sensor;
        private readonly PrintHumidityData _uiCallback;

        private readonly string _fileName;
        private readonly string _location;


        // Directory state
        private string _localDirectory;
        private string _serverDirectory;
        private int _year;
        private int _month;


        public HumidityMeasurement(IMeasurementDevice device,HumiditySensor sensor,string fileName,string location,PrintHumidityData uiCallback)
        {
            _device = device;
            _sensor = sensor;
            _fileName = fileName;
            _location = location;
            _uiCallback = uiCallback;
        }

        public IMeasurementDevice Device => _device;
        public object Sensor => _sensor;
        public bool Log { get; set; } = true;

        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);
        public DateTime NextRun { get; set; } = DateTime.UtcNow;

        public void MeasureOnce()
        {
            if (!Log) return;

            double value = _device.ReadSensor(_sensor);

            if(value==double.NaN) return;
            value = Math.Round(value, 2);

            string timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            WriteToFile(value, timestamp);

            _uiCallback?.Invoke(value, $"%rh {_fileName} ({_location})", 0);
        }

        private void WriteToFile(double value, string timestamp)
        {
            if (double.IsNaN(value))
                return;

            SetDirectory();

            string path = Path.Combine(
                _localDirectory,
                _fileName + ".txt");

            using (var writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine(
                    $"{value.ToString(CultureInfo.InvariantCulture)}," +
                    $"{timestamp}," +
                    $"{_location}," +
                    $"{_fileName}");
            }
        }

        private void SetDirectory()
        {
            DateTime now = DateTime.Now;

            // Only update when month/year changes
            if (_year == now.Year && _month == now.Month)
                return;

            _year = now.Year;
            _month = now.Month;

            // Map lab location to folder name
            string lab;

            switch (_location)
            {
                case "Hilger Lab": lab = "Hilger Lab"; break;
                case "Long Room": lab = "Long Room"; break;
                case "Laser Lab": lab = "Laser Lab"; break;
                case "Underground Tape Tunnel": lab = "Tunnel"; break;
                case "CMM Lab": lab = "CMM Lab"; break;
                default: lab = "MISC"; break;
            }

            // Build directory paths
            _localDirectory =
                $@"C:\Humidity Monitoring Data\{lab}\{_year}\{_year}-{_month}\";

            _serverDirectory =
                $@"L:\Humidity Monitoring Data\{lab}\{_year}\{_year}-{_month}\";

            // Ensure directories exist
            Directory.CreateDirectory(_localDirectory);
            Directory.CreateDirectory(_serverDirectory);
        }

    }
}
