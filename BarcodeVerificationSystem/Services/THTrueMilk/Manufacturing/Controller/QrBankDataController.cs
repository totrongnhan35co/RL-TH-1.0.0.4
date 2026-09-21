//using BarcodeVerificationSystem.Controller;
//using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ReceiveModel;
//using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ResponseModel;
//using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster;
//using BarcodeVerificationSystem.Utils.Logging;
//using BarcodeVerificationSystem.View;
//using Newtonsoft.Json;
//using Npgsql;
//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Threading.Tasks;

//namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Controller
//{
//    public class QrBankDataController : BaseController
//    {
//        // Event dùng type từ namespace cha (Manufacturing) để tránh trùng
//        public event EventHandler<QrBankDataReceivedEventArgs> DataReceived;

//        /// <summary>POST /api/qrbank/data</summary>
//        public void HandleData(HttpListenerRequest request, HttpListenerResponse response, string body)
//        {
//            try
//            {
//                if (!ValidateMethod(request, response, "POST")) return;

//                string authHeader = request.Headers["Authorization"];
//                if (string.IsNullOrWhiteSpace(authHeader))
//                { Unauthorized(response, "Missing Authorization header"); return; }

//                string token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
//                    ? authHeader.Substring(7).Trim()
//                    : authHeader;

//                if (!QrBankLoginController.ValidateToken(token, out string username, out string tokenError))
//                { Unauthorized(response, tokenError ?? "Invalid or expired token"); return; }

//                if (string.IsNullOrWhiteSpace(body))
//                { BadRequest(response, "Request body is required"); return; }

//                QrBankReceiveData data;
//                try { data = JsonConvert.DeserializeObject<QrBankReceiveData>(body); }
//                catch { BadRequest(response, "Invalid JSON format"); return; }

//                // ── Xây danh sách QR cần lưu (hỗ trợ cả batch qr_codes + single qr_code) ──
//                List<string> qrList = new List<string>();
//                if (data.qr_codes != null && data.qr_codes.Count > 0)
//                    qrList.AddRange(data.qr_codes);
//                else if (!string.IsNullOrWhiteSpace(data.qr_code))
//                    qrList.Add(data.qr_code);

//                if (qrList.Count == 0)
//                { UnprocessableEntity(response, "Field 'qr_codes' (array) or 'qr_code' (string) is required"); return; }

//                DateTime savedAt = DateTime.Now;
//                int inserted = 0, duplicate = 0;
//                string firstQr = null, lastQr = null;
//                string firstError = null;

//                foreach (string qr in qrList)
//                {
//                    if (string.IsNullOrWhiteSpace(qr)) continue;
//                    try
//                    {
//                        string batch = data.batch;
//                        string lineId = data.line_id;

//                        if (SaveQrToPostgres(qr, batch, lineId, data.line_name, data.factory_code, savedAt))
//                        {
//                            inserted++;
//                            if (firstQr == null) firstQr = qr;
//                            lastQr = qr;
//                        }
//                        else
//                        {
//                            duplicate++;
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        DbLogger.Error($"Error saving QR '{qr}': {ex.Message}", ex);
//                        if (firstError == null) firstError = ex.Message;
//                        duplicate++;
//                    }
//                }

//                // ── Lưu lịch sử nhận QR ──
//                if (inserted > 0)
//                {
//                    DbLogger.Info($"Batch saved: {inserted} QR, dup={duplicate}, batch={data.batch}, line={data.line_id}");

//                    RLinkLogService.SaveReceiveHistoryToPostgres(savedAt, inserted, data.batch, data.line_id,
//                        data.line_name, data.factory_code, firstQr, lastQr);

//                    Task.Run(() => RLinkLogService.SaveReceiveHistoryToSQLite(
//                        savedAt, inserted, data.batch ?? "", data.line_id ?? "",
//                        data.line_name ?? "", data.factory_code ?? "", firstQr ?? "", lastQr ?? ""));

//                    DataReceived?.Invoke(this, new QrBankDataReceivedEventArgs
//                    {
//                        QrCode = firstQr,
//                        FactoryCode = data.factory_code,
//                        LineId = data.line_id,
//                        LineName = data.line_name,
//                        Batch = data.batch,
//                        AuthenticatedUser = username,
//                        ReceivedAt = savedAt,
//                        TotalCount = inserted,
//                        FirstQr = firstQr,
//                        LastQr = lastQr
//                    });
//                    DbLogger.Info($"[QrBankDataController] ✓ Batch saved: {inserted} QR | dup: {duplicate}");
//                }
//                else if (firstError != null)
//                {
//                    DbLogger.Warning($"All inserts failed: inserted=0, total={qrList.Count}, firstError={firstError}");
//                }

