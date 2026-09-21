using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response.ResponseProcessOrder;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.Response
{
    public class ResponseAllProducts
    {
        public bool success { get; set; }
        public string message { get; set; }
        public ProductListData data { get; set; }
        public DateTime timestamp { get; set; }
        public int status { get; set; }
    }

    public class ProductListData
    {
        public int total { get; set; }
        public List<ProductItem> data { get; set; }
    }

    public class ProductItem
    {
        public string product_id { get; set; }
        public string product_code { get; set; }
        public string product_name { get; set; }
        public string size { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string product_name_en { get; set; }
        public string product_description { get; set; }
        public string product_images { get; set; }
        public double weight { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string deleted_at { get; set; }
        public List<ListWeight> listweight { get; set; }
    }

    public class ListWeight
    {
        public double weight { get; set; }
    }
}
