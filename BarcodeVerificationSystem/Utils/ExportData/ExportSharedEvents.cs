using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Utils.ExportData
{
    public class ExportSharedEvents
    {
        // Define a shared event
        public static event EventHandler<ExportDataEventArgs> CheckedResultListEvent;
        public static event EventHandler<ExportDataEventArgs> DataHeaderEvent;
        public static event EventHandler<ExportDataEventArgs> ExportAllProgressEvent;
        public static event EventHandler<ExportDataEventArgs> FilterEvent;
        public static event EventHandler<ExportDataEventArgs> CustomStatusEvent;
        public static event EventHandler<ExportDataEventArgs> ExportModeEvent;
        public static event EventHandler<ExportDataEventArgs> SendMultiStatusDbEvent;
        public static event EventHandler<ExportDataEventArgs> CustomHeaderDbEvent;
        public static event EventHandler<ExportDataEventArgs> CheckedResultEvent;
        public static event EventHandler<ExportDataEventArgs> SampleFilterEvent;
        public static event EventHandler<ExportDataEventArgs> DeviceFilterCheckResEvent;
        public static event EventHandler<ExportDataEventArgs> CheckedResultLbEvent;
        public static event EventHandler<ExportDataEventArgs> HeaderCheckedEvent;
        public static event EventHandler<ExportDataEventArgs> CheckedHeaderListEvent;
        public static event EventHandler<ExportDataEventArgs> ExportTemplateEvent;


        // Method to raise the event
        public static void RaiseDataCheckedResultCode(object data)
        {
            CheckedResultListEvent?.Invoke(null, new ExportDataEventArgs(data));
        }
        public static void RaiseHeaderGet(object data)
        {
            DataHeaderEvent?.Invoke(null, new ExportDataEventArgs(data));
        }
        public static void RaiseExportAllProgress(object data)
        {
            ExportAllProgressEvent?.Invoke(null, new ExportDataEventArgs(data));

        }
        public static void RaiseFilterEvent(object data)
        {
            FilterEvent?.Invoke(null, new ExportDataEventArgs(data));
        }
        public static void RaiseCustomStatusEvent(object data)
        {
            CustomStatusEvent?.Invoke(null, new ExportDataEventArgs(data));
        }
        public static void RaiseExportModeEvent(object data)
        {
            ExportModeEvent?.Invoke(null, new ExportDataEventArgs(data));
        }
        public static void RaiseSendMultiStatusDbEvent(object data)
        {
            SendMultiStatusDbEvent?.Invoke(null, new ExportDataEventArgs(data));
        }
        public static void RaiseCustomHeaderDbEvent(object data)
        {
            CustomHeaderDbEvent?.Invoke(null, new ExportDataEventArgs(data));
        }

        #region Checked Result 

        public static void RaiseCheckedResultEvent(object data)
        {
            CheckedResultEvent?.Invoke(null, new ExportDataEventArgs(data));
        }
        public static void RaiseSampleFilterEvent(object data)
        {
            SampleFilterEvent?.Invoke(null, new ExportDataEventArgs(data));
        }
        public static void RaiseDeviceFilterCheckResEvent(object data)
        {
            DeviceFilterCheckResEvent?.Invoke(null, new ExportDataEventArgs(data));
        }
        public static void RaiseCheckedResultLbEvent(object data)
        {
            CheckedResultLbEvent?.Invoke(null, new ExportDataEventArgs(data));
        }
        public static void RaiseHeaderCheckedEvent(object data)
        {
            HeaderCheckedEvent?.Invoke(null, new ExportDataEventArgs(data));
        }

        public static void RaiseCheckedHeaderListEvent(object data)
        {
            CheckedHeaderListEvent?.Invoke(null, new ExportDataEventArgs(data));
        }

        public static void RaiseExportTemplateEvent()
        {
            ExportTemplateEvent?.Invoke(null,null);
        }
        #endregion Checked Result
    }
}
