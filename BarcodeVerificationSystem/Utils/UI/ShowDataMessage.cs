using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Utils.UI
{
    public class ShowDataMessage
    {
        public static void ShowJson(object payload)
        {
            var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
            if (Application.OpenForms["JsonViewerForm"] is Form existing)
            {
                existing.BringToFront();
                if (existing.WindowState == FormWindowState.Minimized)
                    existing.WindowState = FormWindowState.Normal;
                ((TextBox)existing.Controls[0]).Text = json;
                return;
            }

            // Run on UI thread if needed
            var threadOperation = new Action(() =>
            {
                var f = new Form
                {
                    Text = "JSON Payload (Copyable)",
                    Width = 800,
                    Height = 600,
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.SizableToolWindow,
                    TopMost = true // optional: keep on top while debugging
                };

                var tb = new TextBox
                {
                    Multiline = true,
                    ReadOnly = false,
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Both,
                    Font = new Font("Consolas", 10F),
                    Text = json,
                    WordWrap = false
                };

                // Select all + copy shortcut
                tb.KeyDown += (s, e) =>
                {
                    if (e.Control && e.KeyCode == Keys.A) tb.SelectAll();
                    if (e.Control && e.KeyCode == Keys.C) Clipboard.SetText(tb.SelectedText.Length > 0 ? tb.SelectedText : tb.Text);
                };

                f.Controls.Add(tb);
                f.FormClosed += (s, e) => f.Dispose();
                f.Show(); // non-modal but safe
            });

            if (Application.OpenForms.Cast<Form>().Any())
            {
                var mainForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.IsHandleCreated && x != null);
                if (mainForm != null && mainForm.InvokeRequired)
                    mainForm.Invoke(threadOperation);
                else
                    threadOperation();
            }
            else
            {
                // Rare case: no forms yet
                threadOperation();
            }
        }

        public static void SavePayloadToFile(object payload, string fileName = "Data.txt")
        {
            string folderPath = @"C:\ProgramData\R-Link";
            string fullPath = Path.Combine(folderPath, fileName);

            try
            {
                string json = JsonConvert.SerializeObject(payload, Formatting.Indented);

                // Optional: Add timestamp header for easier debugging
                string content = $"{json}";

                // Write to file (overwrite by default)
                File.WriteAllText(fullPath, content, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Fallback: show error in JSON viewer or MessageBox
                var errorPayload = new
                {
                    Error = "Failed to save payload to file",
                    FilePath = fullPath,
                    Exception = ex.Message,
                    StackTrace = ex.StackTrace,
                    OriginalPayload = payload
                };
 
            }
        }

    }
}
