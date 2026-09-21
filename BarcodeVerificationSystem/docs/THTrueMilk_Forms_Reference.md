# THTrueMilk Forms — Tài liệu tham chiếu (frmJob & frmMain)

> Module **THTrueMilk** trong `BarcodeVerificationSystem` (.NET Framework 4.8, WinForms) gồm 2 form chính:
> - `frmJobTHTrueMilk` — **Manufacturing**: tạo/quản lý job, sinh mã, đồng bộ QR-Bank, monitor thiết bị.
> - `frmMainTHTrueMilk` — **Dispatching**: vận hành runtime, kiểm tra/đối chiếu, in & đồng bộ kết quả về SaaS/SAP.
>
> Đường dẫn:
> - `BarcodeVerificationSystem\View\OtherProjects\THTrueMilkUI\Manufacturing\frmJobTHTrueMilk.cs`
> - `BarcodeVerificationSystem\View\OtherProjects\THTrueMilkUI\Dispatching\frmMainTHTrueMilk.cs`

---

## 📑 Mục lục
- [A. frmJobTHTrueMilk](#a-frmjobthtruemilk-manufacturing)
- [B. frmMainTHTrueMilk](#b-frmmainthtruemilk-dispatching)
- [C. Sơ đồ luồng tổng quát](#c-sơ-đồ-luồng-tổng-quát)

---

## A. `frmJobTHTrueMilk` (Manufacturing)

### A.1. Tính năng chính

| Nhóm | Mô tả |
|---|---|
| **Job CRUD** | Tạo, mở, sửa, xóa, lưu, "purge" job; quản lý JobModel bao gồm material/PO/Reservation, PODFormat, TemplatePrint, CompareType. |
| **Code Generation** | Sinh mã offline (chung / Reservation / PO), build CSV gửi sang printer, mark QR đã dùng trong DB. |
| **QrBank integration** | Khởi động HTTP server local (`/api/qrbank/login`, `/data`, `/ping`); nhận QR từ R-Link Master, lưu DB nội bộ. |
| **3 chế độ vận hành** | Mode 1 `BatchOneQrCode` (mỗi batch 1 QR), Mode 2 `AutoRefreshByTime` (reset 24h), Mode 3 `Verified` (kiểm tra giáp lai). |
| **Device Monitoring** | Camera, Printer, Sensor Controller, Serial Device, DB SQLite, Cognex IS Single/Dual; auto reconnect & cảnh báo. |
| **Sync R-Link Master** | Đồng bộ job đã hoàn thành lên server (single/popup multi sync), kèm fallback retry. |
| **Notifications** | Toast/popup cho lỗi thiết bị, kho QR thấp (cooldown 10 phút), job chưa hoàn tất. |
| **Auth/Permission** | Refresh UI sau login, áp permission theo role (Admin / Operator / Viewer). |
| **Localization** | Đa ngôn ngữ qua `Lang.*`, hot-reload bằng event `Shared.OnLanguageChange`. |

### A.2. Danh sách hàm theo nhóm

#### A.2.1 Lifecycle / Init
- `frmJobTHTrueMilk()` — constructor, `InitializeComponent`.
- `OnHandleCreated`, `OnLoad`, `OnFormClosing`, `OnFormClosed`.
- `InitUI`, `InitControls`, `InitEvents`, `InitJobModel`, `InitMode1UI`, `InitMidnightResetTimer`.
- `SetLanguage`, `ApplyPermissions`, `RefreshAfterLogin`, `ShowForm`.

#### A.2.2 Job CRUD
- `CreateJob`, `OpenJob`, `DeleteJob`, `SaveJob`, `PurgeJob`.
- `LoadJobNameList`, `UpdateUIJobInformation`, `UpdateMode1UI`.
- `SaveJobForMode1Or2Async` — lưu job khi chạy Mode 1/2.

#### A.2.3 Sinh mã (Code Generation)
- `GenerateCodesOffline`, `GenerateCodesOfflineRES`, `GenerateCodesOfflinePO`.
- `GenerateReservationCodes`, `GeneratePOCodes`.
- `SendGeneratedCodes` — đẩy CSV sang printer.
- `BuildCsvFromQrBankDbAsync`, `BuildCsvFromSQLiteFallback`, `BuildCsvLines`.
- `MarkQrAsUsedAsync`, `CountAvailableQrInDbAsync`, `LoadCurrentBatchQrFromDb`.

#### A.2.4 QrBank Server local
- `StartQrBankServer`, `StopQrBankServer`.
- `QrBankHandler_DataSaved` — event khi nhận QR mới từ R-Link.
- `QrBankHandler_RequestArrived` — log request đến.

#### A.2.5 Mode logic & Periodic check
- `ApplyMode1_BatchOneQrCode`, `ApplyMode2_AutoRefreshByTime`.
- `TimerMidnightReset_Tick`, `HandleMode1DeltaReset`, `HandleMode2MidnightReset`.
- `CheckQrStockAndAlertAsync` — kiểm tra kho QR + cảnh báo (cooldown 10 phút).
- `NotifyDeviceException` — gửi notify cho Printer/Camera/PLC/QrStockLow.

#### A.2.6 Device Monitoring
- `MonitorCameraConnection`, `MonitorCameraConnection_CognexSupport`.
- `MonitorPrinterConnection`, `MonitorDatabaseConnection`.
- `MonitorSensorControllerConnection`, `MonitorSerialDeviceControllerConnection`.
- `MonitorListenerServer`, `StartListenerServer`.

#### A.2.7 Event Handlers (Shared.*)
- `Shared_OnPrintingStateChange`, `Shared_OnPrinterStatusChange`.
- `Shared_OnCameraStatusChange`, `Shared_OnCameraReadDataChange`.
- `Shared_OnSensorControllerChangeEvent`, `Shared_OnSerialDeviceControllerChangeEvent`.
- `Shared_OnLanguageChange`, `Shared_OnLogError`.
- `PODController_OnPODReceiveDataEvent`, `SensorController_OnPODReceiveMessageEvent`.
- `CamController_OnCamReceiveMessageEvent`, `ActionResult` (router cho UI buttons).

#### A.2.8 Sync với R-Link Master
- `SyncSingleJobAsync`, `ShowSyncPanelForPopupSync`, `StopCurrentSync`.
- `DisposeMultiSyncHandler`, `DisposeSingleHandler`.
- `OnSentPrintedCodesCompleted`.

#### A.2.9 Notifications & UI Status
- `ShowIncompleteJobsPopup`.
- `UpdateStatusLabelDatabase`, `UpdateStatusLabelPrinter`, `UpdateStatusLabelCamera`.
- `DisplayHistory`, `SetupDataGridView`.

#### A.2.10 Helpers / Debug
- `DebugVirtual` — bật control test (chỉ build DEBUG).
- Helpers chung: format QR, validate input, parse PODFormat, …

---

## B. `frmMainTHTrueMilk` (Dispatching)

### B.1. Tính năng chính

| Nhóm | Mô tả |
|---|---|
| **Vận hành runtime** | Start/Stop/Trigger; xử lý queue dữ liệu camera ↔ printer real-time. |
| **So khớp (Compare)** | 3 chế độ: `Database`, `CanRead`, `StaticText`. Tô màu kết quả Valid/Duplicated/Missed/Null/Invalided. |
| **DataGridView ảo** | `dgvDatabase` (mã đã in) + `dgvCheckedResult` (kết quả kiểm tra), virtual mode 500 dòng/page. |
| **Kết quả thống kê** | TotalChecked, Passed, Failed, Printed, Received, Sent, Duplicate + CircularProgressBar. |
| **Đồng bộ SaaS / SAP** | Track SaaSSuccess/Failed, SAPSuccess/Failed; `confirmCompletion`, `syncDataBtn`, `disposeBtn`, `RePrintBtn`. |
| **Verify & Print** | Mode đặc thù: nhận data từ camera → so khớp → gửi lệnh `DATA;` xuống PODController. |
| **Backup/Export** | Export ảnh, CSV checked-result, printed-response, send log, RSFP log; export 1-cho-tất-cả. |
| **Sub-forms** | `FrmSettingsTHTrueMilk`, `FrmViewHistoryProgram`, `FrmPreviewDatabaseTHTrueMilk`, `FrmCheckedResult`. |
| **Status Strip** | 9 indicator: Camera / Printer / Scanner / LineName / User / SensorPLC / OperationStatus / Version / DateTime. |
| **Responsive UI** | `IsFullHD` setter điều chỉnh layout cho màn hình < 850×850. |

### B.2. Cấu trúc UI (designer)
- **StatusStrip**: `lblStatusCamera01`, `lblStatusPrinter01`, `lblStatusSerialDevice`, `LineName`, `UserNameDisplay`, `lblSensorControllerStatus`, `toolStripOperationStatus`, `toolStripVersion`, `toolStripDateTime`.
- **pnlMenu**: `btnJob`, `btnDatabase`, `btnAccount`, `btnHistory`, `btnExportData`, `btnVirtualStart`, `btnVirtualStop`, `btnExportResult`, `btnExportAll`, `btnSettings`, `btnExit`.
- **Job Information** (panel trái): `materialName`, `materialNumber`, `wmsNumber`, `txtJobName`, `txtCodeResult` + nested `panel3` (`txtJobType`, `txtTemplatePrint`, `txtPODFormat`, `txtStaticText`, `txtCompareType`, `txtBarcodeQuality`, `SAPSuccess/Failed`, `SaaSSuccess/Failed`, `RePrintBtn`…).
- **Process panels**: `pnlPictureBox`, `pnlCurrentCheck` (sentSaaSSuccess / sentSAPSuccess / SyncDataText / SyncLoading), `pnlVerificationProcess` + `prBarCheckPassed`, `pnlTotalChecked`, `pnlCheckPassed`, `pnlCheckFailed`.
- **Action buttons**: `confirmCompletion`, `syncDataBtn`, `disposeBtn`, `RePrintBtn`.
- **Data tables**: `pnlDatabase` → `dgvDatabase`; `pnlCheckedResult` → `dgvCheckedResult` + `btnValid` / `btnInvalid` / `btnDuplicate` / `btnNull`.
- **Printed state**: `pnlSentData`, `pnlReveied`, `pnlPrintedCode`.
- **Account dropdown**: `cuzDropdownManageAccount` → `mnManage`, `mnChangePassword`, `mnLogOut`.

### B.3. Danh sách hàm theo nhóm

#### B.3.1 Lifecycle / Init
- `frmMainTHTrueMilk()` / `frmMainTHTrueMilk(frmJobTHTrueMilk parentForm)` — constructor.
- `OnHandleCreated` → gọi `InitControls`, `InitEvents`.
- `InitControls` — set status strip, language, IsFullHD, hide/show controls theo `CompareType`, áp `Shared.UserPermission`, gọi `DebugVirtual` (DEBUG).
- `InitEvents` — wire toàn bộ event Shared.* + button click → `ActionChanged`.
- `FrmMain_FormClosing` — confirm exit + gọi `MonitorSenderService.sendParametersToServerAsync(false)`.
- `FrmMain_FormClosed` — `ReleaseResource`, gọi `_ParentForm.ShowForm()`.
- `WndProc` — chặn move/restore khi maximize, snap top.
- `SetLanguage`, `IsFullHD` setter (responsive layout).

#### B.3.2 Operation Control
- `ActionChanged` — router xử lý click cho mọi button menu/action.
- `StartProcess(bool interactOnUI)` — pre-check, gọi `StartAllThread*`.
- `StopProcessAsync(bool isSuddenly, string reason, bool fromUser, bool …)`.
- `CheckAllTheConditions()` → enum `CheckCondition`.
- `CheckAllSettingsPrinter()` → enum `CheckPrinterSettings`.
- `CheckInitDataErrorAndGenerateMessage`.
- `EnableUIComponent(OperationStatus)`, `EnableUIComponentWhenLoadData(bool)`.
- `BtnTrigger_MouseDown/Up` — manual trigger camera.

#### B.3.3 Threads & Queues (runtime)
- `StartAllThreadForTesting`, `StopAllThreadForTesting`.
- `CompareAsync(token)` — vòng lặp consume `_QueueBufferDataObtained` → so khớp.
- `ReceiveResponseFromPrinterHandlerAsync`.
- `ExportImageToFileAsync`, `ExportCheckedResultToFileAsync`, `ExportPrintedResponseToFileAsync`.
- `UpdateUICheckedResultAsync`, `UpdateUIPrintedResponseAsync`.
- `VirtualTestAsync`, `VirtualTest(token)`.
- `InitVNPUpdatePrintedStatusConditionBuffer` — Verify-and-Print buffer.
- `RaiseOnReceiveVerifyDataEvent`, `SendVerifiedDataToPrinter`.

#### B.3.4 DataGridView (virtual mode)
- `InitDataGridView(dgv, columns, imgIndex, isPOD)`.
- `Database_CellValueNeeded` — hiển thị icon trạng thái Printed/Waiting/Sent/Reprint/Duplicate.
- `CheckedResult_CellValueNeeded` — hiển thị icon Valid/Duplicated/Missed/Null/Invalided.
- `UpdateCheckTotalAndPrintedDatabase`.
- `StopProcessWhileMissingData` — chế độ R&D debug, dừng khi printed missing.

#### B.3.5 Status Indicators
- `UpdateStatusLabelCamera`, `UpdateStatusLabelPrinter`.
- `UpdateUISensorControllerStatus(bool)`, `UpdateUISerialDeviceControllerStatus(bool)`.
- `UpdateJobInfomationInterface`.
- `ChangeCheckMode(Checkmode)` — switch `Camera` / `getSampleWithScanner` / `recheckWithScanner`.

#### B.3.6 Event Handlers (Shared.*)
- `Shared_OnSyncDataParameterChange` — cập nhật SaaS/SAP success/failed.
- `Shared_OnCameraStatusChange`, `Shared_OnCameraReadDataChange`, `Shared_OnCameraPositionDataChange`.
- `Shared_OnSerialDeviceReadDataChange`.
- `Shared_OnPrinterDataChange`, `Shared_OnPrintingStateChange`, `Shared_OnPrinterStatusChange`.
- `Shared_OnSensorControllerChangeEvent`, `Shared_OnSerialDeviceControllerChangeEvent`.
- `Shared_OnLanguageChange`, `Shared_OnLogError`.
- `Shared_OnVerifyAndPrindSendDataMethod`.
- `Shared_OnNumberEventISCountAsync` — đếm Cognex IS Master/Slave.

#### B.3.7 Đồng bộ SaaS/SAP & Dispatching actions
- `confirmCompletion_Click` (qua ActionChanged) — xác nhận hoàn tất job.
- `syncDataBtn_Click` — đẩy data lên server.
- `disposeBtn_Click` — quy trình tiêu hủy mã thất bại.
- `RePrintBtn_Click` — in lại mã.
- `OnSent*Completed` — callback từ ReliableDataSender (TH True Milk).
- Properties auto-update UI: `SentSyncData`, `SaaSSuccess/Failed`, `SAPSuccess/Failed`, `TotalChecked`, `NumberOfCheckPassed/Failed`, `NumberPrinted`, `ReceivedCode`, `NumberOfSentPrinter`, `SendPodTimeMs`.

#### B.3.8 Get Sample / Recheck (Scanner)
- `GetSampleRaise`, `GetSampleWithScanner`.
- `BtnViewLog_Click` — mở log file qua Notepad.

#### B.3.9 Sub-forms & Dialogs
- Mở `FrmSettingsTHTrueMilk`, `FrmViewHistoryProgram`, `FrmPreviewDatabaseTHTrueMilk`, `FrmCheckedResult`.
- `CustomMessageBox.Show(...)` cho confirm/info/error.

#### B.3.10 Test / Debug (chỉ DEBUG)
- `DebugVirtual` — bật `btnVirtualStart/Stop`, `btnValid/Invalid/Duplicate/Null`.
- `AddValidInput`, `AddInvalidInput(int num)`.

#### B.3.11 Helpers
- `GetCompareDataByPODFormat(string[] row, List<PODModel> podFormat)`.
- `ReadPrintedCodeData(string path)`.
- `ReleaseResource` — hủy CTS, dispose handler, đóng controller.
- `PnlMenu_DoubleClick` — toggle WindowState.
- `TimerDateTime_Tick` — cập nhật `toolStripDateTime`.

---

## C. Sơ đồ luồng tổng quát
flowchart LR A["frmJobTHTrueMilk<br/>(Manufacturing)"] -- "Open / ShowForm" --> B["frmMainTHTrueMilk<br/>(Dispatching)"] B -- "FormClosing → ParentForm.Close" --> A
subgraph J["Manufacturing"]
  J1["QrBank Server :8088<br/>(login/data/ping)"]
  J2["GenerateCodes →<br/>BuildCsv → Printer"]
  J3["Mode 1/2/3 logic<br/>+ midnight reset"]
  J4["Monitor: Camera / Printer /<br/>PLC / SerialDevice / DB"]
end

subgraph M["Dispatching"]
  M1["Camera → Queue →<br/>CompareAsync"]
  M2["dgvDatabase /<br/>dgvCheckedResult"]
  M3["confirmCompletion /<br/>syncData / dispose / rePrint"]
  M4["SaaS / SAP sync<br/>ReliableDataSender"]
end

A --- J
B --- M
M3 --> M4
M4 -- "POST /api/..." --> RLM["R-Link Master Server"]
J1 -- "POST /api/qrbank/data" --- RLM
