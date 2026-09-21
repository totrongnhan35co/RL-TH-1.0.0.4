using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Win32;

namespace BarcodeVerificationSystem.Services.WindowsUpdate
{
    /// <summary>
    /// Service for checking and managing Windows Update status
    /// </summary>
    public class WindowsUpdateService
    {
        private const string WindowsUpdateServiceName = "wuauserv";
        private const string RegistryPathPendingReboot = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired";
        private const string RegistryPathUpdateSession = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\OSUpgrade";
        
        // Event to notify when imminent restart is detected
        public event EventHandler<WindowsUpdateStatus> ImminentRestartDetected;
        
        private Thread _monitoringThread;
        private bool _isMonitoring = false;
        private DateTime _lastRestartCheck = DateTime.MinValue;

        /// <summary>
        /// Checks the current Windows Update status
        /// </summary>
        /// <returns>WindowsUpdateStatus object containing update information</returns>
        public WindowsUpdateStatus CheckUpdateStatus()
        {
            var status = new WindowsUpdateStatus
            {
                LastCheckTime = DateTime.Now
            };

            try
            {
                // Check if Windows Update service is running
                status.UpdateServiceRunning = IsWindowsUpdateServiceRunning();

                // Check for pending reboot
                status.RequiresRestart = CheckPendingReboot();

                // Check for pending updates
                status.HasPendingUpdates = CheckPendingUpdates();

                // Count pending updates
                status.PendingUpdateCount = GetPendingUpdateCount();

                // Determine if we can automatically handle the situation
                status.CanAutomaticallyHandle = DetermineAutomaticHandling(status);

                // Generate status message
                status.StatusMessage = GenerateStatusMessage(status);
            }
            catch (Exception ex)
            {
                status.StatusMessage = $"Error checking Windows Update status: {ex.Message}";
                status.CanAutomaticallyHandle = false;
            }

            return status;
        }

