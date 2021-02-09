using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;


namespace Temperature_Monitor
{

    public struct ProcNameHumidity
    {
        public const short CONNECT = 0;
        public const short SEND_RECEIVE = 1;
        public const short EQUATION_FORMAT = 2;
        public const short IDLE = 255;
    }


    public class OmegaTHLogger : Hygrometer
    {
        
        
        private int timer_zero1;
        private int timer_zero2;
        private int timer_1;
        private int timer_2;
        
        private bool error_reported = false;
        private bool isactive = false;
        private short dev_id = 255;
        protected static readonly int port = 14;





        public OmegaTHLogger(string correction_eq, string hostname_, ref PrintHumidityData h_update_) : base(correction_eq, hostname_, ref h_update_)
        {
            TcpClient = new ClientSocket();
        }

        public override void SetHumidity(double hty)
        {
            humidity_result = hty;
        }



        public override double GetHumidity()
        {
            return humidity_result + correction;
        }

        public double Correction
        {
            get { return correction; }
            set { correction = value; }
        }
        public short DevID
        {
            get { return dev_id; }
            set { dev_id = value; }
        }

     

        public void HLoggerQuery(object stateinfo)
        {

            timer_zero1 = Environment.TickCount;
            timer_zero2 = Environment.TickCount;
            timer_1 = timer_zero1 + 10000;
            timer_2 = timer_zero2 + 20000;

            HostName = TcpClient.GetHostName(IP);

            //create a file stream writer to put the data into
            System.IO.StreamWriter writer;
            writer = null;
            while (on)
            {
                SetDirectory();
                appenditure = false;
                 try
                 {
                     Thread.CurrentThread.Join(50);
                     //if the file exists append to it otherwise create a new file
                     if (System.IO.File.Exists(directory + EquipID + ".txt"))
                     {
                         appenditure = true;
                         writer = System.IO.File.AppendText(directory + EquipID + ".txt");
                         
                     }
                     else
                     {
                        
                         System.IO.Directory.CreateDirectory(directory);
                         writer = System.IO.File.CreateText(directory + EquipID + ".txt");
                         writer.WriteLine("Automatically Generated File!\n");
                     }
                    
                 }
                 catch (System.IO.IOException e)
                 {
                    //try closing this instance of the file writer and creating a new instance.. maybe that might fix it
                    if (writer != null)
                    {
                        writer.Close();
                        writer.Dispose();
                        Thread.CurrentThread.Join(10000);
                    }

                    if (System.IO.File.Exists(directory))
                     {
                         writer = System.IO.File.AppendText(directory + EquipID + ".txt");
                     }
                     else
                     {
                         System.IO.Directory.CreateDirectory(directory);
                         writer = System.IO.File.CreateText(directory + EquipID + ".txt");
                         writer.WriteLine("Automatically Generated File!\n");
                     }
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
                if (TcpClient.IsConnected())
                {
                    string result = "";
                    string request = "*SRH\r";
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(request);
                    if (TcpClient.SendReceiveData(data, ref result))
                    {
                        try
                        {
                            double result_ = Convert.ToDouble(result); //convert it
                            humidity_result = result_; //store it
                            CalculateCorrection();//correct it
                            double corrected_result = GetHumidity(); //get corrected result
                            corrected_result = Math.Round(corrected_result, 2); //round it
                            error_reported = false;
                            writer.WriteLine(corrected_result +", "+System.DateTime.Now.ToString()+ ", " + Location +  ", " + EquipType);
                            h_update(corrected_result, "%RH , No error on device " + IP.ToString(), ProcNameHumidity.SEND_RECEIVE);
                            if (isactive == false) num_connected_loggers++;
                            isactive = true;

                            timer_zero2 = Environment.TickCount;
                            error_reported = false;
                        }
                        catch (FormatException)
                        {
                            h_update(-1, "RETURN STRING FORMAT ERROR", ProcNameHumidity.SEND_RECEIVE);
                            continue;
                        }
                    }
                    else if (!error_reported)
                    {
                         h_update(-1, "NO RESPONSE", ProcNameHumidity.SEND_RECEIVE);   //error not reported - report
                         error_reported = true;
                        
                    }


                }
                else
                {
                    //we're not connected - attempt to connect. We don't want to do this too often because it has a high overhead, try connecting every 100s
                    if (timer_1 >= timer_zero1 + 20000)
                    {
                        if (!TryConnect())
                        {
                            
                            if (!error_reported)
                            {
                                h_update(-1, "CONNECTION ERROR", ProcNameHumidity.CONNECT);
                                error_reported = true;
                            }
                        }
                        timer_zero1 = Environment.TickCount;
                    }
                }
                if (on) Thread.CurrentThread.Join(3000);  //we only sample the logger every 3 seconds
                writer.Close();
            }

        }

        public bool TryConnect()
        {
            IPAddress ip = IPAddress.Parse(IP);
            return TcpClient.Connect(ip, port);
        }

        public void SetHostName(string hostname_)
        {
            hostname = hostname_;
        }
        public bool IsActive
        {
            get { return isactive; }
        }

        public static short NumConnectedLoggers
        {
            get { return num_connected_loggers; }
        }
    }
}
