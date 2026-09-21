using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.RunningMode.Dispatching
{
    public class PrintingMode
    {
        public enum PrintingModeLabel
        {
            PrintingMode,
            ReprintMode,
            PrintingModeOffline,
            ProcessOrder,
            Reservation
        }

        private PrintingModeLabel _printingMode = PrintingModeLabel.PrintingMode;
        public bool IsPrintingMode => _printingMode == PrintingModeLabel.PrintingMode;
        public bool IsReprintMode => _printingMode == PrintingModeLabel.ReprintMode;
        public bool IsPrintingModeOffline => _printingMode == PrintingModeLabel.PrintingModeOffline;
        public bool IsProcessOrderMode => _printingMode == PrintingModeLabel.ProcessOrder;
        public bool IsReservationMode => _printingMode == PrintingModeLabel.Reservation;

        public void SetPrintingMode(PrintingModeLabel mode)
        {
            _printingMode = mode;
        }
    }

}