        /// <summary>
        /// Attempts to automatically handle Windows Update state to prevent unexpected restarts
        /// </summary>
        /// <param name="status">Current update status</param>
        /// <returns>True if automatic handling was successful, false otherwise</returns>
        public bool TryAutomaticHandling(WindowsUpdateStatus status)
        {
            try
            {
                // Always try to disable/prevent automatic restarts
                DisableAutomaticRestart();
                
                // If Windows Update service is not running, try to start it
                if (!status.UpdateServiceRunning)
                {
                    StartWindowsUpdateService();
                }

                // If restart is required, try to postpone it aggressively
                if (status.RequiresRestart)
                {
                    // Try multiple methods to postpone restart
                    PostponeAutomaticRestart();
                    
                    // Also try to stop Windows Update service to prevent forced restart
                    StopWindowsUpdateService();
                }
                else
                {
                    // For non-critical cases, try to configure settings to prevent automatic restarts
                    DisableAutomaticRestart();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if Windows Update service is running
        /// </summary>
        private bool IsWindowsUpdateServiceRunning()
        {
            try
            {
                using (var service = new ServiceController(WindowsUpdateServiceName))
                {
                    return service.Status == ServiceControllerStatus.Running;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Starts the Windows Update service if it's not running
        /// </summary>
        private void StartWindowsUpdateService()
        {
            try
            {
                using (var service = new ServiceController(WindowsUpdateServiceName))
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    }
                }
            }
            catch
            {
                // Service may require administrator privileges
            }
        }

        /// <summary>
        /// Stops the Windows Update service to prevent forced restarts
        /// </summary>
        private void StopWindowsUpdateService()
        {
            try
            {
                using (var service = new ServiceController(WindowsUpdateServiceName))
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }
            }
            catch
            {
                // Service may require administrator privileges or may be protected
            }
        }

        /// <summary>
        /// Checks if system has pending reboot
        /// </summary>
        private bool CheckPendingReboot()
        {
            try
            {
                // Check registry for pending reboot indicators
                using (var key = Registry.LocalMachine.OpenSubKey(RegistryPathPendingReboot))
                {
                    if (key != null && key.GetValueNames().Length > 0)
                    {
                        return true;
                    }
                }

                // Check for pending file rename operations
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager"))
                {
                    var pendingFileRenameOperations = key?.GetValue("PendingFileRenameOperations");
                    if (pendingFileRenameOperations != null)
                    {
                        return true;
                    }
                }

                // Check for pending computer rename
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName"))
                {
                    var computerName = key?.GetValue("ComputerName")?.ToString();
                    using (var activeKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName"))
                    {
                        var activeComputerName = activeKey?.GetValue("ComputerName")?.ToString();
                        if (computerName != null && activeComputerName != null && computerName != activeComputerName)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if there are pending updates
        /// </summary>
        private bool CheckPendingUpdates()
        {
            try
            {
                // Check registry for pending updates
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update"))
                {
                    if (key != null)
                    {
                        var subKeys = key.GetSubKeyNames();
                        if (subKeys.Any(sk => sk.Contains("Update")))
                        {
                            return true;
                        }
                    }
                }

                // Check for reboot required flag
                return CheckPendingReboot();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the count of pending updates
        /// </summary>
        private int GetPendingUpdateCount()
        {
            try
            {
                int count = 0;

                // Check registry for update count
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update"))
                {
                    if (key != null)
                    {
                        var subKeys = key.GetSubKeyNames();
                        count = subKeys.Count(sk => sk.Contains("Update"));
                    }
                }

                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Determines if automatic handling is possible
        /// </summary>
        private bool DetermineAutomaticHandling(WindowsUpdateStatus status)
        {
            // Can automatically handle if:
            // 1. Service is not running (can try to start it)
            // 2. No critical pending reboot
            // 3. Updates are pending but not requiring immediate restart

            if (status.RequiresRestart)
            {
                // If restart is required, we can try to postpone but cannot fully prevent
                return false;
            }

            // Can try to manage service state
            return true;
        }

        /// <summary>
        /// Attempts to postpone automatic restart using multiple methods
        /// </summary>
        /// <returns>True if postponement was successful, false otherwise</returns>
        private bool PostponeAutomaticRestart()
        {
            bool success = false;
            
            try
            {
                // Method 1: Modify Windows Update UX settings to notify before restart
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", true))
                    {
                        if (key != null)
                        {
                            // Set UxOption to 1 (Notify before restart instead of auto-restart)
                            key.SetValue("UxOption", 1, RegistryValueKind.DWord);
                            success = true;
                        }
                    }
                }
                catch { }

                // Method 2: Set Active Hours to prevent restarts during work hours
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", true))
                    {
                        if (key != null)
                        {
                            // Set active hours to cover most of the day (6 AM to 11 PM)
                            int activeHoursStart = 6; // 6 AM
                            int activeHoursEnd = 23; // 11 PM
                            
                            key.SetValue("ActiveHoursStart", activeHoursStart, RegistryValueKind.DWord);
                            key.SetValue("ActiveHoursEnd", activeHoursEnd, RegistryValueKind.DWord);
                        }
                    }
                }
                catch { }

                // Method 3: Disable automatic restart via group policy registry
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", true))
                    {
                        if (key == null)
                        {
                            // Create the key if it doesn't exist
                            using (var parentKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true))
                            {
                                if (parentKey != null)
                                {
                                    parentKey.CreateSubKey("AU");
                                }
                            }
                        }
                        
                        using (var key2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", true))
                        {
                            if (key2 != null)
                            {
                                // Set NoAutoRebootWithLoggedOnUsers to prevent restart when user is logged in
                                key2.SetValue("NoAutoRebootWithLoggedOnUsers", 1, RegistryValueKind.DWord);
                                success = true;
                            }
                        }
                    }
                }
                catch { }

                // Method 4: Disable Windows Update automatic restart via UsoScheduler
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", true))
                    {
                        if (key != null)
                        {
                            // Disable scheduled restart
                            key.SetValue("ScheduledRestartTime", "", RegistryValueKind.String);
                        }
                    }
                }
                catch { }
            }
            catch
            {
                // Registry access may require administrator privileges
            }

            return success;
        }

        /// <summary>
        /// Attempts to disable automatic restart using multiple registry methods
        /// </summary>
        private void DisableAutomaticRestart()
        {
            try
            {
                // Method 1: Set UxOption to notify before restart
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", true))
                {
                    if (key != null)
                    {
                        // UxOption: 0 = Never check, 1 = Notify before download, 2 = Auto download and notify, 3 = Auto download and schedule install
                        // Setting to 1 means notify before restart
                        key.SetValue("UxOption", 1, RegistryValueKind.DWord);
                    }
                }

                // Method 2: Set NoAutoRebootWithLoggedOnUsers
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", true))
                    {
                        if (key == null)
                        {
                            using (var parentKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true))
                            {
                                if (parentKey != null)
                                {
                                    parentKey.CreateSubKey("AU");
                                }
                            }
                        }
                        
                        using (var key2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", true))
                        {
                            if (key2 != null)
                            {
                                key2.SetValue("NoAutoRebootWithLoggedOnUsers", 1, RegistryValueKind.DWord);
                            }
                        }
                    }
                }
                catch { }
            }
            catch
            {
                // May require administrator privileges
            }
        }

        /// <summary>
        /// Generates a human-readable status message
        /// </summary>
        private string GenerateStatusMessage(WindowsUpdateStatus status)
        {
            if (!status.UpdateServiceRunning && !status.HasPendingUpdates && !status.RequiresRestart)
            {
                return "Windows Update service is not running, but no pending updates detected.";
            }

            if (status.RequiresRestart)
            {
                return $"System restart is required. {status.PendingUpdateCount} update(s) pending installation.";
            }

            if (status.HasPendingUpdates)
            {
                return $"{status.PendingUpdateCount} update(s) are pending installation.";
            }

            return "Windows Update status: Normal";
        }

        /// <summary>
        /// Starts monitoring for imminent Windows restarts
        /// </summary>
        public void StartMonitoring()
        {
            if (_isMonitoring)
            {
                return;
            }

            _isMonitoring = true;
            _monitoringThread = new Thread(MonitorForImminentRestart)
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _monitoringThread.Start();

            // Also subscribe to Windows session ending event
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
        }

        /// <summary>
        /// Stops monitoring for imminent Windows restarts
        /// </summary>
        public void StopMonitoring()
        {
            _isMonitoring = false;
            SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
            
            if (_monitoringThread != null && _monitoringThread.IsAlive)
            {
                _monitoringThread.Join(1000);
            }
        }

        /// <summary>
        /// Handles Windows session ending event (restart/shutdown)
        /// This is the ONLY time we show a warning - when Windows is actually forcing a restart
        /// </summary>
        private void SystemEvents_SessionEnding(object sender, Microsoft.Win32.SessionEndingEventArgs e)
        {
            // Only show warning when Windows is actually forcing a restart/shutdown
            if (e.Reason == Microsoft.Win32.SessionEndReasons.SystemShutdown)
            {
                // Last attempt to stop it
                TryAutomaticHandling(CheckUpdateStatus());
                
                var status = new WindowsUpdateStatus
                {
                    RequiresRestart = true,
                    StatusMessage = "Windows is forcing a restart now! Save your work immediately!",
                    LastCheckTime = DateTime.Now
                };
                
                OnImminentRestartDetected(status);
            }
        }

        /// <summary>
        /// Monitors for imminent restart in background thread
        /// Continuously tries to delay/stop updates automatically
        /// </summary>
        private void MonitorForImminentRestart()
        {
            // Wait a bit before starting to check (avoid false positives on startup)
            Thread.Sleep(10000);
            
            while (_isMonitoring)
            {
                try
                {
                    // Continuously try to prevent automatic restarts
                    var status = CheckUpdateStatus();
                    
                    // Always try to handle automatically (delay/stop updates)
                    if (status.RequiresRestart || status.HasPendingUpdates)
                    {
                        TryAutomaticHandling(status);
                    }
                    else
                    {
                        // Even if no pending updates, configure settings to prevent future automatic restarts
                        DisableAutomaticRestart();
                    }
                    
                    // Check every 30 seconds and continuously try to delay updates
                    Thread.Sleep(30000);
                }
                catch (Exception)
                {
                    // Continue monitoring even if there's an error
                    Thread.Sleep(30000);
                }
            }
        }

        /// <summary>
        /// Checks if Windows is about to force a restart imminently (within next 15 minutes)
        /// </summary>
        /// <returns>True if restart is truly imminent, false otherwise</returns>
        public bool IsImminentRestartDetected()
        {
            try
            {
                // Method 1: Check for shutdown/restart flags in registry (most reliable for immediate restart)
                if (CheckShutdownFlags())
                {
                    return true;
                }

                // Method 2: Check for scheduled restart time (only if within 15 minutes)
                if (CheckScheduledRestartTime())
                {
                    return true;
                }

                // Don't check other methods as they may give false positives
                // SystemEvents.SessionEnding will handle actual shutdown/restart events
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks for Windows Update restart countdown timer
        /// </summary>
        private bool CheckRestartCountdownTimer()
        {
            try
            {
                // Check for Windows Update restart timer
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update"))
                {
                    if (key != null)
                    {
                        // Check for PostRebootReportingTime which indicates scheduled restart
                        var postRebootTime = key.GetValue("PostRebootReportingTime");
                        if (postRebootTime != null)
                        {
                            return true;
                        }
                    }
                }

                // Check for UsoScheduler restart time
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings"))
                {
                    if (key != null)
                    {
                        var scheduledRestartTime = key.GetValue("ScheduledRestartTime");
                        if (scheduledRestartTime != null)
                        {
                            // Check if restart is scheduled within the next hour
                            try
                            {
                                if (DateTime.TryParse(scheduledRestartTime.ToString(), out DateTime restartTime))
                                {
                                    if (restartTime <= DateTime.Now.AddHours(1) && restartTime > DateTime.Now)
                                    {
                                        return true;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks for scheduled restart time (only if within 15 minutes)
        /// </summary>
        private bool CheckScheduledRestartTime()
        {
            try
            {
                // Check Windows Update scheduled install time
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired"))
                {
                    if (key != null && key.GetValueNames().Length > 0)
                    {
                        // Check if there's a scheduled time
                        var subKeys = key.GetSubKeyNames();
                        foreach (var subKeyName in subKeys)
                        {
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                if (subKey != null)
                                {
                                    var scheduledTime = subKey.GetValue("ScheduledInstallTime");
                                    if (scheduledTime != null)
                                    {
                                        try
                                        {
                                            int hour = Convert.ToInt32(scheduledTime);
                                            var today = DateTime.Now.Date;
                                            var scheduledDateTime = today.AddHours(hour);
                                            
                                            // Only consider it imminent if scheduled within next 15 minutes
                                            var timeUntilRestart = scheduledDateTime - DateTime.Now;
                                            if (timeUntilRestart.TotalMinutes > 0 && timeUntilRestart.TotalMinutes <= 15)
                                            {
                                                return true;
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                    }
                }

                // Check UsoScheduler restart time
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings"))
                {
                    if (key != null)
                    {
                        var scheduledRestartTime = key.GetValue("ScheduledRestartTime");
                        if (scheduledRestartTime != null)
                        {
                            try
                            {
                                if (DateTime.TryParse(scheduledRestartTime.ToString(), out DateTime restartTime))
                                {
                                    var timeUntilRestart = restartTime - DateTime.Now;
                                    // Only if restart is within next 15 minutes
                                    if (timeUntilRestart.TotalMinutes > 0 && timeUntilRestart.TotalMinutes <= 15)
                                    {
                                        return true;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks for Windows Update restart notification
        /// </summary>
        private bool CheckWindowsUpdateRestartNotification()
        {
            try
            {
                // Check for active restart notification
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\OSUpgrade"))
                {
                    if (key != null)
                    {
                        var reserved = key.GetValue("Reserved");
                        if (reserved != null)
                        {
                            // This registry key being present often indicates imminent restart
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks for shutdown/restart flags
        /// </summary>
        private bool CheckShutdownFlags()
        {
            try
            {
                // Check for shutdown flags in registry
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Windows"))
                {
                    if (key != null)
                    {
                        var shutdownTime = key.GetValue("ShutdownTime");
                        if (shutdownTime != null)
                        {
                            // Shutdown time is set, restart is imminent
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Raises the ImminentRestartDetected event
        /// </summary>
        protected virtual void OnImminentRestartDetected(WindowsUpdateStatus status)
        {
            // Only raise event if enough time has passed since last detection (avoid spam)
            if ((DateTime.Now - _lastRestartCheck).TotalMinutes >= 1)
            {
                _lastRestartCheck = DateTime.Now;
                ImminentRestartDetected?.Invoke(this, status);
            }
        }
    }
}

