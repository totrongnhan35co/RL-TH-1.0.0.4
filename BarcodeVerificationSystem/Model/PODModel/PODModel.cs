namespace BarcodeVerificationSystem.Model
{
    public class PODModel
    {
        private int _Index =0;
        private string _Value = "";
        private TypePOD _Type;
        private string _PODName ="";

        public int Index { get => _Index; set => _Index = value; }
        public string Value { get => _Value; set => _Value = value; }
        public TypePOD Type { get => _Type; set => _Type = value; }
        public string PODName { get => _PODName; set => _PODName = value; }

        public PODModel(int Index, string Value, TypePOD Type, string PODName)
        {
            _Index = Index;
            _Value = Value;
            _Type = Type;
            _PODName = PODName;
        }
        public PODModel()
        {

        }
 
        public enum TypePOD {
            TEXT,
            FIELD,
            DATETIME
        }

        public override string ToString()
        {
            if (_Type == TypePOD.DATETIME)
            {
                return "<DATETIME>";
            }
            else if (_Type == TypePOD.FIELD)
            {
                return PODName != "" ? $"<{PODName}> ({TypePOD.FIELD.ToString() + _Index})" : $"<{TypePOD.FIELD.ToString() + _Index}>";
            }
            else
            {
                return $"<TEXT>";
            }
        }

        public string ToStringSample()
        {
            if (_Type == TypePOD.DATETIME)
            {
                return "<DATETIME>";
            }
            else if (_Type == TypePOD.FIELD)
            {
                return $"<{PODName}>";
            }
            else
            {
                return $"<{Value}>";
            }
        }

        public PODModel Clone()
        {
            var POD = new PODModel
            {
                Index = Index,
                Value = Value,
                Type = Type,
                PODName = PODName
            };
            return POD;
        }

    }
}
