using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response
{
    public class Material
    {
        public string material_number { get; set; }
        public string material_name { get; set; }
        public string status_desc { get; set; }
        public string item_group { get; set; }
        public string uom_name { get; set; }
        public int case_cnt { get; set; }
        public int pallet { get; set; }
        public int original_qty { get; set; }
        public int total_qty_ctn { get; set; }
        public int gross_wgt { get; set; }
        public int cube { get; set; }
    }

    public class MaterialGroup
    {
        public string material_group { get; set; }
        public List<Material> materials { get; set; }
    }

    internal class ResponseRePrint
    {
        public bool isSuccessed { get; set; }
        public int status_code { get; set; }
        public string error_code { get; set; }
        public string message { get; set; }
        public List<MaterialGroup> material_groups { get; set; }
    }
}
