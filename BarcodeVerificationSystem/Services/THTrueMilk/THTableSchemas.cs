using System.Collections.Generic;
using static BarcodeVerificationSystem.Services.THTrueMilk.THDb;

namespace BarcodeVerificationSystem.Services.THTrueMilk
{
    /// <summary>
    /// Tập trung tất cả schema bảng — thêm cột 1 lần, CREATE TABLE tự sinh.
    /// Khi thêm cột mới: thêm 1 dòng ColumnDef vào schema tương ứng.
    /// Khi đổi tên: sửa THDbSchema.cs → rebuild → tự áp dụng.
    /// </summary>
    public static class THTableSchemas
    {
        // ════════════════════════════════════════════════════════════
        //  rlink_log_in
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaLogIn = new TableSchema(LogIn, new List<ColumnDef>
        {
            new ColumnDef(Id,            "INTEGER PRIMARY KEY AUTOINCREMENT", "BIGSERIAL PRIMARY KEY"),
            new ColumnDef(LineId,        "TEXT NOT NULL",                     "TEXT NOT NULL"),
            new ColumnDef(RlinkName,     "TEXT",                              "TEXT"),
            new ColumnDef(JobName,       "TEXT",                              "TEXT"),
            new ColumnDef(Batch,         "TEXT",                              "TEXT"),
            new ColumnDef(ProductId,     "TEXT",                              "TEXT"),
            new ColumnDef(ProductName,   "TEXT",                              "TEXT"),
            new ColumnDef(Status,        "TEXT NOT NULL",                     "TEXT NOT NULL"),
            new ColumnDef(RlinkStatus,   "TEXT",                              "TEXT"),
            new ColumnDef(OperatorUser,  "TEXT",                              "TEXT"),
            new ColumnDef(Qty,           "INTEGER DEFAULT 0",                 "INTEGER DEFAULT 0"),
            new ColumnDef(QrCode,        "TEXT DEFAULT ''",                   "TEXT DEFAULT ''"),
            new ColumnDef(QrDetail,                          "TEXT DEFAULT ''", "TEXT DEFAULT ''"),
            new ColumnDef(ManufacturedDate,                  "TEXT DEFAULT ''", "TEXT DEFAULT ''"),
            new ColumnDef(ExpiryDate,                        "TEXT DEFAULT ''", "TEXT DEFAULT ''"),
            new ColumnDef(LastPrintedAt,                     "TEXT DEFAULT ''", "TEXT DEFAULT ''"),
            new ColumnDef(PrinterLastProductManufacturedDate, "TEXT DEFAULT ''", "TEXT DEFAULT ''"),
            new ColumnDef(Timestamp,     "TEXT NOT NULL",                     "TIMESTAMP NOT NULL"),
            new ColumnDef(CreatedAt,     "TEXT NOT NULL DEFAULT ({SqliteNow})", "TIMESTAMP DEFAULT NOW()"),
            new ColumnDef(IsSent,        "INTEGER DEFAULT 0",                 "INTEGER DEFAULT 0"),
            new ColumnDef(SentAt,        "TEXT",                              "TIMESTAMP"),
        });

        // ════════════════════════════════════════════════════════════
        //  rlink_log_camera
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaLogCamera = new TableSchema(LogCamera, new List<ColumnDef>
        {
            new ColumnDef(Id,            "INTEGER PRIMARY KEY AUTOINCREMENT", "BIGSERIAL PRIMARY KEY"),
            new ColumnDef(LineId,        "TEXT NOT NULL",                     "TEXT NOT NULL"),
            new ColumnDef(RlinkName,     "TEXT",                              "TEXT"),
            new ColumnDef(JobName,       "TEXT",                              "TEXT"),
            new ColumnDef(Batch,         "TEXT",                              "TEXT"),
            new ColumnDef(ProductId,     "TEXT",                              "TEXT"),
            new ColumnDef(ProductName,   "TEXT",                              "TEXT"),
            new ColumnDef(Status,        "TEXT DEFAULT ''",                   "TEXT DEFAULT ''"),
            new ColumnDef(RlinkStatus,   "TEXT",                              "TEXT"),
            new ColumnDef(OperatorUser,  "TEXT",                              "TEXT"),
            new ColumnDef(StatusGood,    "INTEGER DEFAULT 0",                 "INTEGER DEFAULT 0"),
            new ColumnDef(StatusFail,    "INTEGER DEFAULT 0",                 "INTEGER DEFAULT 0"),
            new ColumnDef(TotalCheck,    "INTEGER DEFAULT 0",                 "INTEGER DEFAULT 0"),
            new ColumnDef(QrCode,        "TEXT DEFAULT ''",                   "TEXT DEFAULT ''"),
            new ColumnDef(QrDetail,      "TEXT DEFAULT ''",                   "TEXT DEFAULT ''"),
            new ColumnDef(FrameInfo,                          "TEXT DEFAULT ''", "TEXT DEFAULT ''"),
            new ColumnDef(CameraManufacturedDate,              "TEXT DEFAULT ''", "TEXT DEFAULT ''"),
            new ColumnDef(CameraExpiryDate,                    "TEXT DEFAULT ''", "TEXT DEFAULT ''"),
            new ColumnDef(CameraLastPacketReceivedAt,          "TEXT DEFAULT ''", "TEXT DEFAULT ''"),
            new ColumnDef(CameraLastProductManufacturedDate,   "TEXT DEFAULT ''", "TEXT DEFAULT ''"),
            new ColumnDef(Timestamp,     "TEXT NOT NULL",                     "TIMESTAMP NOT NULL"),
            new ColumnDef(CreatedAt,     "TEXT NOT NULL DEFAULT ({SqliteNow})", "TIMESTAMP DEFAULT NOW()"),
            new ColumnDef(IsSent,        "INTEGER DEFAULT 0",                 "INTEGER DEFAULT 0"),
            new ColumnDef(SentAt,        "TEXT",                              "TIMESTAMP"),
        });

