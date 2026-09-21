using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BarcodeVerificationSystem.Model
{
    public class DataISCamera
    {
        public int Index { get; set; }  
        public JToken JTokenData { get; set; }
        public string ImageUrl { get; set; }
    }
}
