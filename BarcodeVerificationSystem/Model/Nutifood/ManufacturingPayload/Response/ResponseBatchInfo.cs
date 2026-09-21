using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response
{
    public class ResponseBatchInfo
    {
        public bool is_success { get; set; }
        public string message { get; set; }
        public string process_order { get; set; }
        public string sales_org { get; set; }
        public string plant { get; set; }
        public string sloc { get; set; }
        public string material_number { get; set; }
        public string material_name { get; set; }
        public int qty { get; set; }
        public string uom { get; set; }
        public int qty_per_carton { get; set; }
        public string uom_per_carton { get; set; }
        public int qty_per_pallet { get; set; }
        public string uom_per_pallet { get; set; }
        public string basic_start_date { get; set; }
        public string basic_end_date { get; set; }
        public object act_release_date { get; set; }
        public int shelf_life { get; set; }
        public string period_indicator_for_shelf_life { get; set; }
        public string status { get; set; }
        public string resource_code { get; set; }
        public string resource_name { get; set; }
        public List<ResponseProcessOrder.BatchInfo> batch_info { get; set; }
        public int add_qty { get; set; }
        public string print_template_name { get; set; }
        public int printed_count { get; set; }

        //public class BatchInfo
        //{
        //    public string batch { get; set; }
        //    public DateTime mauf_date { get; set; }
        //    public DateTime expired_date { get; set; }
        //}
    }
}
