using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Length_Stds_Environmental_Monitoring
{
    public sealed class VaisalaPTU300SeriesDevice : IMeasurementDevice
    {
        private TcpClient _tcpClient;
        private readonly int _port;
        private readonly string _ip;

        private readonly object _lock = new object();

        // Cached values (raw or base-processed)
        private double _cachedPressure;
        private double _cachedHumidity;
        private double _cachedTemperature;

        // Prevent duplicate reads per scheduler cycle
        private bool _hasReadThisCycle = false;

        private bool isRising = false;

        public VaisalaPTU300SeriesDevice(string ip, int port)
        {
            _tcpClient = new TcpClient();
            _ip = ip ?? throw new ArgumentNullException(nameof(ip));
            _port = port;
        }

        // ---------------- CONNECTION ----------------

        private bool EnsureConnected()
        {
            try
            {
                if (_tcpClient.Connected)
                    return true;

                _tcpClient.Connect(IPAddress.Parse(_ip), _port);
                return _tcpClient.Connected;
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

        // ---------------- CORE DEVICE READ ----------------


        private void ReadFromDevice()
        {
            if (!EnsureConnected())
                return;

            var stream = _tcpClient.GetStream();

            // ✅ Set timeout (milliseconds)
            stream.ReadTimeout = 2000;   // 2 seconds (adjust as required)
            stream.WriteTimeout = 2000;

            const string quote = "\"";
            //separate form command from send command?
           /* string command =
                "form 7.2 " + quote + "P=" + quote + " P " + quote + " " +
                quote + " U7 4.2 " + quote + "T=" + quote + " T " + quote + " " +
                quote + " U3 4.2 " + quote + "RH=" + quote + " RH " + quote + " " +
                quote + " U4 \r\nSEND\r\n";*/

            string command =
                "form 7.2 " + quote + "P=" + quote + " P " + quote + "trend=" + quote + "p3h" + quote + " " +
                quote + " U7 4.2 " + quote + "T=" + quote + " T " + quote + " " +
                quote + " U3 4.2 " + quote + "RH=" + quote + " RH " + quote + " " +
                quote + " U4 \r\nSEND\r\n";

            byte[] buffer = Encoding.ASCII.GetBytes(command);

            try
            {
                // ✅ Send command
                stream.Write(buffer, 0, buffer.Length);

                byte[] readBuffer = new byte[1024];

                // ✅ Read with timeout instead of sleep
                int bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);

                if (bytesRead == 0)
                    return;

                string result = Encoding.ASCII.GetString(readBuffer, 0, bytesRead);
                result = result.Substring(result.IndexOf("SEND\r\n",0));
                ParseResult(result);
            }
            catch (IOException)
            {
                // ✅ Timeout or connection issue
                _cachedPressure = double.NaN;
                _cachedHumidity = double.NaN;
                _cachedTemperature = double.NaN;
            }
            catch
            {
                _cachedPressure = double.NaN;
                _cachedHumidity = double.NaN;
                _cachedTemperature = double.NaN;
            }
        }

        private void ParseResult(string result)
        {
            try
            {
                int trendStart = result.IndexOf("trend")+6;
                int trendEnd = result.IndexOf('h');

                string trend = result.Substring(trendStart, trendEnd - trendStart);
                if (trend.Contains('*')) isRising = false; //we don't have a trend established yet, so default to falling (trend is established after 3hours of uptime)

                else
                {
                    double trend_double = Convert.ToDouble(trend);
                    if (trend_double < 0) isRising = false;
                    else isRising = true;
                }

                int pStart = result.IndexOf("P=") + 2;
                int pEnd = result.IndexOf('t');
                _cachedPressure = Convert.ToDouble(result.Substring(pStart, pEnd - pStart));

                int rhStart = result.IndexOf("RH=") + 3;
                int rhEnd = result.IndexOf('%', rhStart);
                _cachedHumidity = Convert.ToDouble(result.Substring(rhStart, rhEnd - rhStart));

                int tStart = result.IndexOf("T=") + 2;
                int tEnd = result.IndexOf("'C", tStart);
                _cachedTemperature = Convert.ToDouble(result.Substring(tStart, tEnd - tStart));
            }
            catch
            {
                isRising = false;
                _cachedPressure = double.NaN;
                _cachedHumidity = double.NaN;
                _cachedTemperature = double.NaN;
            }
        }


        // ---------------- OUTPUT ----------------

        public double ReadSensor(object sensor)
        {
            switch (sensor)
            {
                case PressureSensor ps:
                    return ps.Calibration != null
                        ? ps.ApplyCalibration(_cachedPressure)
                        : _cachedPressure;

                case HumiditySensor hs:
                    return hs.Calibration != null
                        ? hs.Calibration.Apply(_cachedHumidity)
                        : _cachedHumidity;

                case TemperatureSensor:
                    return _cachedTemperature;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported sensor type {sensor.GetType().Name} for PTU device.");
            }
        }
    }
}