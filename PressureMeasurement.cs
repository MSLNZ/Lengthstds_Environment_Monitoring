using System;
using System.Globalization;
using System.IO;

namespace Length_Stds_Environmental_Monitoring
{
    public sealed class PressureMeasurement : IMeasurementTask
    {
        private readonly IMeasurementDevice _device;
        private readonly PressureSensor _sensor;
        private readonly PrintPressureData _uiCallback;

        private readonly string _fileName;
        private readonly string _location;

        // Directory state
        private string _localDirectory;
        private string _serverDirectory;
        private int _year;
        private int _month;

        public PressureMeasurement(
            IMeasurementDevice device,
            PressureSensor sensor,
            string fileName,
            string location,
            PrintPressureData uiCallback)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            _location = location ?? "Unknown";
            _uiCallback = uiCallback;
        }

        public IMeasurementDevice Device => _device;
        public object Sensor => _sensor;

        /// <summary>
        /// Controls whether this measurement participates in scheduler execution.
        /// If false, the device will not even be read for this measurement.
        /// </summary>
        public bool Enabled { get; set; } = true;

        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);
        public DateTime NextRun { get; set; } = DateTime.UtcNow;

        public void MeasureOnce()
        {
            if (!Enabled)
                return;

            // ⚠ Device must already be prepared by scheduler
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
                $"{_fileName} ({_location}) hPa",
                0);
        }

        private void SetDirectory()
        {
            DateTime now = DateTime.Now;

            // Only update directory when month/year changes
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
                $@"C:\Pressure Monitoring Data\{lab}\{_year}\{_year}-{_month}\";

            _serverDirectory =
                $@"L:\Pressure Monitoring Data\{lab}\{_year}\{_year}-{_month}\";

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