        // ════════════════════════════════════════════════════════════
        //  rlink_log_camera_error
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaLogCameraError = new TableSchema(LogCameraError, new List<ColumnDef>
        {
            new ColumnDef(Id,            "INTEGER PRIMARY KEY AUTOINCREMENT", "BIGSERIAL PRIMARY KEY"),
            new ColumnDef(LineId,        "TEXT NOT NULL",                     "TEXT NOT NULL"),
            new ColumnDef(RlinkName,     "TEXT",                              "TEXT"),
            new ColumnDef(JobName,       "TEXT",                              "TEXT"),
            new ColumnDef(Batch,         "TEXT",                              "TEXT"),
            new ColumnDef(ProductId,     "TEXT",                              "TEXT"),
            new ColumnDef(ProductName,   "TEXT",                              "TEXT"),
            new ColumnDef(RlinkStatus,   "TEXT",                              "TEXT"),
            new ColumnDef(OperatorUser,  "TEXT",                              "TEXT"),
            new ColumnDef(QrCode,        "TEXT",                              "TEXT"),
            new ColumnDef(ErrorManufacturedDate, "TEXT DEFAULT ''",            "TEXT DEFAULT ''"),
            new ColumnDef(ErrorExpiryDate,       "TEXT DEFAULT ''",            "TEXT DEFAULT ''"),
            new ColumnDef(ErrorFrameInfo,        "TEXT DEFAULT ''",            "TEXT DEFAULT ''"),
            new ColumnDef(ErrorType,     "TEXT",                              "TEXT"),
            new ColumnDef(ImagePath,     "TEXT",                              "TEXT"),
            new ColumnDef(Timestamp,     "TEXT NOT NULL",                     "TIMESTAMP NOT NULL"),
            new ColumnDef(CreatedAt,     "TEXT NOT NULL DEFAULT ({SqliteNow})", "TIMESTAMP DEFAULT NOW()"),
            new ColumnDef(IsSent,        "INTEGER DEFAULT 0",                 "INTEGER DEFAULT 0"),
            new ColumnDef(SentAt,        "TEXT",                              "TIMESTAMP"),
        });

        // ════════════════════════════════════════════════════════════
        //  rlink_allocated_qr
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaAllocatedQr = new TableSchema(AllocatedQr, new List<ColumnDef>
        {
            new ColumnDef(Id,            "INTEGER PRIMARY KEY AUTOINCREMENT", "BIGSERIAL PRIMARY KEY"),
            new ColumnDef(LineId,        "TEXT NOT NULL",                     "TEXT NOT NULL"),
            new ColumnDef(RlinkName,     "TEXT",                              "TEXT"),
            new ColumnDef(JobName,       "TEXT NOT NULL",                     "TEXT NOT NULL"),
            new ColumnDef(Batch,         "TEXT",                              "TEXT"),
            new ColumnDef(QrCode,        "TEXT NOT NULL",                     "TEXT NOT NULL"),
            new ColumnDef(AllocatedAt,   "TEXT NOT NULL",                     "TIMESTAMP NOT NULL"),
            new ColumnDef(CreatedAt,     "TEXT NOT NULL DEFAULT ({SqliteNow})", "TIMESTAMP DEFAULT NOW()"),
        });

