using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.BouncyCastle.Asn1.Crmf;

namespace BarcodeVerificationSystem.Utils
{
    internal class CreateScrollablePanel
    {

        private object _currentItem;
        private int _curentStartX = 10;
        private int _curentStartY = 10;

        public Panel CreatePanel(int locationX, int locationY, int width, int height, Control.ControlCollection control)
        {
            Panel scrollPanel;
            // Create scrollable panel
            scrollPanel = new Panel();
            scrollPanel.Location = new System.Drawing.Point(locationX, locationY);
            scrollPanel.Size = new System.Drawing.Size(width, height);
            scrollPanel.AutoScroll = true;
            scrollPanel.BorderStyle = BorderStyle.FixedSingle;

            control.Add(scrollPanel);
            return scrollPanel;
        }

        public PropertyInfo[] GetPropertyInfos(object item)
        {
            return item.GetType().GetProperties();
        }

        public void CreateTextBoxes(object item, string itemName , Panel scrollPanel)
        {
            _currentItem = item; // Store the current item for later use
            PropertyInfo[]  propertyInfo = item.GetType().GetProperties();
            int count = itemName == "payload" ? propertyInfo.Count()-1 : propertyInfo.Count();

            int startX = _curentStartX;
            int startY = _curentStartY;
            int spacing = 60; // increase to account for label height
            int width = scrollPanel.Width - 40; // account for padding

            for (int i = 0; i < count; i++)
            {
                // Get property values
                string originalName = propertyInfo[i].Name;
                string itemValue = propertyInfo[i].GetValue(item)?.ToString() ?? string.Empty;
                string displayName = ConvertToFriendlyName(originalName);

                // Create and configure label
                Label label = new Label
                {
                    Text = displayName,
                    Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular),
                    ForeColor = Color.DimGray,
                    AutoSize = true,
                    Location = new Point(startX, startY + i * spacing)
                };
                scrollPanel.Controls.Add(label);

                // Create and configure CuzTextBox
                var textBox = new DesignUI.CuzUI.CuzTextBox
                {
                    _ReadOnlyBackColor = Color.WhiteSmoke,
                    _ReadOnlyBorderFocusColor = Color.Gainsboro,
                    BackColor = Color.White,
                    BorderColor = SystemColors.ScrollBar,
                    BorderFocusColor = Color.Silver,
                    BorderRadius = 6,
                    BorderSize = 1,
                    Font = new Font("Microsoft Sans Serif", 12F),
                    ForeColor = Color.FromArgb(64, 64, 64),
                    Multiline = false,
                    Name = originalName,
                    Padding = new Padding(10, 7, 10, 7),
                    PasswordChar = false,
                    PlaceholderColor = Color.DarkGray,
                    PlaceholderText = "",
                    ReadOnly = false,
                    Size = new Size(width, 35),
                    TabIndex = 121,
                    UnderlinedStyle = false,
                    Location = new Point(startX, startY + i * spacing + 20) // below label
                };

                // Set text value after all styling
                textBox.Text = itemValue;

                // Optional: handle text changed
                textBox.TextChanged += TextBox_TextChanged;

                // Add to panel
                scrollPanel.Controls.Add(textBox);

            }
            _curentStartX = startX;
            _curentStartY = startY + count * spacing + 20; // Update the Y position for the next set of controls
        }

        //public void TextBox_TextChanged(object sender, EventArgs e)
        //{
        //    Control control = sender as Control;

        //    // Traverse upward until you find the parent CuzTextBox
        //    while (control != null && control.GetType().Name != "CuzTextBox")
        //    {
        //        control = control.Parent;
        //    }

        //    if (control is DesignUI.CuzUI.CuzTextBox txt)
        //    {
        //        Debug.WriteLine($"{txt.Name} changed: {txt.Text}");
        //    }

        //}


        private void TextBox_TextChanged(object sender, EventArgs e)
       {
            Control control = sender as Control;

            // Traverse upward until you find the parent CuzTextBox
            while (control != null && control.GetType().Name != "CuzTextBox")
            {
                control = control.Parent;
            }

            if (control is DesignUI.CuzUI.CuzTextBox textBox)
            {
                if (_currentItem == null)
                {
                    Debug.WriteLine("Current item is null in TextBox_TextChanged");
                    return;
                }

                string propertyName = textBox.Name;
                PropertyInfo propertyInfo = _currentItem.GetType().GetProperty(propertyName);
                if (propertyInfo == null)
                {
                    Debug.WriteLine($"Property {propertyName} not found on item");
                    return;
                }

                try
                {
                    object convertedValue;
                    Type propertyType = propertyInfo.PropertyType;

                    if (propertyType == typeof(DateTime) && DateTime.TryParse(textBox.Text, out var dateValue))
                    {
                        convertedValue = dateValue;
                    }
                    else if (propertyType == typeof(int) && int.TryParse(textBox.Text, out var intValue))
                    {
                        convertedValue = intValue;
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(textBox.Text, propertyType);
                    }

                    propertyInfo.SetValue(_currentItem, convertedValue);
                    Debug.WriteLine($"{textBox.Name} changed: {textBox.Text}, updated item property");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating property {propertyName}: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine("Sender or its parent is not a CuzTextBox");
            }
        }

        public string ConvertToFriendlyName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return string.Empty;

            string[] parts = propertyName.Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
                }
            }

            return string.Join(" ", parts);
        }


    }
}
