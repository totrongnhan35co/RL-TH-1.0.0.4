using Cognex.InSight.Web.Controls;
using System.Collections.Generic;
using System.Drawing;

namespace BarcodeVerificationSystem.Model
{
    public class DetectModel
    {
        private int _Index = 0;
        public int Index { get => _Index; set => _Index = value; }
        private int _OldIndex = 0;
        public int OldIndex { get => _OldIndex; set => _OldIndex = value; }
        private Bitmap _Image = null;
        public Bitmap Image { get => _Image; set => _Image = value; }
        private ComparisonResult _CompareResult = ComparisonResult.Valid;
        public ComparisonResult CompareResult { get => _CompareResult; set => _CompareResult = value; }
        private double _CompareTime = 0;
        public double CompareTime { get => _CompareTime; set => _CompareTime = value; }
        private string _ProcessingDateTime = "";
        public string ProcessingDateTime { get => _ProcessingDateTime; set => _ProcessingDateTime = value; }
        private string _Text = "";
        public string Text { get => _Text; set => _Text = value; }
        public string Device = "Camera";
        public string Sampled = " ";
        public string isBarcodeWithinThreshold = "False";
        public string CodeQuality = " ";

        private RoleOfStation _RoleOfCamera = RoleOfStation.ForProduct;
        public RoleOfStation RoleOfCamera { get => _RoleOfCamera; set => _RoleOfCamera = value; }
        public CvsDisplay CvsDisplayImage { get; set; }

        /// <summary>
        /// Các trường OCR bổ sung từ chunk data Hikrobot.
        /// Key = ToolKey (vd: "dlocrdetect:6"), Value = kết quả đọc được.
        /// </summary>
        public Dictionary<string, string> ExtraFields { get; set; } = new Dictionary<string, string>();
    }
}
