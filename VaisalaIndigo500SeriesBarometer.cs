using System;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;

namespace Temperature_Monitor
{
    public class VaisalaIndigo500SeriesBarometer : Barometer
    {
        private string host_name;
        private int port;
        private bool connection_pending;
        private bool connected;
        private PrintPressureData p_delgate2;
        private string IP_address;
        private bool error_reported = false;
        private string pressure950;
        private string pressure960;
        private string pressure970;
        private string pressure980;
        private string pressure990;
        private string pressure1000;
        private string pressure1010;
        private string pressure1020;
        private string pressure1030;
        private string pressure1040;
        private string pressure1050;
        private int timer_zero1;
        private int timer_zero2;
        private int timer_1;
        private int timer_2;
        private VaisalaIndigo500SeriesHygrometer hygro;
        private bool isactive = false;
        public VaisalaIndigo500SeriesBarometer(string hostname_, int port_, ref PrintPressureData delgate_)
        {
            tcpClient = new TcpClient();
            host_name = hostname_;
            port = port_;
            connected = false;
            p_delgate2 = delgate_;
            connection_pending = true;
        }
        public VaisalaIndigo500SeriesHygrometer HumidityTransducer
        {
            set { hygro = value; }
            get { return hygro; }
        }

        protected override void SetPressure(double pressure_)
        {

        }

        public override double GetPressure()
        {
            return pressure;
        }

        public string P950
        {
            set { pressure950 = value; }
            get { return pressure950; }
        }
        public string P960
        {
            set { pressure960 = value; }
            get { return pressure960; }
        }
        public string P970
        {
            set { pressure970 = value; }
            get { return pressure970; }
        }
        public string P980
        {
            set { pressure980 = value; }
            get { return pressure980; }
        }
        public string P990
        {
            set { pressure990 = value; }
            get { return pressure990; }
        }
        public string P1000
        {
            set { pressure1000 = value; }
            get { return pressure1000; }
        }
        public string P1010
        {
            set { pressure1010 = value; }
            get { return pressure1010; }
        }
        public string P1020
        {
            set { pressure1020 = value; }
            get { return pressure1020; }
        }
        public string P1030
        {
            set { pressure1030 = value; }
            get { return pressure1030; }
        }
        public string P1040
        {
            set { pressure1040 = value; }
            get { return pressure1040; }
        }
        public string P1050
        {
            set { pressure1050 = value; }
            get { return pressure1050; }
        }

