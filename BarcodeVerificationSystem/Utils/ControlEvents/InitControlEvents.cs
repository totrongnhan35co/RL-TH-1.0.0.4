using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Utils.ControlEvents
{
    public static class InitControlEvents
    {
        public static void InitEvent(EventHandler handler, params Control[] controls)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (controls == null) throw new ArgumentNullException(nameof(controls));

            foreach (var control in controls)
            {
                if (control == null) continue;

                switch (control)
                {
                    case RadioButton rb:
                        rb.CheckedChanged += handler;
                        break;

                    case ComboBox cb:
                        cb.SelectedIndexChanged += handler;
                        break;

                    case TextBox tb:
                        tb.TextChanged += handler;
                        break;

                    case NumericUpDown nud:
                        nud.ValueChanged += handler;
                        break;

                        // add more control types if needed
                }
            }
        }
        public static void RegisterNumericControls(EventHandler valueChanged, KeyEventHandler keyUp, params NumericUpDown[] controls)
        {
            foreach (var nud in controls)
            {
                if (valueChanged != null)
                    nud.ValueChanged += valueChanged;
                if (keyUp != null)
                    nud.KeyUp += keyUp;
            }
        }

        public static void RegisterNumericKeyUpOnlyControls(KeyEventHandler keyUp, params NumericUpDown[] controls)
        {
            foreach (var nud in controls)
            {
                if (keyUp != null)
                    nud.KeyUp += keyUp;
            }
        }

        public static void RegisterTextBoxWithKeyUpControls(EventHandler textChanged, KeyEventHandler keyUp, params TextBox[] controls)
        {
            foreach (var tb in controls)
            {
                if (textChanged != null)
                    tb.TextChanged += textChanged;
                if (keyUp != null)
                    tb.KeyUp += keyUp;
            }
        }
        public static void RegisterTextBoxControls(EventHandler textChanged, params TextBox[] controls)
        {
            foreach (var tb in controls)
            {
                if (textChanged != null)
                    tb.TextChanged += textChanged;
            }
        }

        public static void RegisterComboBoxControls(EventHandler selectedIndexChanged, params ComboBox[] controls)
        {
            foreach (var cb in controls)
            {
                if (selectedIndexChanged != null)
                    cb.SelectedIndexChanged += selectedIndexChanged;
            }
        }

        public static void RegisterRadioButtonControls(EventHandler checkedChanged, EventHandler adjustHandler, params RadioButton[] controls)
        {
            foreach (var rb in controls)
            {
                if (checkedChanged != null)
                    rb.CheckedChanged += checkedChanged;
                if (adjustHandler != null)
                    rb.CheckedChanged += adjustHandler;
            }
        }
        public static void RegisterRadioButtonControls(EventHandler adjustHandler, params RadioButton[] controls)
        {
            foreach (var rb in controls)
            {
                if (adjustHandler != null)
                    rb.CheckedChanged += adjustHandler;
            }
        }

        public static void RegisterButtonControls(EventHandler click, params Button[] controls)
        {
            foreach (var btn in controls)
            {
                if (click != null)
                    btn.Click += click;
            }
        }

        public static void RegisterCheckBoxControls(EventHandler checkedChanged, params CheckBox[] controls)
        {
            foreach (var chk in controls)
            {
                if (checkedChanged != null)
                    chk.CheckedChanged += checkedChanged;
            }
        }

    }
}
