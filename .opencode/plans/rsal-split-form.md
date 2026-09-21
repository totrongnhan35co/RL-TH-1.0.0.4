# Plan: Tách thông báo lỗi và cảnh báo thành 2 form riêng

## Mục tiêu
- Thông báo lỗi (001,002,004-007) → 1 form màu đỏ
- Cảnh báo (003,008-011) → 1 form màu cam
- 2 form hiển thị cạnh nhau giữa màn hình

## File 1: RsalAlertForm.cs
- Constructor: `RsalAlertForm(bool isWarning)`
- Property: `bool IsWarning` — quyết định màu sắc
- Single `HashSet<string> _shownCodes` — dedup theo errorCode
- Method: `AppendMessage(headIdx, errorCode, detail, instruction)`
- Không cần tham số isWarning trong AppendMessage (form đã biết loại)

## File 2: RsalAlertForm.Designer.cs
Layout đơn giản 1 cột:
```
┌──────────────────────────────────┐
│ ⚠ Cảnh báo máy in (hoặc ❌ Lỗi)  │ ← màu cam/đỏ tuỳ loại
├──────────────────────────────────┤
│ POD 1: Hết mực                   │ ← FlowLayoutPanel
│ Mã: RSAL-007                     │     TopDown
│ Thay cartridge                   │
├──────────────────────────────────┤
│                    [ Xác nhận ]   │
└──────────────────────────────────┘
```
Controls: `pnlTitle` + `picIcon` + `lblTitle` + `pnlMessages` (FlowLayoutPanel) + `pnlFooter` + `btnXacNhan`

## File 3: frmMainTHTrueMilk.cs
### Thay field:
```csharp
// Xoá: private RsalAlertForm _rsalAlertForm = null;
// Thêm:
private RsalAlertForm _rsalErrorForm = null;
private RsalAlertForm _rsalWarningForm = null;
```

### Thêm method PositionForm:
```csharp
private void PositionForm(RsalAlertForm form, bool isWarning)
{
    var screen = Screen.PrimaryScreen.WorkingArea;
    int w = 450, h = 300, gap = 10;
    bool errorOpen = _rsalErrorForm != null && !_rsalErrorForm.IsDisposed;
    bool warnOpen = _rsalWarningForm != null && !_rsalWarningForm.IsDisposed;

    if (errorOpen && warnOpen)
    {
        int totalW = w * 2 + gap;
        int left = (screen.Width - totalW) / 2;
        int top = (screen.Height - h) / 2;
        _rsalErrorForm.Location = new Point(left, top);
        _rsalWarningForm.Location = new Point(left + w + gap, top);
    }
    else
    {
        form.StartPosition = FormStartPosition.CenterScreen;
    }
}
```

### Cập nhật BeginInvoke:
```csharp
ref RsalAlertForm formRef = ref isWarning ? ref _rsalWarningForm : ref _rsalErrorForm;

if (formRef == null || formRef.IsDisposed)
{
    formRef = new RsalAlertForm(isWarning);
    formRef.FormClosed += (o, args) => formRef = null;
    formRef.AppendMessage(alarmIdx, alarmErr, friendlyMsg, instruction);
    PositionForm(formRef, isWarning);
    formRef.Show();
}
else
{
    formRef.AppendMessage(alarmIdx, alarmErr, friendlyMsg, instruction);
}
```

### Cập nhật màu sắc trong code-behind RsalAlertForm:
```csharp
// Trong constructor sau InitializeComponent():
if (isWarning)
{
    pnlTitle.BackColor = Color.FromArgb(255, 193, 7);    // cam
    picIcon.Image = Properties.Resources.icons8_warning_24px1;
    lblTitle.Text = "Cảnh báo máy in";
}
else
{
    pnlTitle.BackColor = Color.FromArgb(220, 53, 69);   // đỏ
    picIcon.Image = Properties.Resources.icons8_warning_24px1; // hoặc icon lỗi nếu có
    lblTitle.Text = "Lỗi máy in";
}
```

## Thứ tự thực hiện
1. Sửa RsalAlertForm.Designer.cs — layout 1 cột
2. Sửa RsalAlertForm.cs — constructor + AppendMessage đơn giản
3. Sửa frmMainTHTrueMilk.cs — 2 field + PositionForm + BeginInvoke
