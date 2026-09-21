using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BarcodeVerificationSystem.Model.CodeGeneration
{
    public class Dispatching
    {
        private string _shiptoCode; // = Shared.Settings.DispatchingOrderPayload.payload.shipto_code; 
        private string _shipmentCode; // = Shared.Settings.DispatchingOrderPayload.payload.shipment;
        private string _shiptoName; // = Shared.Settings.DispatchingOrderPayload.payload.shipment;

        private string _lineCode; // = Shared.Settings.FactoryCode; 

        public string getShiptoCode() => _shiptoCode;
        public string getShipmentCode() => _shipmentCode;
        public string getShiptoNameCode() => _shiptoName;


        public Dispatching(string shiptoCode, string shipment, string ShiptoName)
        {
            _shiptoCode = shiptoCode;
            _shipmentCode = shipment;
            _shiptoName = ShiptoName;
            _lineCode = Shared.Settings.FactoryCode;
        }

        public string GenerateCode(string _randomCode)
        {

            //if (_shiptoCode.Length != 10 || _shipmentCode.Length != 8 || _lineCode.Length != 2 || _randomCode.Length != 10) // 6
            //{
            //}

            //if (Shared.Settings.IsManufacturingMode)
            //{
            //    return string.Empty;
            //}

            return $"{Shared.Settings.ApiDomain}{_shiptoCode}{_shipmentCode}{_lineCode}{_randomCode}"; // tong cu: 26 , mới là : 32
        }

        public string GetHumanReadableCode(string fullCode)
        {
            string randomCode = string.Empty;
            if (string.IsNullOrWhiteSpace(fullCode) || fullCode.Length != 30)
                return "";

            string shipToPart = fullCode.Substring(0, 10);
            string shipmentPart = fullCode.Substring(10, 8);
            string lineCodePart = fullCode.Substring(18, 2);
            string randomPart = fullCode.Substring(20, 10);

            // Validate parts length (redundant here but makes logic explicit)
            if (shipToPart.Length == 10 &&
                shipmentPart.Length == 8 &&
                lineCodePart.Length == 2 &&
                randomPart.Length == 10)
            {
                randomCode = randomPart;
                return randomCode;
            }
            return "";
        }

        public bool TryParse(string fullCode, out string randomCode)
        {
            randomCode = string.Empty;

            if (string.IsNullOrWhiteSpace(fullCode) || fullCode.Length != 26)
                return false;

            string shipToPart = fullCode.Substring(0, 10);
            string shipmentPart = fullCode.Substring(10, 8);
            string lineCodePart = fullCode.Substring(18, 2);
            string randomPart = fullCode.Substring(20, 6);

            // Validate parts length (redundant here but makes logic explicit)
            if (shipToPart.Length == 10 &&
                shipmentPart.Length == 8 &&
                lineCodePart.Length == 2 &&
                randomPart.Length == 10)
            {
                randomCode = randomPart;
                return true;
            }
            return false;
        }
    }
}
