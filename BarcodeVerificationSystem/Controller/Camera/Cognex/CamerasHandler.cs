using Cognex.InSight.Web.Controls;

namespace BarcodeVerificationSystem.Controller
{
    public abstract class CamerasHandler
    {
        public abstract string IPAddress { get; set; }
        public abstract string Port { get; set; }
        public virtual void Connect(string ipadd,string port = null){}
        public virtual void ConnectAsync(CvsDisplay cvsDisplay, string ipadd, string port = null){}
    }
}
