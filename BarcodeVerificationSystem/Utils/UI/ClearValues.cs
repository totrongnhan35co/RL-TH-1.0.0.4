using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Utils.UI
{
    public class ClearValues
    {
        public static void ClearTextBoxes(params DesignUI.CuzUI.CuzTextBox[] textBoxes)
        {
            foreach (var textBox in textBoxes)
            {
                if (textBox != null) textBox.Text = string.Empty;
            }
        }
    }
}
