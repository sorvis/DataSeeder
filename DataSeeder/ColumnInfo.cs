namespace DataSeeder
{
    public class ColumnInfo
    {
        public string ColumnName { get; private set; }
        public bool IsNullable { get; private set; }
        public bool IsRequired { get; private set; }
    }
}