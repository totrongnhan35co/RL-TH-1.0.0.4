using BarcodeVerificationSystem.Controller;
using OperationLog.Controller;
using OperationLog.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{
    public partial class FrmViewHistoryProgram : frmBased
    {
        private const int CS_DropShadow = 0x00020000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ClassStyle = CS_DropShadow;
                return createParams;
            }
        }
        private List<LoggingModel> _ListHistory = new List<LoggingModel>();

        public FrmViewHistoryProgram()
        {
            InitializeComponent();
            SetLanguage();
            UpdateIcon();
        }

        public FrmViewHistoryProgram(string key)
        {
            InitializeComponent();
            if (LoggingController.LoginToAccess(key))
            {
                InitControl();
                InitEvent();
            }
            SetLanguage();
            UpdateIcon();
        }

        #region Init

        private void InitControl()
        {
            if (UserController.LogedInUsername == "Supporter" || UserController.LogedInUsername == "Administrator")
            {
                btnClearLog.Visible = true;
            }
            else
            {
                btnClearLog.Visible = false;
            }
            if (Screen.PrimaryScreen.Bounds.Width <= 800)
            {
                Size = MinimumSize;
            }
            string[] colNameList = { "image_column", "KeyWord", "Command", "Message", "Date", "User" };
            InitDataGridView(dgrHistory, colNameList, 0);

            AdjustData(this, EventArgs.Empty);
            btnRefresh.Click += AdjustData;
            Load += FrmViewHistoryProgram_Load;
        }

        private void FrmViewHistoryProgram_Load(object sender, EventArgs e)
        {

        }

        private void InitEvent()
        {
            chbError.CheckedChanged += CheckBoxButton_CheckedChanged;
            chbInfo.CheckedChanged += CheckBoxButton_CheckedChanged;
            chbWarning.CheckedChanged += CheckBoxButton_CheckedChanged;
            chbLogin.CheckedChanged += CheckBoxButton_CheckedChanged;
            chbLogout.CheckedChanged += CheckBoxButton_CheckedChanged;
            chbStartPrint.CheckedChanged += CheckBoxButton_CheckedChanged;
            chbStopPrint.CheckedChanged += CheckBoxButton_CheckedChanged;

            chbError.CheckedChanged += AdjustData;
            chbInfo.CheckedChanged += AdjustData;
            chbWarning.CheckedChanged += AdjustData;
            chbLogin.CheckedChanged += AdjustData;
            chbLogout.CheckedChanged += AdjustData;
            chbStartPrint.CheckedChanged += AdjustData;
            chbStopPrint.CheckedChanged += AdjustData;

            datFrom.ValueChanged += AdjustData;
            datTo.ValueChanged += AdjustData;

            btnExport.Click += ButtonClicked;
            btnClearLog.Click += ButtonClicked;

            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Resize += new EventHandler(FrmViewHistoryProgram_Resize);
        }

        private void CheckBoxButton_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                checkBox.BackColor = checkBox.Checked ? Color.FromArgb(210, 232, 255) : Color.White;
            }
        }

        private void Shared_OnLanguageChange(object sender,EventArgs e)
        {
            SetLanguage();
        }

        void FrmViewHistoryProgram_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                return;
            }
            int deltaX = 10;
            int deltaY = 10;
            if (chbStopPrint.Right + 5 > datFrom.Left)
            {
              
                chbStopPrint.Location = new Point(btnClearLog.Location.X - chbStopPrint.Width - 5 - deltaX, btnClearLog.Location.Y - deltaY);
                chbStopPrint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            if (chbStartPrint.Right + 5 > datFrom.Left)
            {
                
                chbStartPrint.Location = new Point(chbStopPrint.Location.X - chbStartPrint.Width - 5, chbStopPrint.Location.Y);
                chbStartPrint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            if (datFrom.Left - chbLogout.Right > chbStartPrint.Width + 10)
            {
                
                chbStartPrint.Location = new Point(chbLogout.Right + 5, chbLogout.Top);
                chbStartPrint.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }
            if (datFrom.Left - chbStartPrint.Right > chbStopPrint.Width + 10 && chbStartPrint.Top == chbLogout.Top)
            {
                
                chbStopPrint.Location = new Point(chbStartPrint.Right + 5, chbStartPrint.Top);
                chbStopPrint.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }
        }

        #endregion Init

        #region Events
        private void AdjustData(object sender, EventArgs e)
        {
            
            var searchTypes = new List<LoggingType>();
            if (chbError.Checked)
            {
                searchTypes.Add(LoggingType.Error);
            }
            if (chbInfo.Checked)
            {
                searchTypes.Add(LoggingType.Info);
            }
            if (chbWarning.Checked)
            {
                searchTypes.Add(LoggingType.Warning);
            }
            if (chbLogin.Checked)
            {
                searchTypes.Add(LoggingType.LogedIn);
            }
            if (chbLogout.Checked)
            {
                searchTypes.Add(LoggingType.LogedOut);
            }
            if (chbStartPrint.Checked)
            {
                searchTypes.Add(LoggingType.Started);
            }
            if (chbStopPrint.Checked)
            {
                searchTypes.Add(LoggingType.Stopped);
            }
            DateTime dateFrom = datFrom.Value;
            DateTime dateTo = datTo.Value;
            LoadData(searchTypes, dateFrom, dateTo);
        }

        private void ButtonClicked(object sender, EventArgs e)
        {
            if (sender == btnExport)
            {
                ExportData();
            }
            else if (sender == btnClearLog)
            {
                if (MessageBox.Show(this, Lang.DoYouWantToContinue, Lang.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    ClearData();
                }
            }
        }

        private void DgrHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex == -1 || e.RowIndex >= _ListHistory.Count)
            {
                return;
            }
            if (e.ColumnIndex == 0)
            {
                switch (_ListHistory[e.RowIndex].LogType)
                {
                    case LoggingType.Error:
                        e.Value = Properties.Resources.icons8_xbox_x_16px;
                        break;
                    case LoggingType.Info:
                        e.Value = Properties.Resources.icons8_info_16px;
                        break;
                    case LoggingType.LogedIn:
                        e.Value = Properties.Resources.icons8_enter_16px;
                        break;
                    case LoggingType.LogedOut:
                        e.Value = Properties.Resources.icons8_logout_16px_1;
                        break;
                    case LoggingType.Started:
                        e.Value = Properties.Resources.icons8_play_16px;
                        break;
                    case LoggingType.Stopped:
                        e.Value = Properties.Resources.icons8_Stop_Circled_16px;
                        break;
                    case LoggingType.Warning:
                        e.Value = Properties.Resources.icons8_warning_16px;
                        break;
                }
            }
        }

        #endregion Events

        #region Methods

        private void LoadData(List<LoggingType> searchTypes, DateTime dateFrom, DateTime dateTo)
        {
            _ListHistory = LoggingController.ListHistory(dateFrom, dateTo, searchTypes);
            dgrHistory.Rows.Clear();
            var list = new List<DataGridViewRow>();
            string loginUserName = SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember");
            if (_ListHistory != null)
            {
                foreach (LoggingModel item in _ListHistory)
                {
                    if (Shared.LoggedInUser != null && Shared.LoggedInUser.Role != 1000)
                    {
                        if (loginUserName != item.User)
                        {
                            continue;
                        }
                        if (item.LogType == LoggingType.Error)
                        {
                            continue;
                        }
                    }
                    using (var row = new DataGridViewRow())
                    {
                        row.CreateCells(dgrHistory);
                        row.Height = 35;
                        switch (item.LogType)
                        {
                            case LoggingType.Error:
                                row.Cells[0].Value = Properties.Resources.icons8_xbox_x_16px;
                                break;
                            case LoggingType.Info:
                                row.Cells[0].Value = Properties.Resources.icons8_info_16px;
                                break;
                            case LoggingType.LogedIn:
                                row.Cells[0].Value = Properties.Resources.icons8_enter_16px;
                                break;
                            case LoggingType.LogedOut:
                                row.Cells[0].Value = Properties.Resources.icons8_logout_16px_1;
                                break;
                            case LoggingType.Started:
                                row.Cells[0].Value = Properties.Resources.icons8_play_16px;
                                break;
                            case LoggingType.Stopped:
                                row.Cells[0].Value = Properties.Resources.icons8_Stop_Circled_16px;
                                break;
                            case LoggingType.Warning:
                                row.Cells[0].Value = Properties.Resources.icons8_warning_16px;
                                break;
                        }
                        row.Cells[1].Value = item.KeyWord;
                        row.Cells[2].Value = item.Command;
                        row.Cells[3].Value = item.Message;
                        row.Cells[4].Value = item.Date;
                        row.Cells[5].Value = item.User;
                        list.Add(row);
                    }
                }
                dgrHistory.Rows.AddRange(list.ToArray());
                list.Clear();
            }

            dgrHistory.RowTemplate.Height = 35;
        }

        private void ExportData()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV|*.csv"
                };
                sfd.ShowDialog();
                if (sfd.FileName == "")
                {
                    return;
                }

                var csv = new StringBuilder();
                csv.AppendLine(LoggingModel.ExportHeader());
                string loginUserName = SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember");
                foreach (LoggingModel mode in _ListHistory)
                {
                    if (Shared.LoggedInUser != null && Shared.LoggedInUser.Role != 1000)
                    {
                        if (loginUserName != mode.User)
                        {
                            continue;
                        }
                        if (mode.LogType == LoggingType.Error)
                        {
                            continue;
                        }
                    }
                    csv.AppendLine(mode.ExportStringData());
                }

                File.WriteAllText(sfd.FileName, csv.ToString(), Encoding.UTF8);
                Process.Start(sfd.FileName);
            }
            catch (Exception)
            {
            }

        }

        private void ClearData()
        {
            var searchTypes = new List<LoggingType>();
            if (chbError.Checked)
            {
                searchTypes.Add(LoggingType.Error);
            }
            if (chbInfo.Checked)
            {
                searchTypes.Add(LoggingType.Info);
            }
            if (chbWarning.Checked)
            {
                searchTypes.Add(LoggingType.Warning);
            }
            if (chbLogin.Checked)
            {
                searchTypes.Add(LoggingType.LogedIn);
            }
            if (chbLogout.Checked)
            {
                searchTypes.Add(LoggingType.LogedOut);
            }
            if (chbStartPrint.Checked)
            {
                searchTypes.Add(LoggingType.Started);
            }
            if (chbStopPrint.Checked)
            {
                searchTypes.Add(LoggingType.Stopped);
            }
            DateTime dateFrom = datFrom.Value;
            DateTime dateTo = datTo.Value;

            if (Shared.LoggedInUser.Role == 0)
            {
                LoggingController.ClearHistory(dateFrom, dateTo, searchTypes);
            }
            else
            {
                if (chbWarning.Checked)
                {
                    LoggingController.ClearHistory(dateFrom, dateTo, new List<LoggingType> { LoggingType.Warning });
                }
                if (chbStartPrint.Checked)
                {
                    LoggingController.ClearHistory(dateFrom, dateTo, new List<LoggingType> { LoggingType.Started });
                }
                if (chbStopPrint.Checked)
                {
                    LoggingController.ClearHistory(dateFrom, dateTo, new List<LoggingType> { LoggingType.Stopped });
                }
            }

            LoadData(searchTypes, dateFrom, dateTo);
        }

        public void InitDataGridView(DataGridView dgv, string[] columns, int imgIndex = 0)
        {
            _ = dgv.Width;
            _ = (float)1 / columns.Length;
            int tableCodeProductListWidth = dgv.Width - 39;
            for (int index = 0; index < columns.Length; index++)
            {
                string name = columns[index];
                if (index == imgIndex)
                {
                    var col = new DataGridViewImageColumn
                    {
                        HeaderText = name
                    };
                    col.HeaderText = "";
                    col.Name = columns[index].Trim();
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    if (index == 0)
                    {
                        col.Width = (int)(0.75 * tableCodeProductListWidth);
                    }
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv.Columns.Add(col);
                }
                else
                {
                    var col = new DataGridViewTextBoxColumn
                    {
                        HeaderText = name,
                        Name = columns[index].Trim(),
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    if (index == 0)
                    {
                        col.Width = (int)(0.75 * tableCodeProductListWidth);
                    }
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv.Columns.Add(col);
                }
            }

            try
            {
                DataGridViewColumn imgColumn = dgv.Columns["image_column"];
                if (imgColumn != null)
                {
                    imgColumn.Width = 90;
                }

                DataGridViewColumn keyColumn = dgv.Columns["KeyWord"];
                if (keyColumn != null)
                {
                    Size textSize = TextRenderer.MeasureText(keyColumn.HeaderText, dgv.Font);
                    keyColumn.Width = textSize.Width + 10;
                }

                DataGridViewColumn userColumn = dgv.Columns["User"];
                if (userColumn != null)
                {
                    Size textSize = TextRenderer.MeasureText(userColumn.HeaderText, dgv.Font);
                    userColumn.Width = textSize.Width + 20;
                }
            }
            catch
            {

            }
        }

        private void SetLanguage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }

            Text = Lang.ViewLog;
            grbFilter.Text = Lang.Filter;
            chbError.Text = Lang.Error;
            chbWarning.Text = Lang.Warning;
            chbInfo.Text = Lang.Info;

            chbLogin.Text = Lang.Login;
            chbLogout.Text = Lang.LogOut;
            chbStartPrint.Text = Lang.Start;
            chbStopPrint.Text = Lang.Stop;
            lblFrom.Text = Lang.From;
            lblTo.Text = Lang.To;
            btnClearLog.Text = Lang.ClearLog;
            btnExport.Text = Lang.Export;
            btnRefresh.Text = Lang.Refresh;
            lblFormName.Text = Lang.ProgramHistory;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    Close();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion Methods
    }
}
