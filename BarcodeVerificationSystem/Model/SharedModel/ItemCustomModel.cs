namespace BarcodeVerificationSystem.Model
{
    public class ItemCustomModel
    {
        private string _DisplayText = "";
        private int _Value = -1;
      

        public ItemCustomModel(string displayText,int value)
        {
            _DisplayText = displayText;
            _Value = value;
        }

        public string DisplayText { get => _DisplayText; set => _DisplayText = value; }
        public int Value { get => _Value; set => _Value = value; }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
