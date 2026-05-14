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
    public sealed class TemperatureMeasurement
    {
        // --- Domain dependencies ---
        private readonly PRT _prt;
        private readonly ResistanceBridge _bridge;
        private readonly short _channel;

        // --- Metadata ---
        private readonly string _labLocation;
        private readonly string _fileName;

        // --- UI callback ---
        private readonly PrintTemperatureData _uiCallback;

        // --- Directory state ---
        private string _localDirectory;
        private string _serverDirectory;
        private int _year;
        private int _month;

        // --- Public, serialisable metadata (used by UI/config) ---
        public string Filename => _fileName;
        public short Channel => _channel;
        public PRT Probe => _prt;
        public string LabLocation => _labLocation;
        public ResistanceBridge Bridge => _bridge;


        // Optional legacy metadata (purely descriptive)
        public string BridgeName { get; set; }
        public string MUXName { get; set; }

        public int MeasurementIndex { get; }

        public TemperatureMeasurement(
            int measurementIndex,
            PRT prt,
            ResistanceBridge bridge,
            short channel,
            string labLocation,
            string fileName,
            PrintTemperatureData uiCallback)
        {
            MeasurementIndex = measurementIndex;

            _prt = prt ?? throw new ArgumentNullException(nameof(prt));
            _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
            _channel = channel;

            _labLocation = labLocation ?? string.Empty;
            _fileName = string.IsNullOrWhiteSpace(fileName)
                ? "Temperature"
                : fileName;

            _uiCallback = uiCallback;
        }

        /// <summary>
        /// Performs exactly one temperature measurement.
        /// This method is synchronous by design.
        /// </summary>
        public void MeasureOnce()
        {
            double temperature;

            try
            {
                temperature = _bridge.GetTemperature(
                    _prt,
                    _channel,
                    probe_has_changed: false);
            }
            catch (Exception ex)
            {
                NotifyUI(double.NaN, $"ERROR: {ex.Message}");
                return;
            }

            string timestamp = DateTime.Now.ToString(
                "dd/MM/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture);

            WriteToFile(temperature, timestamp);
            NotifyUI(temperature);
        }

        // ---------------- Private helpers ----------------

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
                    $"{_channel}," +
                    $"{timestamp}," +
                    $"{_labLocation}," +
                    $"{_fileName}");
            }
        }

        private void NotifyUI(double value, string messageOverride = null)
        {
            string msg = messageOverride ??
                $"{_fileName} on CH{_channel} in {_labLocation}";

            _uiCallback?.Invoke(value, msg, MeasurementIndex);
        }

        /// <summary>
        /// Sets local/server directories based on current month and lab.
        /// Matches legacy behaviour.
        /// </summary>
        private void SetDirectory()
        {
            DateTime now = DateTime.Now;

            if (_year == now.Year && _month == now.Month)
                return;

            _year = now.Year;
            _month = now.Month;

            string lab;

            switch (_labLocation)
            {
                case "Hilger Lab": lab = "Hilger Lab"; break;
                case "Long Room": lab = "Long Room"; break;
                case "Laser Lab": lab = "Laser Lab"; break;
                case "Underground Tape Tunnel": lab = "Tunnel"; break;
                case "CMM Lab": lab = "CMM Lab"; break;
                default: lab = "MISC"; break;
            }

            _localDirectory =
                $@"C:\Temperature Monitoring Data\{lab}\{_year}\{_year}-{_month}\";

            _serverDirectory =
                $@"L:\Temperature Monitoring Data\{lab}\{_year}\{_year}-{_month}\";

            Directory.CreateDirectory(_localDirectory);
            Directory.CreateDirectory(_serverDirectory);
        }
    }



    /// <summary>
    /// Executes temperature measurements in a deterministic sequence.
    /// Preserves legacy behaviour: one measurement at a time, in order added.
    /// </summary>

    public sealed class SeqeuencedTemperatureMeasurementManager
    {
        private readonly List<TemperatureMeasurement> _measurements =
            new List<TemperatureMeasurement>();

        private CancellationTokenSource _cts;
        private Task _schedulerTask;

        // Interval is mutable and UI‑controlled
        public TimeSpan Interval { get; set; }

        public SeqeuencedTemperatureMeasurementManager(TimeSpan initialInterval)
        {
            Interval = initialInterval;
        }

        public void Add(TemperatureMeasurement measurement)
        {
            if (measurement == null)
                throw new ArgumentNullException(nameof(measurement));

            _measurements.Add(measurement);
        }

        public void Remove(TemperatureMeasurement measurement)
        {
            _measurements.Remove(measurement);
        }

        public void Start()
        {
            if (_schedulerTask != null)
                return;

            _cts = new CancellationTokenSource();
            _schedulerTask = Task.Run(() => RunAsync(_cts.Token));
        }

        public async Task StopAsync()
        {
            if (_schedulerTask == null)
                return;

            _cts.Cancel();

            try
            {
                await _schedulerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _schedulerTask = null;
            }
        }


        private async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var measurement in _measurements.ToArray())
                {
                    // 1. Ensure correct MUX channel
                    measurement.Bridge.SetCurrentChannel(measurement.Channel);

                    // 2. Dead time / settling time (legacy interval)
                    await Task.Delay(Interval, token);

                    // 3. Take the measurement
                    measurement.MeasureOnce();
                }
            }
        }




        public IReadOnlyList<TemperatureMeasurement> Measurements =>
            _measurements.AsReadOnly();
    }


}