//                Ok(response, new QrBankResponseData
//                {
//                    is_success = true,
//                    duplicate = duplicate > 0,
//                    total_inserted = inserted,
//                    total_duplicate = duplicate,
//                    message = inserted > 0
//                        ? $"Lưu thành công {inserted} mã" + (duplicate > 0 ? $", bỏ qua {duplicate} mã trùng" : "")
//                        : (firstError != null
//                            ? $"Lỗi DB: {firstError}. Kiểm tra bảng code đã được tạo và user có quyền INSERT."
//                            : "Không có mã nào được lưu."),
//                    saved_at = inserted > 0 ? savedAt.ToString("yyyy-MM-dd HH:mm:ss") : null
//                });
//            }
//            catch (Exception ex)
//            {
//                DbLogger.Error($"[QrBankDataController] ✗ {ex.Message}");
//                InternalServerError(response, "Data processing failed", ex);
//            }
//        }

//        /// <summary>GET /api/qrbank/ping</summary>
//        public void HandlePing(HttpListenerRequest request, HttpListenerResponse response)
//        {
//            Ok(response, new
//            {
//                status = "ok",
//                database = IsDbAlive() ? "connected" : "disconnected",
//                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
//            });
//        }

//        private bool SaveQrToPostgres(string qrCode, string batch, string lineId,
//            string lineName, string factoryCode, DateTime savedAt)
//        {
//            string table = Shared.Settings.THLocalDbTable;
//            if (string.IsNullOrWhiteSpace(table)) table = "code";

//            string connStr = frmDatabase.GetConnectionString(
//                "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
//                Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
//                Shared.Settings.THLocalDbDatabase);

//            using (var conn = new NpgsqlConnection(connStr))
//            {
//                conn.Open();

//                // Kiểm tra bảng tồn tại, nếu không → tạo
//                EnsureCodeTable(conn, table);

//                // Xác nhận bảng đã được tạo thành công
//                bool tableExists = false;
//                using (var checkCmd = new NpgsqlCommand(
//                    $"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = @t)", conn))
//                {
//                    checkCmd.Parameters.AddWithValue("t", table);
//                    tableExists = (bool)checkCmd.ExecuteScalar();
//                }

//                if (!tableExists)
//                {
//                    string msg = $"Bảng \"{table}\" không tồn tại và không thể tạo. Kiểm tra quyền CREATE TABLE của user PostgreSQL.";
//                    DbLogger.Error(msg);
//                    throw new InvalidOperationException(msg);
//                }

//                string insertSql = $@"
//INSERT INTO ""{table}"" (qr_code, batch, line_id, line_name, factory_code, received_at)
//VALUES (@qr_code, @batch, @line_id, @line_name, @factory_code, @received_at)
//ON CONFLICT (qr_code) DO NOTHING";

//                using (var cmd = new NpgsqlCommand(insertSql, conn))
//                {
//                    cmd.Parameters.AddWithValue("qr_code", qrCode ?? "");
//                    cmd.Parameters.AddWithValue("batch", (object)batch ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("line_id", (object)lineId ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("line_name", (object)lineName ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("factory_code", (object)factoryCode ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("received_at", savedAt);

//                    int rows;
//                    try
//                    {
//                        rows = cmd.ExecuteNonQuery();
//                    }
//                    catch (PostgresException ex) when (ex.SqlState == "42P10")
//                    {
//                        DbLogger.Error($"ON CONFLICT fail — bảng \"{table}\" thiếu UNIQUE constraint trên qr_code: {ex.Message}");
//                        throw new InvalidOperationException(
//                            $"Bảng \"{table}\" thiếu ràng buộc UNIQUE trên cột qr_code. " +
//                            $"Chạy: ALTER TABLE \"{table}\" ADD CONSTRAINT uq_{table}_qr_code UNIQUE (qr_code);", ex);
//                    }

//                    if (rows == 0) return false;

//                    Task.Run(() => RLinkLogService.InsertQrCodeToSQLiteStatic(qrCode, savedAt));
//                    return true;
//                }
//            }
//        }

