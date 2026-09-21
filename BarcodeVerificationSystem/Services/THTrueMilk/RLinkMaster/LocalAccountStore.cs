//using BarcodeVerificationSystem.Controller;
//using BarcodeVerificationSystem.View;
//using Newtonsoft.Json;
//using Npgsql;
//using System;
//using System.Threading.Tasks;
//using static BarcodeVerificationSystem.Controller.Shared;

//namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
//{
//    /// <summary>
//    /// Lưu và xác thực tài khoản R-Link Master vào PostgreSQL local — dùng khi offline.
//    /// Bảng: rlink_accounts (username, password_hash, full_name, role, permissions_json, updated_at)
//    /// </summary>
//    public static class LocalAccountStore
//    {
//        private const string TableName = Accounts;

//        // ── Lấy connection string từ Shared.Settings ───────────────────────
//        private static string GetConnStr()
//        {
//            return frmDatabase.GetConnectionString(
//                "postgresql",
//                Shared.Settings.THLocalDbServer,
//                Shared.Settings.THLocalDbPort,
//                Shared.Settings.THLocalDbUsername,
//                Shared.Settings.THLocalDbPassword,
//                Shared.Settings.THLocalDbDatabase);
//        }

//        // ── Tạo bảng nếu chưa có ───────────────────────────────────────────
//        private static void EnsureTable(NpgsqlConnection conn)
//        {
//            string sql = $@"
//                CREATE TABLE IF NOT EXISTS ""{TableName}"" (
//                    username         TEXT PRIMARY KEY,
//                    password_hash    TEXT NOT NULL,
//                    full_name        TEXT,
//                    role             TEXT,
//                    permissions_json TEXT,
//                    updated_at       TIMESTAMPTZ DEFAULT NOW()
//                )";
//            using (var cmd = new NpgsqlCommand(sql, conn))
//                cmd.ExecuteNonQuery();
//        }

//        /// <summary>
//        /// Upsert tài khoản sau khi đăng nhập online thành công.
//        /// Password được mã hoá bằng SecurityController trước khi lưu.
//        /// </summary>
//        public static async Task SaveAccountAsync(
//            string username, string password,
//            string fullName, string role, object permissions)
//        {
//            try
//            {
//                string encryptedPwd = SecurityController.Encrypt(password, "rlink_offline_key");
//                string permissionsJson = permissions != null
//                    ? JsonConvert.SerializeObject(permissions)
//                    : "{}";

//                string connStr = GetConnStr();
//                using (var conn = new NpgsqlConnection(connStr))
//                {
//                    await conn.OpenAsync();
//                    EnsureTable(conn);

//                    string sql = $@"
//                        INSERT INTO ""{TableName}"" (username, password_hash, full_name, role, permissions_json, updated_at)
//                        VALUES (@u, @p, @fn, @r, @pj, NOW())
//                        ON CONFLICT (username) DO UPDATE
//                            SET password_hash    = EXCLUDED.password_hash,
//                                full_name        = EXCLUDED.full_name,
//                                role             = EXCLUDED.role,
//                                permissions_json = EXCLUDED.permissions_json,
//                                updated_at       = NOW()";

//                    using (var cmd = new NpgsqlCommand(sql, conn))
//                    {
//                        cmd.Parameters.AddWithValue("u", username);
//                        cmd.Parameters.AddWithValue("p", encryptedPwd);
//                        cmd.Parameters.AddWithValue("fn", (object)fullName ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("r", (object)role ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("pj", permissionsJson);
//                        await cmd.ExecuteNonQueryAsync();
//                    }
//                }

//                Console.WriteLine($"[LocalAccountStore] Đã lưu tài khoản '{username}'.");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[LocalAccountStore] SaveAccountAsync lỗi: {ex.Message}");
//            }
//        }

//        /// <summary>
//        /// Xác thực offline: so sánh password với bản đã lưu trong DB.
//        /// Trả về OfflineAccountInfo nếu thành công, null nếu thất bại.
//        /// </summary>
//        public static async Task<OfflineAccountInfo> ValidateOfflineAsync(string username, string password)
//        {
//            try
//            {
//                string connStr = GetConnStr();
//                using (var conn = new NpgsqlConnection(connStr))
//                {
//                    await conn.OpenAsync();
//                    EnsureTable(conn);