        // ════════════════════════════════════════════════════════════
        //  rlink_products
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaProducts = new TableSchema(Products, new List<ColumnDef>
        {
            new ColumnDef(ProductId,   "TEXT PRIMARY KEY",   "TEXT PRIMARY KEY"),
            new ColumnDef(ProductName, "TEXT",               "TEXT"),
            new ColumnDef(ProductGtin, "TEXT DEFAULT ''",    "TEXT DEFAULT ''"),
            new ColumnDef(ProductImage,"TEXT DEFAULT ''",    "TEXT DEFAULT ''"),
            new ColumnDef(Volume,      "INTEGER DEFAULT 0",  "INTEGER DEFAULT 0"),
            new ColumnDef(Exp,         "INTEGER DEFAULT 0",  "INTEGER DEFAULT 0"),
            new ColumnDef(DataJson,    "TEXT",               "TEXT"),
            new ColumnDef(UpdatedAt,   "TEXT NOT NULL",      "TIMESTAMPTZ DEFAULT NOW()"),
        });

        // ════════════════════════════════════════════════════════════
        //  tb_Setting
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaSettings = new TableSchema(TableSettings, new List<ColumnDef>
        {
            new ColumnDef(LineId,    "TEXT PRIMARY KEY",   "TEXT PRIMARY KEY"),
            new ColumnDef(DataJson,  "TEXT",               "TEXT"),
            new ColumnDef(UpdatedAt, "TEXT NOT NULL",      "TIMESTAMPTZ DEFAULT NOW()"),
        });

        // ════════════════════════════════════════════════════════════
        //  rlink_accounts
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaAccounts = new TableSchema(TableAccounts, new List<ColumnDef>
        {
            new ColumnDef(Username,        "TEXT PRIMARY KEY",      "TEXT PRIMARY KEY"),
            new ColumnDef(PasswordHash,    "TEXT NOT NULL",         "TEXT NOT NULL"),
            new ColumnDef(FullName,        "TEXT",                  "TEXT"),
            new ColumnDef(Role,            "TEXT",                  "TEXT"),
            new ColumnDef(PermissionsJson, "TEXT",                  "TEXT"),
            new ColumnDef(AccId,           "TEXT",                  "TEXT"),
            new ColumnDef(DisplayName,     "TEXT",                  "TEXT"),
            new ColumnDef(PerDeviceId,     "TEXT",                  "TEXT"),
            new ColumnDef(UpdatedAt,       "TEXT NOT NULL DEFAULT ({SqliteNow})", "TIMESTAMPTZ DEFAULT NOW()"),
        });

        // ════════════════════════════════════════════════════════════
        //  rlink_permissions
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaPermissions = new TableSchema(TablePermissions, new List<ColumnDef>
        {
            new ColumnDef(Username,        "TEXT PRIMARY KEY",      "TEXT PRIMARY KEY"),
            new ColumnDef(PermissionsJson, "TEXT",                  "TEXT"),
            new ColumnDef(UpdatedAt,       "TEXT",                  "TIMESTAMPTZ DEFAULT NOW()"),
        });

        // ════════════════════════════════════════════════════════════
        //  configline
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaConfigLine = new TableSchema(ConfigLine, new List<ColumnDef>
        {
            new ColumnDef(Id,             "INTEGER PRIMARY KEY DEFAULT 1",  ""),
            new ColumnDef(LineId,         "TEXT",                           ""),
            new ColumnDef(LineName,       "TEXT",                           ""),
            new ColumnDef(FactoryCode,    "TEXT",                           ""),
            new ColumnDef(FactoryName,    "TEXT",                           ""),
            new ColumnDef(MachineIp,      "TEXT",                           ""),
            new ColumnDef(OperatingMode,  "INTEGER DEFAULT 0",              ""),
            new ColumnDef(BufferCount,    "INTEGER DEFAULT 0",              ""),
            new ColumnDef(AssignedAt,     "TEXT NOT NULL",                  ""),
        });

