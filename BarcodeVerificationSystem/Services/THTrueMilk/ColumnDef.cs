namespace BarcodeVerificationSystem.Services.THTrueMilk
{
    /// <summary>Định nghĩa 1 cột với type cho cả SQLite và PG</summary>
    public class ColumnDef
    {
        /// <summary>Tên cột (giá trị THDb constant đã resolve, vd: "line_id")</summary>
        public string Name { get; }
        public string SqliteType { get; }
        public string PgType { get; }

        public ColumnDef(string name, string sqliteType, string pgType)
        {
            Name = name;
            SqliteType = sqliteType;
            PgType = pgType;
        }
    }
}
