
//using System.Resources;
using System.Net;
using System.Text;
using VisaComLib;


namespace Length_Stds_Environmental_Monitoring
{


    public interface ITransport
    {
        void SendCommand(string command);
        string ReadResponse();
    }

    /// <summary>
    /// A class to serve temperature data
    /// </summary>

    public sealed class GpibOverLanTransport : ITransport
    {
        private FormattedIO488 ioDmm;
        
        private readonly string _ip;
        private readonly int _port;
        private readonly string _siclInterfaceId;
        private readonly int _gpibAddress;
        private string error_status = "no_error";
        private string sendstring = ""; 
        public GpibOverLanTransport(
            string ip,
            int port,
            string siclInterfaceId,
            int gpibAddress)
        {
            try
            {
                //create the formatted io object
                ioDmm = new FormattedIO488Class();
                //iodss = new ITcpipInstr();
            }
            catch (SystemException ex)
            {
                error_status = "FormattedIO488Class object creation failure. " + ex.Source + "  " + ex.Message;
                return;
            }
            _ip = ip;
            _port = port;
            _siclInterfaceId = siclInterfaceId;
            _gpibAddress = gpibAddress;
            InitIO();
        }
        public void InitIO()
        {
            sendstring = _siclInterfaceId + _gpibAddress;

            try
            {

                //create the resource manager and open a session with the instrument specified on txtAddress
                ResourceManager grm = new ResourceManager();


                ioDmm.IO = (IMessage)grm.Open(sendstring, AccessMode.SHARED_LOCK, 2000, "");


            }
            catch (SystemException ex)
            {
                ioDmm.IO = null;
                error_status = "Open failed on " + sendstring + " " + ex.Source + "  " + ex.Message;

            }
            error_status = "No Error";

        }
        public void CloseConnection()
        {
            //close the session
            ioDmm.IO.Close();
        }
        public void SendCommand(string command)
        {

            while (true)
            {
                try
                {
                    ioDmm.WriteString(command, true);
                    break;
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    //try flushing the buffers and resending waiting a bit and sending again
                    //If it falls over here it is probably due to a problem a network issue.
                    try
                    {
                        if (ioDmm.IO != null)
                        {
                            ioDmm.IO.Close();
                        }
                        ioDmm = new FormattedIO488();
                        //create the resource manager and open a session with the instrument specified on txtAddress     
                        ResourceManager grm = new ResourceManager();
                        ioDmm.IO = (IMessage)grm.Open(sendstring, AccessMode.SHARED_LOCK, 2000, "");         //this is set to null if io is down and it triggers a com exception
                    }

                    catch (System.Runtime.InteropServices.COMException)
                    {
                        System.Threading.Thread.CurrentThread.Join(5000);
                        continue;
                    }
                }
            }
        }
        public string ReadResponse()
        {
            string val = "";
            try
            {
                val = ioDmm.ReadString();
            }
            catch (System.Runtime.InteropServices.COMException)
            {

            }
            return val;
        }
    }

}
