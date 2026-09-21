using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BarcodeVerificationSystem.Utils
{
    internal class ResetValues
    {
        public static void SetTextsEmpty(params Control[] controls)
        {
            if (controls == null) throw new ArgumentNullException(nameof(controls));
            foreach (var control in controls)
            {
                if (control != null) control.Text = "";
            }
        }
    }
}
