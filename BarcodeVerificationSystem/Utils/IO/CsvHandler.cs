using System.IO;
using System.Text.RegularExpressions;

namespace BarcodeVerificationSystem.Controller
{
    public sealed class CsvReader : System.IDisposable
    {
        public CsvReader(string fileName) : this(new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
        }
        public CsvReader(Stream stream)
        {
            __reader = new StreamReader(stream);
        }
        public System.Collections.IEnumerable RowEnumerator
        {
            get
            {
                if (null == __reader)
                    throw new System.ApplicationException("I can't start reading without CSV input.");

                __rowno = 0;
                string sLine;
                string sNextLine;

                while (null != (sLine = __reader.ReadLine()))
                {
                    while (rexRunOnLine.IsMatch(sLine) && null != (sNextLine = __reader.ReadLine()))
                        sLine += "\n" + sNextLine;

                    __rowno++;
                    string[] values = rexCsvSplitter.Split(sLine);

                    for (int i = 0; i < values.Length; i++)
                        values[i] = Csv.Unescape(values[i]);

                    yield return values;
                }

                __reader.Close();
            }
        }

        public long RowIndex { get { return __rowno; } }

        public void Dispose()
        {
            __reader?.Dispose();
        }

        private long __rowno = 0;
        private readonly TextReader __reader;
        private static readonly Regex rexCsvSplitter = new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))");
        private static readonly Regex rexRunOnLine = new Regex(@"^[^""]*(?:""[^""]*""[^""]*)*""[^""]*$");
    }
    public sealed class CsvWriter : System.IDisposable
    {
        public CsvWriter(string fileName) : this(new FileStream(fileName, FileMode.Append, FileAccess.Write))
        {
        }

        public CsvWriter(Stream stream)
        {
            __writter = new StreamWriter(stream);
        }

        public void RowWriterator(string[] sLines)
        {
            if (null == __writter)
                throw new System.ApplicationException("I can't start writting without CSV input.");

            for (int i = 0; i < sLines.Length; i++)
            {
                sLines[i] = Csv.Escape(sLines[i]);
            }

            __writter.WriteLine(string.Join(",", sLines));
        }

        public long RowIndex { get { return __rowno; } }

        public void Dispose()
        {
            __writter?.Dispose();
        }

        private readonly long __rowno = 0;
        private readonly TextWriter __writter;
    }
    public static class Csv
    {
        public static string Escape(string s)
        {
            if (s.Contains(QUOTE))
                s = s.Replace(QUOTE, ESCAPED_QUOTE);
            if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                s = QUOTE + s + QUOTE;
            return s;
        }
        public static string Unescape(string s)
        {
            if (s == null) return null;
            if (s.StartsWith(QUOTE) && s.EndsWith(QUOTE))
            {
                s = s.Substring(1, s.Length - 2);

                if (s.Contains(ESCAPED_QUOTE))
                    s = s.Replace(ESCAPED_QUOTE, QUOTE);
            }

            return s;
        }

        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static readonly char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', ';','"', '\n' };
    }
}