//                    string sql = $@"
//                        SELECT password_hash, full_name, role, permissions_json
//                        FROM ""{TableName}""
//                        WHERE username = @u
//                        LIMIT 1";

//                    using (var cmd = new NpgsqlCommand(sql, conn))
//                    {
//                        cmd.Parameters.AddWithValue("u", username);
//                        using (var reader = await cmd.ExecuteReaderAsync())
//                        {
//                            if (!reader.Read()) return null;

//                            string storedHash = reader.GetString(0);
//                            string decrypted = SecurityController.Decrypt(storedHash, "rlink_offline_key");

//                            if (!string.Equals(decrypted, password, StringComparison.Ordinal))
//                                return null;

//                            return new OfflineAccountInfo
//                            {
//                                Username = username,
//                                FullName = reader.IsDBNull(1) ? username : reader.GetString(1),
//                                Role = reader.IsDBNull(2) ? "0" : reader.GetString(2),
//                                PermissionsJson = reader.IsDBNull(3) ? "{}" : reader.GetString(3)
//                            };
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[LocalAccountStore] ValidateOfflineAsync lỗi: {ex.Message}");
//                return null;
//            }
//        }
//    }

//    public class OfflineAccountInfo
//    {
//        public string Username { get; set; }
//        public string FullName { get; set; }
//        public string Role { get; set; }
//        public string PermissionsJson { get; set; }
//    }
//}

