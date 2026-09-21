using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BarcodeVerificationSystem.Controller;

namespace BarcodeVerificationSystem.Model.THTrueMilk
{
    /// <summary>
    /// Lưu trữ database barcode theo dạng virtual cho Mode 1/2 (BatchOneQrCode / AutoRefreshByTime).
    /// Chỉ giữ 1 prototype QR + NSX + HSD + mảng byte status (3.5 MB cho 3.5M dòng)
    /// thay vì List&lt;string[]&gt; (&gt; 2 GB).
    ///
    /// Per-row QR/NSX/HSD: giữ lịch sử đúng cho mã đã gửi/đã in, chỉ đổi mã Waiting.
    /// </summary>
    internal class PrintedCodeVirtualList : IList<string[]>
    {
        // ── internal state ──────────────────────────────────────────────
        private readonly int _totalCount;
        private readonly byte[] _status;      // 0=Waiting, 1=Printed, 2=Duplicate, 3=Reprint, 4=Sent
        private readonly string[] _sendTime;  // sparse — null nếu chưa gửi
        private readonly string[] _printTime; // sparse — null nếu chưa in
        // Current values (dùng cho reference, UI labels)
        private string _qrCode;
        private string _nsx;
        private string _hsd;
        // Per-row storage — giữ lịch sử riêng cho từng dòng
        private readonly string[] _qrCodePerRow;
        private readonly string[] _nsxPerRow;
        private readonly string[] _hsdPerRow;
        private QRObjectCls[] _qrCodePerRowArr;//NhanTo_260907_1003 
        private readonly int _fieldCount;     // số cột data (5: STT, Status, QR, NSX, HSD)
        private readonly int _displayFieldCount; // số cột hiển thị (7: + T.Gửi, T.In)

        // tuần tự cho FindNextWaiting
        private int _nextWaitingIndex;
        private int _printedCount;

        // ── constants ─────────────────────────────────────────────────────
        private const byte StatusWaiting = 0;
        private const byte StatusPrinted = 1;
        private const byte StatusDuplicate = 2;
        private const byte StatusReprint = 3;
        private const byte StatusSent = 4;

        // ── constructor ─────────────────────────────────────────────────
        public PrintedCodeVirtualList(int totalCount, string qrCode, string nsx, string hsd, int fieldCount, int displayFieldCount = 7)
        {
            if (totalCount < 0) throw new ArgumentOutOfRangeException(nameof(totalCount));
            _totalCount = totalCount;
            _status = new byte[totalCount]; // tất cả = 0 (Waiting)
            _sendTime = new string[totalCount];
            _printTime = new string[totalCount];
            _qrCode = qrCode ?? "";
            _nsx = nsx ?? "";
            _hsd = hsd ?? "";
            _fieldCount = fieldCount;
            _displayFieldCount = displayFieldCount;
            _nextWaitingIndex = 0;
            _printedCount = 0;

            // Per-row: fill tất cả với giá trị ban đầu
            //_qrCodePerRow = new string[totalCount];//NhanTo_260907_1003: Command
            //_nsxPerRow = new string[totalCount];//NhanTo_260907_1003: Command
            //_hsdPerRow = new string[totalCount];//NhanTo_260907_1003: Command
            _qrCodePerRowArr = new QRObjectCls[totalCount];//NhanTo_260907_1003: Add new
           
            for (int i = 0; i < totalCount; i++)
            {
                //_qrCodePerRow[i] = _qrCode;//NhanTo_260907_1003: Command
                //_nsxPerRow[i] = _nsx;//NhanTo_260907_1003: Command
                //_hsdPerRow[i] = _hsd;//NhanTo_260907_1003: Command
                //
                //
                QRObjectCls updateQR = new QRObjectCls() { QR = _qrCode, NSX = _nsx, EXP = _hsd };//NhanTo_260907_1003: Add new
                //
                _qrCodePerRowArr[i] = updateQR;//NhanTo_260907_1003: Add new
            }
        }