//        private static void EnsureCodeTable(NpgsqlConnection conn, string table)
//        {
//            ExecuteNonQuery(conn, $@"
//CREATE TABLE IF NOT EXISTS ""{table}"" (
//    id           BIGSERIAL    PRIMARY KEY,
//    qr_code      TEXT         NOT NULL,
//    factory_code VARCHAR(20)  DEFAULT '',
//    line_id      VARCHAR(50)  DEFAULT '',
//    line_name    VARCHAR(100) DEFAULT '',
//    batch        VARCHAR(100) DEFAULT '',
//    received_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
//    is_used      BOOLEAN      NOT NULL DEFAULT FALSE
//)");
//            ExecuteNonQuery(conn, $@"CREATE INDEX IF NOT EXISTS idx_{table}_received_at ON ""{table}"" (received_at DESC)");
//            ExecuteNonQuery(conn, $@"CREATE INDEX IF NOT EXISTS idx_{table}_is_used ON ""{table}"" (is_used) WHERE is_used = FALSE");
//            ExecuteNonQuery(conn, $@"ALTER TABLE ""{table}"" ADD COLUMN IF NOT EXISTS received_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW()");
//            ExecuteNonQuery(conn, $@"ALTER TABLE ""{table}"" ADD COLUMN IF NOT EXISTS is_used      BOOLEAN      NOT NULL DEFAULT FALSE");
//            ExecuteNonQuery(conn, $@"ALTER TABLE ""{table}"" ADD COLUMN IF NOT EXISTS factory_code VARCHAR(20)  DEFAULT ''");
//            ExecuteNonQuery(conn, $@"ALTER TABLE ""{table}"" ADD COLUMN IF NOT EXISTS line_id      VARCHAR(50)  DEFAULT ''");
//            ExecuteNonQuery(conn, $@"ALTER TABLE ""{table}"" ADD COLUMN IF NOT EXISTS line_name    VARCHAR(100) DEFAULT ''");
//            ExecuteNonQuery(conn, $@"ALTER TABLE ""{table}"" ADD COLUMN IF NOT EXISTS batch        VARCHAR(100) DEFAULT ''");
//            ExecuteNonQuery(conn, $@"ALTER TABLE ""{table}"" ADD COLUMN IF NOT EXISTS job_name     VARCHAR(200) DEFAULT ''");
//            ExecuteNonQuery(conn, $@"ALTER TABLE ""{table}"" ADD COLUMN IF NOT EXISTS used_at      TIMESTAMPTZ");
//            ExecuteNonQuery(conn, $@"
//DO $$
//BEGIN
//    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_{table}_qr_code') THEN
//        DELETE FROM ""{table}"" a USING ""{table}"" b WHERE a.id > b.id AND a.qr_code = b.qr_code;
//        ALTER TABLE ""{table}"" ADD CONSTRAINT uq_{table}_qr_code UNIQUE (qr_code);
//    END IF;
//END$$");
//        }

//        // ── Helper chạy lệnh DDL riêng lẻ, bỏ qua lỗi không nghiêm trọng ──
//        private static void ExecuteNonQuery(NpgsqlConnection conn, string sql)
//        {
//            try
//            {
//                using (var cmd = new NpgsqlCommand(sql, conn))
//                    cmd.ExecuteNonQuery();
//            }
//            catch (PostgresException ex) when (ex.SqlState == "42701" || ex.SqlState == "42P07" || ex.SqlState == "42P16")
//            { /* column/index/constraint already exists — OK */ }
//            catch (Exception ex)
//            {
//                string preview = sql.Substring(0, Math.Min(100, sql.Length)).Replace('\n', ' ');
//                DbLogger.Error($"DDL fail: {preview}... → {ex.Message}", ex);
//                throw;
//            }
//        }

//        private bool IsDbAlive()
//        {
//            try
//            {
//                string connStr = frmDatabase.GetConnectionString(
//                    "postgresql",
//                    Shared.Settings.THLocalDbServer,
//                    Shared.Settings.THLocalDbPort,
//                    Shared.Settings.THLocalDbUsername,
//                    Shared.Settings.THLocalDbPassword,
//                    Shared.Settings.THLocalDbDatabase);

//                using (var conn = new NpgsqlConnection(connStr))
//                {
//                    conn.Open();
//                    using (var cmd = new NpgsqlCommand("SELECT 1", conn))
//                        cmd.ExecuteScalar();
//                }
//                return true;
//            }
//            catch { return false; }
//        }
//    }
//}

