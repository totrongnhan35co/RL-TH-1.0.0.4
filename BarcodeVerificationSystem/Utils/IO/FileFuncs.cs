using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace BarcodeVerificationSystem.Utils
{
    public class FileFuncs
    {
        // Static constructor to register encoding provider (required for ExcelDataReader)
        static FileFuncs()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static List<string> GetFirstColumn(List<string> list)
        {
            return list
                .Select(x =>
                {
                    var parts = x.Split(',');
                    return parts.Length > 0 ? parts[0] : x;
                })
                .ToList();
        }

        public static List<string[]> ReadCodeData(string fullFilePath, char delimiter = ',')
        {
            var result = new List<string[]>();

            if (string.IsNullOrWhiteSpace(fullFilePath) || !File.Exists(fullFilePath))
                return result;

            try
            {
                using (var fs = new FileStream(
                    fullFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var values = line.Split(delimiter);
                            result.Add(values);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file at {fullFilePath}: {ex.Message}");
            }

            return result;
        }

        public static void WriteStringListToCsv(List<string> list, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var lineCode in list)
                    {
                        writer.WriteLine(lineCode);
                    }
                }
                Console.WriteLine($"Successfully wrote list to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to CSV file: {ex.Message}");
            }
        }

        public static List<string> ReadStringListFromCsv(string filePath)
        {
            var result = new List<string>();

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        result.Add(line);
                    }
                }
                Console.WriteLine($"Successfully read list from {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV file: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Reads Excel file (.xlsx or .xls) and returns rows as List of string arrays
        /// Uses ExcelDataReader which supports both .xlsx and .xls formats
        /// </summary>
        /// <param name="fullFilePath">Full path to Excel file</param>
        /// <param name="sheetName">Sheet name to read (default: first sheet)</param>
        /// <returns>List of string arrays representing rows</returns>
        private static readonly object _readLock = new object();

        public static List<string[]> ReadExcelData(string fullFilePath, string sheetName = null)
        {
            var result = new List<string[]>();

            if (string.IsNullOrWhiteSpace(fullFilePath) || !File.Exists(fullFilePath))
                return result;

            try
            {
                // Lock to ensure only one thread executes the ExcelDataReader logic at a time
                lock (_readLock)
                {
                    using (var stream = File.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                {
                                    UseHeaderRow = false
                                }
                            });

                            if (dataSet == null || dataSet.Tables.Count == 0)
                            {
                                System.Diagnostics.Trace.WriteLine($"Error: Excel file '{fullFilePath}' has no worksheets");
                                return result;
                            }

                            DataTable table;
                            if (string.IsNullOrWhiteSpace(sheetName))
                            {
                                table = dataSet.Tables[0];
                            }
                            else
                            {
                                table = dataSet.Tables[sheetName];
                                if (table == null)
                                {
                                    System.Diagnostics.Trace.WriteLine($"Sheet '{sheetName}' not found in Excel file '{fullFilePath}'");
                                    return result;
                                }
                            }

                            foreach (DataRow row in table.Rows)
                            {
                                var rowValues = new List<string>();
                                for (int col = 0; col < table.Columns.Count; col++)
                                {
                                    object value = row[col];
                                    string stringValue = ConvertCellValue(value);
                                    rowValues.Add(stringValue);
                                }

                                if (rowValues.Any(v => !string.IsNullOrWhiteSpace(v)))
                                {
                                    result.Add(rowValues.ToArray());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Use thread-safe tracing/logging instead of Console
                System.Diagnostics.Trace.WriteLine($"Error reading Excel file at {fullFilePath}: {ex.Message}");
            }

            return result;
        }
        /// <summary>
        /// Gets the first row (header or first data row) from Excel file
        /// </summary>
        /// <param name="fullFilePath">Full path to Excel file</param>
        /// <param name="hasHeader">Whether first row is header (default: true)</param>
        /// <returns>String array representing first row</returns>

        public static string[] GetFirstRowFromExcel(string fullFilePath, bool hasHeader = true)
        {
            // Note: The 'hasHeader' parameter is currently unused in logic.
            // If you intend to skip the header or use it as column names, adjust accordingly.

            if (string.IsNullOrWhiteSpace(fullFilePath) || !File.Exists(fullFilePath))
                return new string[0];

            try
            {
                // Serialize access to ExcelDataReader to avoid known concurrency issues in the library
                lock (_readLock)
                {
                    using (var stream = File.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            // Read the first data row (header if present)
                            if (reader.Read())
                            {
                                var rowValues = new List<string>();
                                for (int col = 0; col < reader.FieldCount; col++)
                                {
                                    object value = reader.GetValue(col);
                                    string stringValue = ConvertCellValue(value);
                                    rowValues.Add(stringValue);
                                }
                                return rowValues.ToArray();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Thread-safe logging alternative
                System.Diagnostics.Trace.WriteLine($"Error reading first row from Excel file at {fullFilePath}: {ex.Message}");
            }

            return new string[0];
        }
        /// <summary>
        /// Converts cell value to string, handling different types
        /// Also unescapes XML-encoded control characters like _x001d_ back to actual characters
        /// </summary>
        private static string ConvertCellValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            string result;

            if (value is DateTime dt)
                result = dt.ToString("yyyy-MM-dd HH:mm:ss");
            else if (value is double d)
                result = d.ToString();
            else if (value is float f)
                result = f.ToString();
            else if (value is decimal dec)
                result = dec.ToString();
            else
                result = value.ToString() ?? string.Empty;

            // Unescape XML-encoded control characters (e.g., _x001d_ -> GS character)
            result = UnescapeXmlControlCharacters(result);

            return result;
        }

        /// <summary>
        /// Unescapes XML-encoded control characters in Excel strings.
        /// Excel escapes control characters as _xHHHH_ where HHHH is hex code.
        /// For example: _x001d_ represents GS (Group Separator) character (0x1D)
        /// </summary>
        private static string UnescapeXmlControlCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Pattern matches _xHHHH_ where HHHH is a 4-digit hex number
            // This is how Excel encodes control characters in Open XML format
            return Regex.Replace(input, @"_x([0-9A-Fa-f]{4})_", match =>
            {
                string hexValue = match.Groups[1].Value;
                int charCode = Convert.ToInt32(hexValue, 16);
                return ((char)charCode).ToString();
            });
        }
    }
}
