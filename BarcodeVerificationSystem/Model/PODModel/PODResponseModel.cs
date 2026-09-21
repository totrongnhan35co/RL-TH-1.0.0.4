using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model
{

    [DataContract]
    public class PODResponseModel
    {
        [DataMember(Name = "command")]
        public string Command { get; set; }

        [DataMember(Name = "template")]
        public string[] Template { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }
    }
}