        // ── indexer ───────────────────────────────────────────────────────
        public string[] this[int index]
        {
            get
            {
                if (index < 0 || index >= _totalCount)
                    return new string[_displayFieldCount];
               
                    var row = new string[_displayFieldCount];
                    row[0] = (index + 1).ToString();
                    row[1] = StatusToString(_status[index]);
                    //NhanTo_260907_1003: Command
                    //if (_displayFieldCount > 2) row[2] = _qrCodePerRow[index];
                    //if (_displayFieldCount > 3) row[3] = _nsxPerRow[index];
                    //if (_displayFieldCount > 4) row[4] = _hsdPerRow[index];
                    //if (_displayFieldCount > 5) row[5] = _sendTime[index] ?? "";
                    //if (_displayFieldCount > 6) row[6] = _printTime[index] ?? "";
                    //End NhanTo_260907_1003: Command
                    //
                    //NhanTo_260907_1003: Add new
                    if (_displayFieldCount > 2) row[2] = _qrCodePerRowArr[index].QR;
                    if (_displayFieldCount > 3) row[3] = _qrCodePerRowArr[index].NSX;
                    if (_displayFieldCount > 4) row[4] = _qrCodePerRowArr[index].EXP;
                    //
                    if (_displayFieldCount > 5) row[5] = _sendTime[index] ?? "";
                    if (_displayFieldCount > 6) row[6] = _printTime[index] ?? "";
                    //NhanTo_260907_1003: Add new
                    return row;
                
            }
            set
            {
                if (index < 0 || index >= _totalCount || value == null) return;
                byte oldStatus = _status[index];
                if (value.Length > 1)
                {
                    _status[index] = StringToStatus(value[1]);
                    // cập nhật đếm nhanh
                    if (oldStatus == StatusWaiting && _status[index] != StatusWaiting)
                        Interlocked.Increment(ref _printedCount);
                    else if (oldStatus != StatusWaiting && _status[index] == StatusWaiting)
                        Interlocked.Decrement(ref _printedCount);
                }
                if (value.Length > 5) _sendTime[index] = value[5];
                if (value.Length > 6) _printTime[index] = value[6];
                // cập nhật _nextWaitingIndex nếu index đang bị đánh dấu Printed
                if (_status[index] != StatusWaiting && index == _nextWaitingIndex)
                    _nextWaitingIndex = index + 1;
            }
        }

        // ── count ─────────────────────────────────────────────────────────
        public int Count => _totalCount;
        public bool IsReadOnly => false;
        public string QrCode => _qrCode;
        public string Nsx => _nsx;
        public string Hsd => _hsd;
        public int FieldCount => _fieldCount;
        public int DisplayFieldCount => _displayFieldCount;

        // ── helpers ───────────────────────────────────────────────────────
        public int FindNextWaiting()
        {
            for (int i = _nextWaitingIndex; i < _totalCount; i++)
            {
                if (_status[i] == StatusWaiting)
                {
                    _nextWaitingIndex = i;
                    return i;
                }
            }
            return -1;
        }

        public string GetQrCode(int index)
        {
            if (index < 0 || index >= _totalCount) return _qrCode;
            //return _qrCodePerRow[index] ?? _qrCode;//NhanTo_260907: Lay Ma QR theo Index: Command
            return _qrCodePerRowArr[index].QR ?? _qrCode;//NhanTo_260907: Lay Ma QR theo Index: Add new
        }

        public int GetPrintedCount()
        {
            return _printedCount;
        }

        public int GetWaitingCount()
        {
            return _totalCount - _printedCount;
        }

        //Nhan.To_070926_Them_GetCsvSnapshot_de_optimize_snapshot_khi_saveCSV_giam_lock_contention
        /// <summary>
        /// Trả snapshot CSV: copy trực tiếp từ internal arrays, không qua indexer.
        /// Nhanh hơn nhiều so với gọi this[i] cho từng row.
        /// </summary>
        public (string qr, string nsx, string hsd)[] GetCsvSnapshot()
        {
            var result = new (string qr, string nsx, string hsd)[_totalCount];
            for (int i = 0; i < _totalCount; i++)
            {
                result[i] = (
                    _qrCodePerRowArr[i].QR ?? "",
                    _qrCodePerRowArr[i].NSX ?? "",
                    _qrCodePerRowArr[i].EXP ?? ""
                );
            }
            return result;
        }

        //Nhan.To_070926_Them_WriteCsvDirect_ghi_truc_tiep_tu_internal_arrays_khong_snapshot
        /// <summary>
        /// Ghi CSV trực tiếp từ internal arrays — không snapshot, không alloc, không lock.
        /// Phù hợp dataset lớn (1M+ rows), tiết kiệm ~100MB+ memory.
        /// </summary>
        public void WriteCsvDirect(string path, string delimiter, string[] dataColumns, bool writeHeader)
        {
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                if (writeHeader && dataColumns != null && dataColumns.Length >= 3)
                {
                    var hdr = dataColumns.Take(3).Select(x => Csv.Escape(x ?? ""));
                    sw.WriteLine(string.Join(delimiter, hdr));
                }
                for (int i = 0; i < _totalCount; i++)
                {
                    sw.Write(Csv.Escape(_qrCodePerRowArr[i].QR ?? ""));
                    sw.Write(delimiter);
                    sw.Write(Csv.Escape(_qrCodePerRowArr[i].NSX ?? ""));
                    sw.Write(delimiter);
                    sw.WriteLine(Csv.Escape(_qrCodePerRowArr[i].EXP ?? ""));
                }
            }
        }

