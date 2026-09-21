using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.UserPermission
{

    public class UserPermission
    {
        public bool isOnline = false;
        private static string _settings = "settings";
        private static string _controls = "controls";
        private static string _exports = "exports";
        private static string _accounts = "accounts";
        private static string _createJob = "createJob";
        private static string _deleteJob = "deleteJob";
        private static string _manufacturingMode = "manufacturingMode";
        private static string _dispatchingMode = "dispatchingMode";
        private static string _partialDisplay = "partialDisplay";
        private static string _repackFactory = "repackFactory";
        private static string _repackWarehouse = "repackWarehouse";
        private static string _productionSettings = "productionSettings";
        private static string _increaseProductionQuantity = "increaseProductionQuantity";
        private static string _viewSetting = "viewSetting";

        public OnlineUserModel OnlineUserModel = null;

        public static UserPermission AdminPermission = new UserPermission
        {
            Permissions = new Dictionary<string, bool>
            {
                { _settings, true },
                { _controls, true },
                { _exports, true },
                { _accounts, true },
                { _createJob, true },
                { _deleteJob, true },
                { _manufacturingMode, true },
                { _dispatchingMode, true },
                { _partialDisplay, true },
                { _repackFactory, true },
                { _repackWarehouse, true },
                { _productionSettings, false },
                { _increaseProductionQuantity, true },
            }
        };

        public static UserPermission OperatorPermission = new UserPermission
        {
            Permissions = new Dictionary<string, bool>
            {
                { _settings, true },
                { _controls, true },
                { _exports, false },
                { _accounts, false },
                { _createJob, true },
                { _deleteJob, false },
                { _manufacturingMode, false },
                { _dispatchingMode, false },
                { _partialDisplay, false },
                { _repackFactory, false },
                { _repackWarehouse, false },
                { _productionSettings, false },
                { _increaseProductionQuantity, true },

            }
        };

        public Dictionary<string, bool> Permissions { get; set; }

        //public bool this[string functionKey]
        //{
        //    get
        //    {
        //        return Permissions != null &&
        //                Permissions.ContainsKey(functionKey) &&
        //                Permissions[functionKey];
        //    }
        //}

        public bool this[string functionKey]
        {
            get
            {
                if (Permissions == null) return false;
                // Tìm key không phân biệt hoa/thường
                var match = Permissions.FirstOrDefault(
                    kv => string.Equals(kv.Key, functionKey, StringComparison.OrdinalIgnoreCase));
                return match.Value;
            }
        }

        public bool CreateJob => this[_createJob];
        public bool DeleteJob => this[_deleteJob];
        public bool Settings => this[_settings];
        public bool Controls => this[_controls];
        public bool Exports => this[_exports];
        public bool Accounts => this[_accounts];
        public bool ManufacturingMode => this[_manufacturingMode];
        public bool DispatchingMode => this[_dispatchingMode];
        public bool PartialDisplay => this[_partialDisplay];
        public bool RepackFactory => this[_repackFactory];
        public bool RepackWarehouse => this[_repackWarehouse];
        public bool ProductionSettings => this[_productionSettings];
        public bool IncreaseProductionQuantity => this[_increaseProductionQuantity];
        public bool ViewSetting => this[_viewSetting];

    }


}
