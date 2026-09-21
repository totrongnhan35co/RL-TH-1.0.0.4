//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;

//namespace BarcodeVerificationSystem.Model.THTrueMilk
//{
//    /// <summary>
//    /// Lưu trữ database barcode theo dạng virtual cho Mode 1/2 (BatchOneQrCode / AutoRefreshByTime).
//    /// Chỉ giữ 1 prototype QR + NSX + HSD + mảng byte status (3.5 MB cho 3.5M dòng)
//    /// thay vì List&lt;string[]&gt; (&gt; 2 GB).
//    ///
//    /// Per-row QR/NSX/HSD: giữ lịch sử đúng cho mã đã gửi/đã in, chỉ đổi mã Waiting.
//    /// </summary>
//    internal class PrintedCodeVirtualList_BK260709 : IList<string[]>
//    {
//        // ── internal state ──────────────────────────────────────────────
//        private readonly int _totalCount;
//        private readonly byte[] _status;      // 0=Waiting, 1=Printed, 2=Duplicate, 3=Reprint, 4=Sent
//        private readonly string[] _sendTime;  // sparse — null nếu chưa gửi
//        private readonly string[] _printTime; // sparse — null nếu chưa in
//        // Current values (dùng cho reference, UI labels)
//        private string _qrCode;
//        private string _nsx;
//        private string _hsd;
//        // Per-row storage — giữ lịch sử riêng cho từng dòng
//        private readonly string[] _qrCodePerRow;
//        private readonly string[] _nsxPerRow;
//        private readonly string[] _hsdPerRow;
//        private readonly QRObjectCls[] _qrCodePerRowArr;//NhanTo_260907_1003 
//        private readonly int _fieldCount;     // số cột data (5: STT, Status, QR, NSX, HSD)
//        private readonly int _displayFieldCount; // số cột hiển thị (7: + T.Gửi, T.In)

//        // tuần tự cho FindNextWaiting
//        private int _nextWaitingIndex;
//        private int _printedCount;

//        // ── constants ─────────────────────────────────────────────────────
//        private const byte StatusWaiting = 0;
//        private const byte StatusPrinted = 1;
//        private const byte StatusDuplicate = 2;
//        private const byte StatusReprint = 3;
//        private const byte StatusSent = 4;

//        // ── constructor ─────────────────────────────────────────────────
//        public PrintedCodeVirtualList_BK260709(int totalCount, string qrCode, string nsx, string hsd, int fieldCount, int displayFieldCount = 7)
//        {
//            if (totalCount < 0) throw new ArgumentOutOfRangeException(nameof(totalCount));
//            _totalCount = totalCount;
//            _status = new byte[totalCount]; // tất cả = 0 (Waiting)
//            _sendTime = new string[totalCount];
//            _printTime = new string[totalCount];
//            _qrCode = qrCode ?? "";
//            _nsx = nsx ?? "";
//            _hsd = hsd ?? "";
//            _fieldCount = fieldCount;
//            _displayFieldCount = displayFieldCount;
//            _nextWaitingIndex = 0;
//            _printedCount = 0;

//            // Per-row: fill tất cả với giá trị ban đầu
//            _qrCodePerRow = new string[totalCount];
//            _nsxPerRow = new string[totalCount];
//            _hsdPerRow = new string[totalCount];
//            for (int i = 0; i < totalCount; i++)
//            {
//                _qrCodePerRow[i] = _qrCode;
//                _nsxPerRow[i] = _nsx;
//                _hsdPerRow[i] = _hsd;
//            }
//        }

//        // ── indexer ───────────────────────────────────────────────────────
//        public string[] this[int index]
//        {
//            get
//            {
//                if (index < 0 || index >= _totalCount)
//                    return new string[_displayFieldCount];
//                lock (_updateLock)
//                {
//                    var row = new string[_displayFieldCount];
//                    row[0] = (index + 1).ToString();
//                    row[1] = StatusToString(_status[index]);
//                    if (_displayFieldCount > 2) row[2] = _qrCodePerRow[index];
//                    if (_displayFieldCount > 3) row[3] = _nsxPerRow[index];
//                    if (_displayFieldCount > 4) row[4] = _hsdPerRow[index];
//                    if (_displayFieldCount > 5) row[5] = _sendTime[index] ?? "";
//                    if (_displayFieldCount > 6) row[6] = _printTime[index] ?? "";
//                    return row;
//                }
//            }
//            set
//            {
//                if (index < 0 || index >= _totalCount || value == null) return;
//                byte oldStatus = _status[index];
//                if (value.Length > 1)
//                {
//                    _status[index] = StringToStatus(value[1]);
//                    // cập nhật đếm nhanh
//                    if (oldStatus == StatusWaiting && _status[index] != StatusWaiting)
//                        Interlocked.Increment(ref _printedCount);
//                    else if (oldStatus != StatusWaiting && _status[index] == StatusWaiting)
//                        Interlocked.Decrement(ref _printedCount);
//                }
//                if (value.Length > 5) _sendTime[index] = value[5];
//                if (value.Length > 6) _printTime[index] = value[6];
//                // cập nhật _nextWaitingIndex nếu index đang bị đánh dấu Printed
//                if (_status[index] != StatusWaiting && index == _nextWaitingIndex)
//                    _nextWaitingIndex = index + 1;
//            }
//        }

