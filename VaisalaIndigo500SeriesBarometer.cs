using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Length_Stds_Environmental_Monitoring
{
    public sealed class VaisalaIndigo500SeriesDevice : IMeasurementDevice
    {
        private readonly TcpClient _tcpClient;
        private readonly int _port;
        private readonly string _ip;
        private readonly RollingLinearFit _rollingLinearFit;

        private readonly object _lock = new object();

        IList<double> _pressureReadings = new List<double>();

        // Cached values (already calibrated if needed)
        private double _cachedPressure;
        private double _cachedHumidity;
        private double _cachedTemperature;

        // Prevent duplicate reads per scheduler cycle
        private bool _hasReadThisCycle = false;

        //The pressure trend for the last 100 readings (can be used for hysteresis calibration)
        private bool isRising = false;

        public VaisalaIndigo500SeriesDevice(string ip, int port)
        {
            _tcpClient = new TcpClient();
            _ip = ip ?? throw new ArgumentNullException(nameof(ip));
            _port = port;
            _rollingLinearFit = new RollingLinearFit(100); // Keep last 100 readings for trend analysis
        }

        // ---------------- Connection ----------------

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

        // ---------------- Scheduler hook ----------------

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

        /// <summary>
        /// Called automatically by scheduler cycle reset (external logic).
        /// </summary>
        public void ResetCycle()
        {
            _hasReadThisCycle = false;
        }

        // ---------------- Core Read ----------------

        private void ReadFromDevice()
        {
            if (!EnsureConnected())
                return;

            var stream = _tcpClient.GetStream();

            // ---- Pressure ----
            var pressureFrame = BuildFrame(ModbusHeader.UnitIds.transmitter, 0x2A00);
            SendFrame(stream, pressureFrame);

            var pressureResponse = ReadResponse(stream);
            if (pressureResponse != null)
            {
                double raw = ParseFloat(pressureResponse);
                if (raw == 0) _cachedPressure = double.NaN;
                else
                {
                    isRising =_rollingLinearFit.AddReading(raw);
                    _cachedPressure = raw;
                }
            }

            // ---- Humidity ----
            var humidityFrame = BuildFrame(ModbusHeader.UnitIds.probe1, 0x0000);
            SendFrame(stream, humidityFrame);

            var humidityResponse = ReadResponse(stream);
            if (humidityResponse != null)
            {
                double raw = ParseFloat(humidityResponse);
                if (raw == 0) _cachedHumidity = double.NaN;
                else _cachedHumidity = raw;
            }

            // ---- Temperature (not implemented yet) ----
            var temperatureFrame = BuildFrame(ModbusHeader.UnitIds.probe1, 0x0200);
            SendFrame(stream, temperatureFrame);

            var temperatureResponse = ReadResponse(stream);
            if (temperatureResponse != null)
            {
                double raw = ParseFloat(temperatureResponse);
                if (raw == 0) _cachedTemperature = double.NaN;
                else _cachedTemperature = raw;
            }
        }

        // ---------------- Helpers ----------------

        private void SendFrame(NetworkStream stream, Frame frame)
        {
            int size = Marshal.SizeOf(frame);
            byte[] buffer = new byte[size];
            IntPtr ptr = IntPtr.Zero;

            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(frame, ptr, true);
                Marshal.Copy(ptr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            stream.Write(buffer, 0, buffer.Length);
        }

        private byte[] ReadResponse(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];

            // NOTE: keeping simple blocking delay for now (can replace later with timeout)
            System.Threading.Thread.Sleep(500);

            if (stream.Read(buffer, 0, buffer.Length) == 0)
                return null;

            return buffer;
        }

        private double ParseFloat(byte[] buffer)
        {
          
                byte[] data = new byte[]
                {
                buffer[10], buffer[9],
                buffer[12], buffer[11]
                };
                float pres = BitConverter.ToSingle(data, 0);
                return Math.Round(pres, 3);
            
        }

        private Frame BuildFrame(ModbusHeader.UnitIds unitId, ushort register)
        {
            return new Frame
            {
                transaction_identifier = 0x0100,
                protocol_identifier = 0,
                length_field = 0x0600,
                unit_identifier = (byte)unitId,
                function_code = (byte)ModbusHeader.FunctionCodes.readholdingregisters,
                register_address = register,
                read_size = 0x0200
            };
        }

        // ---------------- Output ----------------

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
                        $"Unsupported sensor type {sensor.GetType().Name} for Indigo device.");
            }
        }

        // ---------------- Low-level protocol structures ----------------

        public struct Frame
        {
            public ushort transaction_identifier;
            public ushort protocol_identifier;
            public ushort length_field;
            public byte unit_identifier;
            public byte function_code;
            public ushort register_address;
            public ushort read_size;
        }

        public static class ModbusHeader
        {
            public enum UnitIds : byte
            {
                transmitter = 240,
                probe1 = 241,
                probe2 = 242
            }

            public enum FunctionCodes : byte
            {
                readholdingregisters = 0x03
            }
        }
    }

    public class RollingLinearFit
    {
        private readonly Queue<double> _buffer = new Queue<double>();
        private readonly int _maxSize;

        public RollingLinearFit(int size = 100)
        {
            _maxSize = size;
        }

        public bool AddReading(double value)
        {
            //bool to indicate the slope
            bool dir = false;

            // Add new reading
            _buffer.Enqueue(value);

            // Remove oldest if exceeding max size
            if (_buffer.Count > _maxSize)
            {
                _buffer.Dequeue();
            }

            // Only compute when we have enough data
            if (_buffer.Count == _maxSize)
            {
                double slope = CalculateSlope(_buffer);
                string direction = GetSlopeDirection(slope);

                
                switch (direction)
                {
                    case "Positive":
                        dir = true;
                        break;
                    case "Negative":
                        dir = false;
                        break;
                    case "Flat":
                        dir = false; // Treat flat as not rising
                        break;
                    default:
                        dir = false;
                        break;
                }
            }
            return dir;
        }

        private double CalculateSlope(IEnumerable<double> values)
        {
            int n = _maxSize;

            double sumX = 0;
            double sumY = 0;
            double sumXY = 0;
            double sumXX = 0;

            int i = 0;
            foreach (var y in values)
            {
                double x = i++;

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumXX += x * x;
            }

            double numerator = n * sumXY - sumX * sumY;
            double denominator = n * sumXX - sumX * sumX;

            if (Math.Abs(denominator) < 1e-12)
                return 0;

            return numerator / denominator;
        }

        private string GetSlopeDirection(double slope)
        {
            if (slope > 0) return "Positive";
            if (slope < 0) return "Negative";
            return "Flat";
        }
    }
}