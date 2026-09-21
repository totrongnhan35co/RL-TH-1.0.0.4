# Plan: Fix UI freeze khi rebuild lúc 12h đêm

## Vấn đề
Khi delta reset (đổi QR/NSX/HSD trước 12h đêm), hệ thống **đứng UI** — `lblPrintedCodeValue` bị đóng băng.

## Nguyên nhân

`SaveCurrentDataToCsv()` (line 8031) **握持 `_SyncObjCodeList` trong toàn bộ quá trình ghi file I/O**:

```
Rebuild thread:  [==== lock(_SyncObj) ==== SaveCurrentDataToCsv (I/O chậm) ==== unlock ====]
Send loop:       [BLOCKED...............chờ lock............................chạy lại]
UI timer:        [↑ _sessionPrintedCount không tăng → lblPrintedCodeValue đứng yên]
```

- `SaveCurrentDataToCsv()` ghi toàn bộ rows vào file →握 lock `_SyncObjCodeList` suốt quá trình ghi (line 8031-8057)
- Send loop (`SendPODDataProductToPrinter`, line 6935) cũng cần `_SyncObjCodeList` → bị block
- Khi send loop block → `_sessionPrintedCount` không tăng → `lblPrintedCodeValue` không cập nhật → **UI đứng**

## Giải pháp: Snapshot nhanh → Release lock → Ghi file từ copy

Thay đổi duy nhất tại **1 phương thức**: `SaveCurrentDataToCsv()`

### File: `frmMainTHTrueMilk.cs`
### Vị trí: lines 8031-8057

### Code hiện tại (O(n × I/O) lock):
```csharp
lock (_SyncObjCodeList)
{
    using (var sw = new StreamWriter(path, false, Encoding.UTF8))
    {
        if (_SelectedJob.IsFirstRowHeader && _DatabaseColunms != null && _DatabaseColunms.Length > 2)
        {
            var headerFields = _DatabaseColunms.Skip(2).Take(3).Select(x => Csv.Escape(x ?? ""));
            sw.WriteLine(string.Join(delimiter, headerFields));
        }
        foreach (var row in _PrintedCodeObtainFromFile)
        {
            if (row.Length <= 2) continue;
            int dataCols = row.Length - 2;
            int endDataCol = row.Length;
            if (row.Length >= 7 && _PrintedCodeObtainFromFile is PrintedCodeVirtualList)
                endDataCol = row.Length - 2;
            var dataFields = row.Skip(2).Take(endDataCol - 2).Select(x => Csv.Escape(x ?? ""));
            sw.WriteLine(string.Join(delimiter, dataFields));
        }
    }
}
```

### Code thay thế (O(n × Array.Copy) lock — <1ms):
```csharp
// ── Snapshot nhanh: lock ngắn, copy data rồi release ──
List<string[]> snapshot;
bool writeHeader = _SelectedJob.IsFirstRowHeader
    && _DatabaseColunms != null
    && _DatabaseColunms.Length > 2;
bool isVirtualList = _PrintedCodeObtainFromFile is PrintedCodeVirtualList;

lock (_SyncObjCodeList)
{
    snapshot = new List<string[]>(_PrintedCodeObtainFromFile.Count);
    for (int i = 0; i < _PrintedCodeObtainFromFile.Count; i++)
    {
        var row = _PrintedCodeObtainFromFile[i];
        var copy = new string[row.Length];
        Array.Copy(row, copy, row.Length);
        snapshot.Add(copy);
    }
}
// → Lock release ngay — send loop chạy tiếp

// ── Ghi file từ snapshot, KHÔNG lock ──
using (var sw = new StreamWriter(path, false, Encoding.UTF8))
{
    if (writeHeader)
    {
        var headerFields = _DatabaseColunms.Skip(2).Take(3).Select(x => Csv.Escape(x ?? ""));
        sw.WriteLine(string.Join(delimiter, headerFields));
    }
    foreach (var row in snapshot)
    {
        if (row.Length <= 2) continue;
        int endDataCol = row.Length;
        if (row.Length >= 7 && isVirtualList)
            endDataCol = row.Length - 2;
        var dataFields = row.Skip(2).Take(endDataCol - 2).Select(x => Csv.Escape(x ?? ""));
        sw.WriteLine(string.Join(delimiter, dataFields));
    }
}
```

## Tại sao an toàn

| | Trước | Sau |
|---|---|---|
| Lock duration | O(n × I/O time) — hàng trăm ms | O(n × Array.Copy) — <1ms |
| Send loop | Bị block, `_sessionPrintedCount` đứng | Chạy bình thường |
| Logic CSV | — | **Không đổi** (cùng data, cùng format) |
| Thread safety | — | Snapshot copy đủ an toàn (string immutable) |

## Verify

1. Build solution — không lỗi compile
2. Kiểm tra CSV output khi rebuild: nội dung giống hệt file cũ
3. Kiểm tra `lblPrintedCodeValue` cập nhật mượt khi rebuild lúc 12h đêm
4. Kiểm tra send loop không bị gián đoạn trong rebuild
