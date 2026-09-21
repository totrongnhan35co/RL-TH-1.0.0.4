using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Labels.DevModeLabel
{
    internal class DevMode
    {
        public enum DevModeLabel
        {
            OffDevMode,
            OnDevMode,
        }
        public enum LoginLabel
        {
            BinhduongXH,
            BinhduongSX,
            OperatorOfflineMode,
            SupportOfflineMode,
            AdminOfflineMode,
            AdminOnlineMode,
        }
        private static LoginLabel _labelType = LoginLabel.BinhduongSX;

        private static DevModeLabel _devMode = DevModeLabel.OffDevMode; // on mode
        public static bool IsDevMode => _devMode == DevModeLabel.OnDevMode;
        public static LoginLabel LabelType => _labelType;
    }
}
