namespace BarcodeVerificationSystem.Model
{
    public class PODDataModel
    {
        private string _IP = "";
        private int _Port = 0;
        private RoleOfStation _RoleOfPrinter = RoleOfStation.ForProduct;
        private string _Text = "";
        private string _RawText = "";

        public string IP { get => _IP; set => _IP = value; }
        public int Port { get => _Port; set => _Port = value; }
        public RoleOfStation RoleOfPrinter { get => _RoleOfPrinter; set => _RoleOfPrinter = value; }
        public string Text { get => _Text; set => _Text = value; }
        public string RawText { get => _RawText; set => _RawText = value; }
    }
}
