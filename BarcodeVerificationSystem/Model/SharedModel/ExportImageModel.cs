using System.Drawing;

namespace BarcodeVerificationSystem.Model
{
    public class ExportImageModel
    {
        private Bitmap _Image = null;
        public Bitmap Image
        {
            get { return _Image; }
            set { _Image = value; }
        }

        private int _Index = 0;
        public int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        public ExportImageModel(Bitmap image, int index)
        {
            _Image = image;
            _Index = index;
        }
    }  
}