        public void SetPrinted(int index, string time)
        {
            if (index < 0 || index >= _totalCount) return;
            if (_status[index] == StatusWaiting)
                Interlocked.Increment(ref _printedCount);
            _status[index] = StatusPrinted;
            _printTime[index] = time;
            if (index == _nextWaitingIndex)
                _nextWaitingIndex = index + 1;
        }

        public void SetDuplicate(int index)
        {
            if (index < 0 || index >= _totalCount) return;
            if (_status[index] == StatusWaiting)
                Interlocked.Increment(ref _printedCount);
            _status[index] = StatusDuplicate;
        }

        public void SetSent(int index, string time)
        {
            if (index < 0 || index >= _totalCount) return;
            //if (_status[index] == StatusWaiting)
            //    Interlocked.Increment(ref _printedCount);
            _status[index] = StatusSent;
            _sendTime[index] = time;
        }

        /// <summary>
        /// Đổi QR cho tất cả dòng Waiting.
        /// updateAllWaiting=true: update TẤT CẢ Waiting (dùng khi start job mới — printer đã CLPB).
        /// updateAllWaiting=false: chỉ update Waiting chưa gửi (sendTime==null) — dùng khi midnight reset (printer còn buffer).
        /// </summary>
        public void UpdateQrCode(string qrCode, bool updateAllWaiting = false)
        {
            _qrCode = qrCode ?? "";
            for (int i = 0; i < _totalCount; i++)
                if (_status[i] == StatusWaiting)
                {
                    if (updateAllWaiting || _sendTime[i] == null)
                        //_qrCodePerRow[i] = _qrCode;//NhanTo_260907_1003: Command
                        _qrCodePerRowArr[i].QR = _qrCode;//NhanTo_260907_1003: Add new
                }
        }

        /// <summary>
        /// Đổi NSX/HSD cho tất cả dòng Waiting.
        /// updateAllWaiting=true: update TẤT CẢ Waiting (dùng khi start job mới — printer đã CLPB).
        /// updateAllWaiting=false: chỉ update Waiting chưa gửi (sendTime==null) — dùng khi midnight reset.
        /// </summary>
        public void UpdateNsxHsd(string nsx, string hsd, bool updateAllWaiting = false)
        {
            _nsx = nsx ?? "";
            _hsd = hsd ?? "";
            for (int i = 0; i < _totalCount; i++)
                if (_status[i] == StatusWaiting)
                {
                    if (updateAllWaiting || _sendTime[i] == null)
                    {
                        //_nsxPerRow[i] = _nsx;//NhanTo_260907_1003: Command
                        //_hsdPerRow[i] = _hsd;//NhanTo_260907_1003: Command
                        //_qrCodePerRow[i] = _qrCode;//NhanTo_260907_1003: Command
                        _qrCodePerRowArr[i].NSX = _nsx;//NhanTo_260907_1003: Add new
                        _qrCodePerRowArr[i].EXP = _hsd;//NhanTo_260907_1003: Add new
                    }
                }
        }
        /// <summary>
        /// Doi QR + NSX/HSD trong 1 lan duyet — dam bao dong bo.
        /// </summary>
        /// 

      //  private readonly object _updateLock = new object();
        public void UpdateQrCodeAndNsxHsd(string qrCode, string nsx, string hsd, bool updateAllWaiting = false)
        {
            _qrCode = qrCode ?? "";
            _nsx = nsx ?? "";
            _hsd = hsd ?? "";
           
                for (int i = 0; i < _totalCount; i++)
                {
                    if (_status[i] != StatusWaiting) continue;
                    if (!updateAllWaiting && _sendTime[i] != null) continue;
                    //
                    //_qrCodePerRow[i] = _qrCode;//NhanTo_260907_1003: Command
                    //_nsxPerRow[i] = _nsx;//NhanTo_260907_1003: Command
                    //_hsdPerRow[i] = _hsd;//NhanTo_260907_1003: Command
                    //
                    _qrCodePerRowArr[i].QR = _qrCode;//NhanTo_260907_1003: Add new
                    _qrCodePerRowArr[i].NSX = _nsx;//NhanTo_260907_1003: Add new
                    _qrCodePerRowArr[i].EXP = _hsd;//NhanTo_260907_1003: Add new
                }
            
        }