        private double CalculatePressure(double pressure_reading, bool rising_pressure)
        {
            string correction_string = "";
            try
            {

                if (pressure_reading < 945) throw new ArgumentOutOfRangeException();
                else if (pressure_reading >= 945 && pressure_reading < 955) correction_string = P950;
                else if (pressure_reading >= 955 && pressure_reading < 965) correction_string = P960;
                else if (pressure_reading >= 965 && pressure_reading < 975) correction_string = P970;
                else if (pressure_reading >= 975 && pressure_reading < 985) correction_string = P980;
                else if (pressure_reading >= 985 && pressure_reading < 995) correction_string = P990;
                else if (pressure_reading >= 995 && pressure_reading < 1005) correction_string = P1000;
                else if (pressure_reading >= 1005 && pressure_reading < 1015) correction_string = P1010;
                else if (pressure_reading >= 1015 && pressure_reading < 1025) correction_string = P1020;
                else if (pressure_reading >= 1025 && pressure_reading < 1035) correction_string = P1030;
                else if (pressure_reading >= 1035 && pressure_reading < 1045) correction_string = P1040;
                else if (pressure_reading >= 1045 && pressure_reading < 1055) correction_string = P1050;
                else throw new ArgumentOutOfRangeException();
            }
            catch (ArgumentOutOfRangeException)
            {
                p_delgate2(-1, "Pressure correction error, pressure out of range", 0);
                return 0.0;
            }
            int colon = 0;

            try
            {
                colon = correction_string.IndexOf(":");

                if (colon == -1) throw new FormatException();

            }
            catch (FormatException)
            {
                p_delgate2(-1, "Invalid format of pressure correction string", 0);
                return 0.0;
            }


            //if the pressure is rising choose the first part of the pressure correction string
            if (rising_pressure)
            {
                correction_string = correction_string.Remove(colon);
            }
            else if (!rising_pressure)
            {
                correction_string = correction_string.Substring(colon + 1);
            }

            double return_value = 0.0;
            try
            {
                return_value = Convert.ToDouble(correction_string);
            }
            catch (FormatException e)
            {
                p_delgate2(-1, e.ToString(), 0);
                return 0;
            }
            return return_value;

        }
        //periodically get pressure measurements from the Barometer
        public void Measure(object current_measurement)
        {
            timer_zero1 = Environment.TickCount;
            timer_zero2 = Environment.TickCount;
            timer_1 = timer_zero1 + 10000;
            timer_2 = timer_zero2 + 20000;
            //HostName = tcpClient.GetHostName(IP);

            //create a file stream writer to put the data into
            System.IO.StreamWriter writer = null;
            System.IO.StreamWriter writer2 = null;

            //Create a file to save this pressure measurement to.
            while (on)
            {
                SetDirectory();
                while (hygro == null) Thread.CurrentThread.Join(1000);  //wait here until the hygrometer object has been instantiated 
                hygro.SetDirectory();

                try
                {
                    if (System.IO.File.Exists(directory + EquipID + ".txt"))
                    {
                        appenditure = true;
                        FileStream fs = new FileStream(directory + EquipID + ".txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    else
                    {
                        Directory.CreateDirectory(directory);
                        FileStream fs = new FileStream(directory + EquipID + ".txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                }
                catch (System.IO.IOException)
                {


                    //try closing this instance of the file writer and creating a new instance.. maybe that might fix it
                    if (writer != null)
                    {
                        writer.Close();
                        writer.Dispose();
                        Thread.CurrentThread.Join(10000);
                    }
                    try
                    {
                        //if the file exists append to it otherwise create a new file
                        if (File.Exists(directory + EquipID + ".txt"))
                        {
                            FileStream fs = new FileStream(directory + EquipID + ".txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                            writer = new StreamWriter(fs);
                        }
                        else
                        {
                            Directory.CreateDirectory(directory);
                            FileStream fs = new FileStream(directory + EquipID + ".txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                            writer = new StreamWriter(fs);
                        }
                    }
                    catch (System.IO.IOException e)
                    {
                        Thread.CurrentThread.Join(10000);
                        continue; //just ignore the issues and hope the connectivity resolves by itself.
                    }
                    catch (Exception)
                    {
                        Thread.CurrentThread.Join(10000);
                        continue;
                    }
                }
                catch (Exception)
                {
                    Thread.CurrentThread.Join(10000);
                    continue;
                }


                try
                {
                    //if the file exists append to it otherwise create a new file
                    if (File.Exists(hygro.Directory1 + hygro.EquipID + ".txt"))
                    {
                        FileStream fs = new FileStream(hygro.Directory1 + hygro.EquipID + ".txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        writer2 = new StreamWriter(fs);
                    }
                    else
                    {
                        Directory.CreateDirectory(hygro.Directory1);
                        FileStream fs = new FileStream(hygro.Directory1 + hygro.EquipID + ".txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                        writer2 = new StreamWriter(fs);
                    }

                }
                catch (System.IO.IOException)
                {
                    if (writer2 != null)
                    {
                        writer2.Close();
                        writer2.Dispose();
                        Thread.CurrentThread.Join(10000);
                    }

                    try
                    {
                        //if the file exists append to it otherwise create a new file
                        if (File.Exists(hygro.Directory1 + hygro.EquipID + ".txt"))
                        {
                            FileStream fs = new FileStream(hygro.Directory1 + hygro.EquipID + ".txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                            writer2 = new StreamWriter(fs);
                        }
                        else
                        {
                            Directory.CreateDirectory(hygro.Directory1);
                            FileStream fs = new FileStream(hygro.Directory1 + hygro.EquipID + ".txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                            writer2 = new StreamWriter(fs);
                        }
                    }
                    catch (System.IO.IOException)
                    {
                        continue;
                    }
                    catch (Exception)
                    {
                        continue;
                    }


                }
                catch (Exception)
                {
                    continue;
                }



                //get the latest times
                timer_1 = Environment.TickCount;
                timer_2 = Environment.TickCount;


                //if we haven't had a valid humidity reading for more than 30 s then set to inactive
                if (timer_2 > timer_zero2 + 30000)
                {
                    if (isactive == true) num_connected_loggers--;
                    isactive = false;

                }

                //check if we are connected
                if (tcpClient.Connected)
                {
                    string result = "";

                    Frame barometer_query_frame;
                    barometer_query_frame.transaction_identifier = 0x0100;
                    barometer_query_frame.protocol_identifier = 0;
                    barometer_query_frame.length_field = 0x0600;
                    barometer_query_frame.unit_identifier = (byte) ModbusHeader.UnitIds.transmitter;
                    barometer_query_frame.function_code = (byte) ModbusHeader.FunctionCodes.readholdingregisters;
                    barometer_query_frame.register_address = 0x2A00;
                    barometer_query_frame.read_size = 0x0200;

                    int size = Marshal.SizeOf(barometer_query_frame);
                    byte[] send_bytes = new byte[size];
                    IntPtr ptr = IntPtr.Zero;

                    try
                    {
                        ptr = Marshal.AllocHGlobal(size);
                        Marshal.StructureToPtr(barometer_query_frame, ptr, true);
                        Marshal.Copy(ptr, send_bytes, 0, size);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }


                    NetworkStream stream = tcpClient.GetStream();
                    stream.Write(send_bytes,0, send_bytes.Length);
                    byte[] read_buffer = new byte[1024];
                    Thread.CurrentThread.Join(10000);
                    if (stream.Read(read_buffer, 0, read_buffer.Length)!=0)
                    {
                        try
                        {
                           
                            byte[] data = new byte[] { read_buffer[10], read_buffer[9], read_buffer[12], read_buffer[11] };
                            float pres = BitConverter.ToSingle(data, 0);
                            double p = Math.Round(pres, 3);

                            pressure = p + CalculatePressure(p, true);
                            error_reported = false;

                            writer.WriteLine(GetPressure() + ", " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", " + Location + ", " + EquipID.ToString());
                            p_delgate2(GetPressure(), " hPa, No error on device " + IP.ToString(), ProcNameHumidity.SEND_RECEIVE);

                            if (isactive == false) num_connected_loggers++;
                            isactive = true;
                            timer_zero2 = Environment.TickCount;
                            error_reported = false;
                        }
                        catch (FormatException)
                        {
                            p_delgate2(-1, "RETURN STRING FORMAT ERROR", ProcNameHumidity.SEND_RECEIVE);
                            if (writer != null) writer.Close();
                            if (writer2 != null) writer2.Close();
                            continue;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            p_delgate2(-1, "RETURN STRING FORMAT ERROR", ProcNameHumidity.SEND_RECEIVE);
                            if (writer != null) writer.Close();
                            if (writer2 != null) writer2.Close();
                            continue;
                        }
                        catch (ObjectDisposedException)
                        {
                            continue;
                        }
                    }
                    else if (!error_reported)
                    {
                        p_delgate2(-1, "NO RESPONSE", ProcNameHumidity.SEND_RECEIVE);   //error not reported - report
                        error_reported = true;

                    }
                    Frame hygrometer_query_frame;
                    hygrometer_query_frame.transaction_identifier = 0x0100;
                    hygrometer_query_frame.protocol_identifier = 0;
                    hygrometer_query_frame.length_field = 0x0600;
                    hygrometer_query_frame.unit_identifier = (byte)ModbusHeader.UnitIds.probe1;
                    hygrometer_query_frame.function_code = (byte)ModbusHeader.FunctionCodes.readholdingregisters;
                    hygrometer_query_frame.register_address = 0x0000;
                    hygrometer_query_frame.read_size = 0x0200;

                    size = Marshal.SizeOf(hygrometer_query_frame);
                    send_bytes = new byte[size];
                    ptr = IntPtr.Zero;

                    try
                    {
                        ptr = Marshal.AllocHGlobal(size);
                        Marshal.StructureToPtr(hygrometer_query_frame, ptr, true);
                        Marshal.Copy(ptr, send_bytes, 0, size);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }

                    stream = tcpClient.GetStream();
                    stream.Write(send_bytes, 0, send_bytes.Length);
                    read_buffer = new byte[1024];
                    Thread.CurrentThread.Join(10000);
                    if (stream.Read(read_buffer, 0, read_buffer.Length) != 0)
                    {
                        try
                        {
                            byte[] data = new byte[] { read_buffer[10], read_buffer[9], read_buffer[12], read_buffer[11] };
                            float hum = BitConverter.ToSingle(data, 0);

                            double h = Math.Round(hum, 2);

                            error_reported = false;
                            hygro.SetHumidity(h);
                            writer2.WriteLine(hygro.GetHumidity() + ", " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", " + hygro.Location + "," + hygro.EquipID.ToString());
                            hygro.HUpdate(hygro.GetHumidity(), " %RH, No error of device " + IP.ToString(), ProcNameHumidity.SEND_RECEIVE);

                            timer_zero2 = Environment.TickCount;
                            error_reported = false;
                        }
                        catch (FormatException)
                        {
                            p_delgate2(-1, "RETURN STRING FORMAT ERROR", ProcNameHumidity.SEND_RECEIVE);
                            if (writer != null) writer.Close();
                            if (writer2 != null) writer2.Close();
                            continue;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            p_delgate2(-1, "RETURN STRING FORMAT ERROR", ProcNameHumidity.SEND_RECEIVE);
                            if (writer != null) writer.Close();
                            if (writer2 != null) writer2.Close();
                            continue;
                        }
                        catch (ObjectDisposedException)
                        {
                            continue;
                        }
                    }
                    else if (!error_reported)
                    {
                        p_delgate2(-1, "NO RESPONSE", ProcNameHumidity.SEND_RECEIVE);   //error not reported - report
                        error_reported = true;

                    }
                }
                else
                {
                    //we're not connected - attempt to connect. We don't want to do this too often because it has a high overhead, try connecting every 10s
                    if (timer_1 >= timer_zero1 + 10000)
                    {
                        if (!TryConnect())
                        {
                            if (!error_reported)
                            {
                                p_delgate2(-1, "CONNECTION ERROR", ProcNameHumidity.CONNECT);
                                error_reported = true;
                            }
                        }
                        timer_zero1 = Environment.TickCount;
                    }
                }


                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();

                }
                if (writer2 != null)
                {
                    writer2.Close();
                    writer2.Dispose();

                }
                if (on) Thread.CurrentThread.Join(3000);  //we only sample the logger every 3 seconds
            }
        }

        public bool TryConnect()
        {
            IPAddress ip = IPAddress.Parse(IP);
            tcpClient.Connect(ip, port);
            return tcpClient.Connected;
        }

        public string IP
        {
            get { return IP_address; }
            set { IP_address = value; }
        }
    }

    

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

    public struct ModbusHeader
    {
        public enum UnitIds : byte
        {
            transmitter = 240,
            probe1 = 241,
            probe2 = 242
        }
        public enum FunctionCodes : byte
        {
            readholdingregisters = 0x03,
            readdeviceinformation1 = 0x2B,
            readdeviceinformation2 = 0x0E
        }
    }
    
    public static class Indigo500Registers
    {
       

        public enum FloatPointMeasurementRegister : ushort
        {
            //floating point measurement data registers (read only)
            barometric_pressure = 0x002A,   //Barometric pressure	32-bit float hPa
            actual_current_ai = 0x0034, //Actual current level of analog input (mA)
            validated_current_ai = 0x0036 //Validated current level of analog input (mA) or NaN if analog input is out of valid range(3.8–20.5 mA)
        }

        public enum IntMeasurementRegister : ushort
        {

        }

        public enum StatusRegisters : ushort
        {
            barometer_status = 0x0201,
            Barometer_error_flags = 0x0202
        }

        public struct ConfigRegisters
        {
            enum Analogoutput1 : ushort
            {
                output_mode = 0x0700,
                output_parameter = 0x0701,
                scale_low_end = 0x0702,
                scale_high_end = 0x0704,
                error_output = 0x0706,
                Low_clipping_limit = 0x0708,
                high_clipping_limit = 0x070C,
                output_level = 0x0710,
                force_output_level = 0x0712
            }
            enum Analogoutput2 : ushort
            {
                output_mode = 0x0800,
                output_parameter = 0x0801,
                scale_low_end = 0x0802,
                scale_high_end = 0x0804,
                error_output = 0x0806,
                Low_clipping_limit = 0x0808,
                high_clipping_limit = 0x080C,
                output_level = 0x0810,
                force_output_level = 0x0812
            }
            enum Analogoutput3 : ushort
            {
                output_mode = 0x0900,
                output_parameter = 0x0901,
                scale_low_end = 0x0902,
                scale_high_end = 0x0904,
                error_output = 0x0906,
                Low_clipping_limit = 0x0908,
                high_clipping_limit = 0x090C,
                output_level = 0x0910,
                force_output_level = 0x0912
            }
            enum Analogoutput4 : ushort
            {
                output_mode = 0x0A00,
                output_parameter = 0x0A01,
                scale_low_end = 0x0A02,
                scale_high_end = 0x0A04,
                error_output = 0x0A06,
                Low_clipping_limit = 0x0A08,
                high_clipping_limit = 0x0A0C,
                output_level = 0x0A10,
                force_output_level = 0x0A12
            }
            enum Relay1 : ushort
            {
                output_mode = 0x0A00,
                output_parameter = 0x0A01,
                scale_low_end = 0x0A02,
                scale_high_end = 0x0A04,
                error_output = 0x0A06,
                Low_clipping_limit = 0x0A08,
                high_clipping_limit = 0x0A0C,
                output_level = 0x0A10,
                force_output_level = 0x0A12
            }

        }

        public enum TimeZone : byte
        {
            none,	//Modbus register value for when a time zone outside of this list is selected. Writing 0 is ignored.
            Pacific_Pago_Pago,
            Niue_Pacific_Niue,
            Pacific_Honolulu,
            Pacific_Tahiti,
            America_Adak,
            Pacific_Marquesas,
            Pacific_Gambier,
            America_Anchorage,
            Pacific_Pitcairn,
            America_Los_Angeles,
            America_Phoenix,
            America_Denver,
            Pacific_Galapagos,
            America_Mexico_City,
            Pacific_Easter,
            America_Chicago,
            America_Lima,
            America_Jamaica,
            America_Havana,
            America_New_York,
            America_Caracas,
            America_Santo_Domingo,
            America_Santiago,
            America_Asuncion,
            America_Halifax,
            America_St_Johns,
            America_Sao_Paulo,
            America_Miquelon,
            America_Noronha,
            Greenland_America_Nuuk,
            Atlantic_Cape_Verde,
            Atlantic_Azores,
            Africa_Abidjan,
            Europe_London,
            Europe_Lisbon,
            Antarctica_Troll,
            Africa_Algiers,
            Africa_Lagos,
            Europe_Dublin,
            Africa_Casablanca,
            Europe_Paris,
            Africa_Maputo,
            Africa_Tripoli,
            Africa_Johannesburg,
            Europe_Athens,
            Africa_Cairo,
            Asia_Beirut,
            Europe_Chisinau,
            Asia_Gaza,
            Asia_Jerusalem,
            Europe_Istanbul,
            Africa_Nairobi,
            Europe_Moscow,
            Asia_Tehran,
            Asia_Dubai,
            Asia_Kabul,
            Asia_Tashkent,
            Asia_Karachi,
            Asia_Colombo,
            Asia_Kolkata,
            Asia_Kathmandu,
            Asia_Dhaka,
            Asia_Yangon,
            Asia_Bangkok,
            Asia_Jakarta,
            Asia_Singapore,
            Australia_Perth,
            Asia_Shanghai,
            Asia_Hong_Kong,
            Asia_Manila,
            Asia_Makassar,
            Australia_Eucla,
            Asia_Chita,
            Asia_Tokyo,
            Asia_Seoul,
            Asia_Jayapura,
            Australia_Darwin,
            Australia_Adelaide,
            Asia_Vladivostok,
            Australia_Brisbane,
            Pacific_Guam,
            Australia_Sydney,
            Australia_Lord_Howe,
            Pacific_Bougainville,
            Pacific_Norfolk,
            Asia_Kamchatka,
            Pacific_Auckland,
            Pacific_Chatham,
            Pacific_Tongatapu,
            Pacific_Kiritimati
        }

    }
}
