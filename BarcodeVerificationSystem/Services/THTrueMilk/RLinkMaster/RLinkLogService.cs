using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;
using BarcodeVerificationSystem.Utils;
using CommonVariable;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using BarcodeVerificationSystem.Services.THTrueMilk;
using static BarcodeVerificationSystem.Services.THTrueMilk.THDb;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
{
    /// <summary>
    /// Ghi log vào 3 bảng riêng biệt (SQLite + PostgreSQL):
    ///   - rlink_log_in           : trạng thái job (start/running/stop/completed)
    ///   - rlink_log_camera       : kết quả kiểm tra camera (ok/fail tổng hợp)
    ///   - rlink_log_camera_error : chi tiết từng lỗi của camera
    ///   - rlink_allocated_qr     : QR codes được cấp phát
    ///   - settings               : cấu hình ứng dụng
    ///   - product                : thông tin sản phẩm
    ///   - configline             : cấu hình dây chuyền
    ///   - code                   : mirror QrBank (offline cache) — nhận từ allocated + complete job
    /// </summary>
    public class RLinkLogService : IRLinkLogService
    {
        // ── SQLite ───────────────────────────────────────────────────
        private static readonly string DbDir =
            Path.Combine(CommVariables.PathProgramDataApp, "RLinkData");
        private static readonly string DbPath =
            Path.Combine(DbDir, "RLinkLog.db");
        //internal static readonly string ConnStr =
        //    $"Data Source={DbPath};Version=3;Busy Timeout=1000;";

        internal static readonly string ConnStr =
    $"Data Source={DbPath};Version=3;Busy Timeout=3000;Journal Mode=WAL;";


        // SQLite: created_at dùng localtime để khớp múi giờ với PG
        private const string SqliteNow = "datetime('now','localtime')";

        // ── Tên bảng PostgreSQL ──────────────────────────────────────
        private const string PgTableLogIn = THDb.LogIn;
        private const string PgTableLogCamera = THDb.LogCamera;
        private const string PgTableLogCameraError = THDb.LogCameraError;
        private const string PgTableAllocatedQr = THDb.AllocatedQr;

        private bool _isDbReady = false;
        private bool _isPgReady = false;

        public RLinkLogService()
        {
            EnsureSQLiteCreated();
            EnsurePostgresTablesCreated();
        }
        // ════════════════════════════════════════════════════════════
        //  PUBLIC STATIC — Mirror PG → SQLite (gọi từ LocalAccountStore)
        // ════════════════════════════════════════════════════════════
        /// <summary>Đếm QR chưa dùng trong SQLite offline — dùng khi PG offline.</summary>
        public static int CountAvailableQrInSQLite(string lineId, string gtin = "")
        {
            try
            {
                EnsureCodeTableExists();
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    int count = 0;
                    if (!string.IsNullOrWhiteSpace(lineId))
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            string sql = $"SELECT COUNT(*) FROM {Code} WHERE {IsUsed} = 0 AND {LineId} = @li";
                            if (!string.IsNullOrWhiteSpace(gtin))
                                sql += " AND product_gtin = @gtin";
                            cmd.CommandText = sql;
                            cmd.Parameters.AddWithValue("@li", lineId);
                            if (!string.IsNullOrWhiteSpace(gtin))
                                cmd.Parameters.AddWithValue("@gtin", gtin);
                            count = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    if (count == 0)
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            string sql = $"SELECT COUNT(*) FROM {Code} WHERE {IsUsed} = 0";
                            if (!string.IsNullOrWhiteSpace(gtin))
                                sql += " AND product_gtin = @gtin";
                            cmd.CommandText = sql;
                            if (!string.IsNullOrWhiteSpace(gtin))
                                cmd.Parameters.AddWithValue("@gtin", gtin);
                            count = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    ProjectLogger.WriteDebug($"[RLinkLogService] SQLite code available: {count}");

                    return count;
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ CountAvailableQrInSQLite: " + ex.Message, ex);

                return 0;
            }
        }

        /// <summary>
        /// Lấy danh sách QR chưa dùng từ SQLite theo FIFO (id ASC).
        /// Validate baseUrl + numberOfUrl giống PG query.
        /// </summary>
        public static List<(int id, string qrCode)> FetchQrCodesFromSQLite(
            string lineId, int limit, string gtin = "",
            string baseUrl = "", int numberOfUrl = 0)
        {
            var list = new List<(int id, string qrCode)>();
            try
            {
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    // Query giống PG: filter lineId + gtin, ORDER BY id ASC
                    // Đọc nhiều rows rồi validate baseUrl/len trong code
                    int fetchLimit = Math.Max(limit * 5, 50);
                    using (var cmd = conn.CreateCommand())
                    {
                        string sql = $@"SELECT {Id}, {QrCode} FROM {Code}
                            WHERE {IsUsed} = 0 AND {LineId} = @li";
                        if (!string.IsNullOrWhiteSpace(gtin))
                            sql += " AND product_gtin = @gtin";
                        sql += $" ORDER BY {Id} ASC LIMIT @lim";
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("@li", lineId);
                        if (!string.IsNullOrWhiteSpace(gtin))
                            cmd.Parameters.AddWithValue("@gtin", gtin);
                        cmd.Parameters.AddWithValue("@lim", fetchLimit);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string qr = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                if (string.IsNullOrEmpty(qr)) continue;

                                // Validate baseUrl + numberOfUrl giống PG
                                bool urlOk = string.IsNullOrEmpty(baseUrl)
                                    || qr.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase);
                                bool lenOk = numberOfUrl <= 0 || qr.Length == numberOfUrl;

                                if (urlOk && lenOk)
                                {
                                    list.Add((id, qr));
                                    if (list.Count >= limit) break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ FetchQrCodesFromSQLite: " + ex.Message, ex);
            }
            return list;
        }





        /// <summary>
        /// Đánh dấu is_used=1 cho các QR code trong SQLite theo qr_code (không dùng id vì PG/SQLite id khác nhau).
        /// </summary>
        public static void MarkQrAsUsedInSQLite(List<int> ids)
        {
            // ids ở đây là PG ids — không dùng được cho SQLite
            // Giữ lại overload cũ để không break signature, nhưng không làm gì
            // Dùng overload mới MarkQrAsUsedInSQLiteByCode thay thế
        }

       
        /// <summary>
        /// Mirror danh sách sản phẩm vào SQLite rlink_products — cùng schema với PG.
        /// Dùng INSERT OR REPLACE để upsert theo product_id (PRIMARY KEY).
        /// </summary>
        public static void UpsertProductsToSQLite(List<BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models.ProductItem> products)
        {
            if (products == null || products.Count == 0) return;
            try
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        using (var delCmd = conn.CreateCommand())
                        {
                            delCmd.Transaction = tx;
                            delCmd.CommandText = $"DELETE FROM {Products}";
                            delCmd.ExecuteNonQuery();
                        }

                        foreach (var p in products)
                        {
                            if (string.IsNullOrWhiteSpace(p.ProductId)) continue;
                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(p);
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.Transaction = tx;
                                cmd.CommandText = $@"
                                    INSERT OR REPLACE INTO {Products}
                                        ({ProductId}, {ProductName}, {ProductGtin}, {ProductImage}, {Volume}, {Exp}, {DataJson}, {UpdatedAt})
                                    VALUES (@id, @n, @gtin, @img, @vol, @exp, @j, @updated_at)";
                                cmd.Parameters.AddWithValue("@id", p.ProductId);
                                cmd.Parameters.AddWithValue("@n", (object)p.ProductName ?? "");
                                cmd.Parameters.AddWithValue("@gtin", (object)p.ProductGtin ?? "");
                                cmd.Parameters.AddWithValue("@img", (object)p.Image ?? "");
                                cmd.Parameters.AddWithValue("@vol", (object)p.Volume ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@exp", (object)p.Exp ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@j", json);
                                cmd.Parameters.AddWithValue("@updated_at", now);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
                ProjectLogger.WriteInfo($"[RLinkLogService] ✔ SQLite rlink_products: upsert {products.Count} sản phẩm");

            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ UpsertProductsToSQLite: " + ex.Message, ex);

            }
        }
        /// <summary>
        /// Đảm bảo bảng <c>code</c> tồn tại trong SQLite — gọi trước mỗi static insert/query.
        /// Idempotent, thread-safe.
        /// </summary>
        /// <summary>
        /// Đảm bảo bảng <c>code</c> tồn tại trong SQLite — gọi trước mỗi static insert/query.
        /// Idempotent, thread-safe.
        /// </summary>
        private static void EnsureCodeTableExists()
        {
            try
            {
                if (!Directory.Exists(DbDir))
                    Directory.CreateDirectory(DbDir);

                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        // ── Chỉ lưu qr_code + received_at khi nhận
                        // ── Các cột job_name, line_id, factory_code, batch, used_at
                        //    sẽ được điền khi đánh dấu is_used = 1
                        cmd.CommandText = $@"
                            CREATE TABLE IF NOT EXISTS {Code} (
                                {Id}              INTEGER  PRIMARY KEY AUTOINCREMENT,
                                {QrCode}         TEXT     NOT NULL UNIQUE,
                                {FactoryCode}    TEXT     DEFAULT '',
                                {LineId}         TEXT     DEFAULT '',
                                {LineName}       TEXT     DEFAULT '',
                                {Batch}           TEXT     DEFAULT '',
                                {ProductId}       TEXT     DEFAULT '',
                                {ManufacturedDate} TEXT    DEFAULT '',
                                {ExpiryDate}      TEXT    DEFAULT '',
                                {JobName}        TEXT     DEFAULT '',
                                {IsUsed}         INTEGER  DEFAULT 0,
                                {IsUsedForJob} INTEGER  DEFAULT 0,
                                {IsPrinted}      INTEGER  DEFAULT 0,
                                {PrintedAt}      TEXT,
                                {UsedAt}         TEXT,
                                {ReceivedAt}     TEXT     NOT NULL,
                                {CreatedAt}      TEXT     NOT NULL DEFAULT (datetime('now','localtime')),
                                product_gtin     TEXT     DEFAULT ''
                            )";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = $@"
                            CREATE INDEX IF NOT EXISTS idx_{Code}_received_at
                            ON {Code} ({ReceivedAt} DESC)";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = $@"
                            CREATE INDEX IF NOT EXISTS idx_{Code}_is_used
                            ON {Code} ({IsUsed}) WHERE {IsUsed} = 0";
                        cmd.ExecuteNonQuery();
                    }

                    // Migrate DB cũ — thêm cột còn thiếu
                    TryAddSqliteColumn(conn, Code, JobName, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, Code, UsedAt, "TEXT");
                    TryAddSqliteColumn(conn, Code, IsUsedForJob, "INTEGER DEFAULT 0");
                    TryAddSqliteColumn(conn, Code, IsPrinted, "INTEGER DEFAULT 0");
                    TryAddSqliteColumn(conn, Code, PrintedAt, "TEXT");
                    TryAddSqliteColumn(conn, Code, "product_gtin", "TEXT DEFAULT ''");
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ EnsureCodeTableExists: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lưu QR code vào SQLite bảng <c>code</c> — chỉ ghi qr_code + received_at.
        /// Các trường job_name, line_id, factory_code, batch, used_at sẽ được cập nhật
        /// khi QR được sử dụng thực tế qua <see cref="MarkQrAsUsedInSQLiteByCodeWithInfo"/>.
        /// </summary>
        public static void InsertQrCodeToSQLiteStatic(string qrCode, DateTime receivedAt, string productGtin = "")
        {
            if (string.IsNullOrWhiteSpace(qrCode)) return;
            try
            {
                EnsureCodeTableExists();
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"
                            INSERT OR IGNORE INTO {Code} ({QrCode}, {ReceivedAt}, product_gtin)
                            VALUES (@qr, @ra, @gtin)";
                        cmd.Parameters.AddWithValue("@qr", qrCode);
                        cmd.Parameters.AddWithValue("@ra", receivedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@gtin", productGtin ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
                ProjectLogger.WriteDebug($"[RLinkLogService] ✔ SQLite code insert: {qrCode}");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ InsertQrCodeToSQLiteStatic: " + ex.Message, ex);
            }
        }

        // ── Overload tương thích ngược (deprecated) — chuyển hướng sang overload mới ──
        public static void InsertQrCodeToSQLiteStatic(
            string qrCode, string factoryCode, string lineId, string lineName, string batch, DateTime receivedAt)
        {
            // Bỏ qua metadata lúc nhận — chỉ lưu qr_code + received_at
            InsertQrCodeToSQLiteStatic(qrCode, receivedAt);
        }
        /// <summary>
        /// Lưu lịch sử nhận QR vào SQLite.
        /// </summary>
        public static void SaveReceiveHistoryToSQLite(DateTime receivedAt, int totalCodes,
            string batch, string lineId, string lineName, string factoryCode,
            string firstQr, string lastQr, string jobName, string sender)
        {
            try
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                                        cmd.CommandText = $@"
                CREATE TABLE IF NOT EXISTS {ReceiveHistory} (
                    {Id}           INTEGER  PRIMARY KEY AUTOINCREMENT,
                    {ReceivedAt}   TEXT    NOT NULL,
                    {ColTotalCodes}   INTEGER NOT NULL,
                    {Batch}         TEXT    DEFAULT '',
                    {LineId}       TEXT    DEFAULT '',
                    {LineName}     TEXT    DEFAULT '',
                    {FactoryCode}  TEXT    DEFAULT '',
                    {FirstQr}      TEXT    DEFAULT '',
                    {LastQr}       TEXT    DEFAULT '',
                    {JobName}      TEXT    DEFAULT '',
                    {Sender}        TEXT    DEFAULT ''
                );
                CREATE INDEX IF NOT EXISTS idx_sqlite_qrh_received_at ON {ReceiveHistory} ({ReceivedAt} DESC);
                CREATE INDEX IF NOT EXISTS idx_sqlite_qrh_line_id ON {ReceiveHistory} ({LineId});";
                                        cmd.ExecuteNonQuery();
                                    }
                                    TryAddSqliteColumn(conn, ReceiveHistory, JobName, "TEXT DEFAULT ''");
                                    TryAddSqliteColumn(conn, ReceiveHistory, Sender, "TEXT DEFAULT ''");
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = $@"
                INSERT INTO {ReceiveHistory} ({ReceivedAt}, {ColTotalCodes}, {Batch}, {LineId}, {LineName}, {FactoryCode}, {FirstQr}, {LastQr}, {JobName}, {Sender})
                VALUES (@received_at, @total, @batch, @line_id, @line_name, @factory_code, @first_qr, @last_qr, @job_name, @sender)";
                        cmd.Parameters.AddWithValue("@received_at", receivedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@total", totalCodes);
                        cmd.Parameters.AddWithValue("@batch", batch ?? "");
                        cmd.Parameters.AddWithValue("@line_id", lineId ?? "");
                        cmd.Parameters.AddWithValue("@line_name", lineName ?? "");
                        cmd.Parameters.AddWithValue("@factory_code", factoryCode ?? "");
                        cmd.Parameters.AddWithValue("@first_qr", firstQr ?? "");
                        cmd.Parameters.AddWithValue("@last_qr", lastQr ?? "");
                        cmd.Parameters.AddWithValue("@job_name", jobName ?? "");
                        cmd.Parameters.AddWithValue("@sender", sender ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine($"[RLinkLogService] ✔ SQLite qr_receive_history: {totalCodes} QR @ {receivedAt:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] SaveReceiveHistoryToSQLite lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu lịch sử nhận QR vào PostgreSQL (bảng qr_receive_history).
        /// </summary>
        public static void SaveReceiveHistoryToPostgres(DateTime receivedAt, int totalCodes,
            string batch, string lineId, string lineName, string factoryCode,
            string firstQr, string lastQr, string jobName, string sender)
        {
            try
            {
                string connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                    "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);
                if (string.IsNullOrWhiteSpace(connStr)) return;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();

                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                    CREATE TABLE IF NOT EXISTS {ReceiveHistory} (
                        {Id}           BIGSERIAL    PRIMARY KEY,
                        {ReceivedAt}  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
                        {ColTotalCodes}  INTEGER      NOT NULL,
                        {Batch}        VARCHAR(100) DEFAULT '',
                        {LineId}      VARCHAR(50)  DEFAULT '',
                        {LineName}    VARCHAR(100) DEFAULT '',
                        {FactoryCode} VARCHAR(20)  DEFAULT '',
                        {FirstQr}     TEXT         DEFAULT '',
                        {LastQr}      TEXT         DEFAULT '',
                        {JobName}     VARCHAR(200) DEFAULT '',
                        {Sender}       VARCHAR(200) DEFAULT ''
                    )", conn))
                                            cmd.ExecuteNonQuery();

                                        using (var cmd = new Npgsql.NpgsqlCommand($"CREATE INDEX IF NOT EXISTS idx_qrh_received_at ON {ReceiveHistory} ({ReceivedAt} DESC)", conn))
                                            cmd.ExecuteNonQuery();
                                        using (var cmd = new Npgsql.NpgsqlCommand($"CREATE INDEX IF NOT EXISTS idx_qrh_line_id ON {ReceiveHistory} ({LineId})", conn))
                                            cmd.ExecuteNonQuery();

                                        using (var cmd = new Npgsql.NpgsqlCommand($"ALTER TABLE {ReceiveHistory} ADD COLUMN IF NOT EXISTS {JobName} VARCHAR(200) DEFAULT ''", conn))
                                            { try { cmd.ExecuteNonQuery(); } catch { } }
                                        using (var cmd = new Npgsql.NpgsqlCommand($"ALTER TABLE {ReceiveHistory} ADD COLUMN IF NOT EXISTS {Sender} VARCHAR(200) DEFAULT ''", conn))
                                            { try { cmd.ExecuteNonQuery(); } catch { } }

                                        using (var cmd = new Npgsql.NpgsqlCommand($@"
                    INSERT INTO {ReceiveHistory} ({ReceivedAt}, {ColTotalCodes}, {Batch}, {LineId}, {LineName}, {FactoryCode}, {FirstQr}, {LastQr}, {JobName}, {Sender})
                    VALUES (@received_at, @total, @batch, @line_id, @line_name, @factory_code, @first_qr, @last_qr, @job_name, @sender)", conn))
                    {
                        cmd.Parameters.AddWithValue("received_at", receivedAt);
                        cmd.Parameters.AddWithValue("total", totalCodes);
                        cmd.Parameters.AddWithValue("batch", (object)batch ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("line_id", (object)lineId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("line_name", (object)lineName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("factory_code", (object)factoryCode ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("first_qr", (object)firstQr ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("last_qr", (object)lastQr ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("job_name", (object)jobName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("sender", (object)sender ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine($"[RLinkLogService] ✔ PG qr_receive_history: {totalCodes} QR @ {receivedAt:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] SaveReceiveHistoryToPostgres lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Đánh dấu QR đã dùng trong PostgreSQL theo qr_code,
        /// đồng thời cập nhật metadata thực tế tại thời điểm sử dụng.
        /// </summary>
        public static void MarkQrAsUsedInPgByCode(
            List<string> qrCodes,
            string jobName,
            string lineId,
            string lineName,
            string factoryCode,
            string batch,
            string manufacturedDate = "",
            string expiryDate = "",
            string productId = "")
        {
            if (qrCodes == null || qrCodes.Count == 0) return;
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return;

                string table = Code;

                DateTime usedAt = DateTime.Now;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        foreach (var qr in qrCodes)
                        {
                            if (string.IsNullOrWhiteSpace(qr)) continue;
                            using (var cmd = new Npgsql.NpgsqlCommand($@"
                        UPDATE ""{table}""
                        SET {IsUsed}         = TRUE,
                            {IsUsedForJob}   = TRUE,
                            {ProductId}      = @product_id,
                            {ManufacturedDate} = @manufactured_date,
                            {ExpiryDate}       = @expiry_date,
                            {UsedAt}         = @used_at,
                            {JobName}        = @job_name,
                            {LineId}         = @line_id,
                            {LineName}       = @line_name,
                            {FactoryCode}    = @factory_code,
                            {Batch}          = @batch
                        WHERE {QrCode} = @qr_code", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("qr_code", qr);
                                cmd.Parameters.AddWithValue("product_id", productId ?? "");
                                cmd.Parameters.AddWithValue("used_at", usedAt);
                                cmd.Parameters.AddWithValue("manufactured_date", manufacturedDate ?? "");
                                cmd.Parameters.AddWithValue("expiry_date", expiryDate ?? "");
                                cmd.Parameters.AddWithValue("job_name", jobName ?? "");
                                cmd.Parameters.AddWithValue("line_id", lineId ?? "");
                                cmd.Parameters.AddWithValue("line_name", lineName ?? "");
                                cmd.Parameters.AddWithValue("factory_code", factoryCode ?? "");
                                cmd.Parameters.AddWithValue("batch", batch ?? "");
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
                ProjectLogger.WriteDebug(
                    $"[RLinkLogService] ✔ PG MarkUsed: {qrCodes.Count} QR" +
                    $" | job='{jobName}' line='{lineId}' batch='{batch}' product_id='{productId}'");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ MarkQrAsUsedInPgByCode: " + ex.Message, ex);
            }
        }
        // ── Overload tương thích ngược (không có metadata) ──────────────────
        public static void MarkQrAsUsedInSQLiteByCode(List<string> qrCodes)
        {
            MarkQrAsUsedInSQLiteByCodeWithInfo(qrCodes, "", "", "", "", "");
        }

        /// <summary>
        /// Đánh dấu QR đã dùng trong SQLite theo qr_code,
        /// đồng thời cập nhật metadata thực tế của line tại thời điểm sử dụng.
        /// </summary>
        public static void MarkQrAsUsedInSQLiteByCodeWithInfo(
            List<string> qrCodes,
            string jobName,
            string lineId,
            string lineName,
            string factoryCode,
            string batch,
            string manufacturedDate = "",
            string expiryDate = "",
            string productId = "")
        {
            if (qrCodes == null || qrCodes.Count == 0) return;
            try
            {
                EnsureCodeTableExists();
                string usedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        foreach (var qr in qrCodes)
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.Transaction = tx;
                                cmd.CommandText = $@"
                                                    UPDATE {Code}
                                                    SET {IsUsed}         = 1,
                                                        {IsUsedForJob}   = 1,
                                                        {ProductId}      = @product_id,
                                                        {ManufacturedDate} = @manufactured_date,
                                                        {ExpiryDate}       = @expiry_date,
                                                        {UsedAt}         = @used_at,
                                                        {JobName}        = @job_name,
                                                        {LineId}         = @line_id,
                                                        {LineName}       = @line_name,
                                                        {FactoryCode}    = @factory_code,
                                                        {Batch}          = @batch
                                                    WHERE {QrCode} = @qr";
                                cmd.Parameters.AddWithValue("@qr", qr);
                                cmd.Parameters.AddWithValue("@product_id", productId ?? "");
                                cmd.Parameters.AddWithValue("@used_at", usedAt);
                                cmd.Parameters.AddWithValue("@manufactured_date", manufacturedDate ?? "");
                                cmd.Parameters.AddWithValue("@expiry_date", expiryDate ?? "");
                                cmd.Parameters.AddWithValue("@job_name", jobName ?? "");
                                cmd.Parameters.AddWithValue("@line_id", lineId ?? "");
                                cmd.Parameters.AddWithValue("@line_name", lineName ?? "");
                                cmd.Parameters.AddWithValue("@factory_code", factoryCode ?? "");
                                cmd.Parameters.AddWithValue("@batch", batch ?? "");
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
                ProjectLogger.WriteDebug(
                    $"[RLinkLogService] ✔ SQLite MarkUsed+Info: {qrCodes.Count} QR" +
                    $" | job='{jobName}' line='{lineId}' lineName='{lineName}' batch='{batch}'");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ MarkQrAsUsedInSQLiteByCodeWithInfo: " + ex.Message, ex);
            }
        }

        /// <summary>Bỏ đánh dấu QR (is_used=0) trong SQLite khi job complete với 0 sản phẩm.</summary>
        public static void MarkQrAsUnusedInSQLiteByCode(List<string> qrCodes)
        {
            if (qrCodes == null || qrCodes.Count == 0) return;
            try
            {
                EnsureCodeTableExists();
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        foreach (var qr in qrCodes)
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.Transaction = tx;
                                cmd.CommandText = $@"UPDATE {Code} SET {IsUsed}=0, {UsedAt}=NULL, {JobName}='', {Batch}='', {ProductId}='', {ManufacturedDate}='', {ExpiryDate}='', {IsUsedForJob}=0, {IsPrinted}=0, {PrintedAt}=NULL WHERE {QrCode}=@qr";
                                cmd.Parameters.AddWithValue("@qr", qr);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
                ProjectLogger.WriteInfo($"[RLinkLogService] ✔ SQLite MarkUnused: {qrCodes.Count} QR");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ MarkQrAsUnusedInSQLiteByCode: " + ex.Message, ex);
            }
        }

        /// <summary>Bỏ đánh dấu QR (is_used=FALSE) trong PG khi job complete với 0 sản phẩm.</summary>
        public static void MarkQrAsUnusedInPgByCode(List<string> qrCodes)
        {
            if (qrCodes == null || qrCodes.Count == 0) return;
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return;

                string table = Code;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        foreach (var qr in qrCodes)
                        {
                            using (var cmd = new Npgsql.NpgsqlCommand(
                                $@"UPDATE ""{table}"" SET {IsUsed}=FALSE, {UsedAt}=NULL, {JobName}='', {Batch}='', {ProductId}='', {ManufacturedDate}='', {ExpiryDate}='', {IsUsedForJob}=FALSE, {IsPrinted}=FALSE, {PrintedAt}=NULL WHERE {QrCode}=@qr", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("qr", qr);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
                ProjectLogger.WriteInfo($"[RLinkLogService] ✔ PG MarkUnused: {qrCodes.Count} QR");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ MarkQrAsUnusedInPgByCode: " + ex.Message, ex);
            }
        }

        /// <summary>Đánh dấu QR đã in trên SQLite — is_printed=1 + lưu product_id, NSX, HSD.</summary>
        public static void MarkQrAsPrinted(string qrCode, string productId = "", string prod = "", string exp = "")
        {
            if (string.IsNullOrWhiteSpace(qrCode)) return;
            try
            {
                EnsureCodeTableExists();
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"UPDATE {Code} SET {IsPrinted}=1, {PrintedAt}=datetime('now','localtime'), {ProductId}=@pid, {ManufacturedDate}=@prod, {ExpiryDate}=@exp WHERE {QrCode}=@qr";
                        cmd.Parameters.AddWithValue("@qr", qrCode);
                        cmd.Parameters.AddWithValue("@pid", productId ?? "");
                        cmd.Parameters.AddWithValue("@prod", prod ?? "");
                        cmd.Parameters.AddWithValue("@exp", exp ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ MarkQrAsPrinted: " + ex.Message, ex);
            }
        }

        /// <summary>Đánh dấu QR đã in trên PostgreSQL — is_printed=TRUE + lưu product_id, NSX, HSD.</summary>
        public static void MarkQrAsPrintedPg(string qrCode, string productId = "", string prod = "", string exp = "")
        {
            if (string.IsNullOrWhiteSpace(qrCode)) return;
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return;

                string table = Code;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(
                        $"UPDATE \"{table}\" SET {IsPrinted}=TRUE, {PrintedAt}=NOW(), \"{ProductId}\"=@pid, \"{ManufacturedDate}\"=@prod, \"{ExpiryDate}\"=@exp WHERE {QrCode}=@qr", conn))
                    {
                        cmd.Parameters.AddWithValue("qr", qrCode);
                        cmd.Parameters.AddWithValue("pid", productId ?? "");
                        cmd.Parameters.AddWithValue("prod", prod ?? "");
                        cmd.Parameters.AddWithValue("exp", exp ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ MarkQrAsPrintedPg: " + ex.Message, ex);
            }
        }

        /// <summary>Tính batch theo quy cách NSX(ddMMyy) + LineId. Ví dụ: "05 12 26" + "A12" → "051226A12".</summary>
        public static string ComputeBatchNsxLine(string nsxDdMmYy, string lineId)
        {
            if (string.IsNullOrWhiteSpace(nsxDdMmYy)) return "";
            string nsx = nsxDdMmYy.Replace(" ", ""); // "05 12 26" → "051226"
            if (string.IsNullOrWhiteSpace(lineId)) return nsx;
            return nsx + lineId;
        }

        /// <summary>Đánh dấu nhiều QR đã in trên SQLite trong 1 transaction.</summary>
        public static int MarkQrAsPrintedBatch(List<(string qr, string pid, string prod, string exp, string batch)> items)
        {
            if (items == null || items.Count == 0) return 0;
            int affected = 0;
            try
            {
                EnsureCodeTableExists();
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.CommandText = $"UPDATE {Code} SET {IsPrinted}=1, {PrintedAt}=datetime('now','localtime'), {Batch}=@batch, {ProductId}=@pid, {ManufacturedDate}=@prod, {ExpiryDate}=@exp WHERE {QrCode}=@qr";
                            var pQr = cmd.Parameters.Add("@qr", System.Data.DbType.String);
                            var pBatch = cmd.Parameters.Add("@batch", System.Data.DbType.String);
                            var pPid = cmd.Parameters.Add("@pid", System.Data.DbType.String);
                            var pProd = cmd.Parameters.Add("@prod", System.Data.DbType.String);
                            var pExp = cmd.Parameters.Add("@exp", System.Data.DbType.String);
                            foreach (var item in items)
                            {
                                pQr.Value = item.qr ?? "";
                                pBatch.Value = item.batch ?? "";
                                pPid.Value = item.pid ?? "";
                                pProd.Value = item.prod ?? "";
                                pExp.Value = item.exp ?? "";
                                affected += cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[RLinkLogService] ✘ MarkQrAsPrintedBatch ({items.Count} QR): " + ex.Message, ex);
            }
            return affected;
        }

        /// <summary>Đánh dấu nhiều QR đã in trên PostgreSQL trong 1 transaction.</summary>
        public static int MarkQrAsPrintedBatchPg(List<(string qr, string pid, string prod, string exp, string batch)> items)
        {
            if (items == null || items.Count == 0) return 0;
            int affected = 0;
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return 0;
                string table = Code;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.CommandText = $"UPDATE \"{table}\" SET {IsPrinted}=TRUE, {PrintedAt}=NOW(), {Batch}=@batch, \"{ProductId}\"=@pid, \"{ManufacturedDate}\"=@prod, \"{ExpiryDate}\"=@exp WHERE {QrCode}=@qr";
                            var pQr = cmd.Parameters.Add("@qr", NpgsqlTypes.NpgsqlDbType.Text);
                            var pBatch = cmd.Parameters.Add("@batch", NpgsqlTypes.NpgsqlDbType.Text);
                            var pPid = cmd.Parameters.Add("@pid", NpgsqlTypes.NpgsqlDbType.Text);
                            var pProd = cmd.Parameters.Add("@prod", NpgsqlTypes.NpgsqlDbType.Text);
                            var pExp = cmd.Parameters.Add("@exp", NpgsqlTypes.NpgsqlDbType.Text);
                            foreach (var item in items)
                            {
                                pQr.Value = item.qr ?? "";
                                pBatch.Value = item.batch ?? "";
                                pPid.Value = item.pid ?? "";
                                pProd.Value = item.prod ?? "";
                                pExp.Value = item.exp ?? "";
                                affected += cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[RLinkLogService] ✘ MarkQrAsPrintedBatchPg ({items.Count} QR): " + ex.Message, ex);
            }
            return affected;
        }

        /// <summary>Đọc batch, product_id, NSX, HSD từ bảng Code theo QR. PG trước, SQLite sau.</summary>
        public static (string batch, string productId, string prod, string exp, string jobName, string printedAt) GetQrCodeData(string qrCode)
        {
            // ── 1. PostgreSQL trước (primary, luôn đồng bộ) ──
            string pgConn = GetPgConnStr();
            if (!string.IsNullOrWhiteSpace(pgConn))
            {
                try
                {
                    using (var conn = new Npgsql.NpgsqlConnection(pgConn))
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = $"SELECT {Batch}, {ProductId}, {ManufacturedDate}, {ExpiryDate}, {JobName}, {PrintedAt} FROM \"{Code}\" WHERE {QrCode}=@qr";
                            cmd.Parameters.AddWithValue("@qr", qrCode);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string printedAt = "";
                                    if (reader[5] != null && reader[5] != DBNull.Value)
                                    {
                                        if (reader[5] is DateTime dt)
                                            printedAt = dt.ToString("yyyy-MM-dd HH:mm:ss.ffffff") + "+07:00";
                                        else
                                            printedAt = reader[5].ToString() ?? "";
                                        //if (reader[5] is DateTimeOffset dto)
                                        //    printedAt = dto.DateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff") + "+07:10";
                                        //else if (reader[5] is DateTime dt)
                                        //    printedAt = dt.ToString("yyyy-MM-dd HH:mm:ss.ffffff") + "+07:30";
                                        //else
                                        //    printedAt = reader[5].ToString() ?? "";
                                    }
                                    return (
                                        reader[0]?.ToString() ?? "",
                                        reader[1]?.ToString() ?? "",
                                        reader[2]?.ToString() ?? "",
                                        reader[3]?.ToString() ?? "",
                                        reader[4]?.ToString() ?? "",
                                        printedAt);
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            // ── 2. Fallback SQLite ──
            try
            {
                EnsureCodeTableExists();
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT {Batch}, {ProductId}, {ManufacturedDate}, {ExpiryDate}, {JobName}, {PrintedAt} FROM {Code} WHERE {QrCode}=@qr";
                        cmd.Parameters.AddWithValue("@qr", qrCode);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                return (
                                    reader[0]?.ToString() ?? "",
                                    reader[1]?.ToString() ?? "",
                                    reader[2]?.ToString() ?? "",
                                    reader[3]?.ToString() ?? "",
                                    reader[4]?.ToString() ?? "",
                                    reader[5]?.ToString() ?? "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ GetQrCodeData: " + ex.Message, ex);
            }
            return ("", "", "", "", "", "");
        }

        /// <summary>Đánh dấu QR đã gửi mark-used lên API thành công.</summary>
        public static void MarkQrAsSentToMaster(string qrCode)
        {
            if (string.IsNullOrWhiteSpace(qrCode)) return;
            try
            {
                EnsureCodeTableExists();
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"UPDATE {Code} SET {IsSentToMaster}=1, {MarkedSentAt}=datetime('now','localtime') WHERE {QrCode}=@qr";
                        cmd.Parameters.AddWithValue("@qr", qrCode);
                        cmd.ExecuteNonQuery();
                    }
                }

                string connStr = GetPgConnStr();
                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand(
                            $"UPDATE \"{Code}\" SET {IsSentToMaster}=TRUE, {MarkedSentAt}=NOW() WHERE {QrCode}=@qr", conn))
                        {
                            cmd.Parameters.AddWithValue("qr", qrCode);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ MarkQrAsSentToMaster: " + ex.Message, ex);
            }
        }

        /// <summary>Kiểm tra QR đã được gửi mark-used lên master chưa.</summary>
        public static bool IsQrSentToMaster(string qrCode)
        {
            if (string.IsNullOrWhiteSpace(qrCode)) return false;
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return false;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(
                        $"SELECT {IsSentToMaster} FROM \"{Code}\" WHERE {QrCode}=@qr", conn))
                    {
                        cmd.Parameters.AddWithValue("qr", qrCode);
                        var result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value && (bool)result;
                    }
                }
            }
            catch { return false; }
        }

        /// <summary>Lấy danh sách QR đã in nhưng chưa gửi mark-used lên API.</summary>
        public static List<string> GetUnsentMarkedQr(int limit = 100)
        {
            var result = new List<string>();
            try
            {
                EnsureCodeTableExists();
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT {QrCode} FROM {Code} WHERE {IsPrinted}=1 AND {IsSentToMaster}=0 LIMIT @lim";
                        cmd.Parameters.AddWithValue("@lim", limit);
                        using (var r = cmd.ExecuteReader())
                            while (r.Read()) result.Add(r.GetString(0));
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ GetUnsentMarkedQr: " + ex.Message, ex);
            }
            return result;
        }

        public static List<string> GetQrCodesUsedByJob(string jobName)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(jobName)) return result;
            try
            {
                EnsureCodeTableExists();
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT DISTINCT {QrCode} FROM {Code} WHERE {IsPrinted}=1 AND {JobName}=@jn";
                        cmd.Parameters.AddWithValue("@jn", jobName);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                                result.Add(r.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ GetQrCodesUsedByJob: " + ex.Message, ex);
            }
            return result;
        }

        public static List<string> GetQrCodesUsedByJobPg(string jobName)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(jobName)) return result;
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return result;
                string table = Code;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(
                        $"SELECT DISTINCT {QrCode} FROM \"{table}\" WHERE {IsPrinted}=TRUE AND {JobName}=@jn", conn))
                    {
                        cmd.Parameters.AddWithValue("jn", jobName);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                                result.Add(r.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ GetQrCodesUsedByJobPg: " + ex.Message, ex);
            }
            return result;
        }

        /// <summary>Lấy danh sách QR đã cấp cho job nhưng chưa in (is_used=TRUE, is_printed=FALSE) — PG.</summary>
        public static List<string> GetQrUsedButNotPrintedByJobPg(string jobName)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(jobName)) return result;
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return result;
                string table = Code;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(
                        $"SELECT DISTINCT {QrCode} FROM \"{table}\" WHERE {IsUsed}=TRUE AND {IsPrinted}=FALSE AND {JobName}=@jn", conn))
                    {
                        cmd.Parameters.AddWithValue("jn", jobName);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                                result.Add(r.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ GetQrUsedButNotPrintedByJobPg: " + ex.Message, ex);
            }
            return result;
        }

        /// <summary>Lấy danh sách QR đã cấp cho job nhưng chưa in (is_used=1, is_printed=0) — SQLite.</summary>
        public static List<string> GetQrUsedButNotPrintedByJob(string jobName)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(jobName)) return result;
            try
            {
                EnsureCodeTableExists();
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT DISTINCT {QrCode} FROM {Code} WHERE {IsUsed}=1 AND {IsPrinted}=0 AND {JobName}=@jn";
                        cmd.Parameters.AddWithValue("@jn", jobName);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                                result.Add(r.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ GetQrUsedButNotPrintedByJob: " + ex.Message, ex);
            }
            return result;
        }


        /// <summary>
        /// Mirror cấu hình settings vào SQLite {TableSettings} — cùng schema với PG.
        /// Dùng INSERT OR REPLACE để upsert theo line_id (PRIMARY KEY).
        /// </summary>
        public static void UpsertSettingsToSQLite(string lineId, BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models.RLinkSettings settings)
        {
            if (settings == null || string.IsNullOrWhiteSpace(lineId)) return;
            try
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"
                            INSERT OR REPLACE INTO {TableSettings}
                                ({LineId}, {DataJson}, {UpdatedAt})
                            VALUES (@lid, @j, @updated_at)";
                        cmd.Parameters.AddWithValue("@lid", lineId);
                        cmd.Parameters.AddWithValue("@j", json);
                        cmd.Parameters.AddWithValue("@updated_at", now);
                        cmd.ExecuteNonQuery();
                    }
                }
                ProjectLogger.WriteDebug($"[RLinkLogService] ✔ SQLite {TableSettings}: upsert line='{lineId}'");

            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ UpsertSettingsToSQLite: " + ex.Message, ex);

            }
        }

        // ════════════════════════════════════════════════════════════
        //  SQLite — tạo bảng (cấu trúc khớp PostgreSQL)
        // ════════════════════════════════════════════════════════════

        private void EnsureSQLiteCreated()
        {
            try
            {
                if (!Directory.Exists(DbDir))
                    Directory.CreateDirectory(DbDir);

                if (File.Exists(DbPath) && !IsValidSQLiteFile(DbPath))
                {
                    ProjectLogger.WriteWarning($"[RLinkLogService] File DB bị lỗi, xóa và tạo lại: {DbPath}");
                    File.Delete(DbPath);
                }

                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        // ── rlink_log_in ─────────────────────────────
                        cmd.CommandText = THTableSchemas.SchemaLogIn.CreateSQLite(SqliteNow);
                        cmd.ExecuteNonQuery();

                        // ── rlink_log_camera ─────────────────────────
                        cmd.CommandText = THTableSchemas.SchemaLogCamera.CreateSQLite(SqliteNow);
                        cmd.ExecuteNonQuery();

                        // ── rlink_log_camera_error ───────────────────
                        cmd.CommandText = THTableSchemas.SchemaLogCameraError.CreateSQLite(SqliteNow);
                        cmd.ExecuteNonQuery();

                        // ── rlink_allocated_qr ───────────────────────
                        cmd.CommandText = $@"
                            CREATE TABLE IF NOT EXISTS {AllocatedQr} (
                                {Id}           INTEGER  PRIMARY KEY AUTOINCREMENT,
                                {LineId}      TEXT     NOT NULL,
                                {RlinkName}   TEXT,
                                {JobName}     TEXT     NOT NULL,
                                {Batch}        TEXT,
                                {QrCode}      TEXT     NOT NULL,
                                {AllocatedAt} TEXT     NOT NULL,
                                {CreatedAt}   TEXT     NOT NULL DEFAULT ({SqliteNow})
                            )";
                        cmd.ExecuteNonQuery();

                        // ── rlink_issuance_qr ──────────────────────────
                        cmd.CommandText = THTableSchemas.SchemaReceiveHistory.CreateSQLite(SqliteNow);
                        cmd.ExecuteNonQuery();

                        // ── rlink_products (mirror PG rlink_products) ─────────
                        cmd.CommandText = $@"
                            CREATE TABLE IF NOT EXISTS {Products} (
                                {ProductId}   TEXT PRIMARY KEY,
                                {ProductName} TEXT,
                                {ProductGtin} TEXT DEFAULT '',
                                {ProductImage} TEXT DEFAULT '',
                                {Volume}      INTEGER DEFAULT 0,
                                {Exp}         INTEGER DEFAULT 0,
                                {DataJson}    TEXT,
                                {UpdatedAt}   TEXT NOT NULL
                            )";
                        cmd.ExecuteNonQuery();

                        // ── {TableSettings} (mirror PG {TableSettings}) ─────────
                        cmd.CommandText = $@"
                            CREATE TABLE IF NOT EXISTS {TableSettings} (
                                {LineId}    TEXT PRIMARY KEY,
                                {DataJson}  TEXT,
                                {UpdatedAt} TEXT NOT NULL
                            )";
                        cmd.ExecuteNonQuery();

                        // ── rlink_accounts (mirror PG rlink_accounts) ─────
                        cmd.CommandText = $@"
                            CREATE TABLE IF NOT EXISTS {TableAccounts} (
                                {Username}         TEXT PRIMARY KEY,
                                {PasswordHash}    TEXT NOT NULL,
                                {FullName}        TEXT,
                                {Role}            TEXT,
                                {PermissionsJson} TEXT,
                                {AccId}           TEXT,
                                {DisplayName}     TEXT,
                                {PerDeviceId}     TEXT,
                                {UpdatedAt}       TEXT NOT NULL DEFAULT ({SqliteNow})
                            )";
                        cmd.ExecuteNonQuery();

                        // ── configline ───────────────────────────────
                        cmd.CommandText = $@"
                            CREATE TABLE IF NOT EXISTS {ConfigLine} (
                                {Id}              INTEGER  PRIMARY KEY DEFAULT 1,
                                {LineId}         TEXT,
                                {LineName}       TEXT,
                                {FactoryCode}    TEXT,
                                {FactoryName}    TEXT,
                                {MachineIp}      TEXT,
                                {OperatingMode}  INTEGER  DEFAULT 0,
                                {BufferCount}    INTEGER  DEFAULT 0,
                                {AssignedAt}     TEXT     NOT NULL
                            )";
                        cmd.ExecuteNonQuery();

                        // ── code (offline mirror QrBank PG) ──────────
                        cmd.CommandText = $@"
                            CREATE TABLE IF NOT EXISTS {Code} (
                                {Id}              INTEGER  PRIMARY KEY AUTOINCREMENT,
                                {QrCode}         TEXT     NOT NULL UNIQUE,
                                {FactoryCode}    TEXT     DEFAULT '',
                                {LineId}         TEXT     DEFAULT '',
                                {LineName}       TEXT     DEFAULT '',
                                {Batch}           TEXT     DEFAULT '',
                                {ProductId}       TEXT     DEFAULT '',
                                {ManufacturedDate} TEXT    DEFAULT '',
                                {ExpiryDate}      TEXT    DEFAULT '',
                                {JobName}        TEXT     DEFAULT '',
                                {IsUsed}         INTEGER  DEFAULT 0,
                                {IsUsedForJob} INTEGER  DEFAULT 0,
                                {IsPrinted}      INTEGER  DEFAULT 0,
                                {UsedAt}         TEXT,
                                {ReceivedAt}     TEXT     NOT NULL,
                                {CreatedAt}      TEXT     NOT NULL DEFAULT ({SqliteNow})
                            )";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = $@"
                            CREATE INDEX IF NOT EXISTS idx_{Code}_received_at
                            ON {Code} ({ReceivedAt} DESC)";
                        cmd.ExecuteNonQuery();

                        // ── tb_CompletedJobLogs ─────────────────────────
                        cmd.CommandText = $@"
                            CREATE TABLE IF NOT EXISTS {CompletedJobLogs} (
                                no              INTEGER PRIMARY KEY AUTOINCREMENT,
                                {JobName}        TEXT,
                                {UserRlink}      TEXT,
                                {LineId}         TEXT,
                                {ColCreateDate}  TEXT NOT NULL,
                                {ProductId}      TEXT,
                                {QrUsed}         INTEGER DEFAULT 0,
                                {ColTotalPrint}  INTEGER DEFAULT 0,
                                {StatusGood}     INTEGER DEFAULT 0,
                                {StatusFailed}   INTEGER DEFAULT 0,
                                {TotalCheck}     INTEGER DEFAULT 0
                            )";
                        cmd.ExecuteNonQuery();


                    }

                    // ── Migrate các cột còn thiếu cho DB cũ ────────
                    TryAddSqliteColumn(conn, LogIn, RlinkStatus, "TEXT");
                    TryAddSqliteColumn(conn, LogIn, OperatorUser, "TEXT");
                    TryAddSqliteColumn(conn, LogIn, QrCode, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogIn, QrDetail, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogIn, CreatedAt, $"TEXT NOT NULL DEFAULT ({SqliteNow})");
                    TryAddSqliteColumn(conn, LogIn, IsSent, "INTEGER DEFAULT 0");
                    TryAddSqliteColumn(conn, LogIn, SentAt, "TEXT");
                    TryAddSqliteColumn(conn, LogIn, ManufacturedDate, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogIn, ExpiryDate, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogIn, LastPrintedAt, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogIn, PrinterLastProductManufacturedDate, "TEXT DEFAULT ''");

                    TryAddSqliteColumn(conn, LogCamera, RlinkStatus, "TEXT");
                    TryAddSqliteColumn(conn, LogCamera, OperatorUser, "TEXT");
                    TryAddSqliteColumn(conn, LogCamera, Status, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogCamera, QrCode, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogCamera, QrDetail, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogCamera, CreatedAt, $"TEXT NOT NULL DEFAULT ({SqliteNow})");
                    TryAddSqliteColumn(conn, LogCamera, IsSent, "INTEGER DEFAULT 0");
                    TryAddSqliteColumn(conn, LogCamera, SentAt, "TEXT");
                    TryAddSqliteColumn(conn, LogCamera, FrameInfo, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogCamera, CameraManufacturedDate, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogCamera, CameraExpiryDate, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogCamera, CameraLastPacketReceivedAt, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogCamera, CameraLastProductManufacturedDate, "TEXT DEFAULT ''");

                    TryAddSqliteColumn(conn, LogCameraError, ImagePath, "TEXT");
                    TryAddSqliteColumn(conn, LogCameraError, CreatedAt, $"TEXT NOT NULL DEFAULT ({SqliteNow})");
                    TryAddSqliteColumn(conn, LogCameraError, IsSent, "INTEGER DEFAULT 0");
                    TryAddSqliteColumn(conn, LogCameraError, SentAt, "TEXT");
                    TryAddSqliteColumn(conn, LogCameraError, ErrorManufacturedDate, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogCameraError, ErrorExpiryDate, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, LogCameraError, ErrorFrameInfo, "TEXT DEFAULT ''");

                    TryAddSqliteColumn(conn, AllocatedQr, CreatedAt, $"TEXT NOT NULL DEFAULT ({SqliteNow})");
                    TryAddSqliteColumn(conn, Code, CreatedAt, $"TEXT NOT NULL DEFAULT ({SqliteNow})");
                    TryAddSqliteColumn(conn, Code, IsUsed, "INTEGER DEFAULT 0");

                    TryAddSqliteColumn(conn, TableAccounts, AccId, "TEXT");
                    TryAddSqliteColumn(conn, TableAccounts, DisplayName, "TEXT");
                    TryAddSqliteColumn(conn, TableAccounts, PerDeviceId, "TEXT");

                    TryAddSqliteColumn(conn, Products, ProductGtin, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, Products, ProductImage, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, Products, Volume, "INTEGER DEFAULT 0");
                    TryAddSqliteColumn(conn, Products, Exp, "INTEGER DEFAULT 0");

                    TryAddSqliteColumn(conn, Code, CreatedAt, $"TEXT NOT NULL DEFAULT ({SqliteNow})");
                    TryAddSqliteColumn(conn, Code, IsUsed, "INTEGER DEFAULT 0");
                    TryAddSqliteColumn(conn, Code, JobName, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, Code, UsedAt, "TEXT");
                    TryAddSqliteColumn(conn, Code, IsUsedForJob, "INTEGER DEFAULT 0");
                    TryAddSqliteColumn(conn, Code, IsPrinted, "INTEGER DEFAULT 0");
                    TryAddSqliteColumn(conn, Code, ProductId, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, Code, ManufacturedDate, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, Code, ExpiryDate, "TEXT DEFAULT ''");
                    TryAddSqliteColumn(conn, Code, IsSentToMaster, "INTEGER DEFAULT 0");
                    TryAddSqliteColumn(conn, Code, MarkedSentAt, "TEXT");
                }

                _isDbReady = true;
                ProjectLogger.WriteInfo($"[RLinkLogService] ✔ SQLite sẵn sàng: {DbPath}");
            }
            catch (Exception ex)
            {
                _isDbReady = false;
                ProjectLogger.WriteError("[RLinkLogService] ✘ EnsureSQLiteCreated lỗi: " + ex.Message, ex);

            }
        }
        // ════════════════════════════════════════════════════════════
        //  PUBLIC STATIC — Retry sync từ 3 bảng log (thay thế pending_logs)
        // ════════════════════════════════════════════════════════════

        public static List<(long id, LogStatusPayload payload)> GetUnsentLogIn(int limit = 50, string jobName = "", string batch = "")
        {
            var list = new List<(long, LogStatusPayload)>();
            try
            {
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        string filter = "";
                        if (!string.IsNullOrWhiteSpace(jobName))
                            filter += $" AND {JobName} = @jobName";
                        if (!string.IsNullOrWhiteSpace(batch))
                            filter += $" AND {Batch} = @batch";

                        cmd.CommandText = $@"
                            SELECT {Id}, {LineId}, {RlinkName}, {JobName}, {Batch},
                                   {ProductId}, {ProductName}, {Status}, {RlinkStatus},
                                   {OperatorUser}, {Qty}, {QrCode}, {QrDetail}, {Timestamp}
                            FROM {LogIn}
                            WHERE {IsSent} = 0 {filter}
                            ORDER BY {Id} ASC LIMIT @lim";
                        cmd.Parameters.AddWithValue("@lim", limit);
                        if (!string.IsNullOrWhiteSpace(jobName))
                            cmd.Parameters.AddWithValue("@jobName", jobName);
                        if (!string.IsNullOrWhiteSpace(batch))
                            cmd.Parameters.AddWithValue("@batch", batch);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add((r.GetInt64(0), new LogStatusPayload
                                {
                                    line_id = r.GetString(1),
                                    rlink_name = r.IsDBNull(2) ? "" : r.GetString(2),
                                    job_name = r.IsDBNull(3) ? "" : r.GetString(3),
                                    batch = r.IsDBNull(4) ? "" : r.GetString(4),
                                    product_id = r.IsDBNull(5) ? "" : r.GetString(5),
                                    product_name = r.IsDBNull(6) ? "" : r.GetString(6),
                                    status = r.GetString(7),
                                    rlink_status = r.IsDBNull(8) ? "" : r.GetString(8),
                                    operator_user = NormalizeOperator(r.IsDBNull(9) ? "" : r.GetString(9)),
                                    produced = r.IsDBNull(10) ? 0 : r.GetInt32(10),
                                    qr_code = r.IsDBNull(11) ? "" : r.GetString(11),
                                    timestamp = DateTime.TryParse(r.GetString(13), out var dt1) ? dt1 : DateTime.Now
                                }));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ProjectLogger.WriteError("[RLinkLogService] GetUnsentLogIn: " + ex.Message); }
            return list;
        }

        private static string NormalizeOperator(string storedValue)
        {
            if (string.IsNullOrWhiteSpace(storedValue)) return "ACC001";
            return storedValue;
        }

        private static string SerializeQrDetail(List<QrDetailItem> list)
        {
            if (list == null || list.Count == 0) return "";
            return Newtonsoft.Json.JsonConvert.SerializeObject(list);
        }

        private static List<QrDetailItem> DeserializeQrDetail(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<QrDetailItem>();
            try { return Newtonsoft.Json.JsonConvert.DeserializeObject<List<QrDetailItem>>(json) ?? new List<QrDetailItem>(); }
            catch { return new List<QrDetailItem>(); }
        }

        public static List<(long id, LogCameraPayload payload)> GetUnsentLogCamera(int limit = 50, string jobName = "", string batch = "")
        {
            var list = new List<(long, LogCameraPayload)>();
            try
            {
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        string filter = "";
                        if (!string.IsNullOrWhiteSpace(jobName))
                            filter += $" AND {JobName} = @jobName";
                        if (!string.IsNullOrWhiteSpace(batch))
                            filter += $" AND {Batch} = @batch";

                        cmd.CommandText = $@"
                            SELECT {Id}, {LineId}, {RlinkName}, {JobName}, {Batch},
                                   {ProductId}, {ProductName}, {Status}, {RlinkStatus}, {OperatorUser},
                                   {StatusGood}, {StatusFail}, {QrCode}, {QrDetail}, {Timestamp}
                            FROM {LogCamera}
                            WHERE {IsSent} = 0 {filter}
                            ORDER BY {Id} ASC LIMIT @lim";
                        cmd.Parameters.AddWithValue("@lim", limit);
                        if (!string.IsNullOrWhiteSpace(jobName))
                            cmd.Parameters.AddWithValue("@jobName", jobName);
                        if (!string.IsNullOrWhiteSpace(batch))
                            cmd.Parameters.AddWithValue("@batch", batch);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add((r.GetInt64(0), new LogCameraPayload
                                {
                                    line_id = r.GetString(1),
                                    rlink_name = r.IsDBNull(2) ? "" : r.GetString(2),
                                    job_name = r.IsDBNull(3) ? "" : r.GetString(3),
                                    batch = r.IsDBNull(4) ? "" : r.GetString(4),
                                    product_id = r.IsDBNull(5) ? "" : r.GetString(5),
                                    product_name = r.IsDBNull(6) ? "" : r.GetString(6),
                                    status = r.IsDBNull(7) ? "" : r.GetString(7),
                                    rlink_status = r.IsDBNull(8) ? "" : r.GetString(8),
                                    operator_user = NormalizeOperator(r.IsDBNull(9) ? "" : r.GetString(9)),
                                    camera_ok = r.IsDBNull(10) ? 0 : r.GetInt32(10),
                                    camera_fail = r.IsDBNull(11) ? 0 : r.GetInt32(11),
                                    qr_code = r.IsDBNull(12) ? "" : r.GetString(12),
                                    timestamp = DateTime.TryParse(r.GetString(14), out var dt2) ? dt2 : DateTime.Now
                                }));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ProjectLogger.WriteError("[RLinkLogService] GetUnsentLogCamera: " + ex.Message); }
            return list;
        }

        public static List<(long id, LogCameraErrorPayload payload)> GetUnsentLogCameraError(int limit = 50, string jobName = "", string batch = "")
        {
            var list = new List<(long, LogCameraErrorPayload)>();
            try
            {
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        string filter = "";
                        if (!string.IsNullOrWhiteSpace(jobName))
                            filter += $" AND {JobName} = @jobName";
                        if (!string.IsNullOrWhiteSpace(batch))
                            filter += $" AND {Batch} = @batch";

                        cmd.CommandText = $@"
                            SELECT {Id}, {LineId}, {RlinkName}, {JobName}, {Batch},
                                   {ProductId}, {ProductName}, {RlinkStatus}, {OperatorUser},
                                   {QrCode}, {ErrorManufacturedDate}, {ErrorExpiryDate}, {ErrorType},
                                   {ErrorFrameInfo}, {ImagePath}, {Timestamp}
                            FROM {LogCameraError}
                            WHERE {IsSent} = 0 {filter}
                            ORDER BY {Id} ASC LIMIT @lim";
                        cmd.Parameters.AddWithValue("@lim", limit);
                        if (!string.IsNullOrWhiteSpace(jobName))
                            cmd.Parameters.AddWithValue("@jobName", jobName);
                        if (!string.IsNullOrWhiteSpace(batch))
                            cmd.Parameters.AddWithValue("@batch", batch);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add((r.GetInt64(0), new LogCameraErrorPayload
                                {
                                    line_id = r.GetString(1),
                                    rlink_name = r.IsDBNull(2) ? "" : r.GetString(2),
                                    job_name = r.IsDBNull(3) ? "" : r.GetString(3),
                                    batch = r.IsDBNull(4) ? "" : r.GetString(4),
                                    product_id = r.IsDBNull(5) ? "" : r.GetString(5),
                                    product_name = r.IsDBNull(6) ? "" : r.GetString(6),
                                    rlink_status = r.IsDBNull(7) ? "" : r.GetString(7),
                                    operator_user = NormalizeOperator(r.IsDBNull(8) ? "" : r.GetString(8)),
                                    qr_code = r.IsDBNull(9) ? "" : r.GetString(9),
                                    error_manufactured_date = r.IsDBNull(10) ? "" : r.GetString(10),
                                    error_expiry_date = r.IsDBNull(11) ? "" : r.GetString(11),
                                    error_type = r.IsDBNull(12) ? "" : r.GetString(12),
                                    error_frame_info = r.IsDBNull(13) ? "" : r.GetString(13),
                                    image_path = r.IsDBNull(14) ? "" : r.GetString(14),
                                    timestamp = DateTime.TryParse(r.GetString(15), out var dt3) ? dt3 : DateTime.Now
                                }));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ProjectLogger.WriteError("[RLinkLogService] GetUnsentLogCameraError: " + ex.Message); }
            return list;
        }

        /// <summary>Đánh dấu is_sent=1, sent_at=now cho 1 row trong bảng log bất kỳ.</summary>
        public static void MarkLogRowSent(string table, long id)
        {
            try
            {
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"UPDATE {table} SET {IsSent}=1, {SentAt}=datetime('now','localtime') WHERE {Id}=@id";
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) { ProjectLogger.WriteError($"[RLinkLogService] MarkLogRowSent {table}#{id}: " + ex.Message); }
        }

        /// <summary>Đánh dấu is_sent=TRUE, sent_at=NOW() cho 1 row trong bảng log PostgreSQL.</summary>
        public static void MarkLogRowSentPg(string table, long id)
        {
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(
                        $"UPDATE \"{table}\" SET {IsSent}=TRUE, {SentAt}=NOW() WHERE {Id}=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", (int)id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) { ProjectLogger.WriteError($"[RLinkLogService] MarkLogRowSentPg {table}#{id}: " + ex.Message); }
        }
        /// <summary>SQLite không hỗ trợ ADD COLUMN IF NOT EXISTS — bắt exception khi cột đã tồn tại.</summary>
        private static void TryAddSqliteColumn(SQLiteConnection conn, string table, string column, string type)
        {
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {type}";
                        cmd.ExecuteNonQuery();
                    }
                }
            catch (SQLiteException) { /* cột đã tồn tại */ }
            catch (Exception ex) { Console.WriteLine($"[RLinkLogService] AddCol {table}.{column}: {ex.Message}"); }
        }

        private static bool IsValidSQLiteFile(string path)
        {
            try
            {
                byte[] header = new byte[16];
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length < 16) return false;
                    fs.Read(header, 0, 16);
                }
                return System.Text.Encoding.ASCII.GetString(header, 0, 15) == "SQLite format 3";
            }
            catch { return false; }
        }

        // ════════════════════════════════════════════════════════════
        //  PostgreSQL
        // ════════════════════════════════════════════════════════════

        private void EnsurePostgresTablesCreated()
        {
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr))
                {
                    ProjectLogger.WriteWarning("[RLinkLogService] PostgreSQL: chưa cấu hình DB — bỏ qua.");

                    return;
                }

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();

                    using (var cmd = new Npgsql.NpgsqlCommand(
                        THTableSchemas.SchemaLogIn.CreatePg(), conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        ALTER TABLE {PgTableLogIn} ADD COLUMN IF NOT EXISTS {RlinkStatus}  TEXT;
                        ALTER TABLE {PgTableLogIn} ADD COLUMN IF NOT EXISTS {OperatorUser} TEXT;
                        ALTER TABLE {PgTableLogIn} ADD COLUMN IF NOT EXISTS {QrCode} TEXT DEFAULT '';
                        ALTER TABLE {PgTableLogIn} ADD COLUMN IF NOT EXISTS {QrDetail} TEXT DEFAULT '';
                        ALTER TABLE {PgTableLogIn} ADD COLUMN IF NOT EXISTS {IsSent} INTEGER DEFAULT 0;
                        ALTER TABLE {PgTableLogIn} ADD COLUMN IF NOT EXISTS {SentAt} TIMESTAMP;
                        ALTER TABLE {PgTableLogIn} ADD COLUMN IF NOT EXISTS {ManufacturedDate} TEXT DEFAULT '';
                        ALTER TABLE {PgTableLogIn} ADD COLUMN IF NOT EXISTS {ExpiryDate} TEXT DEFAULT '';
                        ALTER TABLE {PgTableLogIn} ADD COLUMN IF NOT EXISTS {LastPrintedAt} TEXT DEFAULT '';
                        ALTER TABLE {PgTableLogIn} ADD COLUMN IF NOT EXISTS {PrinterLastProductManufacturedDate} TEXT DEFAULT '';
                        ALTER TABLE {PgTableLogIn} DROP COLUMN IF EXISTS {IsUsed};
                        ALTER TABLE {PgTableLogIn} DROP COLUMN IF EXISTS {UsedAt};", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new Npgsql.NpgsqlCommand(
                        THTableSchemas.SchemaReceiveHistory.CreatePg(), conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new Npgsql.NpgsqlCommand(
                        THTableSchemas.SchemaLogCamera.CreatePg(), conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new Npgsql.NpgsqlCommand(
                        THTableSchemas.SchemaLogCameraError.CreatePg(), conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        ALTER TABLE {PgTableLogCameraError} ADD COLUMN IF NOT EXISTS {ImagePath} TEXT;
                        ALTER TABLE {PgTableLogCameraError} ADD COLUMN IF NOT EXISTS {IsSent} INTEGER DEFAULT 0;
                        ALTER TABLE {PgTableLogCameraError} ADD COLUMN IF NOT EXISTS {SentAt} TIMESTAMP;
                        ALTER TABLE {PgTableLogCameraError} ADD COLUMN IF NOT EXISTS {ErrorManufacturedDate} TEXT DEFAULT '';
                        ALTER TABLE {PgTableLogCameraError} ADD COLUMN IF NOT EXISTS {ErrorExpiryDate} TEXT DEFAULT '';
                        ALTER TABLE {PgTableLogCameraError} ADD COLUMN IF NOT EXISTS {ErrorFrameInfo} TEXT DEFAULT '';
                        ALTER TABLE {PgTableLogCameraError} DROP COLUMN IF EXISTS {IsUsed};
                        ALTER TABLE {PgTableLogCameraError} DROP COLUMN IF EXISTS {UsedAt};", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        CREATE TABLE IF NOT EXISTS {PgTableAllocatedQr} (
                            {Id}           BIGSERIAL PRIMARY KEY,
                            {LineId}      TEXT NOT NULL,
                            {RlinkName}   TEXT,
                            {JobName}     TEXT NOT NULL,
                            {Batch}        TEXT,
                            {QrCode}      TEXT NOT NULL,
                            {AllocatedAt} TIMESTAMP NOT NULL,
                            {CreatedAt}   TIMESTAMP DEFAULT NOW()
                        );", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // ── rlink_accounts ─────────────────────────────────
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        CREATE TABLE IF NOT EXISTS ""{TableAccounts}"" (
                            {Username}         TEXT PRIMARY KEY,
                            {PasswordHash}    TEXT NOT NULL,
                            {FullName}        TEXT,
                            {Role}            TEXT,
                            {PermissionsJson} TEXT,
                            {AccId}           TEXT,
                            {DisplayName}     TEXT,
                            {PerDeviceId}     TEXT,
                            {UpdatedAt}       TIMESTAMPTZ DEFAULT NOW()
                        );
                        ALTER TABLE ""{TableAccounts}"" ADD COLUMN IF NOT EXISTS {AccId} TEXT;
                        ALTER TABLE ""{TableAccounts}"" ADD COLUMN IF NOT EXISTS {DisplayName} TEXT;
                        ALTER TABLE ""{TableAccounts}"" ADD COLUMN IF NOT EXISTS {PerDeviceId} TEXT;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // ── rlink_permissions ──────────────────────────────
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        CREATE TABLE IF NOT EXISTS {TablePermissions} (
                            {Username}         TEXT PRIMARY KEY,
                            {PermissionsJson}  TEXT,
                            {UpdatedAt}        TIMESTAMPTZ DEFAULT NOW()
                        );", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // ── tb_CompletedJobLogs ─────────────────────────────
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        CREATE TABLE IF NOT EXISTS {CompletedJobLogs} (
                            no              BIGSERIAL PRIMARY KEY,
                            {JobName}        TEXT,
                            {UserRlink}      TEXT,
                            {LineId}         TEXT,
                            {ColCreateDate}  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                            {ProductId}      TEXT,
                            {QrUsed}         INTEGER DEFAULT 0,
                            {ColTotalPrint}  INTEGER DEFAULT 0,
                            {StatusGood}     INTEGER DEFAULT 0,
                            {StatusFailed}   INTEGER DEFAULT 0,
                            {TotalCheck}     INTEGER DEFAULT 0
                        );", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // ── Bảng code (QrBank mirror) ──────────────────────
                    string codeTable = Code;

                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        CREATE TABLE IF NOT EXISTS ""{codeTable}"" (
                            {Id}           BIGSERIAL    PRIMARY KEY,
                            {QrCode}      TEXT         NOT NULL,
                            {FactoryCode} VARCHAR(20)  DEFAULT '',
                            {LineId}      VARCHAR(50)  DEFAULT '',
                            {LineName}    VARCHAR(100) DEFAULT '',
                            {Batch}        VARCHAR(100) DEFAULT '',
                            {JobName}     VARCHAR(200) DEFAULT '',
                            {ReceivedAt}  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
                            {UsedAt}      TIMESTAMPTZ,
                            {IsUsed}      BOOLEAN      NOT NULL DEFAULT FALSE
                        );
                        CREATE INDEX IF NOT EXISTS idx_{codeTable}_received_at ON ""{codeTable}"" ({ReceivedAt} DESC);
                        CREATE INDEX IF NOT EXISTS idx_{codeTable}_is_used     ON ""{codeTable}"" ({IsUsed});
                        DO $$
                        BEGIN
                            IF NOT EXISTS (
                                SELECT 1 FROM pg_constraint WHERE LOWER(conname) = LOWER('uq_{codeTable}_{QrCode}')
                            ) THEN
                                ALTER TABLE ""{codeTable}"" ADD CONSTRAINT uq_{codeTable}_{QrCode} UNIQUE ({QrCode});
                            END IF;
                            IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{codeTable}' AND column_name='{ReceivedAt}') THEN
                                ALTER TABLE ""{codeTable}"" ADD COLUMN {ReceivedAt} TIMESTAMPTZ NOT NULL DEFAULT NOW();
                            END IF;
                            IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{codeTable}' AND column_name='{IsUsed}') THEN
                                ALTER TABLE ""{codeTable}"" ADD COLUMN {IsUsed} BOOLEAN NOT NULL DEFAULT FALSE;
                            END IF;
                            IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{codeTable}' AND column_name='{JobName}') THEN
                                ALTER TABLE ""{codeTable}"" ADD COLUMN {JobName} VARCHAR(200) DEFAULT '';
                            END IF;
                            IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{codeTable}' AND column_name='{UsedAt}') THEN
                                ALTER TABLE ""{codeTable}"" ADD COLUMN {UsedAt} TIMESTAMPTZ;
                            END IF;
                        END$$;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                _isPgReady = true;
                ProjectLogger.WriteInfo($"[RLinkLogService] ✔ PostgreSQL sẵn sàng: {PgTableLogIn}, {PgTableLogCamera}, {PgTableLogCameraError}");

            }
            catch (Exception ex)
            {
                _isPgReady = false;
                ProjectLogger.WriteError("[RLinkLogService] ✘ EnsurePostgresTablesCreated lỗi: " + ex.Message, ex);

            }
        }

        internal static string GetPgConnStr()
        {
            var s = Shared.Settings;
            if (string.IsNullOrWhiteSpace(s.THLocalDbServer) ||
                string.IsNullOrWhiteSpace(s.THLocalDbDatabase))
                return null;

            return BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                "postgresql",
                s.THLocalDbServer,
                s.THLocalDbPort,
                s.THLocalDbUsername,
                s.THLocalDbPassword,
                s.THLocalDbDatabase);
        }

        /// <summary>Đảm bảo bảng code (QrBank) tồn tại trên PG — gọi trước mọi SELECT/INSERT.</summary>
        public static void EnsurePgQrBankTable(Npgsql.NpgsqlConnection conn)
        {
            string table = Code;

            using (var cmd = new Npgsql.NpgsqlCommand($@"
                CREATE TABLE IF NOT EXISTS ""{table}"" (
                    {Id}              BIGSERIAL    PRIMARY KEY,
                    {QrCode}         TEXT         NOT NULL UNIQUE,
                    {FactoryCode}    VARCHAR(20)  DEFAULT '',
                    {LineId}         VARCHAR(50)  DEFAULT '',
                    {LineName}       VARCHAR(100) DEFAULT '',
                    {Batch}           VARCHAR(100) DEFAULT '',
                    {JobName}        VARCHAR(200) DEFAULT '',
                    {ReceivedAt}     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
                    {UsedAt}         TIMESTAMPTZ,
                    {IsUsed}         BOOLEAN      NOT NULL DEFAULT FALSE,
                    {IsUsedForJob} BOOLEAN      DEFAULT FALSE,
                    {IsPrinted}      BOOLEAN      DEFAULT FALSE,
                    {PrintedAt}      TIMESTAMPTZ,
                    {IsSentToMaster} BOOLEAN      DEFAULT FALSE
                )", conn))
            {
                cmd.ExecuteNonQuery();
            }

            try
            {
                using (var cmd2 = new Npgsql.NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD CONSTRAINT IF NOT EXISTS uq_{table}_{QrCode} UNIQUE ({QrCode})", conn))
                    cmd2.ExecuteNonQuery();
            }
            catch { }
            try
            {
                using (var cmd3 = new Npgsql.NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS {IsUsedForJob} BOOLEAN DEFAULT FALSE", conn))
                    cmd3.ExecuteNonQuery();
            }
            catch { }
            try
            {
                using (var cmd4 = new Npgsql.NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS {IsPrinted} BOOLEAN DEFAULT FALSE", conn))
                    cmd4.ExecuteNonQuery();
            }
            catch { }
            try
            {
                using (var cmd5 = new Npgsql.NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS {PrintedAt} TIMESTAMPTZ", conn))
                    cmd5.ExecuteNonQuery();
            }
            catch { }
            try
            {
                using (var cmd = new Npgsql.NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS {ProductId} VARCHAR(200) DEFAULT ''", conn))
                    cmd.ExecuteNonQuery();
            }
            catch { }
            try
            {
                using (var cmd = new Npgsql.NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS {ManufacturedDate} TEXT DEFAULT ''", conn))
                    cmd.ExecuteNonQuery();
            }
            catch { }
            try
            {
                using (var cmd = new Npgsql.NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS {ExpiryDate} TEXT DEFAULT ''", conn))
                    cmd.ExecuteNonQuery();
            }
            catch { }
            try
            {
                using (var cmd5 = new Npgsql.NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS {IsSentToMaster} BOOLEAN DEFAULT FALSE", conn))
                    cmd5.ExecuteNonQuery();
            }
            catch { }
            try
            {
                using (var cmd6 = new Npgsql.NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS {MarkedSentAt} TIMESTAMPTZ", conn))
                    cmd6.ExecuteNonQuery();
            }
            catch { }
            try
            {
                using (var cmd = new Npgsql.NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS product_gtin VARCHAR(50) DEFAULT ''", conn))
                    cmd.ExecuteNonQuery();
            }
            catch { }
        }

        // ════════════════════════════════════════════════════════════
        //  INSERT — Log In
        // ════════════════════════════════════════════════════════════

        /// <summary>INSERT SQLite rlink_log_in (is_sent=0). Trả về rowId vừa insert.</summary>
        private long InsertLogInToSQLite(LogStatusPayload p)
        {
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"
                        INSERT INTO {LogIn}
                            ({LineId}, {RlinkName}, {JobName}, {Batch}, {ProductId}, {ProductName},
                             {Status}, {RlinkStatus}, {OperatorUser}, {Qty}, {QrCode}, {QrDetail},
                             {ManufacturedDate}, {ExpiryDate}, {LastPrintedAt}, {PrinterLastProductManufacturedDate}, {Timestamp}, {IsSent})
                        VALUES
                            (@line_id, @rlink_name, @job_name, @batch, @product_id, @product_name,
                             @status, @rlink_status, @operator_user, @qty, @qr_code, @qr_detail,
                             @manufactured_date, @expiry_date, @last_printed_at, @printer_last_product_manufactured_date, @timestamp, 0)";
                    cmd.Parameters.AddWithValue("@line_id", p.line_id ?? "");
                    cmd.Parameters.AddWithValue("@rlink_name", p.rlink_name ?? "");
                    cmd.Parameters.AddWithValue("@job_name", p.job_name ?? "");
                    cmd.Parameters.AddWithValue("@batch", p.batch ?? "");
                    cmd.Parameters.AddWithValue("@product_id", p.product_id ?? "");
                    cmd.Parameters.AddWithValue("@product_name", p.product_name ?? "");
                    cmd.Parameters.AddWithValue("@status", p.status ?? "");
                    cmd.Parameters.AddWithValue("@rlink_status", p.rlink_status ?? "");
                    cmd.Parameters.AddWithValue("@operator_user", p.operator_user ?? "");
                    cmd.Parameters.AddWithValue("@qty", p.produced);
                    cmd.Parameters.AddWithValue("@qr_code", p.qr_code ?? "");
                    cmd.Parameters.AddWithValue("@qr_detail", SerializeQrDetail(p.qr_detail));
                    cmd.Parameters.AddWithValue("@manufactured_date", p.manufactured_date ?? "");
                    cmd.Parameters.AddWithValue("@expiry_date", p.expiry_date ?? "");
                    cmd.Parameters.AddWithValue("@last_printed_at", p.last_printed_at ?? "");
                    cmd.Parameters.AddWithValue("@printer_last_product_manufactured_date", p.printer_last_product_manufactured_date ?? "");
                    cmd.Parameters.AddWithValue("@timestamp", p.timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
                return conn.LastInsertRowId;
            }
        }


        private long InsertLogInToPostgres(LogStatusPayload p)
        {
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return -1;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        INSERT INTO {PgTableLogIn}
                            ({LineId}, {RlinkName}, {JobName}, {Batch}, {ProductId}, {ProductName},
                             {Status}, {RlinkStatus}, {OperatorUser}, {Qty}, {QrCode}, {QrDetail},
                             {ManufacturedDate}, {ExpiryDate}, {LastPrintedAt}, {PrinterLastProductManufacturedDate}, {Timestamp})
                        VALUES
                            (@line_id, @rlink_name, @job_name, @batch, @product_id, @product_name,
                             @status, @rlink_status, @operator_user, @qty, @qr_code, @qr_detail,
                             @manufactured_date, @expiry_date, @last_printed_at, @printer_last_product_manufactured_date, @timestamp)
                        RETURNING {Id}",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("line_id", p.line_id ?? "");
                        cmd.Parameters.AddWithValue("rlink_name", p.rlink_name ?? "");
                        cmd.Parameters.AddWithValue("job_name", p.job_name ?? "");
                        cmd.Parameters.AddWithValue("batch", p.batch ?? "");
                        cmd.Parameters.AddWithValue("product_id", p.product_id ?? "");
                        cmd.Parameters.AddWithValue("product_name", p.product_name ?? "");
                        cmd.Parameters.AddWithValue("status", p.status ?? "");
                        cmd.Parameters.AddWithValue("rlink_status", p.rlink_status ?? "");
                        cmd.Parameters.AddWithValue("operator_user", p.operator_user ?? "");
                        cmd.Parameters.AddWithValue("qty", p.produced);
                        cmd.Parameters.AddWithValue("qr_code", p.qr_code ?? "");
                        cmd.Parameters.AddWithValue("qr_detail", SerializeQrDetail(p.qr_detail));
                        cmd.Parameters.AddWithValue("manufactured_date", p.manufactured_date ?? "");
                        cmd.Parameters.AddWithValue("expiry_date", p.expiry_date ?? "");
                        cmd.Parameters.AddWithValue("last_printed_at", p.last_printed_at ?? "");
                        cmd.Parameters.AddWithValue("printer_last_product_manufactured_date", p.printer_last_product_manufactured_date ?? "");
                        cmd.Parameters.AddWithValue("timestamp", p.timestamp);
                        object result = cmd.ExecuteScalar();
                        long id = result != null ? Convert.ToInt64(result) : -1;
                        ProjectLogger.WriteDebug($"[RLinkLogService] ✔ PG LogIn status={p.status} id={id}");
                        return id;
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ InsertLogInToPostgres: " + ex.Message, ex);
                return -1;
            }
        }

        // ════════════════════════════════════════════════════════════
        //  INSERT — Log Camera
        // ════════════════════════════════════════════════════════════

        /// <summary>INSERT SQLite rlink_log_camera (is_sent=0). Trả về rowId vừa insert.</summary>
        private long InsertLogCameraToSQLite(LogCameraPayload p)
        {
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"
                        INSERT INTO {LogCamera}
                            ({LineId}, {RlinkName}, {JobName}, {Batch}, {ProductId}, {ProductName},
                             {Status}, {RlinkStatus}, {OperatorUser},
                             {StatusGood}, {StatusFail}, {TotalCheck}, {QrCode}, {QrDetail}, {FrameInfo},
                             {CameraManufacturedDate}, {CameraExpiryDate}, {CameraLastPacketReceivedAt}, {CameraLastProductManufacturedDate},
                             {Timestamp}, {IsSent})
                        VALUES
                            (@line_id, @rlink_name, @job_name, @batch, @product_id, @product_name,
                             @status, @rlink_status, @operator_user,
                             @status_good, @status_fail, @total_check, @qr_code, @qr_detail, @frame_info,
                             @camera_manufactured_date, @camera_expiry_date, @camera_last_packet_received_at, @camera_last_product_manufactured_date,
                             @timestamp, 0)";
                    cmd.Parameters.AddWithValue("@line_id", p.line_id ?? "");
                    cmd.Parameters.AddWithValue("@rlink_name", p.rlink_name ?? "");
                    cmd.Parameters.AddWithValue("@job_name", p.job_name ?? "");
                    cmd.Parameters.AddWithValue("@batch", p.batch ?? "");
                    cmd.Parameters.AddWithValue("@product_id", p.product_id ?? "");
                    cmd.Parameters.AddWithValue("@product_name", p.product_name ?? "");
                    cmd.Parameters.AddWithValue("@status", p.status ?? "");
                    cmd.Parameters.AddWithValue("@rlink_status", p.rlink_status ?? "");
                    cmd.Parameters.AddWithValue("@operator_user", p.operator_user ?? "");
                    cmd.Parameters.AddWithValue("@status_good", p.camera_ok);
                    cmd.Parameters.AddWithValue("@status_fail", p.camera_fail);
                    cmd.Parameters.AddWithValue("@total_check", p.total_check);
                    cmd.Parameters.AddWithValue("@qr_code", p.qr_code ?? "");
                    cmd.Parameters.AddWithValue("@qr_detail", SerializeQrDetail(p.qr_detail));
                    cmd.Parameters.AddWithValue("@frame_info", p.frame_info ?? "");
                    cmd.Parameters.AddWithValue("@camera_manufactured_date", p.camera_manufactured_date ?? "");
                    cmd.Parameters.AddWithValue("@camera_expiry_date", p.camera_expiry_date ?? "");
                    cmd.Parameters.AddWithValue("@camera_last_packet_received_at", p.camera_last_packet_received_at ?? "");
                    cmd.Parameters.AddWithValue("@camera_last_product_manufactured_date", p.camera_last_product_manufactured_date ?? "");
                    cmd.Parameters.AddWithValue("@timestamp", p.timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
                return conn.LastInsertRowId;
            }
        }

        private long InsertLogCameraToPostgres(LogCameraPayload p)
        {
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return -1;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        INSERT INTO {PgTableLogCamera}
                            ({LineId}, {RlinkName}, {JobName}, {Batch}, {ProductId}, {ProductName},
                             {Status}, {RlinkStatus}, {OperatorUser},
                             {StatusGood}, {StatusFail}, {TotalCheck}, {QrCode}, {QrDetail}, {FrameInfo},
                             {CameraManufacturedDate}, {CameraExpiryDate}, {CameraLastPacketReceivedAt}, {CameraLastProductManufacturedDate},
                             {Timestamp})
                        VALUES
                            (@line_id, @rlink_name, @job_name, @batch, @product_id, @product_name,
                             @status, @rlink_status, @operator_user,
                             @status_good, @status_fail, @total_check, @qr_code, @qr_detail, @frame_info,
                             @camera_manufactured_date, @camera_expiry_date, @camera_last_packet_received_at, @camera_last_product_manufactured_date,
                             @timestamp)
                        RETURNING {Id}",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("line_id", p.line_id ?? "");
                        cmd.Parameters.AddWithValue("rlink_name", p.rlink_name ?? "");
                        cmd.Parameters.AddWithValue("job_name", p.job_name ?? "");
                        cmd.Parameters.AddWithValue("batch", p.batch ?? "");
                        cmd.Parameters.AddWithValue("product_id", p.product_id ?? "");
                        cmd.Parameters.AddWithValue("product_name", p.product_name ?? "");
                        cmd.Parameters.AddWithValue("status", p.status ?? "");
                        cmd.Parameters.AddWithValue("rlink_status", p.rlink_status ?? "");
                        cmd.Parameters.AddWithValue("operator_user", p.operator_user ?? "");
                        cmd.Parameters.AddWithValue("status_good", p.camera_ok);
                        cmd.Parameters.AddWithValue("status_fail", p.camera_fail);
                        cmd.Parameters.AddWithValue("total_check", p.total_check);
                        cmd.Parameters.AddWithValue("qr_code", p.qr_code ?? "");
                        cmd.Parameters.AddWithValue("qr_detail", SerializeQrDetail(p.qr_detail));
                        cmd.Parameters.AddWithValue("frame_info", p.frame_info ?? "");
                        cmd.Parameters.AddWithValue("camera_manufactured_date", p.camera_manufactured_date ?? "");
                        cmd.Parameters.AddWithValue("camera_expiry_date", p.camera_expiry_date ?? "");
                        cmd.Parameters.AddWithValue("camera_last_packet_received_at", p.camera_last_packet_received_at ?? "");
                        cmd.Parameters.AddWithValue("camera_last_product_manufactured_date", p.camera_last_product_manufactured_date ?? "");
                        cmd.Parameters.AddWithValue("timestamp", p.timestamp);
                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt64(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteDebug($"[RLinkLogService] ✘ InsertLogCameraToPostgres: {ex.Message}");
                return -1;
            }
        
                
        }

        // ════════════════════════════════════════════════════════════
        //  INSERT — Log Camera Error
        // ════════════════════════════════════════════════════════════

        /// <summary>INSERT SQLite rlink_log_camera_error (is_sent=0). Trả về rowId vừa insert.</summary>
        private long InsertLogCameraErrorToSQLite(LogCameraErrorPayload p)
        {
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"
                        INSERT INTO {LogCameraError}
                            ({LineId}, {RlinkName}, {JobName}, {Batch}, {ProductId}, {ProductName},
                             {RlinkStatus}, {OperatorUser},
                             {QrCode}, {ErrorManufacturedDate}, {ErrorExpiryDate}, {ErrorType},
                             {ErrorFrameInfo}, {ImagePath}, {Timestamp}, {IsSent})
                        VALUES
                            (@line_id, @rlink_name, @job_name, @batch, @product_id, @product_name,
                             @rlink_status, @operator_user,
                             @qr_code, @error_manufactured_date, @error_expiry_date, @error_type,
                             @error_frame_info, @image_path, @timestamp, 0)";
                    cmd.Parameters.AddWithValue("@line_id", p.line_id ?? "");
                    cmd.Parameters.AddWithValue("@rlink_name", p.rlink_name ?? "");
                    cmd.Parameters.AddWithValue("@job_name", p.job_name ?? "");
                    cmd.Parameters.AddWithValue("@batch", p.batch ?? "");
                    cmd.Parameters.AddWithValue("@product_id", p.product_id ?? "");
                    cmd.Parameters.AddWithValue("@product_name", p.product_name ?? "");
                    cmd.Parameters.AddWithValue("@rlink_status", p.rlink_status ?? "");
                    cmd.Parameters.AddWithValue("@operator_user", p.operator_user ?? "");
                    cmd.Parameters.AddWithValue("@qr_code", p.qr_code ?? "");
                    cmd.Parameters.AddWithValue("@error_manufactured_date", p.error_manufactured_date ?? "");
                    cmd.Parameters.AddWithValue("@error_expiry_date", p.error_expiry_date ?? "");
                    cmd.Parameters.AddWithValue("@error_type", p.error_type ?? "");
                    cmd.Parameters.AddWithValue("@error_frame_info", p.error_frame_info ?? "");
                    cmd.Parameters.AddWithValue("@image_path", p.image_path ?? "");
                    cmd.Parameters.AddWithValue("@timestamp", p.timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
                return conn.LastInsertRowId;
            }
        }

        private long InsertLogCameraErrorToPostgres(LogCameraErrorPayload p)
        {
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return -1;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        INSERT INTO {PgTableLogCameraError}
                            ({LineId}, {RlinkName}, {JobName}, {Batch}, {ProductId}, {ProductName},
                             {RlinkStatus}, {OperatorUser},
                             {QrCode}, {ErrorManufacturedDate}, {ErrorExpiryDate}, {ErrorType},
                             {ErrorFrameInfo}, {ImagePath}, {Timestamp})
                        VALUES
                            (@line_id, @rlink_name, @job_name, @batch, @product_id, @product_name,
                             @rlink_status, @operator_user,
                             @qr_code, @error_manufactured_date, @error_expiry_date, @error_type,
                             @error_frame_info, @image_path, @timestamp)
                        RETURNING {Id}",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("line_id", p.line_id ?? "");
                        cmd.Parameters.AddWithValue("rlink_name", p.rlink_name ?? "");
                        cmd.Parameters.AddWithValue("job_name", p.job_name ?? "");
                        cmd.Parameters.AddWithValue("batch", p.batch ?? "");
                        cmd.Parameters.AddWithValue("product_id", p.product_id ?? "");
                        cmd.Parameters.AddWithValue("product_name", p.product_name ?? "");
                        cmd.Parameters.AddWithValue("rlink_status", p.rlink_status ?? "");
                        cmd.Parameters.AddWithValue("operator_user", p.operator_user ?? "");
                        cmd.Parameters.AddWithValue("qr_code", p.qr_code ?? "");
                        cmd.Parameters.AddWithValue("error_manufactured_date", p.error_manufactured_date ?? "");
                        cmd.Parameters.AddWithValue("error_expiry_date", p.error_expiry_date ?? "");
                        cmd.Parameters.AddWithValue("error_type", p.error_type ?? "");
                        cmd.Parameters.AddWithValue("error_frame_info", p.error_frame_info ?? "");
                        cmd.Parameters.AddWithValue("image_path", p.image_path ?? "");
                        cmd.Parameters.AddWithValue("timestamp", p.timestamp);
                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt64(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ InsertLogCameraErrorToPostgres: {ex.Message}");
                return -1;
            }
        }
                
        // ════════════════════════════════════════════════════════════
        //  HELPER — Đánh dấu đã gửi server thành công (is_sent=1)
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// Cập nhật is_sent=1, sent_at=now cho hàng vừa insert.
        /// Gọi sau khi API R-Link Master trả về success.
        /// </summary>
        internal static void MarkTableRowSent(string table, long rowId)
        {
            try
            {
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"UPDATE {table} SET {IsSent}=1, {SentAt}=datetime('now','localtime') WHERE {Id}=@id";
                        cmd.Parameters.AddWithValue("@id", rowId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ MarkTableRowSent {table}#{rowId}: {ex.Message}");
            }
        }

        /// <summary>Đánh dấu is_sent=1 trên PG — tìm theo job_name + batch + timestamp.</summary>
        public static void MarkTableRowSentPg(string table, string jobName, string batch, string timestamp)
        {
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(
                        $"UPDATE {table} SET {IsSent}=1, {SentAt}=NOW() WHERE {JobName}=@jn AND {Batch}=@bt AND {Timestamp}=@ts AND {IsSent}=0", conn))
                    {
                        cmd.Parameters.AddWithValue("jn", jobName ?? "");
                        cmd.Parameters.AddWithValue("bt", batch ?? "");
                        cmd.Parameters.AddWithValue("ts", DateTime.Parse(timestamp));
                        int updated = cmd.ExecuteNonQuery();
                        if (updated > 0)
                            Console.WriteLine($"[RLinkLogService] ✔ PG MarkSent {table}: {updated} row(s)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ MarkTableRowSentPg {table}: {ex.Message}");
            }
        }

        /// <summary>Đánh dấu is_sent=1 trên PG bằng row id.</summary>
        public static void MarkTableRowSentPgById(string table, long pgRowId)
        {
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(
                        $"UPDATE {table} SET {IsSent}=1, {SentAt}=NOW() WHERE {Id}=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", pgRowId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ MarkTableRowSentPgById {table}#{pgRowId}: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════
        //  GetUnsent từ PostgreSQL (PG-first kiến trúc)
        // ════════════════════════════════════════════════════════════

        public static List<(long id, LogStatusPayload payload)> GetUnsentLogInPg(int limit = 50, string jobName = "", string batch = "")
        {
            var list = new List<(long, LogStatusPayload)>();
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return list;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        SELECT {Id}, {LineId}, {RlinkName}, {JobName}, {Batch},
                               {ProductId}, {ProductName}, {Status}, {RlinkStatus},
                               {OperatorUser}, {Qty}, {QrCode}, {QrDetail},
                               {ManufacturedDate}, {ExpiryDate}, {LastPrintedAt},
                               {PrinterLastProductManufacturedDate}, {Timestamp}
                        FROM {PgTableLogIn}
                        WHERE {IsSent} = 0
                        {(string.IsNullOrWhiteSpace(jobName) ? "" : $"AND {JobName} = @jobName")}
                        {(string.IsNullOrWhiteSpace(batch) ? "" : $"AND {Batch} = @batch")}
                        ORDER BY {Id} ASC LIMIT @lim", conn))
                    {
                        cmd.Parameters.AddWithValue("lim", limit);
                        if (!string.IsNullOrWhiteSpace(jobName))
                            cmd.Parameters.AddWithValue("jobName", jobName);
                        if (!string.IsNullOrWhiteSpace(batch))
                            cmd.Parameters.AddWithValue("batch", batch);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add((r.GetInt64(0), new LogStatusPayload
                                {
                                    line_id = r.IsDBNull(1) ? "" : r.GetString(1),
                                    rlink_name = r.IsDBNull(2) ? "" : r.GetString(2),
                                    job_name = r.IsDBNull(3) ? "" : r.GetString(3),
                                    batch = r.IsDBNull(4) ? "" : r.GetString(4),
                                    product_id = r.IsDBNull(5) ? "" : r.GetString(5),
                                    product_name = r.IsDBNull(6) ? "" : r.GetString(6),
                                    status = r.IsDBNull(7) ? "" : r.GetString(7),
                                    rlink_status = r.IsDBNull(8) ? "" : r.GetString(8),
                                    operator_user = NormalizeOperator(r.IsDBNull(9) ? "" : r.GetString(9)),
                                    produced = r.IsDBNull(10) ? 0 : r.GetInt32(10),
                                    qr_code = r.IsDBNull(11) ? "" : r.GetString(11),
                                    qr_detail = DeserializeQrDetail(r.IsDBNull(12) ? "" : r.GetString(12)),
                                    manufactured_date = r.IsDBNull(13) ? "" : r.GetString(13),
                                    expiry_date = r.IsDBNull(14) ? "" : r.GetString(14),
                                    last_printed_at = r.IsDBNull(15) ? "" : r.GetString(15),
                                    printer_last_product_manufactured_date = r.IsDBNull(16) ? "" : r.GetString(16),
                                    timestamp = r.IsDBNull(17) ? DateTime.Now : r.GetDateTime(17)
                                }));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ProjectLogger.WriteError("[RLinkLogService] GetUnsentLogInPg: " + ex.Message); }
            return list;
        }

        public static List<(long id, LogCameraPayload payload)> GetUnsentLogCameraPg(int limit = 50, string jobName = "", string batch = "")
        {
            var list = new List<(long, LogCameraPayload)>();
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return list;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        SELECT {Id}, {LineId}, {RlinkName}, {JobName}, {Batch},
                               {ProductId}, {ProductName}, {Status}, {RlinkStatus}, {OperatorUser},
                               {StatusGood}, {StatusFail}, {QrCode}, {QrDetail},
                               {FrameInfo}, {CameraManufacturedDate}, {CameraExpiryDate},
                               {CameraLastPacketReceivedAt}, {CameraLastProductManufacturedDate}, {Timestamp}
                        FROM {PgTableLogCamera}
                        WHERE {IsSent} = 0
                        {(string.IsNullOrWhiteSpace(jobName) ? "" : $"AND {JobName} = @jobName")}
                        {(string.IsNullOrWhiteSpace(batch) ? "" : $"AND {Batch} = @batch")}
                        ORDER BY {Id} ASC LIMIT @lim", conn))
                    {
                        cmd.Parameters.AddWithValue("lim", limit);
                        if (!string.IsNullOrWhiteSpace(jobName))
                            cmd.Parameters.AddWithValue("jobName", jobName);
                        if (!string.IsNullOrWhiteSpace(batch))
                            cmd.Parameters.AddWithValue("batch", batch);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add((r.GetInt64(0), new LogCameraPayload
                                {
                                    line_id = r.IsDBNull(1) ? "" : r.GetString(1),
                                    rlink_name = r.IsDBNull(2) ? "" : r.GetString(2),
                                    job_name = r.IsDBNull(3) ? "" : r.GetString(3),
                                    batch = r.IsDBNull(4) ? "" : r.GetString(4),
                                    product_id = r.IsDBNull(5) ? "" : r.GetString(5),
                                    product_name = r.IsDBNull(6) ? "" : r.GetString(6),
                                    status = r.IsDBNull(7) ? "" : r.GetString(7),
                                    rlink_status = r.IsDBNull(8) ? "" : r.GetString(8),
                                    operator_user = NormalizeOperator(r.IsDBNull(9) ? "" : r.GetString(9)),
                                    camera_ok = r.IsDBNull(10) ? 0 : r.GetInt32(10),
                                    camera_fail = r.IsDBNull(11) ? 0 : r.GetInt32(11),
                                    qr_code = r.IsDBNull(12) ? "" : r.GetString(12),
                                    qr_detail = DeserializeQrDetail(r.IsDBNull(13) ? "" : r.GetString(13)),
                                    frame_info = r.IsDBNull(14) ? "" : r.GetString(14),
                                    camera_manufactured_date = r.IsDBNull(15) ? "" : r.GetString(15),
                                    camera_expiry_date = r.IsDBNull(16) ? "" : r.GetString(16),
                                    camera_last_packet_received_at = r.IsDBNull(17) ? "" : r.GetString(17),
                                    camera_last_product_manufactured_date = r.IsDBNull(18) ? "" : r.GetString(18),
                                    timestamp = r.IsDBNull(19) ? DateTime.Now : r.GetDateTime(19)
                                }));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ProjectLogger.WriteError("[RLinkLogService] GetUnsentLogCameraPg: " + ex.Message); }
            return list;
        }

        public static List<(long id, LogCameraErrorPayload payload)> GetUnsentLogCameraErrorPg(int limit = 50, string jobName = "", string batch = "")
        {
            var list = new List<(long, LogCameraErrorPayload)>();
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return list;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        SELECT {Id}, {LineId}, {RlinkName}, {JobName}, {Batch},
                               {ProductId}, {ProductName}, {RlinkStatus}, {OperatorUser},
                               {QrCode}, {ErrorManufacturedDate}, {ErrorExpiryDate}, {ErrorType},
                               {ErrorFrameInfo}, {ImagePath}, {Timestamp}
                        FROM {PgTableLogCameraError}
                        WHERE {IsSent} = 0
                        {(string.IsNullOrWhiteSpace(jobName) ? "" : $"AND {JobName} = @jobName")}
                        {(string.IsNullOrWhiteSpace(batch) ? "" : $"AND {Batch} = @batch")}
                        ORDER BY {Id} ASC LIMIT @lim", conn))
                    {
                        cmd.Parameters.AddWithValue("lim", limit);
                        if (!string.IsNullOrWhiteSpace(jobName))
                            cmd.Parameters.AddWithValue("jobName", jobName);
                        if (!string.IsNullOrWhiteSpace(batch))
                            cmd.Parameters.AddWithValue("batch", batch);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add((r.GetInt64(0), new LogCameraErrorPayload
                                {
                                    line_id = r.IsDBNull(1) ? "" : r.GetString(1),
                                    rlink_name = r.IsDBNull(2) ? "" : r.GetString(2),
                                    job_name = r.IsDBNull(3) ? "" : r.GetString(3),
                                    batch = r.IsDBNull(4) ? "" : r.GetString(4),
                                    product_id = r.IsDBNull(5) ? "" : r.GetString(5),
                                    product_name = r.IsDBNull(6) ? "" : r.GetString(6),
                                    rlink_status = r.IsDBNull(7) ? "" : r.GetString(7),
                                    operator_user = NormalizeOperator(r.IsDBNull(8) ? "" : r.GetString(8)),
                                    qr_code = r.IsDBNull(9) ? "" : r.GetString(9),
                                    error_manufactured_date = r.IsDBNull(10) ? "" : r.GetString(10),
                                    error_expiry_date = r.IsDBNull(11) ? "" : r.GetString(11),
                                    error_type = r.IsDBNull(12) ? "" : r.GetString(12),
                                    error_frame_info = r.IsDBNull(13) ? "" : r.GetString(13),
                                    image_path = r.IsDBNull(14) ? "" : r.GetString(14),
                                    timestamp = r.IsDBNull(15) ? DateTime.Now : r.GetDateTime(15)
                                }));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ProjectLogger.WriteError("[RLinkLogService] GetUnsentLogCameraErrorPg: " + ex.Message); }
            return list;
        }

        public static List<(string qrCode, string batch, string productId, string manufacturedDate, string expiryDate, string jobName, string printedAt)> GetUnsentMarkedQrPg(int limit = 10)
        {
            var list = new List<(string, string, string, string, string, string, string)>();
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return list;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        SELECT DISTINCT ON ({QrCode}) {QrCode}, COALESCE({Batch},''), COALESCE({ProductId},''),
                               COALESCE({ManufacturedDate},''), COALESCE({ExpiryDate},''),
                               COALESCE({JobName},''),
                               COALESCE({PrintedAt}::text,'')
                        FROM ""{Code}""
                        WHERE {IsPrinted} = TRUE AND ({IsSentToMaster} = FALSE OR {IsSentToMaster} IS NULL)
                        LIMIT @lim", conn))
                    {
                        cmd.Parameters.AddWithValue("lim", limit);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                                list.Add((r.IsDBNull(0) ? "" : r.GetString(0),
                                          r.IsDBNull(1) ? "" : r.GetString(1),
                                          r.IsDBNull(2) ? "" : r.GetString(2),
                                          r.IsDBNull(3) ? "" : r.GetString(3),
                                          r.IsDBNull(4) ? "" : r.GetString(4),
                                          r.IsDBNull(5) ? "" : r.GetString(5),
                                          r.IsDBNull(6) ? "" : r.GetString(6)));
                        }
                    }
                }
            }
            catch (Exception ex) { ProjectLogger.WriteError("[RLinkLogService] GetUnsentMarkedQrPg: " + ex.Message); }
            return list;
        }

        // ════════════════════════════════════════════════════════════
        //  PUBLIC API (IRLinkLogService)
        // ════════════════════════════════════════════════════════════

        public Task SaveLogInAsync(
            string status, string rlinkStatus, string operatorUser, int qty,
            string lineId, string rlinkName, string jobName, string batch,
            string productId, string productName, string qrCode = "", List<QrDetailItem> qrDetail = null, int qrUsed = 0,
            string manufacturedDate = "", string expiryDate = "", string lastPrintedAt = "",
            string printerLastProductManufacturedDate = "")
        {
            var p = new LogStatusPayload
            {
                line_id = lineId ?? "",
                rlink_name = rlinkName ?? "",
                job_name = jobName ?? "",
                batch = batch ?? "",
                product_id = productId ?? "",
                product_name = productName ?? "",
                status = status ?? "",
                rlink_status = rlinkStatus ?? "",
                operator_user = operatorUser ?? "",
                produced = qty,
                qr_used = qrUsed,
                qr_code = qrCode ?? "",
                qr_detail = qrDetail ?? new List<QrDetailItem>(),
                manufactured_date = NormalizeDateFormat(manufacturedDate ?? ""),
                expiry_date = NormalizeDateFormat(expiryDate ?? ""),
                last_printed_at = NormalizeDateFormat(lastPrintedAt ?? ""),
                printer_last_product_manufactured_date = NormalizeDateFormat(printerLastProductManufacturedDate ?? ""),
                timestamp = DateTime.Now
            };

            // ── 1. PG trước (primary) ─────────────────────────────────
            long pgRowId = -1;
            if (!string.IsNullOrWhiteSpace(GetPgConnStr()))
            {
                try
                {
                    pgRowId = InsertLogInToPostgres(p);
                    _isPgReady = true;
                    Console.WriteLine($"[RLinkLogService] ✔ PG LogIn status={status} id={pgRowId}");
                }
                catch (Exception ex)
                {
                    _isPgReady = false;
                    Console.WriteLine($"[RLinkLogService] ✘ PG LogIn lỗi: {ex.Message}");
                }
            }

            // ── 2. SQLite backup ──────────────────────────────────────
            long sqliteRowId = -1;
            if (_isDbReady)
            {
                try
                {
                    sqliteRowId = InsertLogInToSQLite(p);
                    Console.WriteLine($"[RLinkLogService] ✔ SQLite LogIn status={status} id={sqliteRowId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RLinkLogService] ✘ SQLite LogIn lỗi: {ex.Message}");
                }
            }

            // ── 3. Fire-and-mark: gửi lên R-Link Master ──────────────
            if (pgRowId > 0)
            {
                long capturedPgId = pgRowId;
                Task.Run(async () =>
                {
                    try
                    {
                        var svc = RLinkMasterServiceFactory.Instance;
                        if (svc != null)
                        {
                            var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(p, Newtonsoft.Json.Formatting.Indented);
                            Console.WriteLine($"[RLinkLogService] >>> POST /api/rlink/log/status body:\n{jsonBody}");
                            bool ok = await svc.SendLogStatusAsync(p);
                            if (ok)
                            {
                                MarkTableRowSentPgById(PgTableLogIn, capturedPgId);
                                if (sqliteRowId > 0) MarkTableRowSent(LogIn, sqliteRowId);
                            }
                        }
                    }
                    catch { }
                });
            }

            return Task.CompletedTask;
        }
        /// <summary>
        /// Thống kê số log chưa gửi (is_sent=0) theo từng job_name + batch.
        /// Dùng để điền job_details trong SyncReportPayload.
        /// </summary>
        public static List<Models.SyncJobDetail> GetUnsentCountsByJob()
        {
           
            var result = new List<Models.SyncJobDetail>();
            try
            {
                // ── LOG PATH để xác nhận đang đọc đúng file ──────────────
                ProjectLogger.WriteInfo($"[GetUnsentCountsByJob] DbPath = {DbPath}");
                ProjectLogger.WriteInfo($"[GetUnsentCountsByJob] File exists = {System.IO.File.Exists(DbPath)}");

                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"
                    SELECT
                        COALESCE({JobName}, '') AS {JobName},
                        COALESCE(MAX({Batch}), '') AS {Batch},
                        SUM(CASE WHEN src='in'  THEN 1 ELSE 0 END) AS cnt_in,
                        SUM(CASE WHEN src='cam' THEN 1 ELSE 0 END) AS cnt_cam,
                        SUM(CASE WHEN src='err' THEN 1 ELSE 0 END) AS cnt_err
                    FROM (
                        SELECT COALESCE({JobName},'') AS {JobName}, COALESCE({Batch},'') AS {Batch}, 'in'  AS src FROM {LogIn}          WHERE {IsSent}=0
                        UNION ALL
                        SELECT COALESCE({JobName},'') AS {JobName}, COALESCE({Batch},'') AS {Batch}, 'cam' AS src FROM {LogCamera}      WHERE {IsSent}=0
                        UNION ALL
                        SELECT COALESCE({JobName},'') AS {JobName}, COALESCE({Batch},'') AS {Batch}, 'err' AS src FROM {LogCameraError} WHERE {IsSent}=0
                    ) t
                    GROUP BY {JobName}
                    ORDER BY {JobName} ASC";

                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var item = new Models.SyncJobDetail
                                {
                                    job_name = r.GetString(0),
                                    batch = r.GetString(1),
                                    pending_in = r.GetInt32(2),
                                    pending_camera = r.GetInt32(3),
                                    pending_error = r.GetInt32(4)
                                };
                                result.Add(item);
                                ProjectLogger.WriteInfo($"[GetUnsentCountsByJob] Row: job={item.job_name} batch={item.batch} in={item.pending_in} cam={item.pending_camera} err={item.pending_error}");
                            }
                        }
                    }
                }

                ProjectLogger.WriteInfo($"[GetUnsentCountsByJob] Tổng: {result.Count} job(s)");
            }
            catch (Exception ex)
            {
                // Lỗi sẽ hiển thị rõ ràng thay vì bị nuốt
                ProjectLogger.WriteError("[GetUnsentCountsByJob] EXCEPTION: " + ex.GetType().Name + " — " + ex.Message, ex);
                System.Windows.Forms.MessageBox.Show(
                    "GetUnsentCountsByJob lỗi:\n" + ex.Message,
                    "Debug", System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
            return result;
        }
        /// <summary>Thống kê số log chưa gửi từ PostgreSQL (PG-first).</summary>
        public static List<Models.SyncJobDetail> GetUnsentCountsByJobPg(string lineId = "")
        {
            var result = new List<Models.SyncJobDetail>();
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return result;

                string lineFilter = string.IsNullOrWhiteSpace(lineId)
                    ? ""
                    : $"AND {LineId} = @lineId";

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand($@"
                        SELECT
                            COALESCE({JobName}, '') AS {JobName},
                            COALESCE(MAX({Batch}), '') AS {Batch},
                            SUM(CASE WHEN src='in'  THEN 1 ELSE 0 END) AS cnt_in,
                            SUM(CASE WHEN src='cam' THEN 1 ELSE 0 END) AS cnt_cam,
                            SUM(CASE WHEN src='err' THEN 1 ELSE 0 END) AS cnt_err
                        FROM (
                            SELECT COALESCE({JobName},'') AS {JobName}, COALESCE({Batch},'') AS {Batch}, 'in'  AS src FROM {PgTableLogIn}          WHERE {IsSent}=0 {lineFilter}
                            UNION ALL
                            SELECT COALESCE({JobName},'') AS {JobName}, COALESCE({Batch},'') AS {Batch}, 'cam' AS src FROM {PgTableLogCamera}      WHERE {IsSent}=0 {lineFilter}
                            UNION ALL
                            SELECT COALESCE({JobName},'') AS {JobName}, COALESCE({Batch},'') AS {Batch}, 'err' AS src FROM {PgTableLogCameraError} WHERE {IsSent}=0 {lineFilter}
                        ) t
                        GROUP BY {JobName}
                        ORDER BY {JobName} ASC", conn))
                    {
                        if (!string.IsNullOrWhiteSpace(lineId))
                            cmd.Parameters.AddWithValue("lineId", lineId);

                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                result.Add(new Models.SyncJobDetail
                                {
                                    job_name = r.GetString(0),
                                    batch = r.GetString(1),
                                    pending_in = r.GetInt32(2),
                                    pending_camera = r.GetInt32(3),
                                    pending_error = r.GetInt32(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[GetUnsentCountsByJobPg] " + ex.Message);
            }
            return result;
        }
        public Task SaveLogCameraAsync(
            string status, string rlinkStatus, string operatorUser, int cameraOk, int cameraFail,
            string lineId, string rlinkName, string jobName, string batch,
            string productId, string productName, string qrCode = "", List<QrDetailItem> qrDetail = null,
            string frameInfo = "", string cameraManufacturedDate = "", string cameraExpiryDate = "",
            string cameraLastPacketReceivedAt = "", string cameraLastProductManufacturedDate = "")
        {
            var p = new LogCameraPayload
            {
                line_id = lineId ?? "",
                rlink_name = rlinkName ?? "",
                job_name = jobName ?? "",
                batch = batch ?? "",
                product_id = productId ?? "",
                product_name = productName ?? "",
                status = status ?? "",
                rlink_status = rlinkStatus ?? "",
                operator_user = operatorUser ?? "",
                camera_ok = cameraOk,
                camera_fail = cameraFail,
                qr_code = qrCode ?? "",
                qr_detail = qrDetail ?? new List<QrDetailItem>(),
                frame_info = frameInfo ?? "",
                camera_manufactured_date = NormalizeDateFormat(cameraManufacturedDate ?? ""),
                camera_expiry_date = NormalizeDateFormat(cameraExpiryDate ?? ""),
                camera_last_packet_received_at = NormalizeDateFormat(cameraLastPacketReceivedAt ?? ""),
                camera_last_product_manufactured_date = NormalizeDateFormat(cameraLastProductManufacturedDate ?? ""),
                timestamp = DateTime.Now
            };

            // ── 1. PG trước ───────────────────────────────────────────
            long pgRowId = -1;
            if (!string.IsNullOrWhiteSpace(GetPgConnStr()))
            {
                try
                {
                    pgRowId = InsertLogCameraToPostgres(p);
                    _isPgReady = true;
                    Console.WriteLine($"[RLinkLogService] ✔ PG LogCamera ok={cameraOk} fail={cameraFail} id={pgRowId}");
                }
                catch (Exception ex)
                {
                    _isPgReady = false;
                    Console.WriteLine($"[RLinkLogService] ✘ PG LogCamera lỗi: {ex.Message}");
                }
            }

            // ── 2. SQLite backup ──────────────────────────────────────
            long sqliteRowId = -1;
            if (_isDbReady)
            {
                try
                {
                    sqliteRowId = InsertLogCameraToSQLite(p);
                    Console.WriteLine($"[RLinkLogService] ✔ SQLite LogCamera ok={cameraOk} fail={cameraFail} id={sqliteRowId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RLinkLogService] ✘ SQLite LogCamera lỗi: {ex.Message}");
                }
            }

            // ── 3. Fire-and-mark ──────────────────────────────────────
            if (pgRowId > 0)
            {
                long capturedPgId = pgRowId;
                Task.Run(async () =>
                {
                    try
                    {
                        var svc = RLinkMasterServiceFactory.Instance;
                        if (svc != null)
                        {
                            var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(p, Newtonsoft.Json.Formatting.Indented);
                            Console.WriteLine($"[RLinkLogService] >>> POST /api/rlink/log/camera body:\n{jsonBody}");
                            bool ok = await svc.SendLogCameraAsync(p);
                            if (ok)
                            {
                                MarkTableRowSentPgById(PgTableLogCamera, capturedPgId);
                                if (sqliteRowId > 0) MarkTableRowSent(LogCamera, sqliteRowId);
                            }
                        }
                    }
                    catch { }
                });
            }

            return Task.CompletedTask;
        }

        public Task SaveLogCameraErrorAsync(
              string rlinkStatus, string operatorUser, string qrCode, string resultType,
              string lineId, string rlinkName, string jobName, string batch,
              string productId, string productName,
              string imagePath = "", string errorNsx = "", string errorHsd = "", string errorFrameInfo = "")
        {
            var p = new LogCameraErrorPayload
            {
                line_id = lineId ?? "",
                rlink_name = rlinkName ?? "",
                job_name = jobName ?? "",
                batch = batch ?? "",
                product_id = productId ?? "",
                product_name = productName ?? "",
                rlink_status = rlinkStatus ?? "",
                operator_user = operatorUser ?? "",
                qr_code = qrCode ?? "",
                error_manufactured_date = NormalizeDateFormat(errorNsx ?? ""),
                error_expiry_date = NormalizeDateFormat(errorHsd ?? ""),
                error_type = resultType ?? "",
                error_frame_info = errorFrameInfo ?? "",
                image_path = imagePath ?? "",
                timestamp = DateTime.Now
            };

            // ── 1. PG trước ───────────────────────────────────────────
            long pgRowId = -1;
            if (!string.IsNullOrWhiteSpace(GetPgConnStr()))
            {
                try
                {
                    pgRowId = InsertLogCameraErrorToPostgres(p);
                    _isPgReady = true;
                    Console.WriteLine($"[RLinkLogService] ✔ PG LogCameraError type={resultType} id={pgRowId}");
                }
                catch (Exception ex)
                {
                    _isPgReady = false;
                    Console.WriteLine($"[RLinkLogService] ✘ PG LogCameraError lỗi: {ex.Message}");
                }
            }

            // ── 2. SQLite backup ──────────────────────────────────────
            long sqliteRowId = -1;
            if (_isDbReady)
            {
                try
                {
                    sqliteRowId = InsertLogCameraErrorToSQLite(p);
                    Console.WriteLine($"[RLinkLogService] ✔ SQLite LogCameraError type={resultType} id={sqliteRowId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RLinkLogService] ✘ SQLite LogCameraError lỗi: {ex.Message}");
                }
            }

            // ── 3. Fire-and-mark ──────────────────────────────────────
            if (pgRowId > 0)
            {
                long capturedPgId = pgRowId;
                Task.Run(async () =>
                {
                    try
                    {
                        var svc = RLinkMasterServiceFactory.Instance;
                        if (svc != null)
                        {
                            var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(p, Newtonsoft.Json.Formatting.Indented);
                            Console.WriteLine($"[RLinkLogService] >>> POST /api/rlink/log/camera/error body:\n{jsonBody}");
                            bool ok = await svc.SendLogCameraErrorAsync(p);
                            if (ok)
                            {
                                MarkTableRowSentPgById(PgTableLogCameraError, capturedPgId);
                                if (sqliteRowId > 0) MarkTableRowSent(LogCameraError, sqliteRowId);
                            }
                        }
                    }
                    catch { }
                });
            }

            return Task.CompletedTask;
        }

        private static string NormalizeDateFormat(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return "";
            // yyyy-MM-dd → dd MM yy
            if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                return dt.ToString("dd MM yy");
            // yyyy-MM-dd HH:mm → dd MM yy HH:mm
            if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("dd MM yy HH:mm");
            // yyyy-MM-dd HH:mm:ss → dd MM yy HH:mm:ss
            if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("dd MM yy HH:mm:ss");
            // dd/MM/yyyy → dd MM yy
            if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("dd MM yy");
            // dd/MM/yyyy HH:mm → dd MM yy HH:mm
            if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("dd MM yy HH:mm");
            // dd/MM/yyyy HH:mm:ss → dd MM yy HH:mm:ss
            if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("dd MM yy HH:mm:ss");
            // dd MM yy or dd MM yy HH:mm or dd MM yy HH:mm:ss → giữ nguyên
            if (DateTime.TryParseExact(dateStr, "dd MM yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dateStr;
            if (DateTime.TryParseExact(dateStr, "dd MM yy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dateStr;
            if (DateTime.TryParseExact(dateStr, "dd MM yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dateStr;
            // Fallback: thay / hoặc - bằng space
            string sep = dateStr.Contains("/") ? "/" : dateStr.Contains("-") ? "-" : null;
            if (sep != null)
            {
                var result = dateStr.Replace(sep, " ");
                var parts = result.Split(' ');
                if (parts.Length >= 3 && parts[2].Length == 4)
                    result = $"{parts[0]} {parts[1]} {parts[2].Substring(2, 2)}";
                return result;
            }
            return dateStr;
        }

        // giữ tương thích với code cũ gọi SaveAsync
        public Task SaveAsync(string status, int qrUsed, int produced,
            string jobName = "", string batch = "", string lineId = "", string rlinkName = "")
        {
            return SaveLogInAsync(status, "", "", produced, lineId, rlinkName, jobName, batch, "", "");
        }

        // ════════════════════════════════════════════════════════════
        //  CORE — Ghi vào bảng `code` (dùng chung cho mọi nguồn QR)
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// Insert QR codes vào bảng <c>code</c> (SQLite offline cache).
        /// Chỉ lưu qr_code + received_at — metadata sẽ được điền khi is_used=1.
        /// </summary>
        private void InsertQrToSQLiteCodeTable(
            List<string> codes, string lineId, string lineNameOrRlink, string batch)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    foreach (var qr in codes)
                    {
                        if (string.IsNullOrWhiteSpace(qr)) continue;
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.CommandText = $@"
                                INSERT OR IGNORE INTO {Code} ({QrCode}, {ReceivedAt})
                                VALUES (@qr_code, @received_at)";
                            cmd.Parameters.AddWithValue("@qr_code", qr);
                            cmd.Parameters.AddWithValue("@received_at", now);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
            }
            ProjectLogger.WriteInfo($"[RLinkLogService] ✔ SQLite code: {codes.Count} mã inserted");
        }
        // ════════════════════════════════════════════════════════════
        //  INSERT — Allocated QR Codes
        // ════════════════════════════════════════════════════════════

        private void InsertAllocatedQrToSQLite(List<string> codes, string jobName, string lineId, string batch, string rlinkName)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    foreach (var qr in codes)
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.CommandText = $@"
                                INSERT INTO {AllocatedQr}
                                    ({LineId}, {RlinkName}, {JobName}, {Batch}, {QrCode}, {AllocatedAt})
                                VALUES
                                    (@line_id, @rlink_name, @job_name, @batch, @qr_code, @allocated_at)";
                            cmd.Parameters.AddWithValue("@line_id", lineId ?? "");
                            cmd.Parameters.AddWithValue("@rlink_name", rlinkName ?? "");
                            cmd.Parameters.AddWithValue("@job_name", jobName ?? "");
                            cmd.Parameters.AddWithValue("@batch", batch ?? "");
                            cmd.Parameters.AddWithValue("@qr_code", qr ?? "");
                            cmd.Parameters.AddWithValue("@allocated_at", now);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
            }
            Console.WriteLine($"[RLinkLogService] ✔ SQLite AllocatedQr: {codes.Count} mã → job={jobName}");
        }

        private void InsertAllocatedQrToPostgres(List<string> codes, string jobName, string lineId, string batch, string rlinkName)
        {
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return;

                DateTime now = DateTime.Now;
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        foreach (var qr in codes)
                        {
                            using (var cmd = new Npgsql.NpgsqlCommand($@"
                                INSERT INTO {PgTableAllocatedQr}
                                    ({LineId}, {RlinkName}, {JobName}, {Batch}, {QrCode}, {AllocatedAt})
                                VALUES
                                    (@line_id, @rlink_name, @job_name, @batch, @qr_code, @allocated_at)",
                                conn, tx))
                            {
                                cmd.Parameters.AddWithValue("line_id", lineId ?? "");
                                cmd.Parameters.AddWithValue("rlink_name", rlinkName ?? "");
                                cmd.Parameters.AddWithValue("job_name", jobName ?? "");
                                cmd.Parameters.AddWithValue("batch", batch ?? "");
                                cmd.Parameters.AddWithValue("qr_code", qr ?? "");
                                cmd.Parameters.AddWithValue("allocated_at", now);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
                Console.WriteLine($"[RLinkLogService] ✔ PG AllocatedQr: {codes.Count} mã → job={jobName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ InsertAllocatedQrToPostgres lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu QR codes được cấp phát vào:
        ///   1. SQLite rlink_allocated_qr (lịch sử phân phối)
        ///   2. SQLite code              (offline cache — dùng chung)
        ///   3. PG rlink_allocated_qr             (fire-and-forget)
        ///   4. PG THLocalDbTable       (fire-and-forget)
        /// </summary>
        public Task SaveAllocatedQrCodesAsync(List<string> codes, string jobName, string lineId, string batch)
        {
            if (!_isDbReady || codes == null || codes.Count == 0)
            {
                Console.WriteLine("[RLinkLogService] SaveAllocatedQrCodesAsync: bỏ qua (DB chưa sẵn sàng hoặc danh sách rỗng)");
                return Task.CompletedTask;
            }

            string rlinkName = Shared.Settings?.RLinkName ?? "";

            try
            {
                InsertAllocatedQrToSQLite(codes, jobName, lineId, batch, rlinkName);
                // Ghi vào bảng code (offline cache chung)
                InsertQrToSQLiteCodeTable(codes, lineId, rlinkName, batch);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ SQLite AllocatedQr lỗi: {ex.Message}");
            }

            if (_isPgReady)
            {
                _ = Task.Run(() => InsertAllocatedQrToPostgres(codes, jobName, lineId, batch, rlinkName));
                _ = Task.Run(() => InsertAllocatedQrToQrBankTable(codes, lineId, rlinkName, batch));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Lưu QR codes nhận được khi complete job vào bảng <c>code</c> (SQLite + PG QrBank).
        /// Gọi từ flow CompleteJob khi R-Link Master trả về danh sách mã đã xác nhận.
        /// </summary>
        public Task SaveCompletedQrCodesAsync(List<string> codes, string lineId, string batch)
        {
            if (!_isDbReady || codes == null || codes.Count == 0)
            {
                Console.WriteLine("[RLinkLogService] SaveCompletedQrCodesAsync: bỏ qua (DB chưa sẵn sàng hoặc danh sách rỗng)");
                return Task.CompletedTask;
            }

            string rlinkName = Shared.Settings?.RLinkName ?? "";

            try
            {
                // Ghi vào bảng code — cùng bảng với allocated QR
                InsertQrToSQLiteCodeTable(codes, lineId, rlinkName, batch);
                Console.WriteLine($"[RLinkLogService] ✔ SQLite code (complete job): {codes.Count} mã");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ SQLite code (complete job) lỗi: {ex.Message}");
            }

            if (_isPgReady)
                _ = Task.Run(() => InsertAllocatedQrToQrBankTable(codes, lineId, rlinkName, batch));

            return Task.CompletedTask;
        }

        // ════════════════════════════════════════════════════════════
        //  INSERT — AllocatedQR vào bảng QrBank (THLocalDbTable)
        // ════════════════════════════════════════════════════════════

        private void InsertAllocatedQrToQrBankTable(
            List<string> codes, string lineId, string lineNameOrRlink, string batch)
        {
            try
            {
                string connStr = GetPgConnStr();
                if (string.IsNullOrWhiteSpace(connStr)) return;

                string table = Code;

                DateTime now = DateTime.Now;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();

                    // ── Tạo bảng nếu chưa có ─────────────────────────────────
                    // Các cột metadata (line_id, factory_code, batch, job_name, used_at)
                    // để trống khi insert — chỉ điền khi is_used = TRUE
                    string createSql = $@"
CREATE TABLE IF NOT EXISTS ""{table}"" (
    {Id}              BIGSERIAL    PRIMARY KEY,
    {QrCode}        TEXT         NOT NULL,
    {FactoryCode}   VARCHAR(20)  DEFAULT '',
    {LineId}        VARCHAR(50)  DEFAULT '',
    {LineName}      VARCHAR(100) DEFAULT '',
    {Batch}         VARCHAR(100) DEFAULT '',
    {JobName}       VARCHAR(200) DEFAULT '',
    {ReceivedAt}    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    {UsedAt}        TIMESTAMPTZ,
    {IsUsed}        BOOLEAN      NOT NULL DEFAULT FALSE,
    {IsUsedForJob}  BOOLEAN      DEFAULT FALSE,
    {IsPrinted}     BOOLEAN      DEFAULT FALSE,
    {PrintedAt}     TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS idx_{table}_received_at ON ""{table}"" ({ReceivedAt} DESC);
CREATE INDEX IF NOT EXISTS idx_{table}_is_used     ON ""{table}"" ({IsUsed});
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE LOWER(conname) = LOWER('uq_{table}_{QrCode}')
    ) THEN
        ALTER TABLE ""{table}"" ADD CONSTRAINT uq_{table}_{QrCode} UNIQUE ({QrCode});
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{table}' AND column_name='{ReceivedAt}') THEN
        ALTER TABLE ""{table}"" ADD COLUMN {ReceivedAt} TIMESTAMPTZ NOT NULL DEFAULT NOW();
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{table}' AND column_name='{IsUsed}') THEN
        ALTER TABLE ""{table}"" ADD COLUMN {IsUsed} BOOLEAN NOT NULL DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{table}' AND column_name='{JobName}') THEN
        ALTER TABLE ""{table}"" ADD COLUMN {JobName} VARCHAR(200) DEFAULT '';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{table}' AND column_name='{UsedAt}') THEN
        ALTER TABLE ""{table}"" ADD COLUMN {UsedAt} TIMESTAMPTZ;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{table}' AND column_name='{IsUsedForJob}') THEN
        ALTER TABLE ""{table}"" ADD COLUMN {IsUsedForJob} BOOLEAN DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{table}' AND column_name='{IsPrinted}') THEN
        ALTER TABLE ""{table}"" ADD COLUMN {IsPrinted} BOOLEAN DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{table}' AND column_name='{PrintedAt}') THEN
        ALTER TABLE ""{table}"" ADD COLUMN {PrintedAt} TIMESTAMPTZ;
    END IF;
END$$;";
                    using (var cmdCreate = new Npgsql.NpgsqlCommand(createSql, conn))
                        cmdCreate.ExecuteNonQuery();

                    // ── Chỉ insert qr_code + received_at ─────────────────────
                    string insertSql = $@"
INSERT INTO ""{table}"" ({QrCode}, {ReceivedAt})
VALUES (@qr_code, @received_at)
ON CONFLICT ({QrCode}) DO NOTHING";

                    using (var tx = conn.BeginTransaction())
                    {
                        foreach (var qr in codes)
                        {
                            if (string.IsNullOrWhiteSpace(qr)) continue;
                            using (var cmd = new Npgsql.NpgsqlCommand(insertSql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("qr_code", qr);
                                cmd.Parameters.AddWithValue("received_at", now);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
                ProjectLogger.WriteInfo(
                    $"[RLinkLogService] ✔ QrBankTable '{table}': {codes.Count} mã inserted (qr_code only)");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] ✘ InsertAllocatedQrToQrBankTable: " + ex.Message, ex);
            }
        }

        // ════════════════════════════════════════════════════════════
        //  PUBLIC API — Settings / Product / ConfigLine (SQLite)
        // ════════════════════════════════════════════════════════════

        public void SaveSetting(string key, string value)
        {
            if (!_isDbReady || string.IsNullOrWhiteSpace(key)) return;
            try
            {
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"
                            INSERT INTO {TableSettings} (key, value, {UpdatedAt})
                            VALUES (@key, @value, @updated_at)
                            ON CONFLICT(key) DO UPDATE SET value=excluded.value, {UpdatedAt}=excluded.{UpdatedAt}";
                        cmd.Parameters.AddWithValue("@key", key);
                        cmd.Parameters.AddWithValue("@value", value ?? "");
                        cmd.Parameters.AddWithValue("@updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ SaveSetting lỗi: {ex.Message}");
            }
        }

        public void SaveProduct(string productId, string productName, string batch, string lineId)
        {
            if (!_isDbReady) return;
            try
            {
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"
                            INSERT INTO {Products} ({ProductId}, {ProductName}, {Batch}, {LineId}, {CreatedAt})
                            VALUES (@product_id, @product_name, @batch, @line_id, @created_at)";
                        cmd.Parameters.AddWithValue("@product_id", productId ?? "");
                        cmd.Parameters.AddWithValue("@product_name", productName ?? "");
                        cmd.Parameters.AddWithValue("@batch", batch ?? "");
                        cmd.Parameters.AddWithValue("@line_id", lineId ?? "");
                        cmd.Parameters.AddWithValue("@created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ SaveProduct lỗi: {ex.Message}");
            }
        }

        public static void SaveConfigLine(
            string lineId, string lineName, string factoryCode, string factoryName,
            string machineIp, int operatingMode, int bufferCount)
        {
            try
            {
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    // ── Đảm bảo bảng tồn tại (static method, không phụ thuộc instance) ──
                    using (var cmdCreate = conn.CreateCommand())
                    {
                        // Kiểm tra schema cũ — nếu có cột rlink_name (cũ) thì xóa build lại
                        bool needRebuild = false;
                        try
                        {
                            using (var check = conn.CreateCommand())
                            {
                                check.CommandText = $"SELECT {RlinkName} FROM {ConfigLine} LIMIT 0";
                                check.ExecuteNonQuery();
                                needRebuild = true; // Cột rlink_name tồn tại → schema cũ
                            }
                        }
                        catch { /* Cột không tồn tại → schema mới hoặc bảng chưa có */ }

                        if (needRebuild)
                        {
                            using (var drop = conn.CreateCommand())
                            {
                                drop.CommandText = $"DROP TABLE IF EXISTS {ConfigLine}";
                                drop.ExecuteNonQuery();
                            }
                        }

                        cmdCreate.CommandText = $@"
                            CREATE TABLE IF NOT EXISTS {ConfigLine} (
                                {Id}              INTEGER  PRIMARY KEY DEFAULT 1,
                                {LineId}        TEXT,
                                {LineName}      TEXT,
                                {FactoryCode}   TEXT,
                                {FactoryName}   TEXT,
                                {MachineIp}     TEXT,
                                {OperatingMode} INTEGER  DEFAULT 0,
                                {BufferCount}   INTEGER  DEFAULT 0,
                                {AssignedAt}    TEXT     NOT NULL
                            )";
                        cmdCreate.ExecuteNonQuery();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"
                            INSERT OR REPLACE INTO {ConfigLine} ({Id}, {LineId}, {LineName}, {FactoryCode}, {FactoryName}, {MachineIp}, {OperatingMode}, {BufferCount}, {AssignedAt})
                            VALUES (1, @line_id, @line_name, @factory_code, @factory_name, @machine_ip, @operating_mode, @buffer_count, @assigned_at)";
                        cmd.Parameters.AddWithValue("@line_id", lineId ?? "");
                        cmd.Parameters.AddWithValue("@line_name", lineName ?? "");
                        cmd.Parameters.AddWithValue("@factory_code", factoryCode ?? "");
                        cmd.Parameters.AddWithValue("@factory_name", factoryName ?? "");
                        cmd.Parameters.AddWithValue("@machine_ip", machineIp ?? "");
                        cmd.Parameters.AddWithValue("@operating_mode", operatingMode);
                        cmd.Parameters.AddWithValue("@buffer_count", bufferCount);
                        cmd.Parameters.AddWithValue("@assigned_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine($"[RLinkLogService] ✔ SaveConfigLine line={lineId} ip={machineIp}");

                // ── Mirror vào PG ─────────────────────────────────
                try
                {
                    string pgConn = GetPgConnStr();
                    if (!string.IsNullOrWhiteSpace(pgConn))
                    {
                        using (var pgConn2 = new Npgsql.NpgsqlConnection(pgConn))
                        {
                            pgConn2.Open();
                            using (var pgCreate = new Npgsql.NpgsqlCommand($@"
                                CREATE TABLE IF NOT EXISTS {ConfigLine} (
                                    {Id}              INTEGER  PRIMARY KEY DEFAULT 1,
                                    {LineId}          TEXT,
                                    {LineName}        TEXT,
                                    {FactoryCode}     TEXT,
                                    {FactoryName}     TEXT,
                                    {MachineIp}       TEXT,
                                    {OperatingMode}   INTEGER  DEFAULT 0,
                                    {BufferCount}     INTEGER  DEFAULT 0,
                                    {AssignedAt}      TIMESTAMP NOT NULL DEFAULT NOW()
                                )", pgConn2))
                            {
                                pgCreate.ExecuteNonQuery();
                            }
                            using (var pgCmd = new Npgsql.NpgsqlCommand($@"
                                INSERT INTO {ConfigLine} ({Id}, {LineId}, {LineName}, {FactoryCode}, {FactoryName}, {MachineIp}, {OperatingMode}, {BufferCount}, {AssignedAt})
                                VALUES (1, @lid, @ln, @fc, @fn, @ip, @om, @bc, NOW())
                                ON CONFLICT ({Id}) DO UPDATE
                                    SET {LineId} = @lid, {LineName} = @ln, {FactoryCode} = @fc, {FactoryName} = @fn,
                                        {MachineIp} = @ip, {OperatingMode} = @om, {BufferCount} = @bc, {AssignedAt} = NOW()",
                                pgConn2))
                            {
                                pgCmd.Parameters.AddWithValue("lid", lineId ?? "");
                                pgCmd.Parameters.AddWithValue("ln", lineName ?? "");
                                pgCmd.Parameters.AddWithValue("fc", factoryCode ?? "");
                                pgCmd.Parameters.AddWithValue("fn", factoryName ?? "");
                                pgCmd.Parameters.AddWithValue("ip", machineIp ?? "");
                                pgCmd.Parameters.AddWithValue("om", operatingMode);
                                pgCmd.Parameters.AddWithValue("bc", bufferCount);
                                pgCmd.ExecuteNonQuery();
                            }
                        }
                        Console.WriteLine($"[RLinkLogService] ✔ SaveConfigLine PG: line={lineId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RLinkLogService] ✘ SaveConfigLine PG: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ SaveConfigLine lỗi: {ex.Message}");
            }
        }
        // Thêm vào cuối region PUBLIC STATIC, sau CountAvailableQrInSQLite

        private const string PermissionsTable = THDb.TablePermissions;

        /// <summary>Cache permissions của tài khoản vào SQLite sau khi login R-Link thành công.</summary>
        public static void SavePermissionsToSQLite(string username, Dictionary<string, bool> permissions)
        {
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(
                    permissions ?? new Dictionary<string, bool>());
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"
                    CREATE TABLE IF NOT EXISTS {PermissionsTable} (
                        {Username}         TEXT PRIMARY KEY,
                        {PermissionsJson}  TEXT,
                        {UpdatedAt}        TEXT
                    );
                    INSERT INTO {PermissionsTable} ({Username}, {PermissionsJson}, {UpdatedAt})
                    VALUES (@u, @pj, datetime('now','localtime'))
                    ON CONFLICT({Username}) DO UPDATE
                        SET {PermissionsJson} = EXCLUDED.{PermissionsJson},
                            {UpdatedAt}       = EXCLUDED.{UpdatedAt}";
                        cmd.Parameters.AddWithValue("@u", username ?? "");
                        cmd.Parameters.AddWithValue("@pj", json);
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine($"[RLinkLogService] SavePermissions: {username}");

                // ── Mirror vào PG ─────────────────────────────────
                try
                {
                    string pgConn = GetPgConnStr();
                    if (!string.IsNullOrWhiteSpace(pgConn))
                    {
                        using (var pgConn2 = new Npgsql.NpgsqlConnection(pgConn))
                        {
                            pgConn2.Open();
                            using (var pgCreate = new Npgsql.NpgsqlCommand($@"
                                CREATE TABLE IF NOT EXISTS {TablePermissions} (
                                    {Username}         TEXT PRIMARY KEY,
                                    {PermissionsJson}  TEXT,
                                    {UpdatedAt}        TIMESTAMPTZ DEFAULT NOW()
                                )", pgConn2))
                            {
                                pgCreate.ExecuteNonQuery();
                            }
                            using (var pgCmd = new Npgsql.NpgsqlCommand($@"
                                INSERT INTO {TablePermissions} ({Username}, {PermissionsJson}, {UpdatedAt})
                                VALUES (@u, @pj, NOW())
                                ON CONFLICT ({Username}) DO UPDATE
                                    SET {PermissionsJson} = EXCLUDED.{PermissionsJson},
                                        {UpdatedAt}       = NOW()",
                                pgConn2))
                            {
                                pgCmd.Parameters.AddWithValue("u", username ?? "");
                                pgCmd.Parameters.AddWithValue("pj", json);
                                pgCmd.ExecuteNonQuery();
                                Console.WriteLine($"[RLinkLogService] SavePermissions PG OK: {username}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("[RLinkLogService] SavePermissions PG: không có PG connStr — bỏ qua.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RLinkLogService] SavePermissions PG lỗi: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] SavePermissionsToSQLite lỗi: {ex.Message}");
            }
        }

        /// <summary>Load permissions đã cache từ SQLite — dùng khi offline.</summary>
        public static Dictionary<string, bool> LoadPermissionsFromSQLite(string username)
        {
            try
            {
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmdCreate = conn.CreateCommand())
                    {
                        cmdCreate.CommandText = $@"
                    CREATE TABLE IF NOT EXISTS {PermissionsTable} (
                        {Username}         TEXT PRIMARY KEY,
                        {PermissionsJson}  TEXT,
                        {UpdatedAt}        TEXT
                    )";
                        cmdCreate.ExecuteNonQuery();
                    }
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText =
                            $"SELECT {PermissionsJson} FROM {PermissionsTable} WHERE {Username} = @u LIMIT 1";
                        cmd.Parameters.AddWithValue("@u", username ?? "");
                        var result = cmd.ExecuteScalar() as string;
                        if (!string.IsNullOrWhiteSpace(result))
                            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, bool>>(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] LoadPermissionsFromSQLite lỗi: {ex.Message}");
            }
            return null;
        }

        // ════════════════════════════════════════════════════════════
        //  INSERT — Completed Job Log
        // ════════════════════════════════════════════════════════════

        public void InsertCompletedJobLog(
            string jobName, string userId, string lineId, string productId,
            int qrUsed, int totalPrint, int statusGood, int statusFailed, int totalCheck)
        {
            DateTime now = DateTime.Now;

            // ── PG (sync) ──────────────────────────────────────────
            try
            {
                string connStr = GetPgConnStr();
                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand($@"
                            INSERT INTO {CompletedJobLogs}
                                ({JobName}, {UserRlink}, {LineId}, {ColCreateDate}, {ProductId},
                                 {QrUsed}, {ColTotalPrint}, {StatusGood}, {StatusFailed}, {TotalCheck})
                            VALUES
                                (@jn, @ur, @lid, @cd, @pid,
                                 @qu, @tp, @sg, @sf, @tc)",
                            conn))
                        {
                            cmd.Parameters.AddWithValue("jn", (object)jobName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("ur", (object)userId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("lid", (object)lineId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("cd", now);
                            cmd.Parameters.AddWithValue("pid", (object)productId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("qu", qrUsed);
                            cmd.Parameters.AddWithValue("tp", totalPrint);
                            cmd.Parameters.AddWithValue("sg", statusGood);
                            cmd.Parameters.AddWithValue("sf", statusFailed);
                            cmd.Parameters.AddWithValue("tc", totalCheck);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    ProjectLogger.WriteInfo($"[RLinkLogService] ✔ PG CompletedJobLog: job='{jobName}'");
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkLogService] PG InsertCompletedJobLog: " + ex.Message, ex);
            }

            // ── SQLite (fire-and-forget) ────────────────────────────
            try
            {
                string nowStr = now.ToString("yyyy-MM-dd HH:mm:ss");
                using (var conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"
                            INSERT INTO {CompletedJobLogs}
                                ({JobName}, {UserRlink}, {LineId}, {ColCreateDate}, {ProductId},
                                 {QrUsed}, {ColTotalPrint}, {StatusGood}, {StatusFailed}, {TotalCheck})
                            VALUES
                                (@jn, @ur, @lid, @cd, @pid,
                                 @qu, @tp, @sg, @sf, @tc)";
                        cmd.Parameters.AddWithValue("@jn", jobName ?? "");
                        cmd.Parameters.AddWithValue("@ur", userId ?? "");
                        cmd.Parameters.AddWithValue("@lid", lineId ?? "");
                        cmd.Parameters.AddWithValue("@cd", nowStr);
                        cmd.Parameters.AddWithValue("@pid", productId ?? "");
                        cmd.Parameters.AddWithValue("@qu", qrUsed);
                        cmd.Parameters.AddWithValue("@tp", totalPrint);
                        cmd.Parameters.AddWithValue("@sg", statusGood);
                        cmd.Parameters.AddWithValue("@sf", statusFailed);
                        cmd.Parameters.AddWithValue("@tc", totalCheck);
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine($"[RLinkLogService] ✔ SQLite CompletedJobLog: job='{jobName}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RLinkLogService] ✘ SQLite InsertCompletedJobLog: {ex.Message}");
            }
        }
    }
}