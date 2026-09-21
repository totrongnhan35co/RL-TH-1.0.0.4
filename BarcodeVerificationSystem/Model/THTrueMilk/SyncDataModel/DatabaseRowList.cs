using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BarcodeVerificationSystem.Model.THTrueMilk
{
    internal class DatabaseRowList : IList<string[]>, IDisposable
    {
        private readonly List<long> _offsets; private readonly string _filePath;
        private readonly char[] _status; private readonly string[] _sent; private readonly string[] _printed;
        private readonly int _colCount; private FileStream _fs;
        public DatabaseRowList(List<long> offsets, string filePath, char[] status, string[] sent, string[] printed, int colCount)
        { _offsets = offsets; _filePath = filePath; _status = status; _sent = sent; _printed = printed; _colCount = colCount; }
        public string[] this[int index]
        {
            get
            {
                if (index < 0 || index >= _offsets.Count) return new string[_colCount];
                var stream = GetStream(); lock (stream)
                {
                    stream.Seek(_offsets[index], SeekOrigin.Begin);
                    var rdr = new StreamReader(stream, Encoding.UTF8, false, 4096, leaveOpen: true);
                    string raw = rdr.ReadLine(); if (raw == null) return new string[_colCount];
                    var f = SplitLine(raw); var row = new string[_colCount];
                    row[0] = (index + 1).ToString(); row[1] = StatStr(_status?[index] ?? 'W');
                    for (int i = 2; i < _colCount - 4 && i - 2 < f.Length; i++) row[i] = f[i - 2];
                    int extra = f.Length - (_colCount - 6); int b = extra > 0 ? extra : 0;
                    if (extra > 0) row[_colCount - 4] = f[f.Length - 4 + b] ?? "";
                    if (extra > 1) row[_colCount - 3] = f[f.Length - 3 + b] ?? "";
                    string s = _sent?[index] ?? ""; string p = _printed?[index] ?? "";
                    row[_colCount - 2] = !string.IsNullOrEmpty(s) ? s : (extra > 2 ? (f[f.Length - 2 + b] ?? "") : "");
                    row[_colCount - 1] = !string.IsNullOrEmpty(p) ? p : (extra > 3 ? (f[f.Length - 1 + b] ?? "") : "");
                    return row;
                }
            }
            set { }
        }
        public int Count => _offsets.Count; public bool IsReadOnly => false;
        private FileStream GetStream() { if (_fs == null) _fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536); return _fs; }
        private static string[] SplitLine(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return Array.Empty<string>();
            var l = new List<string>(); bool q = false; int s = 0;
            for (int i = 0; i < raw.Length; i++) { if (raw[i] == '"') q = !q; else if (raw[i] == ',' && !q) { l.Add(raw.Substring(s, i - s).Trim('"')); s = i + 1; } }
            if (s <= raw.Length) l.Add(raw.Substring(s).Trim('"')); return l.ToArray();
        }
        private static string StatStr(char c) { if (c == 'P') return "Printed"; if (c == 'S') return "Sent"; if (c == 'D') return "Duplicate"; if (c == 'R') return "Reprint"; return "Waiting"; }
        public void Dispose() { _fs?.Dispose(); _fs = null; }
        public int IndexOf(string[] item) => -1; public void Insert(int i, string[] v) { } public void RemoveAt(int i) { }
        public void Add(string[] v) { } public void Clear() { } public bool Contains(string[] v) => false;
        public void CopyTo(string[][] a, int ai) { } public bool Remove(string[] v) => false;
        public IEnumerator<string[]> GetEnumerator() => new En(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        private class En : IEnumerator<string[]> { private readonly DatabaseRowList _l; private int _i = -1; public En(DatabaseRowList l) => _l = l; public string[] Current => _l[_i]; object IEnumerator.Current => Current; public bool MoveNext() => ++_i < _l.Count; public void Reset() => _i = -1; public void Dispose() { } }
    }
}
