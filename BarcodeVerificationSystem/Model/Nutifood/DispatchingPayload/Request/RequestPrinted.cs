using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request
{
    public class RequestPrinted
    {
        public int index_qr_code { get; set; }
        public string job_name { get; set; } = Shared.CurrentJob.FileName;
        public string plant { get; set; } = Shared.Settings.FactoryCode;
        public string wave_key { get; set; } = Shared.CurrentJob.DispatchingOrderPayload.payload.wave_key;
        public string wms_number { get; set; } = Shared.CurrentJob.DispatchingOrderPayload.payload.wms_number;
        public string qr_code { get; set; }
        public string unique_code { get; set; }
        public string shipto_code { get; set; }
        public string shipto_name { get; set; }
        //public string shipto_code { get; set; }
        //public string wms_number { get; set; }           
        public string material_number { get; set; } =
            Shared.CurrentJob.DispatchingOrderPayload.payload.items[Shared.CurrentJob.SelectedMaterialIndex].material_number;
        public string device_name { get; set; } = Shared.Settings.RLinkName;
        public string resource_code { get; set; } = Shared.Settings.LineId;
        public string resource_name { get; set; } = Shared.Settings.LineName;
        public string username { get; set; } = CurrentUser.UserCode;
        public DateTime printed_date { get; set; }
        public string status { get; set; } = "Printed";
        public DateTime sync_date { get; set; } = DateTime.Now;

    }

}

//public string plant { get; set; }                    // Plant code
//public string wave_key { get; set; }                 // Wave number (unique per plant)