        public void SetNsx(int index, string nsx)
        {
            if (index >= 0 && index < _totalCount)
                //_nsxPerRow[index] = nsx ?? "";//NhanTo_260907_1003: Command
                _qrCodePerRowArr[index].NSX = nsx ?? "";//NhanTo_260907_1003: Add new
        }
        public void SetHsd(int index, string hsd)
        {
            if (index >= 0 && index < _totalCount)
                //_hsdPerRow[index] = hsd ?? "";//NhanTo_260907_1003: Command
                _qrCodePerRowArr[index].EXP = hsd ?? "";//NhanTo_260907_1003: Add new
        }
        public void SetQrCode(int index, string qrCode)
        {
            if (index >= 0 && index < _totalCount)
                //_qrCodePerRow[index] = qrCode ?? "";//NhanTo_260907_1003: Command
                _qrCodePerRowArr[index].QR = qrCode ?? "";//NhanTo_260907_1003: Add new
        }

        public void ResetNextWaitingIndex()
        {
            _nextWaitingIndex = 0;
        }

        public bool IsIndexWaiting(int index)
        {
            return index >= 0 && index < _totalCount && _status[index] == StatusWaiting;
        }

        public byte GetStatus(int index)
        {
            if (index < 0 || index >= _totalCount) return StatusWaiting;
            return _status[index];
        }

        public void SetStatus(int index, string status)
        {
            if (index < 0 || index >= _totalCount) return;
            byte old = _status[index];
            _status[index] = StringToStatus(status);
            if (old == StatusWaiting && _status[index] != StatusWaiting)
                Interlocked.Increment(ref _printedCount);
            else if (old != StatusWaiting && _status[index] == StatusWaiting)
                Interlocked.Decrement(ref _printedCount);
        }

        public void SetSendTime(int index, string time)
        {
            if (index >= 0 && index < _totalCount)
                _sendTime[index] = time;
        }

        public void SetPrintTime(int index, string time)
        {
            if (index >= 0 && index < _totalCount)
                _printTime[index] = time;
        }

        public string GetSendTime(int index)
        {
            if (index >= 0 && index < _totalCount)
                return _sendTime[index] ?? "";
            return "";
        }

        public string GetPrintTime(int index)
        {
            if (index >= 0 && index < _totalCount)
                return _printTime[index] ?? "";
            return "";
        }

        // ── static helpers ────────────────────────────────────────────────
        private static string StatusToString(byte s)
        {
            switch (s)
            {
                case StatusPrinted: return "Printed";
                case StatusDuplicate: return "Duplicate";
                case StatusReprint: return "Reprint";
                case StatusSent: return "Sent";
                default: return "Waiting";
            }
        }

        private static byte StringToStatus(string s)
        {
            if (s == null) return StatusWaiting;
            switch (s)
            {
                case "Printed": return StatusPrinted;
                case "Duplicate": return StatusDuplicate;
                case "Reprint": return StatusReprint;
                case "Sent": return StatusSent;
                default: return StatusWaiting;
            }
        }

        // ── IList implementation ─────────────────────────────────────────
        public int IndexOf(string[] item) => -1;
        public void Insert(int index, string[] item) { }
        public void RemoveAt(int index) { }
        public void Add(string[] item) { }
        public void Clear()
        {
            for (int i = 0; i < _totalCount; i++)
            {
                _status[i] = StatusWaiting;
                _sendTime[i] = null;
                _printTime[i] = null;
            }
            _nextWaitingIndex = 0;
            _printedCount = 0;
        }
        public bool Contains(string[] item) => false;
        public void CopyTo(string[][] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < _totalCount)
                throw new ArgumentException("Destination array is not long enough.");

            for (int i = 0; i < _totalCount; i++)
                array[arrayIndex + i] = this[i];
        }
        public bool Remove(string[] item) => false;

        public IEnumerator<string[]> GetEnumerator()
        {
            for (int i = 0; i < _totalCount; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class PrinterResponseData
    {
        public int RowIndex { get; set; } = -1;
        public string Data { get; set; }
    }

    internal class QRObjectCls
    {
        private string _QR = "";
        private string _NSX = "";
        private string _EXP = "";
        private string _Time = "";
        private string _LineName = "";
        //
        public string QR
        {
            get {  return _QR; }
            set { _QR = value; }
        }

        public string NSX
        {
            get { return _NSX; }
            set { _NSX = value; }
        }

        public string EXP
        {
            get { return _EXP; }
            set { _EXP = value; }
        }

        public string Time
        {
            get { return _Time; }
            set { _Time = value; }
        }

        public string LineName
        {
            get { return _LineName; }
            set { _LineName = value; }
        }

    }
}
