using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Utils.UI
{

    public static class InputBox
    {
        public static string Show(string title, string prompt)
        {
            Form form = new Form
            {
                Width = 450,
                Height = 220,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                BackColor = Color.White,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label label = new Label
            {
                Left = 20,
                Top = 25,
                Width = 390,
                Text = prompt,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Black
            };

            TextBox textBox = new TextBox
            {
                Left = 20,
                Top = 65,
                Width = 390,
                Font = new Font("Segoe UI", 11, FontStyle.Regular)
            };

            Button buttonOk = new Button
            {
                Text = "OK",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Left = 220,
                Width = 85,
                Top = 120,
                Height = 35,
                DialogResult = DialogResult.OK
            };
            buttonOk.FlatAppearance.BorderSize = 0;

            Button buttonCancel = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Left = 325,
                Width = 85,
                Top = 120,
                Height = 35,
                DialogResult = DialogResult.Cancel
            };
            buttonCancel.FlatAppearance.BorderSize = 0;

            form.Controls.Add(label);
            form.Controls.Add(textBox);
            form.Controls.Add(buttonOk);
            form.Controls.Add(buttonCancel);

            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : null;
        }
    }


}
