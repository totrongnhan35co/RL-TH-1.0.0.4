using System.Collections.Generic;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public class AccountInfo
    {
        public int id { get; set; }
        public string acc_id { get; set; }
        public string acc_name { get; set; }
        public string display_name { get; set; }
        public string password { get; set; }
        public string per_device_id { get; set; }
        public bool is_active { get; set; }
        public Dictionary<string, bool> permissions { get; set; }

        // Backward compat
        public string username => acc_name;
        public string full_name => display_name;
        public string role => "1";
    }
}