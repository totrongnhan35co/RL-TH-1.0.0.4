using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload
{
    public class MonitorPayload
    {
        public string plant { get; set; }
        public string resource_code { get; set; }
        public string resource_name { get; set; }
        public string ip_address_rlink { get; set; }
        public bool is_running { get; set; }
        public string ip_address_printer { get; set; }
        public string ip_address_camera { get; set; }
        public bool is_printer_connected { get; set; }
        public bool is_camera_connected { get; set; }
        public DateTime timestamp { get; set; }
        public int printed_codes_number { get; set; }
        public int generated_codes_number { get; set; }
        public bool is_software_connected { get; set; }
        public int sent_sap_codes { get; set; }
        public int sent_saas_codes { get; set; }
        public string operation_type { get; set; } = Shared.Settings.IsManufacturingMode ? "production" : "shipment";
        public int check_codes_number { get; set; }
        public int sent_saas_check_codes { get; set; }
        public int sent_sap_check_codes { get; set; }
        public string process_order { get; set; } = Shared.CurrentJob?.ProcessOrderItem?.process_order ?? "";
        public string material_doc { get; set; } = Shared.CurrentJob?.Reservation?.material_doc ?? "";
        public string device_name { get; set; } = Shared.Settings.RLinkName;
        public string wms_number { get; set; } = Shared.CurrentJob?.DispatchingOrderPayload?.payload?.wms_number ?? "";
        public string job_name { get; set; } = Shared.CurrentJob?.FileName ?? "Không có";
        public string username { get; set; }
    }


    //public class MonitorPayload
    //{
    //    public int printed_codes_number { get; set; }
    //    public int generated_codes_number { get; set; }
    //    public int sent_saas_codes { get; set; }
    //    public int sent_sap_codes { get; set; }
    //    public bool is_software_connected { get; set; }

    //    public string plant { get; set; }
    //    public string device_code { get; set; } = Shared.Settings.RLinkName;
    //    public string resource_code { get; set; }
    //    public string resource_name { get; set; }
    //    public string ip_address_rlink { get; set; }
    //    public bool is_running { get; set; }
    //    public string username { get; set; } = "user031"; //  CurrentUser.UserCode
    //    public string ip_address_printer { get; set; }
    //    public string ip_address_camera { get; set; }
    //    public bool is_printer_connected { get; set; }
    //    public bool is_camera_connected { get; set; }
    //    public DateTime timestamp { get; set; }
    //}

}