//        // ── count ─────────────────────────────────────────────────────────
//        public int Count => _totalCount;
//        public bool IsReadOnly => false;
//        public string QrCode => _qrCode;
//        public string Nsx => _nsx;
//        public string Hsd => _hsd;
//        public int FieldCount => _fieldCount;
//        public int DisplayFieldCount => _displayFieldCount;

//        // ── helpers ───────────────────────────────────────────────────────
//        public int FindNextWaiting()
//        {
//            for (int i = _nextWaitingIndex; i < _totalCount; i++)
//            {
//                if (_status[i] == StatusWaiting)
//                {
//                    _nextWaitingIndex = i;
//                    return i;
//                }
//            }
//            return -1;
//        }

//        public string GetQrCode(int index)
//        {
//            if (index < 0 || index >= _totalCount) return _qrCode;
//            return _qrCodePerRow[index] ?? _qrCode;
//        }

//        public int GetPrintedCount()
//        {
//            return _printedCount;
//        }

//        public int GetWaitingCount()
//        {
//            return _totalCount - _printedCount;
//        }

//        public void SetPrinted(int index, string time)
//        {
//            if (index < 0 || index >= _totalCount) return;
//            if (_status[index] == StatusWaiting)
//                Interlocked.Increment(ref _printedCount);
//            _status[index] = StatusPrinted;
//            _printTime[index] = time;
//            if (index == _nextWaitingIndex)
//                _nextWaitingIndex = index + 1;
//        }

//        public void SetDuplicate(int index)
//        {
//            if (index < 0 || index >= _totalCount) return;
//            if (_status[index] == StatusWaiting)
//                Interlocked.Increment(ref _printedCount);
//            _status[index] = StatusDuplicate;
//        }

//        public void SetSent(int index, string time)
//        {
//            if (index < 0 || index >= _totalCount) return;
//            //if (_status[index] == StatusWaiting)
//            //    Interlocked.Increment(ref _printedCount);
//            _status[index] = StatusSent;
//            _sendTime[index] = time;
//        }

//        /// <summary>
//        /// Đổi QR cho tất cả dòng Waiting.
//        /// updateAllWaiting=true: update TẤT CẢ Waiting (dùng khi start job mới — printer đã CLPB).
//        /// updateAllWaiting=false: chỉ update Waiting chưa gửi (sendTime==null) — dùng khi midnight reset (printer còn buffer).
//        /// </summary>
//        public void UpdateQrCode(string qrCode, bool updateAllWaiting = false)
//        {
//            _qrCode = qrCode ?? "";
//            for (int i = 0; i < _totalCount; i++)
//                if (_status[i] == StatusWaiting)
//                {
//                    if (updateAllWaiting || _sendTime[i] == null)
//                        _qrCodePerRow[i] = _qrCode;
//                }
//        }

//        /// <summary>
//        /// Đổi NSX/HSD cho tất cả dòng Waiting.
//        /// updateAllWaiting=true: update TẤT CẢ Waiting (dùng khi start job mới — printer đã CLPB).
//        /// updateAllWaiting=false: chỉ update Waiting chưa gửi (sendTime==null) — dùng khi midnight reset.
//        /// </summary>
//        public void UpdateNsxHsd(string nsx, string hsd, bool updateAllWaiting = false)
//        {
//            _nsx = nsx ?? "";
//            _hsd = hsd ?? "";
//            for (int i = 0; i < _totalCount; i++)
//                if (_status[i] == StatusWaiting)
//                {
//                    if (updateAllWaiting || _sendTime[i] == null)
//                    {
//                        _nsxPerRow[i] = _nsx;
//                        _hsdPerRow[i] = _hsd;
//                    }
//                }
//        }
//        /// <summary>
//        /// Doi QR + NSX/HSD trong 1 lan duyet — dam bao dong bo.
//        /// </summary>
//        /// 

