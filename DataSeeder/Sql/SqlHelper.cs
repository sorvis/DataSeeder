using System.Data.Common;
using System.Linq;
using Dapper;

namespace DataSeeder.Sql
{
    public class SqlHelper
    {
        private readonly DbConnection connection;

        public SqlHelper(DbConnection connection)
        {
            this.connection = connection;
        }

        public TableOperations GetTableOperations(string tableName)
        {
//            var columnInfos = this.connection.Query<ColumnInfo>(@"select col.COLUMN_NAME as ColumnName, 
//	(CASE WHEN col.IS_NULLABLE = 'YES' THEN 1 ELSE 0 END) as IsNullable, 
//	(CASE WHEN col.COLUMN_DEFAULT is null THEN 1 ELSE 0 END) as HasDefault 
//from INFORMATION_SCHEMA.COLUMNS col
//where col.TABLE_NAME = @tableName", new {tableName = tableName});

            // Oracle version
            var columnInfos = this.connection.Query<ColumnInfo>(@"SELECT col.COLUMN_NAME AS ColumnName,
  (
  CASE
    WHEN col.NULLABLE = 'N'
    THEN 1
    ELSE 0
  END) AS IsNullable,
  (
  CASE
    WHEN col.DATA_DEFAULT IS NULL
    THEN 1
    ELSE 0
  END) AS HasDefault
FROM ALL_TAB_COLUMNS col
WHERE col.TABLE_NAME = :tableName", new { tableName = tableName });

//            var primaryKeyColumns = this.connection.Query<string>(@"select col.COLUMN_NAME from INFORMATION_SCHEMA.TABLE_CONSTRAINTS con
//join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE col on col.CONSTRAINT_NAME = con.CONSTRAINT_NAME
//where con.TABLE_NAME = @tableName", new {tableName = tableName});

            // Oracle version
            var primaryKeyColumns = this.connection.Query<string>(@"SELECT cols.column_name
FROM all_constraints cons, all_cons_columns cols
WHERE cols.table_name = :tableName
AND cons.constraint_type = 'P'
AND cons.constraint_name = cols.constraint_name
AND cons.owner = cols.owner
ORDER BY cols.table_name, cols.position", new { tableName = tableName });

            return new TableOperations(this.connection, tableName, columnInfos.ToArray(), primaryKeyColumns.ToArray());
        }
    }
}
