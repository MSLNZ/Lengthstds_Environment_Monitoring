using System;
using System.Net;
using System.Text;

namespace Length_Stds_Environmental_Monitoring
{
    public sealed class OmegaTHDevice : IMeasurementDevice
    {
        private readonly ClientSocket _client;
        private readonly object _lock = new object();

        private readonly int _port;
        private readonly string _ip;

        // Cached value (raw or base processed)
        private double _cachedHumidity;

        // Prevent duplicate reads per scheduler cycle
        private bool _hasReadThisCycle = false;

        public OmegaTHDevice(string ip, int port)
        {
            _client = new ClientSocket();
            _ip = ip ?? throw new ArgumentNullException(nameof(ip));
            _port = port;
        }

        // ---------------- CONNECTION ----------------

        private bool EnsureConnected()
        {
            try
            {
                if (_client.IsConnected())
                    return true;

                return _client.Connect(IPAddress.Parse(_ip), _port);
            }
            catch
            {
                return false;
            }
        }

        // ---------------- SCHEDULER ENTRY ----------------

        public void PrepareSensor(object sensor)
        {
            lock (_lock)
            {
                if (_hasReadThisCycle)
                    return;

                ReadFromDevice();
                _hasReadThisCycle = true;
            }
        }

        public void ResetCycle()
        {
            _hasReadThisCycle = false;
        }

        // ---------------- CORE READ ----------------
        private void ReadFromDevice()
        {
            if (!EnsureConnected())
                return;

            string response = "";

            byte[] request = Encoding.ASCII.GetBytes("*SRH\r");

            if (_client.SendReceiveData(request, ref response))
            {
                if (double.TryParse(response, out double raw))
                {
                    _cachedHumidity = raw; // ✅ NO calibration here
                }
                else
                {
                    _cachedHumidity = double.NaN;
                }
            }
        }

        // ---------------- OUTPUT ----------------

        public double ReadSensor(object sensor)
        {
            if (sensor is HumiditySensor hs)
            {
                return hs.Calibration != null
                    ? hs.Calibration.Apply(_cachedHumidity)
                    : _cachedHumidity;
            }

            throw new InvalidOperationException(
                $"Unsupported sensor type {sensor.GetType().Name} for OmegaTH device.");
        }
    }
}
