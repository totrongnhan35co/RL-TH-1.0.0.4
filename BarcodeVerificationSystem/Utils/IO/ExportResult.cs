using BarcodeVerificationSystem.Model;
using CommonVariable;
using DesignUI.CuzAlert;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Controller
{
    public class ExportResult
    {
        public static void ExportNewResult(JobModel job)
        {
            var pathBackupPrintedResponse = CommVariables.PathPrintedResponse + job.PrintedResponePath;
            var pathBackupCheckedResponse = CommVariables.PathCheckedResult + job.CheckedResultPath;

            //var printedNameFilePath = SharedPaths.PathSubJobsApp + $"{index + 1}\\" + "printedPathString";
            //var checkedNameFilePath = SharedPaths.PathCheckedResult + $"Job{index + 1}\\" + "checkedPathString";

            //var namePrintedFilePath = SharedPaths.PathPrintedResponse + $"Job{index + 1}\\"
            //    + SharedFunctions.ReadStringOfPrintedResponePath(printedNameFilePath);
            //var nameCheckedFilePath = SharedPaths.PathCheckedResult + $"Job{index + 1}\\" +
            //    SharedFunctions.ReadStringOfPrintedResponePath(checkedNameFilePath);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                //var outputCsvPath = saveFileDialog.FileName;
                saveFileDialog.Filter = "CSV file (*.csv)|*.csv|Text files (*.txt)|*.txt"; // Set the filter to allow only CSV files
                //"CSV file (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*"
                saveFileDialog.DefaultExt = "csv"; // Set the default file extension to csv
                var outputCsvPath = saveFileDialog.FileName;
                if (!outputCsvPath.EndsWith(".csv") && !outputCsvPath.EndsWith(".txt")) // Ensure the file name ends with .csv
                {
                    outputCsvPath += ".csv";
                }
                try
                {
                    ExportNewFile(pathBackupPrintedResponse, pathBackupCheckedResponse, outputCsvPath);
                }
                catch (IOException file)
                {
                    //CusMsgBox.Show("File not found !", "File Error", Enums.ViewEnums.ButtonStyleMessageBox.OK, Enums.ViewEnums.ImageStyleMessageBox.Warning);
                }
                catch (Exception)
                {

                }
            }
            // load csv from above pah
        }


        public static void ExportNewFile(string namePrintedFilePath, string nameCheckedFilePath, string outputCsvPath)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            // Load data from CSVs

            var firstCsvLines = File.ReadAllLines(namePrintedFilePath);
            var secondCsvLines = File.ReadAllLines(nameCheckedFilePath);

            // Find column indices based on headers
            var firstHeaders = firstCsvLines[0].Split(',');
            var secondHeaders = secondCsvLines[0].Split(',');

            int statusIndex = Array.IndexOf(firstHeaders, "Status");
            int resultIndex = Array.IndexOf(secondHeaders, "Result");

            if (statusIndex == -1 || resultIndex == -1)
            {
                Console.WriteLine("Error: Required columns not found.");
                return;
            }
            // Dictionary to store second CSV results
            Dictionary<string, string> secondCsvResults = secondCsvLines.Skip(1)
                .Select(line => line.Split(','))
                .ToDictionary(
                    line => line[0], // Assuming Index is the key
                    line => line[resultIndex]
                );

            // Prepare the output CSV
            List<string> outputCsv = new List<string> { firstCsvLines[0] }; // Add headers

            // Process records from the first CSV
            foreach (var line in firstCsvLines.Skip(1))
            {
                string[] fields = line.Split(',');
                string status = fields[statusIndex];
                string result = secondCsvResults.ContainsKey(fields[0]) ? secondCsvResults[fields[0]] : null;

                // Apply conditions to modify the 'Status' column
                if (status == "Printed" && result == "Valid")
                {
                    fields[statusIndex] = "Valid";
                }
                else if (status == "Printed" && result == "Duplicate")
                {
                    fields[statusIndex] = "Duplicate";
                }
                outputCsv.Add(string.Join(",", fields));
            }

            // Write the output CSV
            File.WriteAllLines(outputCsvPath, outputCsv);
          //  CusAlert.Show($"Export File Successfully", ImageStyleMessageBox.Info, true);
            sw.Stop();
            Debug.WriteLine($"The Export Result is done within {sw.ElapsedMilliseconds} ms");
        }
    }
}
