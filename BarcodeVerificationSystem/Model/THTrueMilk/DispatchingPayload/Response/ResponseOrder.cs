using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.THTrueMilk.Payload.DispatchingPayload.Response
{
    public class ResponseOrder
    {
        public bool is_success { get; set; }
        public string message { get; set; }
        public Payload payload { get; set; }

        public class Item
        {
            public int printed_count { get; set; }
            public string material_number { get; set; }
            public string material_name { get; set; }
            public string status_desc { get; set; }
            public string material_group { get; set; }
            public string uom { get; set; }
            public int qty_per_carton { get; set; }
            public string uom_per_carton { get; set; }
            public int qty_per_pallet { get; set; }
            public string uom_per_pallet { get; set; }
            public decimal total_gross { get; set; }
            public decimal total_cube { get; set; }
            public int qty { get; set; }
            public int total_qty_ctn { get; set; }
            public int gross_wgt { get; set; }
            public int cube { get; set; }
            public string link_web { get; set; }
        }

        public class Payload
        {
            // thieu % so du code
            public string wave_key { get; set; }
            public string wms_number { get; set; }
            public string shipment { get; set; }
            public string door { get; set; }
            public string pick_count { get; set; }
            public string list_delivery_order { get; set; }
            public string list_sales_order { get; set; }
            public string soldto_code { get; set; }
            public string soldto_name { get; set; }
            public string shipto_code { get; set; }
            public string shipto_name { get; set; }
            public string shipto_address { get; set; }
            public string print_template_name { get; set; }
            public string wave_desc { get; set; }
            public string warehouse { get; set; }
            public string trailer_number { get; set; }
            public string driver_number { get; set; }
            public string wave_type { get; set; }
            public string delivery_date { get; set; }
            public string effective_date { get; set; }
            public int add_qty { get; set; }
            public string notes { get; set; }
            public List<Item> items { get; set; }
        }
    }
}
