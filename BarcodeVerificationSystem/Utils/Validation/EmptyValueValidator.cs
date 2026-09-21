using BarcodeVerificationSystem.View.CustomDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Utils
{
    internal class EmptyValueValidator
    {

        public static bool CheckRequiredFields(params Control[] controls)
        {
            foreach (var control in controls)
            {
                if (string.IsNullOrWhiteSpace(control.Text))
                {
                    CustomMessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Điền thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    control.Focus();
                    return false;
                }
            }
            return true;
        }
        //public static bool CheckRequiredFields(params TextBox[] controls)
        //{
        //    foreach (var control in controls)
        //    {
        //        if (string.IsNullOrWhiteSpace(control.Text))
        //        {
        //            MessageBox.Show("Please input all info", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            control.Focus();
        //            return false;
        //        }
        //    }
        //    return true;
        //}
    }
}