//        private readonly object _updateLock = new object();
//        public void UpdateQrCodeAndNsxHsd(string qrCode, string nsx, string hsd, bool updateAllWaiting = false)
//        {
//            _qrCode = qrCode ?? "";
//            _nsx = nsx ?? "";
//            _hsd = hsd ?? "";
//            lock (_updateLock)
//            {
//                for (int i = 0; i < _totalCount; i++)
//                {
//                    if (_status[i] != StatusWaiting) continue;
//                    if (!updateAllWaiting && _sendTime[i] != null) continue;
//                    _qrCodePerRow[i] = _qrCode;
//                    _nsxPerRow[i] = _nsx;
//                    _hsdPerRow[i] = _hsd;
//                }
//            }
//        }

//        public void SetNsx(int index, string nsx)
//        {
//            if (index >= 0 && index < _totalCount)
//                _nsxPerRow[index] = nsx ?? "";
//        }
//        public void SetHsd(int index, string hsd)
//        {
//            if (index >= 0 && index < _totalCount)
//                _hsdPerRow[index] = hsd ?? "";
//        }
//        public void SetQrCode(int index, string qrCode)
//        {
//            if (index >= 0 && index < _totalCount)
//                _qrCodePerRow[index] = qrCode ?? "";
//        }

//        public void ResetNextWaitingIndex()
//        {
//            _nextWaitingIndex = 0;
//        }

//        public bool IsIndexWaiting(int index)
//        {
//            return index >= 0 && index < _totalCount && _status[index] == StatusWaiting;
//        }

//        public byte GetStatus(int index)
//        {
//            if (index < 0 || index >= _totalCount) return StatusWaiting;
//            return _status[index];
//        }

//        public void SetStatus(int index, string status)
//        {
//            if (index < 0 || index >= _totalCount) return;
//            byte old = _status[index];
//            _status[index] = StringToStatus(status);
//            if (old == StatusWaiting && _status[index] != StatusWaiting)
//                Interlocked.Increment(ref _printedCount);
//            else if (old != StatusWaiting && _status[index] == StatusWaiting)
//                Interlocked.Decrement(ref _printedCount);
//        }

//        public void SetSendTime(int index, string time)
//        {
//            if (index >= 0 && index < _totalCount)
//                _sendTime[index] = time;
//        }

//        public void SetPrintTime(int index, string time)
//        {
//            if (index >= 0 && index < _totalCount)
//                _printTime[index] = time;
//        }

//        public string GetSendTime(int index)
//        {
//            if (index >= 0 && index < _totalCount)
//                return _sendTime[index] ?? "";
//            return "";
//        }

//        public string GetPrintTime(int index)
//        {
//            if (index >= 0 && index < _totalCount)
//                return _printTime[index] ?? "";
//            return "";
//        }

//        // ── static helpers ────────────────────────────────────────────────
//        private static string StatusToString(byte s)
//        {
//            switch (s)
//            {
//                case StatusPrinted: return "Printed";
//                case StatusDuplicate: return "Duplicate";
//                case StatusReprint: return "Reprint";
//                case StatusSent: return "Sent";
//                default: return "Waiting";
//            }
//        }

//        private static byte StringToStatus(string s)
//        {
//            if (s == null) return StatusWaiting;
//            switch (s)
//            {
//                case "Printed": return StatusPrinted;
//                case "Duplicate": return StatusDuplicate;
//                case "Reprint": return StatusReprint;
//                case "Sent": return StatusSent;
//                default: return StatusWaiting;
//            }
//        }

//        // ── IList implementation ─────────────────────────────────────────
//        public int IndexOf(string[] item) => -1;
//        public void Insert(int index, string[] item) { }
//        public void RemoveAt(int index) { }
//        public void Add(string[] item) { }
//        public void Clear()
//        {
//            for (int i = 0; i < _totalCount; i++)
//            {
//                _status[i] = StatusWaiting;
//                _sendTime[i] = null;
//                _printTime[i] = null;
//            }
//            _nextWaitingIndex = 0;
//            _printedCount = 0;
//        }
//        public bool Contains(string[] item) => false;
//        public void CopyTo(string[][] array, int arrayIndex)
//        {
//            if (array == null) throw new ArgumentNullException(nameof(array));
//            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
//            if (array.Length - arrayIndex < _totalCount)
//                throw new ArgumentException("Destination array is not long enough.");

//            for (int i = 0; i < _totalCount; i++)
//                array[arrayIndex + i] = this[i];
//        }
//        public bool Remove(string[] item) => false;

//        public IEnumerator<string[]> GetEnumerator()
//        {
//            for (int i = 0; i < _totalCount; i++)
//                yield return this[i];
//        }

//        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
//    }

//    internal class PrinterResponseData
//    {
//        public int RowIndex { get; set; } = -1;
//        public string Data { get; set; }
//    }

//    internal class QRObjectCls
//    {
//        public string QR { get; set; }

//        public string NSX { get; set; }

//        public string EXP { get; set; }
//    }
//}
