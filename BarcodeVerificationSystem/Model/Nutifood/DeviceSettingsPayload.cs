using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload
{
    internal class DeviceSettingsPayload
    {
        public bool is_success { get; set; }
        public string message { get; set; }
        public int add_qty { get; set; }
        public string batch_date_format { get; set; }
        public string print_template_name { get; set; }
        public string resource_code { get; set; }
        public string resource_name { get; set; }
        public string device_name { get; set; }
        public string resource_type { get; set; }
        public string plant { get; set; }

    }
}