        // ════════════════════════════════════════════════════════════
        //  code (QrBank mirror)
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaCode = new TableSchema(Code, new List<ColumnDef>
        {
            new ColumnDef(Id,            "INTEGER PRIMARY KEY AUTOINCREMENT",  "BIGSERIAL PRIMARY KEY"),
            new ColumnDef(QrCode,        "TEXT NOT NULL UNIQUE",               "TEXT NOT NULL"),
            new ColumnDef(FactoryCode,   "TEXT DEFAULT ''",                    "VARCHAR(20) DEFAULT ''"),
            new ColumnDef(LineId,        "TEXT DEFAULT ''",                    "VARCHAR(50) DEFAULT ''"),
            new ColumnDef(LineName,      "TEXT DEFAULT ''",                    "VARCHAR(100) DEFAULT ''"),
            new ColumnDef(Batch,         "TEXT DEFAULT ''",                    "VARCHAR(100) DEFAULT ''"),
            new ColumnDef(JobName,       "TEXT DEFAULT ''",                    "VARCHAR(200) DEFAULT ''"),
            new ColumnDef(IsUsed,        "INTEGER DEFAULT 0",                  "BOOLEAN NOT NULL DEFAULT FALSE"),
            new ColumnDef(IsUsedForJob,  "INTEGER DEFAULT 0",                  "BOOLEAN DEFAULT FALSE"),
            new ColumnDef(IsPrinted,     "INTEGER DEFAULT 0",                  "BOOLEAN DEFAULT FALSE"),
            new ColumnDef(IsSentToMaster, "INTEGER DEFAULT 0",                  "BOOLEAN DEFAULT FALSE"),
            new ColumnDef(MarkedSentAt,   "TEXT",                               "TIMESTAMPTZ"),
            new ColumnDef(UsedAt,        "TEXT",                               "TIMESTAMPTZ"),
            new ColumnDef(ReceivedAt,    "TEXT NOT NULL",                      "TIMESTAMPTZ NOT NULL DEFAULT NOW()"),
            new ColumnDef(CreatedAt,     "TEXT NOT NULL DEFAULT ({SqliteNow})", ""),
            new ColumnDef(ProductGtin,   "TEXT DEFAULT ''",                    "VARCHAR(50) DEFAULT ''"),
        });

        // ════════════════════════════════════════════════════════════
        //  tb_CompletedJobLogs
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaCompletedJobLogs = new TableSchema(CompletedJobLogs, new List<ColumnDef>
        {
            new ColumnDef("no",               "INTEGER PRIMARY KEY AUTOINCREMENT",  "BIGSERIAL PRIMARY KEY"),
            new ColumnDef(JobName,            "TEXT",                                "TEXT"),
            new ColumnDef(UserRlink,          "TEXT",                                "TEXT"),
            new ColumnDef(LineId,             "TEXT",                                "TEXT"),
            new ColumnDef(ColCreateDate,      "TEXT NOT NULL",                       "TIMESTAMPTZ NOT NULL DEFAULT NOW()"),
            new ColumnDef(ProductId,          "TEXT",                                "TEXT"),
            new ColumnDef(QrUsed,             "INTEGER DEFAULT 0",                   "INTEGER DEFAULT 0"),
            new ColumnDef(ColTotalPrint,      "INTEGER DEFAULT 0",                   "INTEGER DEFAULT 0"),
            new ColumnDef(StatusGood,         "INTEGER DEFAULT 0",                   "INTEGER DEFAULT 0"),
            new ColumnDef(StatusFailed,       "INTEGER DEFAULT 0",                   "INTEGER DEFAULT 0"),
            new ColumnDef(TotalCheck,         "INTEGER DEFAULT 0",                   "INTEGER DEFAULT 0"),
        });

        // ════════════════════════════════════════════════════════════
        //  qr_receive_history
        // ════════════════════════════════════════════════════════════
        public static readonly TableSchema SchemaReceiveHistory = new TableSchema(ReceiveHistory, new List<ColumnDef>
        {
            new ColumnDef(Id,              "INTEGER PRIMARY KEY AUTOINCREMENT",  "BIGSERIAL PRIMARY KEY"),
            new ColumnDef(ReceivedAt,      "TEXT NOT NULL",                      "TIMESTAMPTZ NOT NULL DEFAULT NOW()"),
            new ColumnDef(ColTotalCodes,   "INTEGER NOT NULL",                   "INTEGER NOT NULL"),
            new ColumnDef(Batch,           "TEXT DEFAULT ''",                    "VARCHAR(100) DEFAULT ''"),
            new ColumnDef(LineId,          "TEXT DEFAULT ''",                    "VARCHAR(50) DEFAULT ''"),
            new ColumnDef(LineName,        "TEXT DEFAULT ''",                    "VARCHAR(100) DEFAULT ''"),
            new ColumnDef(FactoryCode,     "TEXT DEFAULT ''",                    "VARCHAR(20) DEFAULT ''"),
            new ColumnDef(FirstQr,         "TEXT DEFAULT ''",                    "TEXT DEFAULT ''"),
            new ColumnDef(LastQr,          "TEXT DEFAULT ''",                    "TEXT DEFAULT ''"),
            new ColumnDef(JobName,         "TEXT DEFAULT ''",                    "VARCHAR(200) DEFAULT ''"),
            new ColumnDef(Sender,          "TEXT DEFAULT ''",                    "VARCHAR(200) DEFAULT ''"),
        });
    }
}
