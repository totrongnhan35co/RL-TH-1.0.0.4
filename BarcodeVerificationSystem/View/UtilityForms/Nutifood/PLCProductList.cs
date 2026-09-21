using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.View.CustomDialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.UtilityForms.Nutifood
{
    public partial class PLCProductList : Form
    {
        private const int SLOT_COUNT = 20;
        private readonly Panel[] _slots = new Panel[SLOT_COUNT];
        private int _selectedIndex = -1;
        private bool isRightDirection = false;
        private TaskCompletionSource<bool> _responseWaitSource;
        private string _expectedResponse;
        private bool _responseReceived = false;
        private System.Windows.Forms.Timer _autoReloadTimer;

        public PLCProductList()
        {
            InitializeComponent();
            InitializeSlotUI();
            InitEvents();
            InitializeAutoReloadTimer();
            RefreshSlotDisplay();
            SendRequestBoxes();
        }

        private void InitializeAutoReloadTimer()
        {
            _autoReloadTimer = new System.Windows.Forms.Timer();
            _autoReloadTimer.Tick += AutoReloadTimer_Tick;
            _autoReloadTimer.Interval = 1000; // Default 1 second, will be updated based on user input
        }

        private void InitEvents()
        {
            //Shared.OnRepeatTCPMessageChange += Shared_OnRepeatTCPMessageChange;
            Shared.SensorController.OnPOD2ReceiveMessageEvent += Shared_OnRepeatTCPMessageChange;
        }
        private void InitializeSlotUI()
        {
            flowLayoutPanelSlots.Controls.Clear();
            flowLayoutPanelSlots.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanelSlots.WrapContents = true;
            flowLayoutPanelSlots.AutoSize = true;
            flowLayoutPanelSlots.Padding = new Padding(10);
            flowLayoutPanelSlots.Margin = new Padding(10);

            for (int i = 0; i < SLOT_COUNT; i++)
            {
                var slot = new Panel
                {
                    Size = new Size(50, 50),
                    Margin = new Padding(8),
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = i,
                    BackColor = Color.LightGray,
                    Cursor = Cursors.Hand
                };

                var label = new Label
                {
                    Text = (i+1).ToString("D2"), // Shows 00, 01, ..., 19
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };

                slot.Controls.Add(label);
                label.Click += Slot_Click;

                _slots[i] = slot;
                flowLayoutPanelSlots.Controls.Add(slot);
            }

            UpdateSlotSelection(); // Initial no selection
        }


        private void Slot_Click(object sender, EventArgs e)
        {
            Panel slot = null;

            if (sender is Panel p)
                slot = p;
            else if (sender is Label lbl && lbl.Parent is Panel parentPanel)
                slot = parentPanel;

            if (slot == null)
                return;

            int index = (int)slot.Tag;
            _selectedIndex = index;

            //MessageBox.Show("Selected index: " + index);
            UpdateSlotSelection();
        }

        private void UpdateSlotSelection()
        {
            for (int i = 0; i < SLOT_COUNT; i++)
            {
                if (i == _selectedIndex)
                    _slots[i].BackColor = Color.DodgerBlue;
                else
                    _slots[i].BackColor = Color.LightGray;
            }
        }

        private void UpdateSlotsFromMessage(string plcMessage)
        {
            if (string.IsNullOrEmpty(plcMessage) || plcMessage.Length < SLOT_COUNT)
                return;

            for (int i = 0; i < SLOT_COUNT; i++)
            {
                char digit = plcMessage[i];
                var slot = _slots[i];

                switch (digit)
                {
                    case '3': // Valid
                        slot.BackColor = Color.LimeGreen;
                        break;
                    case '5': // Invalid
                        slot.BackColor = Color.IndianRed;
                        break;
                    case '7': // Destroyed
                        slot.BackColor = Color.Gray;
                        break;
                    case '0': // Empty
                    default:
                        slot.BackColor = Color.FromArgb(240, 240, 240); // Light gray
                        break;
                }
            }

        }

        private void Shared_OnRepeatTCPMessageChange(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Shared_OnRepeatTCPMessageChange(sender, e)));
                return;
            }

            if (sender is string message)
            {
                richTextBox1.Text = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}: {message}\n" + richTextBox1.Text;
                richTextBox1.SelectionStart = 0;
                richTextBox1.ScrollToCaret();

                // Check if this is the expected response for command confirmation
                if (_responseWaitSource != null && !string.IsNullOrEmpty(_expectedResponse) && !_responseWaitSource.Task.IsCompleted)
                {
                    if (message.Equals(_expectedResponse, StringComparison.OrdinalIgnoreCase))
                    {
                        _responseReceived = true; // Mark that we actually received the response
                        _responseWaitSource.TrySetResult(true);
                        // Don't clear here - let btnSend_Click clean up in finally block
                    }
                }

                // Extract 20 digits from message format: (QUEUESTATUS33335553300000000000)
                string digits = ExtractDigitsFromMessage(message);
                if (!string.IsNullOrEmpty(digits) && digits.Length >= SLOT_COUNT)
                {
                    UpdateSlotsFromMessage(digits.Substring(0, SLOT_COUNT));
                }
            }
        }

        private string ExtractDigitsFromMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return null;

            // Look for QUEUESTATUS followed by digits
            const string prefix = "QUEUESTATUS";
            int prefixIndex = message.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            
            if (prefixIndex >= 0)
            {
                // Extract the part after QUEUESTATUS
                int startIndex = prefixIndex + prefix.Length;
                if (startIndex < message.Length)
                {
                    string remaining = message.Substring(startIndex);
                    // Extract only digits (0-9)
                    string digits = Regex.Match(remaining, @"\d+").Value;
                    return digits;
                }
            }

            // Fallback: if message is already 20+ digits, return first 20
            if (message.Length >= SLOT_COUNT && Regex.IsMatch(message, @"^\d+$"))
            {
                return message.Substring(0, SLOT_COUNT);
            }

            return null;
        }

        private void SyncLoading_Click(object sender, EventArgs e)
        {
            DirectionIcon.Image = !isRightDirection ? Properties.Resources.icons8_arrow_left_1 : Properties.Resources.icons8_arrow_right;
            SensorIcon.Image = !isRightDirection ? Properties.Resources.Cognex_Small : Properties.Resources.Sensor_Small_1;
            CameraIcon.Image = !isRightDirection ? Properties.Resources.Sensor_Small_1 : Properties.Resources.Cognex_Small;
            
            isRightDirection = !isRightDirection;
            RefreshSlotDisplay();
            SendRequestBoxes();
        }

        private void RefreshSlotDisplay()
        {
            flowLayoutPanelSlots.Controls.Clear();

            if (isRightDirection)
            {
                // Normal order: 00 → 19
                for (int i = 0; i < SLOT_COUNT; i++)
                    flowLayoutPanelSlots.Controls.Add(_slots[i]);
            }
            else
            {
                // Reversed order: 19 → 00
                for (int i = SLOT_COUNT - 1; i >= 0; i--)
                    flowLayoutPanelSlots.Controls.Add(_slots[i]);
            }

            UpdateSlotSelection(); // Restore selection highlight
        }

        private void Reload_Click(object sender, EventArgs e)
        {
            SendRequestBoxes();
        }

        private async void btnSendSlot_Click(object sender, EventArgs e)
        {

            if (_selectedIndex >= 0 && _selectedIndex < SLOT_COUNT)
            {
                int indexToSend = _selectedIndex;

                string formattedIndex = indexToSend.ToString("D2"); // "D2" = decimal, minimum 2 digits, padded with zeros
                string command = $"(CANCELI000{formattedIndex})";
                _expectedResponse = command;
                _responseReceived = false; // Reset flag

                // Create a new TaskCompletionSource for this request
                _responseWaitSource = new TaskCompletionSource<bool>();

                // Send the command
                Shared.SendCommandToSensor2Controller(command);

                try
                {
                    // Wait for response with 3 second timeout
                    var timeoutTask = Task.Delay(3000);
                    var completedTask = await Task.WhenAny(_responseWaitSource.Task, timeoutTask);

                    // Default to failure - only set to true if we explicitly receive matching response
                    bool receivedResponse = false;

                    // Only check result if response task completed BEFORE timeout AND we actually received the response
                    if (completedTask == _responseWaitSource.Task &&
                        _responseWaitSource.Task.IsCompleted &&
                        _responseReceived) // Additional check: ensure flag was set
                    {
                        // Response task completed first - verify it completed successfully with result true
                        try
                        {
                            // Double-check: task must be completed successfully AND result must be true
                            if (_responseWaitSource.Task.Status == TaskStatus.RanToCompletion)
                            {
                                bool result = _responseWaitSource.Task.Result;
                                receivedResponse = result == true && _responseReceived; // Explicit comparison with flag
                            }
                        }
                        catch
                        {
                            // Task completed but with exception or cancellation - treat as failure
                            receivedResponse = false;
                        }
                    }
                    // If timeoutTask completed first, receivedResponse remains false

                    if (receivedResponse)
                    {
                        // Success: received matching response
                        CustomMessageBox.Show($"Gửi vị trí loại thùng thành công", "Gửi lệnh",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SendRequestBoxes();
                    }
                    else
                    {
                        // Timeout: no matching response received
                        CustomMessageBox.Show($"Gửi vị trí loại thùng thất bại. Không nhận được phản hồi từ PLC.", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Lỗi khi gửi lệnh: {ex.Message}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Clean up
                    _responseWaitSource = null;
                    _expectedResponse = null;
                    _responseReceived = false;
                }
            }
            else
            {
                CustomMessageBox.Show("Vui lòng chọn vị trí thùng!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            //if (_selectedIndex >= 0 && _selectedIndex < SLOT_COUNT)
            //{
            //    int indexToSend = _selectedIndex; // or +1 if 1-based

            //    string formattedIndex = indexToSend.ToString("D2"); // "D2" = decimal, minimum 2 digits, padded with zeros

            //    Shared.SendCommandToSensor2Controller($"(CANCELI000{formattedIndex})");

            //    CustomMessageBox.Show($"Gửi vị trí loại thùng thành công", "Gửi lệnh",
            //        MessageBoxButtons.OK, MessageBoxIcon.Information);

            //    SendRequestBoxes();
            //}
            //else
            //{
            //    CustomMessageBox.Show("Vui lòng chọn vị trí thùng!", "Thông báo",
            //        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
        }

        private void btnReloadUI_Click(object sender, EventArgs e)
        {
            SendRequestBoxes();
        }

        private void SendRequestBoxes()
        {
            Shared.SendCommandToSensor2Controller("(QUEUEREQUEST)");
        }

        private void AutoReloadTimer_Tick(object sender, EventArgs e)
        {
            // Auto-reload when timer ticks
            SendRequestBoxes();
        }

        private void chkAutoReload_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoReload.Checked)
            {
                // Start auto-reload
                int intervalSeconds = (int)numAutoReloadInterval.Value;
                if (intervalSeconds > 0)
                {
                    _autoReloadTimer.Interval = intervalSeconds * 1000; // Convert seconds to milliseconds
                    _autoReloadTimer.Start();
                }
                else
                {
                    // Invalid interval, uncheck and show warning
                    chkAutoReload.Checked = false;
                    CustomMessageBox.Show("Vui lòng nhập thời gian lớn hơn 0 giây!", "Cảnh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                // Stop auto-reload
                _autoReloadTimer.Stop();
            }
            btnSendSlot.Enabled = !chkAutoReload.Checked;
        }

        private void numAutoReloadInterval_ValueChanged(object sender, EventArgs e)
        {
            // Update timer interval if auto-reload is currently enabled
            if (chkAutoReload.Checked && numAutoReloadInterval.Value > 0)
            {
                _autoReloadTimer.Interval = (int)numAutoReloadInterval.Value * 1000;
                // Restart timer with new interval
                _autoReloadTimer.Stop();
                _autoReloadTimer.Start();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up timer when form closes
            if (_autoReloadTimer != null)
            {
                _autoReloadTimer.Stop();
                _autoReloadTimer.Dispose();
            }
            base.OnFormClosing(e);
        }
    }
}