using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;
using BarcodeVerificationSystem.View;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BarcodeVerificationSystem.Services.THTrueMilk;
using static BarcodeVerificationSystem.Controller.Shared;
using static BarcodeVerificationSystem.Services.THTrueMilk.THDb;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
{
    /// <summary>
    /// Lưu và xác thực tài khoản R-Link Master vào PostgreSQL local — dùng khi offline.
    /// Bảng: rlink_accounts (username, password_hash, full_name, role, permissions_json, updated_at)
    /// </summary>
    public static class LocalAccountStore
    {
        private const string TableName = THDb.TableAccounts;

        private static string GetConnStr()
        {
            if (string.IsNullOrWhiteSpace(Shared.Settings.THLocalDbDatabase))
                throw new InvalidOperationException(
                    "THLocalDbDatabase chưa được cấu hình. Vui lòng vào Settings → Database và điền tên database PostgreSQL.");

            return frmDatabase.GetConnectionString(
                "postgresql",
                Shared.Settings.THLocalDbServer,
                Shared.Settings.THLocalDbPort,
                Shared.Settings.THLocalDbUsername,
                Shared.Settings.THLocalDbPassword,
                Shared.Settings.THLocalDbDatabase);
        }
        public static async Task SyncProductsAsync(List<ProductItem> products)
        {
            if (products == null || products.Count == 0) return;
            try
            {
                string connStr = GetConnStr();
                using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    using (var cmd = new NpgsqlCommand($@"
                CREATE TABLE IF NOT EXISTS {Products} (
                    {ProductId}   TEXT PRIMARY KEY,
                    {ProductName} TEXT,
                    {ProductGtin} TEXT DEFAULT '',
                    {ProductImage} TEXT DEFAULT '',
                    {Volume}      INTEGER DEFAULT 0,
                    {Exp}         INTEGER DEFAULT 0,
                    {DataJson}    TEXT,
                    {UpdatedAt}   TIMESTAMPTZ DEFAULT NOW()
                );
                ALTER TABLE {Products} ADD COLUMN IF NOT EXISTS {ProductGtin} TEXT DEFAULT '';
                ALTER TABLE {Products} ADD COLUMN IF NOT EXISTS {ProductImage} TEXT DEFAULT '';
                ALTER TABLE {Products} ADD COLUMN IF NOT EXISTS {Volume} INTEGER DEFAULT 0;
                ALTER TABLE {Products} ADD COLUMN IF NOT EXISTS {Exp} INTEGER DEFAULT 0;", conn))
                        await cmd.ExecuteNonQueryAsync();

                    using (var delCmd = new NpgsqlCommand($"DELETE FROM {Products}", conn))
                        await delCmd.ExecuteNonQueryAsync();

                    foreach (var p in products)
                    {
                        if (string.IsNullOrWhiteSpace(p.ProductId)) continue;
                        string json = JsonConvert.SerializeObject(p);
                        using (var cmd = new NpgsqlCommand($@"
                    INSERT INTO {Products} ({ProductId}, {ProductName}, {ProductGtin}, {ProductImage}, {Volume}, {Exp}, {DataJson}, {UpdatedAt})
                    VALUES (@id, @n, @gtin, @img, @vol, @exp, @j, NOW())
                    ON CONFLICT ({ProductId}) DO UPDATE
                        SET {ProductName} = EXCLUDED.{ProductName},
                            {ProductGtin} = EXCLUDED.{ProductGtin},
                            {ProductImage} = EXCLUDED.{ProductImage},
                            {Volume}      = EXCLUDED.{Volume},
                            {Exp}         = EXCLUDED.{Exp},
                            {DataJson}    = EXCLUDED.{DataJson},
                            {UpdatedAt}   = NOW()", conn))
                        {
                            cmd.Parameters.AddWithValue("id", p.ProductId);
                            cmd.Parameters.AddWithValue("n", (object)p.ProductName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("gtin", (object)p.ProductGtin ?? DBNull.Value);
                            // Đảm bảo lưu URL gốc vào DB, không lưu local path
                            string imageToSave = p.Image;
                            if (!string.IsNullOrEmpty(p.Image) && (p.Image.Contains(@":\") || p.Image.StartsWith("\\\\")))
                            {
                                if (BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing.ProductImageHelper.TryGetOriginalUrl(p.ProductId, out var origUrl))
                                    imageToSave = origUrl;
                            }
                            cmd.Parameters.AddWithValue("img", (object)imageToSave ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("vol", (object)p.Volume ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("exp", (object)p.Exp ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("j", json);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                Console.WriteLine($"[LocalAccountStore] Đã sync {products.Count} sản phẩm.");

                // ── Mirror vào SQLite rlink_products (cùng schema PG) ──
                await Task.Run(() => RLinkLogService.UpsertProductsToSQLite(products));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalAccountStore] SyncProductsAsync lỗi: {ex.Message}");
            }
        }
        private static void EnsureTable(NpgsqlConnection conn)
        {
            string sql = $@"
                CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                    {Username}         TEXT PRIMARY KEY,
                    {PasswordHash}    TEXT NOT NULL,
                    {FullName}        TEXT,
                    {THDb.Role}             TEXT,
                    {PermissionsJson} TEXT,
                    {AccId}           TEXT,
                    {DisplayName}     TEXT,
                    {PerDeviceId}     TEXT,
                    {UpdatedAt}       TIMESTAMPTZ DEFAULT NOW()
                )";
            // Add missing columns for migration
            var migrate = $@"
                ALTER TABLE ""{TableName}"" ADD COLUMN IF NOT EXISTS {AccId} TEXT;
                ALTER TABLE ""{TableName}"" ADD COLUMN IF NOT EXISTS {DisplayName} TEXT;
                ALTER TABLE ""{TableName}"" ADD COLUMN IF NOT EXISTS {PerDeviceId} TEXT;";
            using (var cmd = new NpgsqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
            try { using (var cmd = new NpgsqlCommand(migrate, conn)) cmd.ExecuteNonQuery(); }
            catch { }
        }

        /// <summary>
        /// Upsert tài khoản sau khi đăng nhập online thành công (có password).
        /// </summary>
        public static async Task SaveAccountAsync(
            string username, string password,
            string fullName, string role, object permissions)
        {
            try
            {
                string encryptedPwd = SecurityController.Encrypt(password, "rlink_offline_key");
                string permissionsJson = permissions != null
                    ? JsonConvert.SerializeObject(permissions)
                    : "{}";

                string connStr = GetConnStr();
                using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    EnsureTable(conn);

                    string sql = $@"
                        INSERT INTO ""{TableName}"" ({Username}, {PasswordHash}, {FullName}, {THDb.Role}, {PermissionsJson}, {UpdatedAt})
                        VALUES (@u, @p, @fn, @r, @pj, NOW())
                        ON CONFLICT ({Username}) DO UPDATE
                            SET {PasswordHash}    = EXCLUDED.{PasswordHash},
                                {FullName}        = EXCLUDED.{FullName},
                                {THDb.Role}       = EXCLUDED.{THDb.Role},
                                {PermissionsJson} = EXCLUDED.{PermissionsJson},
                                {UpdatedAt}       = NOW()";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("u", username);
                        cmd.Parameters.AddWithValue("p", encryptedPwd);
                        cmd.Parameters.AddWithValue("fn", (object)fullName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("r", (object)role ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("pj", permissionsJson);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                Console.WriteLine($"[LocalAccountStore] Đã lưu tài khoản '{username}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalAccountStore] SaveAccountAsync lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Đồng bộ danh sách tài khoản từ R-Link Master vào PostgreSQL.
        /// Chỉ upsert metadata (username, full_name, role, permissions).
        /// Không ghi đè password_hash nếu account đã tồn tại.
        /// Đồng thời sync vào local SQLite (UserController) để hỗ trợ login offline.
        /// </summary>
        public static async Task SyncAccountsFromRLinkAsync(List<AccountInfo> accounts)
        {
            if (accounts == null || accounts.Count == 0) return;

            try
            {
                string connStr = GetConnStr();
                using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    EnsureTable(conn);

                    foreach (var acc in accounts)
                    {
                        if (string.IsNullOrWhiteSpace(acc.username)) continue;

                        string permissionsJson = acc.permissions != null
                            ? JsonConvert.SerializeObject(acc.permissions)
                            : "{}";
                        string pwd = !string.IsNullOrEmpty(acc.password)
                            ? SecurityController.Encrypt(acc.password, "rlink_offline_key")
                            : "__rlink_placeholder__";

                        // Upsert vào PostgreSQL — lưu password từ API + fields mới
                        string sql = $@"
                            INSERT INTO ""{TableName}"" ({Username}, {PasswordHash}, {FullName}, {THDb.Role}, {PermissionsJson}, {AccId}, {DisplayName}, {PerDeviceId}, {UpdatedAt})
                            VALUES (@u, @pwd, @fn, @r, @pj, @aid, @dn, @pdid, NOW())
                            ON CONFLICT ({Username}) DO UPDATE
                                SET {FullName}        = EXCLUDED.{FullName},
                                    {THDb.Role}       = EXCLUDED.{THDb.Role},
                                    {PermissionsJson} = EXCLUDED.{PermissionsJson},
                                    {PasswordHash}    = EXCLUDED.{PasswordHash},
                                    {AccId}           = EXCLUDED.{AccId},
                                    {DisplayName}     = EXCLUDED.{DisplayName},
                                    {PerDeviceId}     = EXCLUDED.{PerDeviceId},
                                    {UpdatedAt}       = NOW()";

                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("u", acc.username);
                            cmd.Parameters.AddWithValue("pwd", pwd);
                            cmd.Parameters.AddWithValue("fn", (object)acc.full_name ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("r", (object)acc.role ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("pj", permissionsJson);
                            cmd.Parameters.AddWithValue("aid", (object)acc.acc_id ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("dn", (object)acc.display_name ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("pdid", (object)acc.per_device_id ?? DBNull.Value);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // ── Lưu vào RLinkLog SQLite mirror (tb_DeviceAccount) ──
                        try
                        {
                            string sqliteConnStr = RLinkLogService.ConnStr;
                            using (var sqliteConn = new System.Data.SQLite.SQLiteConnection(sqliteConnStr))
                            {
                                sqliteConn.Open();
                                using (var sqliteCmd = sqliteConn.CreateCommand())
                                {
                                    sqliteCmd.CommandText = $@"
                                        INSERT OR REPLACE INTO {TableAccounts}
                                            ({Username}, {PasswordHash}, {FullName}, {THDb.Role}, {PermissionsJson},
                                             {AccId}, {DisplayName}, {PerDeviceId}, {UpdatedAt})
                                        VALUES
                                            (@u, @pwd, @fn, @r, @pj,
                                             @aid, @dn, @pdid, datetime('now','localtime'))";
                                    sqliteCmd.Parameters.AddWithValue("@u", acc.username);
                                    sqliteCmd.Parameters.AddWithValue("@pwd", pwd);
                                    sqliteCmd.Parameters.AddWithValue("@fn", (object)acc.full_name ?? DBNull.Value);
                                    sqliteCmd.Parameters.AddWithValue("@r", (object)acc.role ?? DBNull.Value);
                                    sqliteCmd.Parameters.AddWithValue("@pj", permissionsJson);
                                    sqliteCmd.Parameters.AddWithValue("@aid", (object)acc.acc_id ?? DBNull.Value);
                                    sqliteCmd.Parameters.AddWithValue("@dn", (object)acc.display_name ?? DBNull.Value);
                                    sqliteCmd.Parameters.AddWithValue("@pdid", (object)acc.per_device_id ?? DBNull.Value);
                                    sqliteCmd.ExecuteNonQuery();
                                }
                            }
                            Console.WriteLine($"[LocalAccountStore] RLink SQLite INSERT '{acc.username}' OK");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[LocalAccountStore] RLink SQLite sync '{acc.username}': {ex.Message}");
                        }

                        // Sync vào local SQLite AccountDB (tbl_account)
                        try
                        {
                            var existing = UserController.CheckExistUserName(acc.username);
                            // Role: 0=Admin nếu có quyền accounts hoặc settings, 1=Operator
                            int roleInt = (acc.permissions != null &&
                                (acc.permissions.ContainsKey("accounts") || acc.permissions.ContainsKey("settings"))) ? 0 : 1;
                            if (existing == null)
                            {
                                UserController.AddAccount(
                                    acc.full_name ?? acc.username,
                                    acc.username,
                                    pwd,
                                    roleInt);
                                Console.WriteLine($"[LocalAccountStore] SQLite INSERT '{acc.username}' OK");
                            }
                            else
                            {
                                UserController.EditAccount(
                                    acc.full_name ?? acc.username,
                                    acc.username,
                                    pwd,
                                    roleInt,
                                    isChangePassword: !string.IsNullOrEmpty(acc.password));
                                Console.WriteLine($"[LocalAccountStore] SQLite UPDATE '{acc.username}' OK");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[LocalAccountStore] SQLite sync '{acc.username}': {ex.Message}");
                        }
                    }
                }

                Console.WriteLine($"[LocalAccountStore] Đã sync {accounts.Count} tài khoản từ R-Link.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalAccountStore] SyncAccountsFromRLinkAsync lỗi: {ex.Message}");
            }
        }
    
        /// <summary>
        /// Đọc danh sách sản phẩm từ PostgreSQL local (đã sync khi login online).
        /// </summary>
        public static async Task<List<ProductItem>> LoadProductsFromDbAsync()
        {
            var result = new List<ProductItem>();
            try
            {
                string connStr = GetConnStr();
                using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(
                                                $"SELECT {ProductId}, {ProductName}, {ProductGtin}, {ProductImage}, {Volume}, {Exp} FROM {Products} ORDER BY {ProductId}", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            result.Add(new ProductItem
                            {
                                ProductId = reader.IsDBNull(0) ? "" : reader.GetString(0),
                                ProductName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                ProductGtin = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Image = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Volume = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                Exp = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                            });
                        }
                    }
                }
                Console.WriteLine($"[LocalAccountStore] Load {result.Count} sản phẩm từ PostgreSQL.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalAccountStore] LoadProductsFromDbAsync lỗi: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// Đọc cấu hình R-Link settings từ PostgreSQL local theo line_id.
        /// </summary>
        public static async Task<RLinkSettings> LoadSettingsFromDbAsync(string lineId)
        {
            if (string.IsNullOrWhiteSpace(lineId)) return null;
            try
            {
                string connStr = GetConnStr();
                using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(
                                                $"SELECT {DataJson} FROM {TableSettings} WHERE {LineId} = @lid LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue("lid", lineId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read() && !reader.IsDBNull(0))
                            {
                                string json = reader.GetString(0);
                                var settings = JsonConvert.DeserializeObject<RLinkSettings>(json);
                                Console.WriteLine($"[LocalAccountStore] Load settings cho line '{lineId}' từ PostgreSQL.");
                                return settings;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalAccountStore] LoadSettingsFromDbAsync PG lỗi: {ex.Message}");
            }

            // ── Fallback SQLite nếu PG không có hoặc offline ──────
            try
            {
                return await Task.Run(() =>
                {
                    string sqliteConnStr = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ConnStr;
                    using (var conn = new System.Data.SQLite.SQLiteConnection(sqliteConnStr))
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = $"SELECT {DataJson} FROM {TableSettings} WHERE {LineId} = @lid LIMIT 1";
                            cmd.Parameters.AddWithValue("@lid", lineId);
                            var json = cmd.ExecuteScalar() as string;
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                var settings = JsonConvert.DeserializeObject<RLinkSettings>(json);
                                Console.WriteLine($"[LocalAccountStore] Load settings cho line '{lineId}' từ SQLite.");
                                return settings;
                            }
                        }
                    }
                    return null;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalAccountStore] LoadSettingsFromDbAsync SQLite lỗi: {ex.Message}");
            }

            return null;
        }
        /// <summary>
        /// Lưu cấu hình R-Link settings vào PostgreSQL.
        /// Bảng: {TableSettings} ({LineId}, {DataJson}, {UpdatedAt})
        /// </summary>
        public static async Task SaveSettingsAsync(string lineId, RLinkSettings settings)
        {
            if (settings == null || string.IsNullOrWhiteSpace(lineId)) return;
            try
            {
                string connStr = GetConnStr();
                using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    using (var cmd = new NpgsqlCommand($@"
                CREATE TABLE IF NOT EXISTS {TableSettings} (
                    {LineId}    TEXT PRIMARY KEY,
                    {DataJson}  TEXT,
                    {UpdatedAt} TIMESTAMPTZ DEFAULT NOW()
                )", conn))
                        await cmd.ExecuteNonQueryAsync();

                    using (var cmd = new NpgsqlCommand($@"
                INSERT INTO {TableSettings} ({LineId}, {DataJson}, {UpdatedAt})
                VALUES (@lid, @j, NOW())
                ON CONFLICT ({LineId}) DO UPDATE
                    SET {DataJson}  = EXCLUDED.{DataJson},
                        {UpdatedAt} = NOW()", conn))
                    {
                        cmd.Parameters.AddWithValue("lid", lineId);
                        cmd.Parameters.AddWithValue("j", JsonConvert.SerializeObject(settings));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Console.WriteLine($"[LocalAccountStore] Đã lưu settings cho line '{lineId}'.");

                // ── Mirror vào SQLite {TableSettings} (cùng schema PG) ──
                await Task.Run(() => RLinkLogService.UpsertSettingsToSQLite(lineId, settings));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalAccountStore] SaveSettingsAsync lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Xác thực offline: so sánh password với bản đã lưu trong DB.
        /// </summary>
        public static async Task<OfflineAccountInfo> ValidateOfflineAsync(string username, string password)
        {
            try
            {
                string connStr = GetConnStr();
                using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    EnsureTable(conn);

                    string sql = $@"
                        SELECT {PasswordHash}, {FullName}, {THDb.Role}, {PermissionsJson}
                        FROM ""{TableName}""
                        WHERE {Username} = @u
                        LIMIT 1";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("u", username);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!reader.Read()) return null;

                            string storedHash = reader.GetString(0);
                            string decrypted = SecurityController.Decrypt(storedHash, "rlink_offline_key");

                            if (!string.Equals(decrypted, password, StringComparison.Ordinal))
                                return null;

                            return new OfflineAccountInfo
                            {
                                Username = username,
                                FullName = reader.IsDBNull(1) ? username : reader.GetString(1),
                                Role = reader.IsDBNull(2) ? "0" : reader.GetString(2),
                                PermissionsJson = reader.IsDBNull(3) ? "{}" : reader.GetString(3)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalAccountStore] ValidateOfflineAsync lỗi: {ex.Message}");
                return null;
            }
        }
    }

    public class OfflineAccountInfo
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string PermissionsJson { get; set; }
    }
}