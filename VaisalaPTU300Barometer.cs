﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;


namespace Temperature_Monitor
{
    public class VaisalaPTU300Barometer:Barometer
    {
        
        
        private string host_name;
        private int port;
        private bool connection_pending;
        private bool connected;
        private PrintPressureData p_delgate;
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
        private VaisalaPTU300Hygrometer hygro;

        
        private bool isactive = false;



        public VaisalaPTU300Barometer(string hostname_, int port_,ref PrintPressureData delgate_)
        {
            tcpClient = new TcpClient();
            host_name = hostname_;
            port = port_;
            connected = false;
            p_delgate = delgate_;
            connection_pending = true;
        }
        

        
        public VaisalaPTU300Hygrometer HumidityTransducer
        {
            set { hygro = value; }
            get { return hygro; }
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

        private double CalculatePressure(double pressure_reading,bool rising_pressure)
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
                p_delgate(-1, "Pressure correction error, pressure out of range",0);
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
                p_delgate(-1, "Invalid format of pressure correction string", 0);
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
                p_delgate(-1, e.ToString(), 0);
                return 0;
            }
            return return_value;

        }
        protected override void SetPressure(double pressure_)
        {
            
        }

        public override double GetPressure()
        {
            return pressure;
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
            System.IO.StreamWriter writer=null;
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
                    catch (Exception) {
                        Thread.CurrentThread.Join(10000);
                        continue;
                    }
                }
                catch (Exception) {
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
                catch (Exception) {
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
                    NetworkStream stream = tcpClient.GetStream();
                    string result = "";
                    //tcpClient.sendReceiveData("\r",ref result);
                    //Thread.CurrentThread.Join(50);
                    //Byte[] sendBytes = Encoding.UTF8.GetBytes("send\r");
                    const string quote = "\"";
                    
                    string sendstring = "form 7.2 " + quote + "P=" + quote + " P " + quote + " " + quote + " U7 4.2 " + quote + "T=" + quote + " T " + quote + " " + quote + " U3 4.2 " + quote + "RH=" + quote + " RH " + quote + " " + quote + " U4 \r\nSEND\r\n";
                    byte[] buffer = Encoding.ASCII.GetBytes(sendstring);
                    stream.Write(buffer, 0,buffer.Length);

                    //Byte[] cr = System.Text.Encoding.ASCII.GetBytes("\r");
                   // stream.Write(cr, 0, cr.Length);
                    
                    //string sendstring2 = "SEND\r";
                    //byte[] buffer2 = System.Text.Encoding.ASCII.GetBytes(sendstring2);
                    //stream.Write(buffer2, 0, buffer2.Length);

                    byte[] read_buffer = new byte[1024];
                    Thread.CurrentThread.Join(20000);

                    if (stream.Read(read_buffer,0,read_buffer.Length) !=0 )
                    {
                        try
                        {
                            result = ASCIIEncoding.ASCII.GetString(read_buffer);
                            result = result.Substring(sendstring.Length+5);
                            string result2 = result;   //store the whole result string

                            if (result == "") throw new FormatException();
                            if (!result.Contains('=')) new FormatException();

                            //determine the pressure component of the string
                            int start_index = result.IndexOf('=') + 1;
                            int end_index = result.IndexOf('h');
                            result = result.Remove(end_index);
                            result = result.Remove(0, start_index);

                            //determine the humidity component of the string
                            int end_index2 = result2.IndexOf('%');
                            int start_index2 = end_index2 - 6;
                            result2 = result2.Substring(start_index2,6);
                                
                            double result_ = Convert.ToDouble(result);
                            pressure = result_ + CalculatePressure(result_,true);
                            error_reported = false;

                            writer.WriteLine(GetPressure() + ", " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", " + Location + ", " + EquipID.ToString());
                            p_delgate(GetPressure(), " hPa, No error on device " + IP.ToString(), ProcNameHumidity.SEND_RECEIVE);
                            if (isactive == false) num_connected_loggers++;
                            isactive = true;

                            double result2_ = Convert.ToDouble(result2);
                            hygro.SetHumidity(result2_);
                            writer2.WriteLine(hygro.GetHumidity() + ", "  + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", "  + hygro.Location + "," + hygro.EquipID.ToString());
                            hygro.HUpdate(hygro.GetHumidity(), " %RH, No error of device " + IP.ToString(), ProcNameHumidity.SEND_RECEIVE);

                            timer_zero2 = Environment.TickCount;
                            error_reported = false;
                        }
                        catch (FormatException)
                        {
                            p_delgate(-1, "RETURN STRING FORMAT ERROR", ProcNameHumidity.SEND_RECEIVE);
                            if (writer != null) writer.Close();
                            if (writer2 != null) writer2.Close();
                            continue;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            p_delgate(-1, "RETURN STRING FORMAT ERROR", ProcNameHumidity.SEND_RECEIVE);
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
                        p_delgate(-1, "NO RESPONSE", ProcNameHumidity.SEND_RECEIVE);   //error not reported - report
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
                                p_delgate(-1, "CONNECTION ERROR", ProcNameHumidity.CONNECT);
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
}
