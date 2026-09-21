using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using CommonVariable;
using System;
using System.IO;

namespace BarcodeVerificationSystem.View.OtherProjects.CenteryIndiaUI
{
    public static class JobCleanupService
    {
        public static void CleanupOldJobs()
        {
            int retentionDays = Shared.Settings?.CenterIndiaModel?.RetentionDays ?? 190;
            DateTime threshold = DateTime.Now.AddDays(-retentionDays);
            string jobsDir = CommVariables.PathJobsApp;
            if (!Directory.Exists(jobsDir)) return;

            foreach (string jobFile in Directory.GetFiles(jobsDir, "*.rvis"))
            {
                if (File.GetLastWriteTime(jobFile) >= threshold) continue;

                JobModel job = JobModel.LoadFile(jobFile);
                if (job == null) { TryDeleteFile(jobFile); continue; }
                DeleteJobData(job);
            }

            string dbDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "R-Link", "Database");
            if (Directory.Exists(dbDir))
                foreach (string f in Directory.GetFiles(dbDir, "*.csv"))
                    if (File.GetLastWriteTime(f) < threshold)
                        TryDeleteFile(f);
        }

        private static void DeleteJobData(JobModel job)
        {
            string ext = Shared.Settings?.JobFileExtension ?? ".rvis";

            TryDeleteFile(CommVariables.PathJobsApp + job.FileName + ext);
            TryDeleteFile(CommVariables.PathCheckedResult + job.CheckedResultPath);
            TryDeleteFile(CommVariables.PathSentDataChecked + job.CheckedResultPath);
            TryDeleteFile(CommVariables.PathSentDataPallet + job.CheckedResultPath);
            TryDeleteFile(CommVariables.PathSentDataCargo + job.CheckedResultPath);
            TryDeleteFile(CommVariables.PathPrintedResponse + job.PrintedResponePath);
            TryDeleteFile(CommVariables.PathSentDataPrinted + job.PrintedResponePath);
            TryDeleteFile(CommVariables.PathAllValues + job.FileName + "_AllValues.csv");
            TryDeleteDirectory(CommVariables.PathImagesError + job.FileName);

            string exportImg = Shared.Settings?.ExportImagePath;
            if (!string.IsNullOrEmpty(exportImg))
                TryDeleteDirectory(Path.Combine(exportImg, job.FileName));
        }

        private static void TryDeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        private static void TryDeleteDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try { if (Directory.Exists(path)) Directory.Delete(path, true); } catch { }
        }
    }
}
