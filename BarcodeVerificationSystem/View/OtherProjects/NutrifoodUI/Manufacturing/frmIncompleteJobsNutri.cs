using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Controller.HistorySync;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.NutrifoodUI.Manufacturing
{
    public partial class FrmIncompleteJobsNutri : Form
    {
        private DataGridView _dgvIncompleteJobs;
        private readonly frmJobNutri _parentForm;
        private CancellationTokenSource _syncCts;
        private bool _isSyncing;

        public FrmIncompleteJobsNutri(frmJobNutri parentForm)
        {
            _parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadIncompleteJobs();
        }

        private void SetupDataGridView()
        {
            _dgvIncompleteJobs.Columns.Clear();
            _dgvIncompleteJobs.Columns.Add(new DataGridViewTextBoxColumn { Name = "STT", HeaderText = "STT", ReadOnly = true });
            _dgvIncompleteJobs.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaCongViec", HeaderText = "Mã công việc", ReadOnly = true });
            _dgvIncompleteJobs.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaPhieuSoanHang", HeaderText = "Mã phiếu", ReadOnly = true });
            _dgvIncompleteJobs.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaSanPham", HeaderText = "Mã sản phẩm", ReadOnly = true });
            _dgvIncompleteJobs.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongCanXuat", HeaderText = "Số lượng cần xuất", ReadOnly = true });
            _dgvIncompleteJobs.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongDongBoSaaS", HeaderText = "Số lượng đồng bộ SaaS", ReadOnly = true });
            _dgvIncompleteJobs.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongDongBoSAP", HeaderText = "Số lượng đồng bộ HUB", ReadOnly = true });
            _dgvIncompleteJobs.Columns.Add(new DataGridViewTextBoxColumn { Name = "HoanThanh", HeaderText = "Hoàn thành", ReadOnly = true });

            _dgvIncompleteJobs.AllowUserToAddRows = false;
            HistoryUtils.CustomDataGridView(_dgvIncompleteJobs);
        }

        /// <summary>
        /// Loads and displays all jobs that are not completed (similar to DisplayHistory with HistoryFilter.NotFinished).
        /// </summary>
        private void LoadIncompleteJobs()
        {
            SetupDataGridView();
            _dgvIncompleteJobs.Rows.Clear();

            List<string> jobNameList = Shared.GetJobNameList();
            if (jobNameList == null || jobNameList.Count == 0)
                return;

            var rows = SyncDataList.ReturnSyncDataList(jobNameList);
            int i = 0;
            foreach (var row in rows)
            {
                if (row.Hoanthanh)
                    continue;
                i++;
                int rowIndex = _dgvIncompleteJobs.Rows.Add();
                _dgvIncompleteJobs.Rows[rowIndex].Cells["STT"].Value = i;
                _dgvIncompleteJobs.Rows[rowIndex].Cells["MaCongViec"].Value = row.MaCongViec;
                _dgvIncompleteJobs.Rows[rowIndex].Cells["MaPhieuSoanHang"].Value = row.MaPhieuSoanHang;
                _dgvIncompleteJobs.Rows[rowIndex].Cells["MaSanPham"].Value = row.MaSanPham;
                _dgvIncompleteJobs.Rows[rowIndex].Cells["SoLuongCanXuat"].Value = row.SoLuongCanXuat;
                _dgvIncompleteJobs.Rows[rowIndex].Cells["SoLuongDongBoSaaS"].Value = row.SoLuongDongBoSaaS;
                _dgvIncompleteJobs.Rows[rowIndex].Cells["SoLuongDongBoSAP"].Value = row.SoLuongDongBoSAP;
                _dgvIncompleteJobs.Rows[rowIndex].Cells["HoanThanh"].Value = "Chưa Hoàn Thành";
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnSyncAll_Click(object sender, EventArgs e)
        {
            if (_isSyncing) return;
            var jobNames = GetIncompleteJobNames();
            if (jobNames == null || jobNames.Count == 0)
            {
                UpdateStatus("Không có công việc chưa hoàn thành.");
                return;
            }
            _syncCts = new CancellationTokenSource();
            _isSyncing = true;
            SetSyncUIMode(syncing: true);
            _ = RunSyncAllAsync(jobNames);
        }

        private void BtnStopSync_Click(object sender, EventArgs e)
        {
            _syncCts?.Cancel();
            _parentForm?.StopCurrentSync();
        }

        private List<string> GetIncompleteJobNames()
        {
            var list = new List<string>();
            if (_dgvIncompleteJobs?.Rows == null) return list;
            foreach (DataGridViewRow row in _dgvIncompleteJobs.Rows)
            {
                if (row.IsNewRow) continue;
                var v = row.Cells["MaCongViec"]?.Value?.ToString();
                if (!string.IsNullOrEmpty(v)) list.Add(v);
            }
            return list;
        }

        private async Task RunSyncAllAsync(List<string> jobNames)
        {
            try
            {
                int done = 0;
                int total = jobNames.Count;
                for (int i = 0; i < jobNames.Count; i++)
                {
                    _syncCts.Token.ThrowIfCancellationRequested();
                    var jobName = jobNames[i];
                    UpdateStatus($"Đang đồng bộ ({i + 1}/{total}): {jobName}");
                    BeginInvoke(new Action(() => picDatabaseLoading.Visible = true));
                    var job = Shared.GetJob(jobName);
                    bool ok = await _parentForm.SyncSingleJobAsync(job, _syncCts.Token);
                    if (!ok) break;
                    done++;
                }
                UpdateStatus(_syncCts.Token.IsCancellationRequested ? "Đã dừng đồng bộ." : $"Đã đồng bộ xong {done}/{total} công việc.");
                LoadIncompleteJobs();
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Đã dừng đồng bộ.");
                LoadIncompleteJobs();
            }
            catch (Exception ex)
            {
                UpdateStatus("Lỗi: " + ex.Message);
                LoadIncompleteJobs();
            }
            finally
            {
                BeginInvoke(new Action(() => picDatabaseLoading.Visible = false));
                _isSyncing = false;
                SetSyncUIMode(syncing: false);
            }
        }

        private void UpdateStatus(string text)
        {
            if (lblStatus == null) return;
            if (InvokeRequired) BeginInvoke(new Action(() => lblStatus.Text = text));
            else lblStatus.Text = text;
        }

        private void SetSyncUIMode(bool syncing)
        {
            void Update()
            {
                bool enable = !syncing;
                pnlTop.Enabled = enable;
                _dgvIncompleteJobs.Enabled = enable;
                lblStatus.Enabled = enable;
                SyncDataBtn.Enabled = enable;
                editJobBtn.Enabled = enable;
                closeBtn.Enabled = enable;
                StopSyncData.Enabled = syncing;
                picDatabaseLoading.Visible = syncing;
                if (syncing) picDatabaseLoading.BringToFront();
            }
            if (InvokeRequired) BeginInvoke(new Action(Update));
            else Update();
        }

        private void FrmIncompleteJobsNutri_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isSyncing)
            {
                e.Cancel = true;
                return;
            }
            var jobNames = GetIncompleteJobNames();
            if (jobNames != null && jobNames.Count > 0)
            {
                var result = CustomMessageBox.Show(
                    "Vẫn còn công việc chưa hoàn thành, bạn có muốn không đồng bộ ?",
                    "Cảnh báo",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                var jobList = string.Join(", ", jobNames);
                ProjectLogger.WriteError($"[Incomplete Jobs] User closed form with unfinished jobs. Jobs: {jobList}");
            }
            _syncCts?.Cancel();
            _parentForm?.StopCurrentSync();
            _parentForm?.ShowSyncPanelForPopupSync(false, null);
        }

        private void SyncDataBtn_Click(object sender, EventArgs e)
        {
            if (_isSyncing) return;
            var jobNames = GetIncompleteJobNames();
            if (jobNames == null || jobNames.Count == 0)
            {
                UpdateStatus("Không có công việc chưa hoàn thành.");
                return;
            }
            _syncCts = new CancellationTokenSource();
            _isSyncing = true;
            SetSyncUIMode(syncing: true);
            _ = RunSyncAllAsync(jobNames);
        }

        private void StopSyncData_Click(object sender, EventArgs e)
        {
            _syncCts?.Cancel();
            _parentForm?.StopCurrentSync();
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void editJobBtn_Click(object sender, EventArgs e)
        {
            if (_dgvIncompleteJobs.SelectedRows.Count == 0)
            {
                CustomMessageBox.Show("Vui lòng chọn một công việc để chỉnh sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int lineIndex = _dgvIncompleteJobs.SelectedRows[0].Index;
            string jobName = _dgvIncompleteJobs.Rows[lineIndex].Cells["MaCongViec"].Value?.ToString();
            
            if (string.IsNullOrEmpty(jobName))
            {
                CustomMessageBox.Show("Không tìm thấy mã công việc!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            JobModel currentJob = Shared.GetJob(jobName);
            if (currentJob == null)
            {
                CustomMessageBox.Show("Không tìm thấy thông tin công việc!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var formEdit = new frmEditJob(currentJob);
            formEdit.ShowDialog();
            LoadIncompleteJobs();
        }
    }
}
