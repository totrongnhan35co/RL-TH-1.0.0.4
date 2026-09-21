using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonVariable
{
    /// <summary>
    /// @Author: TrangDong
    /// Variables use for all project
    /// </summary>
    public class CommVariables
    {
        private static string PathProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        private static string _cachedPathProgramDataApp = null;
        public static string PathProgramDataApp
        {
            get
            {
                if (_cachedPathProgramDataApp != null) return _cachedPathProgramDataApp;
                try
                {
                    string cfgFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datapath.cfg");
                    if (File.Exists(cfgFile))
                    {
                        string customPath = null;
                        foreach (var line in File.ReadLines(cfgFile))
                        {
                            string trimmed = line.Trim();
                            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                                continue;
                            customPath = trimmed;
                            break;
                        }
                        if (!string.IsNullOrEmpty(customPath))
                        {
                            _cachedPathProgramDataApp = customPath.TrimEnd('\\') + "\\";
                            return _cachedPathProgramDataApp;
                        }
                    }
                }
                catch { }
               //_cachedPathProgramDataApp = PathProgramData + "\\R-Link\\"; // @"D:\DataRLink\ProgramData\\";
                _cachedPathProgramDataApp = @"D:\DataRLink\ProgramData\\";
                return _cachedPathProgramDataApp;
            }
        }

        public static string PathSettingsApp
        {
            get
            {
                return PathProgramDataApp + "Settings\\";
            }
        }
        public static string PathDatabaseApp
        {
            get
            {
                return PathProgramDataApp + "Database\\";
            }
        }
        public static string PathHistoriesApp
        {
            get
            {
                return PathProgramDataApp + "Histories\\";
            }
        }

        public static string PathAccountsApp
        {
            get
            {
                return PathProgramDataApp + "Accounts\\";
            }
        }

        public static string PathJobsApp
        {
            get
            {
                return PathProgramDataApp + "Jobs\\";
            }
        }
        public static string PathJobsCheckApp
        {
            get
            {
                return PathProgramDataApp + "JobCheck\\";
            }
        }
        public static string PathWarehouseInput
        {
            get
            {
                return PathProgramDataApp + "WarehouseInput\\";
            }
        }
        public static string PathCheckedResult
        {
            get
            {
                return PathProgramDataApp + "CheckedResult\\";
            }
        }
        public static string PathSentDataChecked
        {
            get
            {
                return PathProgramDataApp + "PathSendDataChecked\\";
            }
        }

        public static string PathSentDataPallet
        {
            get
            {
                return PathProgramDataApp + "PathSentDataPallet\\";
            }
        }

        public static string PathSentDataCargo
        {
            get
            {
                return PathProgramDataApp + "PathSentDataCargo\\";
            }
        }

        public static string PathPrintedResponse
        {
            get
            {
                return PathProgramDataApp + "PrintedResponse\\";
            }
        }
        public static string PathSentDataPrinted
        {
            get
            {
                return PathProgramDataApp + "PathSentDataPrinted\\";
            }
        }
        public static string PathAllValues
        {
            get
            {
                return PathProgramDataApp + "PathAllValues\\";
            }
        }
        public static string PathAllowPC
        {
            get
            {
                return PathProgramDataApp + "RConfig\\";
            }
        }
        public static string PathExportTemplates
        {
            get
            {
                return PathProgramDataApp + "PathExportTemplates\\";
            }
        }
        // ── THTrueMilk / R-Link logs ──────────────────────────────────────
        public static string PathRLinkLogCamera
        {
            get { return PathProgramDataApp + "RLinkLogs\\Camera\\"; }
        }
        public static string PathRLinkLogPrint
        {
            get { return PathProgramDataApp + "RLinkLogs\\Print\\"; }
        }
        public static string PathRLinkConfig
        {
            get { return PathProgramDataApp + "RLinkLogs\\Config\\"; }
        }
        public static string PathRLinkQrCodes
        {
            get { return PathProgramDataApp + "RLinkLogs\\QrCodes\\"; }
        }
        public static string PathRLinkProducts
        {
            get { return PathProgramDataApp + "RLinkLogs\\Products\\"; }
        }
        public static string PathRLinkMonitor
        {
            get { return PathProgramDataApp + "RLinkLogs\\Monitor\\"; }
        }

        public static string PathImagesError
        {
            get { return PathProgramDataApp + "ImagesError\\"; }
        }

        public static string PathProductImages
        {
            get { return PathProgramDataApp + "ProductImages\\"; }
        }




    }
}
