using DesignUI.CuzMesageBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View.CustomDialogs
{

    public static class CustomMessageBox
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public static DialogResult Show(IWin32Window owner, string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            FormMessageBox formMessageBox = new FormMessageBox(text, title, buttons, icon);

      
            formMessageBox.Text = title;

            formMessageBox.Load += (s, e) =>
            {
                SetWindowPos(formMessageBox.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            };

         
            return formMessageBox.ShowDialog(owner);
        }

     
        public static DialogResult Show(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
         
            Form activeForm = Form.ActiveForm;
            return Show(activeForm, text, title, buttons, icon);
        }

        public static bool IsResultShow(string text)
        {
            
            var result = Show(text, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        public static DialogResult ShowCenterScreen(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            FormMessageBox formMessageBox = new FormMessageBox(text, title, buttons, icon);
            formMessageBox.Text = title;
            formMessageBox.StartPosition = FormStartPosition.CenterScreen;
            formMessageBox.Load += (s, e) =>
            {
                SetWindowPos(formMessageBox.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            };
            return formMessageBox.ShowDialog();
        }

    }
}
