using System.Collections.Generic;
using System.Linq;

namespace BarcodeVerificationSystem.Services.THTrueMilk
{
    /// <summary>
    /// Định nghĩa schema bảng 1 lần — tự sinh CREATE TABLE / ALTER.
    /// Columns[0] = PK (auto-increment).
    /// Khi đổi tên cột/bảng trong THDbSchema → rebuild → values tự cập nhật.
    /// Khi thêm cột → thêm 1 dòng ColumnDef + update INSERT/SELECT nếu cần.
    /// </summary>
    public class TableSchema
    {
        public string TableName { get; }
        public List<ColumnDef> Columns { get; }

        public TableSchema(string tableName, List<ColumnDef> columns)
        {
            TableName = tableName;
            Columns = columns;
        }

        public string ColList =>
            string.Join(", ", Columns.Skip(1).Select(c => c.Name));

        public string ColListWithId =>
            $"{Columns[0].Name}, {ColList}";

        public string ParamListSqlite =>
            string.Join(", ", Columns.Skip(1).Select(c => $"@{c.Name}"));

        public string ParamListPg =>
            string.Join(", ", Columns.Skip(1).Select(c => c.Name));

        /// <summary>CREATE TABLE SQLite</summary>
        public string CreateSQLite(string sqliteNow = null)
        {
            var lines = Columns.Select(c => $"    {c.Name,-20} {c.SqliteType}");
            var sql = $"CREATE TABLE IF NOT EXISTS {TableName} (\n{string.Join(",\n", lines)}\n)";
            if (!string.IsNullOrEmpty(sqliteNow))
                sql = sql.Replace("{SqliteNow}", sqliteNow);
            return sql;
        }

        /// <summary>CREATE TABLE PostgreSQL</summary>
        public string CreatePg()
        {
            var lines = Columns.Select(c => $"    {c.Name,-25} {c.PgType}");
            return $"CREATE TABLE IF NOT EXISTS {TableName} (\n{string.Join(",\n", lines)}\n);";
        }

        /// <summary>INSERT template (SQLite) với {TableName} và danh sách cột</summary>
        public string InsertSQLite() =>
            $"INSERT INTO {TableName} ({ColList}) VALUES ({ParamListSqlite})";

        /// <summary>INSERT template (PG)</summary>
        public string InsertPg() =>
            $"INSERT INTO {TableName} ({ColList}) VALUES ({ParamListPg})";

        /// <summary>Sinh code TryAddSqliteColumn cho migration</summary>
        public string AlterSqlite(string connVar)
        {
            return string.Join("\n",
                Columns.Skip(1).Select(c =>
                    $"            TryAddSqliteColumn({connVar}, {TableName}, {c.Name}, \"{c.SqliteType}\");"));
        }

        /// <summary>Sinh ALTER TABLE ADD COLUMN IF NOT EXISTS cho PG</summary>
        public string AlterPg()
        {
            return string.Join("\n",
                Columns.Skip(1).Select(c =>
                    $"            ALTER TABLE {TableName} ADD COLUMN IF NOT EXISTS {c.Name} {c.PgType};"));
        }
    }
}
