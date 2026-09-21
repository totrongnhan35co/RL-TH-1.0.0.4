using GenCode.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCode.Utils
{
    public static class FactoryCodeHelper
    {
        public static string GetFactoryName(int digit)
        {
            switch ((FactoryCode)digit)
            {
                case FactoryCode.BinhDuong:
                    return "Bình Dương";
                case FactoryCode.GiaLai:
                    return "Gia Lai";
                case FactoryCode.HungYen:
                    return "Hưng Yên";
                default:
                    return "Không xác định";
            }
        }
    }

}
