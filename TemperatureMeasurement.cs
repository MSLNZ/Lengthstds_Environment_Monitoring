using Length_Stds_Environmental_Monitoring;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{

    /// <summary>
    /// Represents a single temperature measurement.
    /// This class is passive: it performs one measurement when asked.
    /// Timing and ordering are handled externally by a scheduler.
    /// </summary>
    public sealed class TemperatureMeasurement : IMeasurementTask
    {
        private readonly IMeasurementDevice _device;
        private readonly TemperatureSensor _sensor;
        private readonly PrintTemperatureData _uiCallback;

        private readonly string _fileName;
        private readonly string _location;

        // Directory state
        private string _localDirectory;
        private string _serverDirectory;
        private int _year;
        private int _month;

        public TemperatureMeasurement(TemperatureSensor sensor, IMeasurementDevice device, string labLocation, string fileName, PrintTemperatureData uiCallback)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            _location = labLocation ?? "Unknown";
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            _uiCallback = uiCallback;
        }

        public IMeasurementDevice Device => _device;
        public object Sensor => _sensor;
        public string Filename => _fileName;
        public string LabLocation => _location;
        public bool Log { get; set; } = true;
        public string BridgeName { get; set; }
        public string MUXName { get; set; }
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);
        public DateTime NextRun { get; set; }
        public int Channel => _sensor.Channel;
        public void MeasureOnce()
        {
            if (!Log)
                return;

            // ✅ Device handles EVERYTHING (MUX + Bridge + calibration + PRT)
            double value = _device.ReadSensor(_sensor);

            
            if (double.IsNaN(value))
                return;
            value = Math.Round(value, 3);
            string timestamp = DateTime.Now.ToString(
                "dd/MM/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture);

            WriteToFile(value, timestamp);
            NotifyUI(value);
        }

        private void WriteToFile(double value, string timestamp)
        {
            SetDirectory();

            string path = Path.Combine(
                _localDirectory,
                EnsureTxtExtension(_fileName));

            using (var writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine(
                    $"{value.ToString(CultureInfo.InvariantCulture)}," +
                    $"{timestamp}," +
                    $"{_location}," +
                    $"{_fileName}");
            }
        }

        private void NotifyUI(double value)
        {
            _uiCallback?.Invoke(
                value,
                $"°C {_location} - {_sensor.SensorId} (Ch {_sensor.Channel})",
                0);
        }

        private void SetDirectory()
        {
            DateTime now = DateTime.Now;

            if (_year == now.Year && _month == now.Month)
                return;

            _year = now.Year;
            _month = now.Month;

            string lab = _location switch
            {
                "Hilger Lab" => "Hilger Lab",
                "Long Room" => "Long Room",
                "Laser Lab" => "Laser Lab",
                "Underground Tape Tunnel" => "Tunnel",
                "CMM Lab" => "CMM Lab",
                _ => "MISC"
            };

            _localDirectory =
                $@"C:\Temperature Monitoring Data\{lab}\{_year}\{_year}-{_month}\";

            _serverDirectory =
                $@"L:\Temperature Monitoring Data\{lab}\{_year}\{_year}-{_month}\";

            Directory.CreateDirectory(_localDirectory);
            Directory.CreateDirectory(_serverDirectory);
        }

        private static string EnsureTxtExtension(string fileName)
        {
            return fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                ? fileName
                : fileName + ".txt";
        }
    }
}