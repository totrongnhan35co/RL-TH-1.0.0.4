using System.Runtime.Serialization;

namespace EncrytionFile.Model
{
    [DataContract]
    public class HardwareIDModel
    {
        #region Properties
        [DataMember(Name = "_HardwareID")]
        public string HardwareID { get; set; }
        #endregion Properties
    }
}
