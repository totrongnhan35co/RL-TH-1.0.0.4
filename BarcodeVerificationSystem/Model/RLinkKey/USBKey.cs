namespace BarcodeVerificationSystem.Model
{
    public class USBKey
    {
        private ushort[] _USBPassword = new ushort[4];

        public ushort[] USBPassword
        {
            get { return _USBPassword; }
            set { _USBPassword = value; }
        }

        private ushort[] _InputValue = new ushort[4];

        public ushort[] InputValue
        {
            get { return _InputValue; }
            set { _InputValue = value; }
        }

        private ushort[] _ExpectedResult = new ushort[4];

        public ushort[] ExpectedResult
        {
            get { return _ExpectedResult; }
            set { _ExpectedResult = value; }
        }
    }
}
