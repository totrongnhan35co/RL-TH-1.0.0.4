using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Controller.ZebraPrinter;
using BarcodeVerificationSystem.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.UcSettings
{
    public partial class ucZebraPrinterSetting : UserControl
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private bool isConnected = false;
        private System.Windows.Forms.Timer keepAliveTimer;
        PrinterModel _PrinterModel = Shared.Settings.ZebraPrinter;

        // ── Helper property ────────────────────────────────────────────
        private bool IsUsbMode => radUSB != null && radUSB.Checked;

        public ucZebraPrinterSetting()
        {
            InitializeComponent();
            cmbBarcodeType.SelectedIndex = 0;
            cmbTextFont.SelectedIndex = 0;
            UpdateConnectionStatus();
            LogMessage("Application started. Ready to connect to printer.");

            keepAliveTimer = new System.Windows.Forms.Timer();
            keepAliveTimer.Interval = 30000;
            keepAliveTimer.Tick += KeepAlive_Tick;
            InitEvents();
            InitControls();
            this.Load += UcZebraPrinterSetting_Load;
        }

        private void UcZebraPrinterSetting_Load(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                // Điều chỉnh vị trí IP+Port (bên dưới radio buttons)
                txtPrinterIP.SetBounds(14, 86, 220, 26);
                txtPrinterIP.MinimumSize = new Size(160, 26);

                numPrinterPort.Left = txtPrinterIP.Right + 40;
                numPrinterPort.Top = txtPrinterIP.Top;
                numPrinterPort.Height = txtPrinterIP.Height;
                numPrinterPort.SetBounds(numPrinterPort.Left, txtPrinterIP.Top, numPrinterPort.Width, txtPrinterIP.Height);

                int gapLabelToInput = 4;
                lblIPrinterIP.Left = txtPrinterIP.Left;
                lblIPrinterIP.Top = txtPrinterIP.Top - lblIPrinterIP.Height - gapLabelToInput;
                lblPrinterPort.Left = numPrinterPort.Left;
                lblPrinterPort.Top = numPrinterPort.Top - lblPrinterPort.Height - gapLabelToInput;

                // Căn thẳng hàng nút Làm mới theo chiều dọc với ComboBox
                btnRefreshPrinters.Top = cmbUsbPrinterName.Top
                    + (cmbUsbPrinterName.Height - btnRefreshPrinters.Height) / 2;

                txtPrinterIP.Invalidate();
                txtPrinterIP.Update();
                txtPrinterIP.Refresh();

                if (txtPrinterIP.Parent != null)
                {
                    txtPrinterIP.Parent.Invalidate();
                    txtPrinterIP.Parent.Update();
                }

                // Áp dụng chế độ kết nối đã lưu
                ToggleConnectionPanel();
            }));
        }

        private bool _IsBinding = false;

        private void InitControls()
        {
            _IsBinding = true;

            txtPrinterIP.Width = 220;
            txtPrinterIP.MinimumSize = new Size(160, 26);
            numBarcodeModuleWidth.Value = _PrinterModel.ZebraSettings.BarcodeModuleWidth;
            numLabelLength.Value = _PrinterModel.ZebraSettings.LabelLength;

            txtPrinterIP.Invalidate();
            txtPrinterIP.Update();
            txtPrinterIP.Refresh();

            // Connection Settings
            txtPrinterIP.Text = _PrinterModel.IP ?? "127.0.0.1";
            numPrinterPort.Value = _PrinterModel.Port > 0 ? _PrinterModel.Port : 9100;

            // ── Chế độ kết nối LAN / USB ──────────────────────────────
            bool isLan = _PrinterModel.ZebraSettings.ConnectionType == ZebraConnectionType.LAN;
            radLAN.Checked = isLan;
            radUSB.Checked = !isLan;
            RefreshUsbPrinterList();
            string savedUsb = _PrinterModel.ZebraSettings.UsbPrinterName;
            if (!string.IsNullOrEmpty(savedUsb))
            {
                int idx = cmbUsbPrinterName.Items.IndexOf(savedUsb);
                if (idx >= 0) cmbUsbPrinterName.SelectedIndex = idx;
            }

            // Text Settings
            chkEnableText.Checked = _PrinterModel.ZebraSettings.EnableText;
            if (cmbTextFont.Items.Count > 0)
            {
                int fontIndex = cmbTextFont.Items.IndexOf(_PrinterModel.ZebraSettings.TextFont);
                cmbTextFont.SelectedIndex = fontIndex >= 0 ? fontIndex : 0;
            }
            txtTextContent.Text = _PrinterModel.ZebraSettings.TextContent ?? "Test QR Code";
            txtTextX.Text = _PrinterModel.ZebraSettings.TextX.ToString();
            txtTextY.Text = _PrinterModel.ZebraSettings.TextY.ToString();
            txtTextFontSize.Text = _PrinterModel.ZebraSettings.TextFontSize.ToString();

            // Barcode Settings
            chkEnableBarcode.Checked = _PrinterModel.ZebraSettings.EnableBarcode;
            if (cmbBarcodeType.Items.Count > 0)
            {
                int barcodeIndex = cmbBarcodeType.Items.IndexOf(_PrinterModel.ZebraSettings.BarcodeType);
                cmbBarcodeType.SelectedIndex = barcodeIndex >= 0 ? barcodeIndex : 0;
            }
            txtBarcodeData.Text = _PrinterModel.ZebraSettings.BarcodeData ?? "https://x.ai";
            txtBarcodeX.Text = _PrinterModel.ZebraSettings.BarcodeX.ToString();
            txtBarcodeY.Text = _PrinterModel.ZebraSettings.BarcodeY.ToString();
            txtBarcodeSize.Text = _PrinterModel.ZebraSettings.BarcodeSize.ToString();
            txtBarcodeText.Text = _PrinterModel.ZebraSettings.BarcodeText ?? "";

            _IsBinding = false;
        }

        private void InitEvents()
        {
            // ── LAN / USB selection ────────────────────────────────────
            radLAN.CheckedChanged += RadConnectionType_CheckedChanged;
            radUSB.CheckedChanged += RadConnectionType_CheckedChanged;
            cmbUsbPrinterName.SelectedIndexChanged += AdjustData;
            btnRefreshPrinters.Click += (s, e) => RefreshUsbPrinterList();

            // Connection Settings (LAN)
            txtPrinterIP.TextChanged += AdjustData;
            numPrinterPort.ValueChanged += AdjustData;

            // Text Settings
            chkEnableText.CheckedChanged += AdjustData;
            cmbTextFont.SelectedIndexChanged += AdjustData;
            txtTextContent.TextChanged += AdjustData;
            txtTextX.TextChanged += AdjustData;
            txtTextY.TextChanged += AdjustData;
            txtTextFontSize.TextChanged += AdjustData;
            numBarcodeModuleWidth.ValueChanged += AdjustData;
            numLabelLength.ValueChanged += AdjustData;

            // Barcode Settings
            chkEnableBarcode.CheckedChanged += AdjustData;
            cmbBarcodeType.SelectedIndexChanged += AdjustData;
            txtBarcodeData.TextChanged += AdjustData;
            txtBarcodeX.TextChanged += AdjustData;
            txtBarcodeY.TextChanged += AdjustData;
            txtBarcodeSize.TextChanged += AdjustData;
            txtBarcodeText.TextChanged += AdjustData;
        }

        // ── Xử lý chuyển đổi LAN ↔ USB ────────────────────────────────
        private void RadConnectionType_CheckedChanged(object sender, EventArgs e)
        {
            if (_IsBinding) return;
            _PrinterModel.ZebraSettings.ConnectionType = radUSB.Checked
                ? ZebraConnectionType.USB
                : ZebraConnectionType.LAN;
            Shared.SaveSettings();
            ToggleConnectionPanel();
        }

        private void ToggleConnectionPanel()
        {
            bool isLan = radLAN.Checked;

            txtPrinterIP.Visible = isLan;
            numPrinterPort.Visible = isLan;
            lblIPrinterIP.Visible = isLan;
            lblPrinterPort.Visible = isLan;
            pnlUSB.Visible = !isLan;

            if (!isLan)
            {
                DisconnectTcp();
                string usbName = _PrinterModel.ZebraSettings.UsbPrinterName;
                isConnected = !string.IsNullOrEmpty(usbName)
                              && RawPrinterHelper.IsPrinterOnline(usbName);
                _PrinterModel.IsConnected = isConnected;
                Shared.RaiseOnZebraPrinterStatusChangeEvent();
                keepAliveTimer.Start();
            }
            else
            {
                keepAliveTimer.Stop();
                isConnected = false;
            }

            UpdateConnectionStatus();
        }
        private void KeepAlive_Tick(object sender, EventArgs e)
        {
            if (!IsUsbMode) return;

            string usbName = _PrinterModel.ZebraSettings.UsbPrinterName;
            if (string.IsNullOrEmpty(usbName)) return;

            bool online = RawPrinterHelper.IsPrinterOnline(usbName);
            if (online == isConnected) return;   // không đổi → bỏ qua

            isConnected = online;
            _PrinterModel.IsConnected = online;
            Shared.RaiseOnZebraPrinterStatusChangeEvent();   // → lblStatusPrinterZebra đổi icon
        }
        private void RefreshUsbPrinterList()
        {
            string current = cmbUsbPrinterName.SelectedItem?.ToString()
                             ?? _PrinterModel.ZebraSettings.UsbPrinterName;
            _IsBinding = true;
            cmbUsbPrinterName.Items.Clear();
            foreach (string printer in PrinterSettings.InstalledPrinters)
                cmbUsbPrinterName.Items.Add(printer);
            _IsBinding = false;

            int idx = cmbUsbPrinterName.Items.IndexOf(current);
            if (idx >= 0)
                cmbUsbPrinterName.SelectedIndex = idx;
            else if (cmbUsbPrinterName.Items.Count > 0)
                cmbUsbPrinterName.SelectedIndex = 0;

            LogMessage($"Tìm thấy {cmbUsbPrinterName.Items.Count} máy in USB.");
        }

        private void AdjustData(object sender, EventArgs e)
        {
            if (_IsBinding) return;

            if (sender == cmbUsbPrinterName)
            {
                if (cmbUsbPrinterName.SelectedItem != null)
                {
                    _PrinterModel.ZebraSettings.UsbPrinterName = cmbUsbPrinterName.SelectedItem.ToString();
                    Shared.SaveSettings();
                }
                return;
            }

            if (sender == txtPrinterIP)
            {
                PrinterModel checkExist = Shared.Settings.ZebraPrinter;
                if (checkExist != null)
                {
                    checkExist.IP = txtPrinterIP.Text;
                    PODController checkExistPOD = Shared.Settings.ZebraPrinter.PODController;
                    checkExistPOD?.Disconnect();
                }
                _PrinterModel.IP = txtPrinterIP.Text;
            }
            else if (sender == numPrinterPort)
            {
                PrinterModel checkExist = Shared.Settings.ZebraPrinter;
                if (checkExist != null)
                {
                    checkExist.Port = (int)numPrinterPort.Value;
                    var checkExistPOD = Shared.Settings.ZebraPrinter.PODController;
                    checkExistPOD?.Disconnect();
                }
                _PrinterModel.Port = (int)numPrinterPort.Value;
            }
            else if (sender == numBarcodeModuleWidth)
            {
                int v = (int)numBarcodeModuleWidth.Value;
                if (v >= 1 && v <= 5)
                    _PrinterModel.ZebraSettings.BarcodeModuleWidth = v;
            }
            else if (sender == numLabelLength)
            {
                int v = (int)numLabelLength.Value;
                if (v >= 100 && v <= 2000)
                    _PrinterModel.ZebraSettings.LabelLength = v;
            }
            else if (sender == chkEnableText)
                _PrinterModel.ZebraSettings.EnableText = chkEnableText.Checked;
            else if (sender == cmbTextFont)
            {
                if (cmbTextFont.SelectedItem != null)
                    _PrinterModel.ZebraSettings.TextFont = cmbTextFont.SelectedItem.ToString();
            }
            else if (sender == txtTextContent)
                _PrinterModel.ZebraSettings.TextContent = txtTextContent.Text;
            else if (sender == txtTextX)
            {
                if (int.TryParse(txtTextX.Text, out int value))
                    _PrinterModel.ZebraSettings.TextX = value;
            }
            else if (sender == txtTextY)
            {
                if (int.TryParse(txtTextY.Text, out int value))
                    _PrinterModel.ZebraSettings.TextY = value;
            }
            else if (sender == txtTextFontSize)
            {
                if (int.TryParse(txtTextFontSize.Text, out int value))
                    _PrinterModel.ZebraSettings.TextFontSize = value;
            }
            else if (sender == chkEnableBarcode)
                _PrinterModel.ZebraSettings.EnableBarcode = chkEnableBarcode.Checked;
            else if (sender == cmbBarcodeType)
            {
                if (cmbBarcodeType.SelectedItem != null)
                    _PrinterModel.ZebraSettings.BarcodeType = cmbBarcodeType.SelectedItem.ToString();
            }
            else if (sender == txtBarcodeData)
                _PrinterModel.ZebraSettings.BarcodeData = txtBarcodeData.Text;
            else if (sender == txtBarcodeX)
            {
                if (int.TryParse(txtBarcodeX.Text, out int value))
                    _PrinterModel.ZebraSettings.BarcodeX = value;
            }
            else if (sender == txtBarcodeY)
            {
                if (int.TryParse(txtBarcodeY.Text, out int value))
                    _PrinterModel.ZebraSettings.BarcodeY = value;
            }
            else if (sender == txtBarcodeSize)
            {
                if (int.TryParse(txtBarcodeSize.Text, out int value))
                    _PrinterModel.ZebraSettings.BarcodeSize = value;
            }
            else if (sender == txtBarcodeText)
                _PrinterModel.ZebraSettings.BarcodeText = txtBarcodeText.Text;

            Shared.SaveSettings();
        }

        private void UpdateConnectionStatus()
        {
            if (IsUsbMode)
            {
                lblConnectionStatus.Text = isConnected ? "Status: USB Online" : "Status: USB Offline";
                lblConnectionStatus.ForeColor = isConnected ? Color.Green : Color.Red;
                return;
            }
            lblConnectionStatus.Text = isConnected ? "Status: Connected" : "Status: Disconnected";
            lblConnectionStatus.ForeColor = isConnected ? Color.Green : Color.Red;
        }

        private void LogMessage(string message)
        {
            if (txtStatusLog.InvokeRequired)
            {
                txtStatusLog.Invoke(new Action(() => LogMessage(message)));
                return;
            }
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            txtStatusLog.AppendText($"[{timestamp}] {message}\r\n");
            txtStatusLog.SelectionStart = txtStatusLog.Text.Length;
            txtStatusLog.ScrollToCaret();
        }

        // ── Kết nối (LAN hoặc USB) ────────────────────────────────────
        private bool ConnectToPrinter(bool silent = false)
        {
            // ── USB mode ──────────────────────────────────────────────
            if (IsUsbMode)
            {
                string usbName = _PrinterModel.ZebraSettings.UsbPrinterName;
                if (string.IsNullOrEmpty(usbName))
                {
                    if (!silent)
                        MessageBox.Show("Vui lòng chọn tên máy in USB.", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                bool online = RawPrinterHelper.IsPrinterOnline(usbName);
                isConnected = online;
                _PrinterModel.IsConnected = online;               // ← thêm
                Shared.RaiseOnZebraPrinterStatusChangeEvent();
                UpdateConnectionStatus();
                LogMessage(online
                    ? $"Máy in USB '{usbName}' đang sẵn sàng."
                    : $"Máy in USB '{usbName}' không tìm thấy hoặc offline.");
                return online;
            }

            // ── LAN mode ──────────────────────────────────────────────
            try
            {
                if (tcpClient != null || networkStream != null)
                    DisconnectTcp();

                string ipAddress = _PrinterModel.IP;
                if (string.IsNullOrEmpty(ipAddress))
                {
                    if (!silent)
                        MessageBox.Show("Vui lòng nhập địa chỉ IP.", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!int.TryParse(_PrinterModel.Port.ToString(), out int port)
                    || port < 1 || port > 65535)
                {
                    if (!silent)
                        MessageBox.Show("Cổng không hợp lệ (1–65535).", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                LogMessage(silent
                    ? $"Auto-reconnecting to {ipAddress}:{port}..."
                    : $"Attempting to connect to {ipAddress}:{port}...");

                tcpClient = new TcpClient();
                tcpClient.ReceiveTimeout = 5000;
                tcpClient.SendTimeout = 5000;
                tcpClient.Connect(ipAddress, port);
                networkStream = tcpClient.GetStream();
                isConnected = true;
                keepAliveTimer?.Start();
                UpdateConnectionStatus();

                LogMessage(silent
                    ? $"Auto-reconnected successfully to {ipAddress}:{port}"
                    : $"Successfully connected to {ipAddress}:{port}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Connection failed: {ex.Message}");
                if (!silent)
                    MessageBox.Show($"Kết nối thất bại: {ex.Message}", "Lỗi kết nối",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisconnectTcp();
                isConnected = false;
                UpdateConnectionStatus();
                return false;
            }
        }

        private void DisconnectTcp()
        {
            try
            {
                keepAliveTimer?.Stop();
                if (networkStream != null) { networkStream.Close(); networkStream = null; }
                if (tcpClient != null) { tcpClient.Close(); tcpClient = null; }
            }
            catch { }
        }

        private void DisconnectFromPrinter()
        {
            try
            {
                DisconnectTcp();
                isConnected = false;
                UpdateConnectionStatus();
                LogMessage(IsUsbMode ? "USB printer disconnected." : "Disconnected from printer.");
            }
            catch (Exception ex)
            {
                LogMessage($"Error during disconnect: {ex.Message}");
            }
        }

        private bool IsConnectionAlive()
        {
            try
            {
                if (tcpClient == null || networkStream == null || !isConnected) return false;
                if (!tcpClient.Connected) return false;
                if (!networkStream.CanWrite) return false;
                bool pollRead = tcpClient.Client.Poll(1000, SelectMode.SelectRead);
                bool pollError = tcpClient.Client.Poll(1000, SelectMode.SelectError);
                if (pollRead && tcpClient.Client.Available == 0) return false;
                if (pollError) return false;
                return true;
            }
            catch { return false; }
        }

        // ── Gửi ZPL (LAN hoặc USB) ───────────────────────────────────
        private bool SendZPLCommand(string zplCommand)
        {
            // ── USB mode ──────────────────────────────────────────────
            if (IsUsbMode)
            {
                string usbName = _PrinterModel.ZebraSettings.UsbPrinterName;
                if (string.IsNullOrEmpty(usbName))
                {
                    MessageBox.Show("Vui lòng chọn tên máy in USB.", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                LogMessage($"Gửi ZPL qua USB → '{usbName}':");
                LogMessage(zplCommand);
                bool result = RawPrinterHelper.SendStringToPrinter(usbName, zplCommand);
                LogMessage(result ? "Gửi USB thành công." : "Gửi USB thất bại.");
                isConnected = result;
                UpdateConnectionStatus();
                return result;
            }

            // ── LAN mode ──────────────────────────────────────────────
            if (!isConnected || networkStream == null || !IsConnectionAlive())
            {
                LogMessage("Mất kết nối. Đang thử kết nối lại...");
                if (!ConnectToPrinter(silent: true))
                {
                    MessageBox.Show("Chưa kết nối và không thể kết nối lại. Kiểm tra cài đặt.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            try
            {
                LogMessage("Sending ZPL command:");
                LogMessage(zplCommand);
                byte[] data = Encoding.UTF8.GetBytes(zplCommand);
                networkStream.Write(data, 0, data.Length);
                networkStream.Flush();
                LogMessage("ZPL command sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Error sending ZPL command: {ex.Message}");
                isConnected = false;
                UpdateConnectionStatus();
                LogMessage("Attempting to reconnect after send error...");
                if (ConnectToPrinter(silent: true))
                {
                    try
                    {
                        if (networkStream != null)
                        {
                            byte[] data = Encoding.UTF8.GetBytes(zplCommand);
                            networkStream.Write(data, 0, data.Length);
                            networkStream.Flush();
                            LogMessage("ZPL command sent successfully after reconnection.");
                            return true;
                        }
                    }
                    catch (Exception retryEx)
                    {
                        LogMessage($"Retry failed: {retryEx.Message}");
                    }
                }
                MessageBox.Show($"Gửi lệnh thất bại: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ── ZPL generation (không đổi) ────────────────────────────────
        private string GenerateZPLText(int x, int y, string font, int fontSize, string text)
        {
            char fontChar = font[0];
            return $"^FO{x},{y}^A{fontChar}N,{fontSize},{fontSize}^FD{text}^FS";
        }

        private string GenerateZPLBarcode(int x, int y, string barcodeType, int size,
            string data, string textLabel = null, int hriTextSize = 0)
        {
            int moduleWidth = _PrinterModel.ZebraSettings.BarcodeModuleWidth > 0
                ? _PrinterModel.ZebraSettings.BarcodeModuleWidth : 1;

            string byCmd = (barcodeType == "GS1-128" || barcodeType == "Code 128"
                || barcodeType == "Code 39" || barcodeType == "EAN-13"
                || barcodeType == "EAN-8" || barcodeType == "UPC-A"
                || barcodeType == "UPC-E")
                ? $"^BY{moduleWidth}" : "";

            string zpl = "";
            switch (barcodeType)
            {
                case "QR Code":
                    zpl = $"^FO{x},{y}^BQN,2,{size}^FDQA,{data}^FS";
                    break;
                case "DataMatrix":
                    zpl = $"^FO{x},{y}^BXN,{size},200^FD{data}^FS";
                    break;
                case "GS1-128":
                case "Code 128":
                    {
                        int barcodeHeight = size * 10;
                        string fdData = barcodeType == "GS1-128" ? $">8{data}" : data;
                        if (moduleWidth == 1)
                        {
                            int hriSize = hriTextSize > 0 ? hriTextSize : 28;
                            int hriY = y + barcodeHeight + 6;
                            string ai = data.Length >= 2 ? data.Substring(0, 2) : data;
                            string rest = data.Length > 2 ? data.Substring(2) : "";
                            string hriText = $"({ai}){rest}";
                            string manualHri = $"^FO{x},{hriY}^A0N,{hriSize},{hriSize}^FD{hriText}^FS";
                            zpl = $"^FO{x},{y}{byCmd}^BCN,{barcodeHeight},N,N,N^FD{fdData}^FS{manualHri}";
                        }
                        else
                            zpl = $"^FO{x},{y}{byCmd}^BCN,{barcodeHeight},Y,N,N^FD{fdData}^FS";
                        break;
                    }
                case "Code 39":
                    zpl = $"^FO{x},{y}{byCmd}^B3N,{size * 10},Y,N,N^FD{data}^FS";
                    break;
                case "EAN-13":
                    zpl = $"^FO{x},{y}{byCmd}^BEN,{size * 10},Y,N,N^FD{data}^FS";
                    break;
                case "EAN-8":
                    zpl = $"^FO{x},{y}{byCmd}^B8N,{size * 10},Y,N,N^FD{data}^FS";
                    break;
                case "UPC-A":
                    zpl = $"^FO{x},{y}{byCmd}^BUN,{size * 10},Y,N,N^FD{data}^FS";
                    break;
                case "UPC-E":
                    zpl = $"^FO{x},{y}{byCmd}^B9N,{size * 10},Y,N,N^FD{data}^FS";
                    break;
                default:
                    zpl = $"^FO{x},{y}^BQN,2,{size}^FDQA,{data}^FS";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(textLabel))
            {
                int labelY = y + (size * 20) + 10;
                zpl += $"\n^FO{x},{labelY}^A0N,20,20^FD{textLabel}^FS";
            }
            return zpl;
        }

        private string GetTextFont()
        {
            if (cmbTextFont.SelectedItem != null)
            {
                string selected = cmbTextFont.SelectedItem.ToString() ?? "A";
                return selected[0].ToString();
            }
            return "A";
        }

        // ── Button handlers ───────────────────────────────────────────
        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectToPrinter();
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectFromPrinter();
        }

        private void btnSendReady_Click(object sender, EventArgs e)
        {
            if (IsUsbMode)
            {
                // ~HS là lệnh LAN, không áp dụng cho USB — kiểm tra trạng thái trực tiếp
                UpdateConnectionStatus();
                LogMessage(isConnected
                    ? $"USB printer '{_PrinterModel.ZebraSettings.UsbPrinterName}' đang online."
                    : $"USB printer '{_PrinterModel.ZebraSettings.UsbPrinterName}' offline.");
                return;
            }
            SendZPLCommand("~HS\r\n");
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            var zplCommands = new List<string>();
            zplCommands.Add("^XA");
            zplCommands.Add($"^LL{_PrinterModel.ZebraSettings.LabelLength}");

            if (chkEnableText.Checked)
            {
                if (!int.TryParse(txtTextX.Text, out int textX)
                    || !int.TryParse(txtTextY.Text, out int textY)
                    || !int.TryParse(txtTextFontSize.Text, out int fontSize))
                {
                    MessageBox.Show("Vui lòng nhập giá trị số hợp lệ cho vị trí và cỡ chữ.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string text = txtTextContent.Text;
                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("Vui lòng nhập nội dung văn bản.", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                zplCommands.Add(GenerateZPLText(textX, textY, GetTextFont(), fontSize, text));
            }

            if (chkEnableBarcode.Checked)
            {
                if (!int.TryParse(txtBarcodeX.Text, out int barcodeX)
                    || !int.TryParse(txtBarcodeY.Text, out int barcodeY)
                    || !int.TryParse(txtBarcodeSize.Text, out int barcodeSize))
                {
                    MessageBox.Show("Vui lòng nhập giá trị số hợp lệ cho vị trí và kích thước mã vạch.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (barcodeSize < 1 || barcodeSize > 20)
                {
                    MessageBox.Show("Kích thước mã vạch phải từ 1 đến 20.", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string barcodeData = txtBarcodeData.Text;
                if (string.IsNullOrWhiteSpace(barcodeData))
                {
                    MessageBox.Show("Vui lòng nhập dữ liệu mã vạch.", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int.TryParse(txtTextFontSize.Text, out int hriSize);
                string barcodeType = cmbBarcodeType.SelectedItem?.ToString() ?? "QR Code";
                string barcodeText = string.IsNullOrWhiteSpace(txtBarcodeText.Text)
                    ? null : txtBarcodeText.Text;
                zplCommands.Add(GenerateZPLBarcode(barcodeX, barcodeY, barcodeType,
                    barcodeSize, barcodeData, barcodeText, hriSize));
            }

            if (!chkEnableText.Checked && !chkEnableBarcode.Checked)
            {
                MessageBox.Show("Vui lòng bật ít nhất Văn bản hoặc Mã vạch để in.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            zplCommands.Add("^XZ");
            string zpl = string.Join("\n", zplCommands);
            SendZPLCommand(zpl);
        }

        private void btnSendCustomZPL_Click(object sender, EventArgs e)
        {
            string customZPL = txtCustomZPL.Text;
            if (string.IsNullOrWhiteSpace(customZPL))
            {
                MessageBox.Show("Vui lòng nhập lệnh ZPL.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SendZPLCommand(customZPL);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            keepAliveTimer?.Stop();
            keepAliveTimer?.Dispose();
            DisconnectFromPrinter();
        }
    }
}