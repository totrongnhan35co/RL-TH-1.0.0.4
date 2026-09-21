using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BarcodeVerificationSystem.Utils
{
    public class AutoTriggerCameraDataman
    {

        private DispatcherTimer _dispatcherTimerAutoTrigger;
        public event EventHandler TriggerEvent;
        public AutoTriggerCameraDataman(double milisenconds)
        {
            SetupTimer(milisenconds);
        }
      
        private void SetupTimer(double milisenconds)
        {
            // Initialize the DispatcherTimer
            _dispatcherTimerAutoTrigger = new DispatcherTimer();

            // Set the interval to 1 second (1000 milliseconds)
            _dispatcherTimerAutoTrigger.Interval = TimeSpan.FromMilliseconds(milisenconds);

            // Attach the Tick event handler
            _dispatcherTimerAutoTrigger.Tick += DispatcherTimer_Tick;
        }
        public void StartTimer()
        {
            // Start the timer if it is not already running
            if (!_dispatcherTimerAutoTrigger.IsEnabled)
            {
                _dispatcherTimerAutoTrigger.Start();
                Console.WriteLine("Timer started.");
            }
        }

        public void StopTimer()
        {
            // Stop the timer if it is running
            if (_dispatcherTimerAutoTrigger.IsEnabled)
            {
                _dispatcherTimerAutoTrigger.Stop();
                Console.WriteLine("Timer stopped.");
            }
        }
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            TriggerEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
