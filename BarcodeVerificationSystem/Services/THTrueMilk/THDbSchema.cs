namespace BarcodeVerificationSystem.Services.THTrueMilk
{
    public static class THDb
    {
        public const string Code             = "tb_QRInventory";
        public const string ConfigLine       = "tb_Configline";
        public const string LogIn            = "tb_PrintingLogs";
        public const string LogCamera        = "tb_CameraLogs";
        public const string LogCameraError   = "tb_ProductDefectLogs";
        public const string AllocatedQr      = "tb_AllocatedQr";
        public const string ReceiveHistory   = "tb_IssuanceQRLogs";
        public const string Products         = "tb_Products";
        public const string TableSettings   = "tb_Setting";
        public const string TableAccounts   = "tb_DeviceAccount";
        public const string TablePermissions = "tb_DevicePermission";
        public const string CompletedJobLogs  = "tb_CompletedJobLogs";

        public const string Id               = "NO";
        public const string QrCode           = "qr";
        public const string LineId           = "line_id";
        public const string LineName         = "line_name";
        public const string Batch            = "batch";
        public const string JobName          = "job_name";
        public const string FactoryCode      = "factory_code";
        public const string FactoryName      = "factory_name";
        public const string ProductId        = "product_id";
        public const string ProductName      = "product_name";
        public const string Volume           = "volume";
        public const string ProductGtin      = "product_gtin";
        public const string ProductImage     = "image";
        public const string Exp              = "exp";
        public const string IsUsed           = "is_used";
        public const string IsUsedForJob     = "is_used_for_job";
        public const string IsPrinted        = "is_printed";
        public const string PrintedAt        = "printed_at";
        public const string IsSentToMaster    = "is_marked_sent";
        public const string MarkedSentAt       = "marked_sent_at";
        public const string IsSent           = "is_sent";
        public const string SentAt           = "sent_at";
        public const string ReceivedAt       = "received_at";
        public const string UsedAt           = "used_at";
        public const string CreatedAt        = "created_at";
        public const string Timestamp        = "timestamp";
        public const string Status           = "status";
        public const string RlinkStatus      = "rlink_status";
        public const string OperatorUser     = "operator_user";
        public const string RlinkName        = "rlink_name";
        public const string Qty              = "qty";
        public const string QrDetail         = "qr_detail";
        public const string FrameInfo        = "frame_info";
        // PrintingLogs extra columns
        public const string ManufacturedDate                    = "manufactured_date";
        public const string ExpiryDate                          = "expiry_date";
        public const string LastPrintedAt                       = "last_printed_at";
        public const string PrinterLastProductManufacturedDate  = "printer_last_product_manufactured_date";
        // CameraLogs extra columns
        public const string CameraManufacturedDate              = "camera_manufactured_date";
        public const string CameraExpiryDate                    = "camera_expiry_date";
        public const string CameraLastPacketReceivedAt          = "camera_last_packet_received_at";
        public const string CameraLastProductManufacturedDate   = "camera_last_product_manufactured_date";
        public const string AllocatedAt      = "allocated_at";
        public const string MachineIp        = "machine_ip";
        public const string OperatingMode    = "operating_mode";
        public const string BufferCount      = "buffer_count";
        public const string AssignedAt       = "assigned_at";
        public const string StatusGood       = "status_good";
        public const string StatusFail       = "status_fail";
        public const string TotalCheck       = "total_check";
        // ProductDefectLogs extra columns
        public const string ErrorManufacturedDate            = "error_manufactured_date";
        public const string ErrorExpiryDate                  = "error_expiry_date";
        public const string ErrorFrameInfo                   = "error_frame_info";
        public const string ErrorType        = "error_type";
        public const string ImagePath        = "image_path";
        public const string ColTotalCodes   = "total_codes";
        public const string FirstQr          = "first_qr";
        public const string LastQr           = "last_qr";
        public const string Sender           = "sender";
        public const string Username         = "username";
        public const string PasswordHash     = "password_hash";
        public const string FullName         = "full_name";
        public const string Role             = "role";
        public const string PermissionsJson  = "permissions_json";
        public const string AccId           = "acc_id";
        public const string DisplayName     = "display_name";
        public const string PerDeviceId     = "per_device_id";
        public const string DataJson         = "data_json";
        public const string UpdatedAt        = "updated_at";
        public const string UserRlink        = "user_rlink";
        public const string StatusFailed     = "status_failed";
        public const string QrUsed           = "qr_used";
        public const string ColTotalPrint    = "total_print";
        public const string ColCreateDate    = "create_date";
        public const string JobNameReceiveQr = "job_name_receive_qr_rlinkmaster";
    }
}