using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ReceiveModel;
using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ResponseModel;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster;
using BarcodeVerificationSystem.Utils.Logging;
using BarcodeVerificationSystem.View;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static BarcodeVerificationSystem.Services.THTrueMilk.THDb;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Controller
{
    public class QrBankDataController : BaseController
    {
        // Event dùng type từ namespace cha (Manufacturing) để tránh trùng
        public event EventHandler<QrBankDataReceivedEventArgs> DataReceived;

        private void SendQrBankError(HttpListenerResponse response, int statusCode, string message,
            int totalDuplicate = 0, int totalEmpty = 0,
            List<string> duplicateList = null, List<string> emptyList = null)
        {
            SendJsonResponse(response, new QrBankResponseData
            {
                is_success = false,
                total_duplicate = totalDuplicate,
                total_empty = totalEmpty,
                duplicate_list = duplicateList,
                empty_list = emptyList,
                message = message,
            }, statusCode);
        }

        /// <summary>POST /api/qrbank/data</summary>
        public void HandleData(HttpListenerRequest request, HttpListenerResponse response, string body)
        {
            try
            {
                if (!ValidateMethod(request, response, "POST")) return;

                string authHeader = request.Headers["Authorization"];
                if (string.IsNullOrWhiteSpace(authHeader))
                { SendQrBankError(response, 401, "Missing Authorization header"); return; }

                string token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader.Substring(7).Trim()
                    : authHeader;

                if (!QrBankLoginController.ValidateToken(token, out string username, out string tokenError))
                { SendQrBankError(response, 401, tokenError ?? "Invalid or expired token"); return; }

                if (string.IsNullOrWhiteSpace(body))
                { SendQrBankError(response, 400, "Request body is required"); return; }

                QrBankReceiveData data;
                try { data = JsonConvert.DeserializeObject<QrBankReceiveData>(body); }
                catch (Exception ex)
                {
                    DbLogger.Warning($"[QrBank] JSON parse thất bại: {ex.Message} | body={body?.Substring(0, Math.Min(200, body?.Length ?? 0))}");
                    SendQrBankError(response, 400, "Invalid JSON format");
                    return;
                }

                // ── Xây danh sách QR cần lưu (hỗ trợ cả batch qr_codes + single qr_code) ──
                List<string> qrList = new List<string>();
                if (data.qr_codes != null && data.qr_codes.Count > 0)
                    qrList.AddRange(data.qr_codes);
                else if (!string.IsNullOrWhiteSpace(data.qr_code))
                    qrList.Add(data.qr_code);

                if (qrList.Count == 0)
                { SendQrBankError(response, 422, "Field 'qr_codes' (array) or 'qr_code' (string) is required"); return; }

                // ── Log chi tiết dữ liệu nhận được từ QrBank server ──
                string rawBodyPreview = body.Length <= 500 ? body : body.Substring(0, 500) + "...";
#if DEBUG
                Console.WriteLine($"[QrBank] ─── POST /api/qrbank/data ───");
                Console.WriteLine($"[QrBank] Raw JSON body: {rawBodyPreview}");
                Console.WriteLine($"[QrBank] Parsed fields:");
                Console.WriteLine($"[QrBank]   factory_code = {data.factory_code}");
                Console.WriteLine($"[QrBank]   line_id      = {data.line_id}");
                Console.WriteLine($"[QrBank]   line_name    = {data.line_name}");
                Console.WriteLine($"[QrBank]   line_ip      = {data.line_ip}");
                Console.WriteLine($"[QrBank]   batch        = {data.batch}");
                Console.WriteLine($"[QrBank]   job_name     = {data.job_name}");
                Console.WriteLine($"[QrBank]   sender       = {data.sender}");
                Console.WriteLine($"[QrBank]   qr_code      = {data.qr_code}");
                Console.WriteLine($"[QrBank]   qr_codes.Count = {data.qr_codes?.Count ?? 0:n0}");
                if (qrList.Count <= 20)
                    Console.WriteLine($"[QrBank]   qr_codes list = [{string.Join(", ", qrList)}]");
                Console.WriteLine($"[QrBank]   user (token) = {username}");
                Console.WriteLine($"[QrBank] ─────────────────────────────");
#endif

                DbLogger.Info($"[QrBank] Nhận {qrList.Count} QR | user={username} | batch={data.batch} | line={data.line_id} | factory={data.factory_code} | job={data.job_name} | sender={data.sender}");

                // ── Lấy danh sách Camera Programs & Printer Templates từ cache ──
                var cameraPrograms = Shared.Settings.CachedCameraPrograms ?? new List<string>();
                var printerTemplates = Shared.Settings.CachedPrinterTemplates ?? new List<string>();

                if (cameraPrograms.Count == 0 || printerTemplates.Count == 0)
                {
                    DbLogger.Warning($"[QrBank] Chưa lấy được danh sách camera/printer: camera={cameraPrograms.Count}, printer={printerTemplates.Count}. Cần kiểm tra kết nối máy in và camera.");
                    SendQrBankError(response, 400,
                        "Chưa lấy được danh sách camera programs / printer templates. " +
                        "Vui lòng kiểm tra kết nối máy in và camera, sau đó refresh lại.");
                    return;
                }

                // ── So sánh product_gtin ──
                string gtinToSave = null;
                if (!string.IsNullOrWhiteSpace(data.product_gtin))
                {
                    string matchedCamera = cameraPrograms
                        .FirstOrDefault(p => p.Split('_').Last() == data.product_gtin);
                    bool matchedPrinter = printerTemplates.Contains(data.product_gtin);

                    if (string.IsNullOrEmpty(matchedCamera) && !matchedPrinter)
                    {
                        DbLogger.Warning($"[QrBank] GTIN '{data.product_gtin}' không khớp camera và printer");
                        SendQrBankError(response, 400,
                            $"GTIN '{data.product_gtin}' không hợp lệ: Không tìm thấy trong camera programs và printer templates");
                        return;
                    }
                    else if (string.IsNullOrEmpty(matchedCamera))
                    {
                        DbLogger.Warning($"[QrBank] GTIN '{data.product_gtin}' không khớp camera programs");
                        SendQrBankError(response, 400,
                            $"GTIN '{data.product_gtin}' không hợp lệ: Không tìm thấy trong camera programs");
                        return;
                    }
                    else if (!matchedPrinter)
                    {
                        DbLogger.Warning($"[QrBank] GTIN '{data.product_gtin}' không khớp printer templates");
                        SendQrBankError(response, 400,
                            $"GTIN '{data.product_gtin}' không hợp lệ: Không tìm thấy trong printer templates");
                        return;
                    }

                    gtinToSave = data.product_gtin;
                    DbLogger.Info($"[QrBank] Matched GTIN: {gtinToSave} → camera={matchedCamera}, template={matchedPrinter}");
                }

                // ── Bước 1: Validate TẤT CẢ QR trước khi insert ──
                DateTime savedAt = DateTime.Now;
                var validQrList = new List<string>();
                var duplicateList = new List<string>();
                var emptyList = new List<string>();

                foreach (string qr in qrList)
                {
                    if (string.IsNullOrWhiteSpace(qr))
                    {
                        emptyList.Add(qr);
                        continue;
                    }

                    if (QrExistsInDb(qr))
                    {
                        duplicateList.Add(qr);
                        DbLogger.Warning($"[QrBank] QR đã tồn tại trong DB (từ chối batch): '{qr}'");
                    }
                    else
                    {
                        validQrList.Add(qr);
                    }
                }

                // ── Bước 2: Nếu có QR bị từ chối → KHÔNG INSERT ──
                int totalReject = duplicateList.Count + emptyList.Count;
                if (totalReject > 0)
                {
                    var reasons = new List<string>();
                    if (emptyList.Count > 0)
                        reasons.Add($"{emptyList.Count} mã rỗng/null");
                    if (duplicateList.Count > 0)
                        reasons.Add($"{duplicateList.Count} mã đã tồn tại trong DB");

                    string detail = string.Join(", ", reasons);
                    var allRejected = duplicateList.Concat(emptyList).ToList();
                    string qrDetail = allRejected.Count <= 10
                        ? string.Join(", ", allRejected)
                        : string.Join(", ", allRejected.Take(10)) + $"... ({allRejected.Count} mã)";

                    DbLogger.Warning($"[QrBank] Batch bị từ chối: {detail} [{qrDetail}] | batch={data.batch} | line={data.line_id}");
                    SendQrBankError(response, 400,
                        $"Không insert được. {detail}: [{qrDetail}]",
                        totalDuplicate: duplicateList.Count,
                        totalEmpty: emptyList.Count,
                        duplicateList: duplicateList.Count > 0 ? duplicateList : null,
                        emptyList: emptyList.Count > 0 ? emptyList : null);
                    return;
                }

                // ── Bước 3: TẤT CẢ hợp lệ → Insert batch ──
                int inserted = 0;
                string firstQr = null, lastQr = null;
                string firstError = null;
                var errorList = new List<string>();

                foreach (string qr in validQrList)
                {
                    try
                    {
                        if (SaveQrToPostgres(qr, data.batch, data.line_id, data.line_name, data.factory_code, savedAt, gtinToSave, data.job_name))
                        {
                            inserted++;
                            if (firstQr == null) firstQr = qr;
                            lastQr = qr;
                        }
                        else
                        {
                            errorList.Add(qr);
                            DbLogger.Warning($"[QrBank] UNEXPECTED: QR '{qr}' trả rows=0 sau khi validate");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorList.Add(qr);
                        if (firstError == null) firstError = ex.Message;
                        string errorDetail = ex is PostgresException pgEx
                            ? $"PostgreSQL [{pgEx.SqlState}]: {pgEx.Message}"
                            : $"{ex.GetType().Name}: {ex.Message}";
                        DbLogger.Error($"[QrBank] Insert batch lỗi: '{qr}' | batch={data.batch} | line={data.line_id} | {errorDetail}", ex);
                    }
                }

                // ── Bước 4: Ghi log tổng kết ──
                if (inserted > 0)
                {
                    string summary = $"[QrBank] Tổng kết: inserted={inserted}, error={errorList.Count}" +
                                     $" | batch={data.batch} | line={data.line_id} | factory={data.factory_code}";
                    if (errorList.Count > 0)
                        DbLogger.Warning(summary);
                    else
                        DbLogger.Info(summary);
                }

                if (errorList.Count > 0)
                    DbLogger.Error($"[QrBank] Danh sách QR lỗi DB ({errorList.Count}): {string.Join(", ", errorList)}");

                // ── Bước 5: Lưu lịch sử nhận QR ──
                if (inserted > 0)
                {
                    RLinkLogService.SaveReceiveHistoryToPostgres(savedAt, inserted, data.batch, data.line_id,
                        data.line_name, data.factory_code, firstQr, lastQr, data.job_name, data.sender);

                    Task.Run(() => RLinkLogService.SaveReceiveHistoryToSQLite(
                        savedAt, inserted, data.batch ?? "", data.line_id ?? "",
                        data.line_name ?? "", data.factory_code ?? "", firstQr ?? "", lastQr ?? "",
                        data.job_name ?? "", data.sender ?? ""));

                    DataReceived?.Invoke(this, new QrBankDataReceivedEventArgs
                    {
                        QrCode = firstQr,
                        FactoryCode = data.factory_code,
                        LineId = data.line_id,
                        LineName = data.line_name,
                        Batch = data.batch,
                        AuthenticatedUser = username,
                        ReceivedAt = savedAt,
                        TotalCount = inserted,
                        FirstQr = firstQr,
                        LastQr = lastQr,
                        JobName = data.job_name,
                        Sender = data.sender
                    });
                    Console.WriteLine($"[QrBankDataController] ✓ Batch saved: {inserted} QR | errors: {errorList.Count}");
                }

                // ── Bước 6: Response ──
                if (errorList.Count > 0)
                {
                    string errorDetail = errorList.Count <= 10
                        ? string.Join(", ", errorList)
                        : string.Join(", ", errorList.Take(10)) + $"... ({errorList.Count} mã)";

                    Ok(response, new QrBankResponseData
                    {
                        is_success = inserted > 0,
                        total_inserted = inserted,
                        total_error = errorList.Count,
                        message = inserted > 0
                            ? $"Insert thành công {inserted}/{validQrList.Count} mã. {errorList.Count} mã lỗi DB: [{errorDetail}]"
                            : $"Insert thất bại. {errorList.Count} mã lỗi DB: [{errorDetail}]. {firstError}",
                        saved_at = inserted > 0 ? savedAt.ToString("yyyy-MM-dd HH:mm:ss") : null
                    });
                }
                else
                {
                    Ok(response, new QrBankResponseData
                    {
                        is_success = true,
                        total_inserted = inserted,
                        message = $"Lưu thành công {inserted} mã",
                        saved_at = savedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
            }
            catch (Exception ex)
            {
                DbLogger.Error($"[QrBank] HandleData thất bại nghiêm trọng: {ex.Message}", ex);
                Console.WriteLine($"[QrBankDataController] ✗ {ex.Message}");
                SendQrBankError(response, 500, $"Data processing failed: {ex.Message}");
            }
        }

        private bool QrExistsInDb(string qrCode)
        {
            try
            {
                string connStr = frmDatabase.GetConnectionString(
                    "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);

                if (string.IsNullOrWhiteSpace(connStr)) return false;

                using (var conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        $"SELECT 1 FROM \"{Code}\" WHERE {QrCode} = @qr LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue("qr", qrCode);
                        return cmd.ExecuteScalar() != null;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>GET /api/qrbank/ping</summary>
        public void HandlePing(HttpListenerRequest request, HttpListenerResponse response)
        {
            bool dbOk = IsDbAlive();
            DbLogger.Info($"[QrBank] Ping — DB: {(dbOk ? "connected" : "DISCONNECTED")}");
            Ok(response, new
            {
                status = "ok",
                database = dbOk ? "connected" : "disconnected",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        /// <summary>POST /api/qrbank/monitor</summary>
        public void HandleMonitor(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if (!ValidateMethod(request, response, "POST")) return;

                string authHeader = request.Headers["Authorization"];
                if (string.IsNullOrWhiteSpace(authHeader))
                { Unauthorized(response, "Missing Authorization header"); return; }

                string token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader.Substring(7).Trim()
                    : authHeader;

                if (!QrBankLoginController.ValidateToken(token, out string username, out string tokenError))
                { Unauthorized(response, tokenError ?? "Invalid or expired token"); return; }

                bool sent = false;
                try
                {
                    var monitor = RLinkMonitorService.Instance;
                    if (monitor != null)
                    {
                        monitor.SendImmediate();
                        sent = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[QrBank] HandleMonitor error: {ex.Message}");
                }

                Ok(response, new QrBankResponseMonitor
                {
                    is_success = sent,
                    message = sent ? "Monitor sent successfully" : "Failed to send monitor"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QrBank] HandleMonitor failed: {ex.Message}");
                InternalServerError(response, "Monitor processing failed", ex);
            }
        }

        private bool SaveQrToPostgres(string qrCode, string batch, string lineId,
            string lineName, string factoryCode, DateTime savedAt, string gtin, string jobName)
        {
            string table = Code;

            string connStr = frmDatabase.GetConnectionString(
                "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                Shared.Settings.THLocalDbDatabase);

            using (var conn = new NpgsqlConnection(connStr))
            {
                conn.Open();

                // Đảm bảo bảng tồn tại với đầy đủ cột — giống cách rlink_log_in được tạo
                EnsureCodeTable(conn, table);

                string insertSql = $@"
INSERT INTO ""{table}"" ({QrCode}, {Batch}, {LineId}, {LineName}, {FactoryCode}, {ReceivedAt}, product_gtin, {JobNameReceiveQr})
VALUES (@{QrCode}, @{Batch}, @{LineId}, @{LineName}, @{FactoryCode}, @{ReceivedAt}, @product_gtin, @{JobNameReceiveQr})
ON CONFLICT ({QrCode}) DO UPDATE SET product_gtin = EXCLUDED.product_gtin";

                try
                {
                    using (var cmd = new NpgsqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue(QrCode, qrCode ?? "");
                        cmd.Parameters.AddWithValue(Batch, (object)batch ?? DBNull.Value);
                        cmd.Parameters.AddWithValue(LineId, (object)lineId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue(LineName, (object)lineName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue(FactoryCode, (object)factoryCode ?? DBNull.Value);
                        cmd.Parameters.AddWithValue(ReceivedAt, savedAt);
                        cmd.Parameters.AddWithValue("product_gtin", (object)gtin ?? DBNull.Value);
                        cmd.Parameters.AddWithValue(JobNameReceiveQr, (object)jobName ?? DBNull.Value);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0) return false;

                        Task.Run(() => RLinkLogService.InsertQrCodeToSQLiteStatic(qrCode, savedAt, gtin ?? ""));
                        return true;
                    }
                }
                catch (PostgresException ex) when (ex.SqlState == "42703")
                {
                    string missingCol = ExtractColumnName(ex.MessageText);
                    Console.WriteLine($"[QrBank] Cột '{missingCol}' thiếu → thêm & retry...");
                    ExecuteNonQuery(conn, $@"ALTER TABLE ""{table}"" ADD COLUMN IF NOT EXISTS {missingCol} VARCHAR(200) DEFAULT ''");
                    
                    using (var cmd = new NpgsqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue(QrCode, qrCode ?? "");
                        cmd.Parameters.AddWithValue(Batch, (object)batch ?? DBNull.Value);
                        cmd.Parameters.AddWithValue(LineId, (object)lineId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue(LineName, (object)lineName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue(FactoryCode, (object)factoryCode ?? DBNull.Value);
                        cmd.Parameters.AddWithValue(ReceivedAt, savedAt);
                        cmd.Parameters.AddWithValue("product_gtin", (object)gtin ?? DBNull.Value);
                        cmd.Parameters.AddWithValue(JobNameReceiveQr, (object)jobName ?? DBNull.Value);
                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0) return false;
                        Task.Run(() => RLinkLogService.InsertQrCodeToSQLiteStatic(qrCode, savedAt, gtin ?? ""));
                        return true;
                    }
                }
            }
        }

        private static string ExtractColumnName(string msg)
        {
            // Message dạng: column "received_at" does not exist
            var match = System.Text.RegularExpressions.Regex.Match(msg, @"column ""(\w+)""");
            return match.Success ? match.Groups[1].Value : "";
        }

        private static void EnsureCodeTable(NpgsqlConnection conn, string table)
        {
            using (var cmd = new NpgsqlCommand($@"
CREATE TABLE IF NOT EXISTS ""{table}"" (
    {Id}              BIGSERIAL    PRIMARY KEY,
    {QrCode}      TEXT         NOT NULL UNIQUE,
    {FactoryCode} VARCHAR(20)  DEFAULT '',
    {LineId}      VARCHAR(50)  DEFAULT '',
    {LineName}    VARCHAR(100) DEFAULT '',
    {Batch}        VARCHAR(100) DEFAULT '',
    {JobName}     VARCHAR(200) DEFAULT '',
    {ReceivedAt}  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    {UsedAt}      TIMESTAMPTZ,
    {IsUsed}      BOOLEAN      NOT NULL DEFAULT FALSE,
    product_gtin  VARCHAR(50)  DEFAULT '',
    {JobNameReceiveQr} VARCHAR(200) DEFAULT ''
)", conn))
            {
                cmd.ExecuteNonQuery();
            }

            try
            {
                using (var cmd2 = new NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD CONSTRAINT IF NOT EXISTS uq_{table}_{QrCode} UNIQUE ({QrCode})", conn))
                    cmd2.ExecuteNonQuery();
            }
            catch { }

            try
            {
                using (var cmd3 = new NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS product_gtin VARCHAR(50) DEFAULT ''", conn))
                    cmd3.ExecuteNonQuery();
            }
            catch { }

            try
            {
                using (var cmd4 = new NpgsqlCommand(
                    $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS {JobNameReceiveQr} VARCHAR(200) DEFAULT ''", conn))
                    cmd4.ExecuteNonQuery();
            }
            catch { }
        }

        // ── Helper chạy lệnh DDL riêng lẻ, bỏ qua lỗi không nghiêm trọng ──
        private static void ExecuteNonQuery(NpgsqlConnection conn, string sql)
        {
            try
            {
                using (var cmd = new NpgsqlCommand(sql, conn))
                    cmd.ExecuteNonQuery();
            }
            catch (PostgresException ex) when (ex.SqlState == "42701") // column already exists
            { /* bỏ qua */ }
            catch (Exception ex)
            {
                Console.WriteLine($"[QrBank] ExecuteNonQuery lỗi (bỏ qua): {ex.Message} | SQL: {sql.Substring(0, Math.Min(100, sql.Length))}...");
            }
        }
            

        private bool IsDbAlive()
        {
            try
            {
                string connStr = frmDatabase.GetConnectionString(
                    "postgresql",
                    Shared.Settings.THLocalDbServer,
                    Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername,
                    Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);

                using (var conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT 1", conn))
                        cmd.ExecuteScalar();
                }
                return true;
            }
            catch (Exception ex)
            {
                DbLogger.Error($"[QrBank] DB ping thất bại: {ex.Message}", ex);
                return false;
            }
        }
    }
}