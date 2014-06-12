using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DataSeeder
{
    public class SqlHelper
    {
        private readonly DbConnection connection;

        public SqlHelper(DbConnection connection)
        {
            this.connection = connection;
        }

        public List<string> GetPrimaryKeyColumns(string tableName)
        {
            return this.connection.Query<string>(@"select col.COLUMN_NAME from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE col on col.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
where tc.TABLE_NAME = @tableName and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'", new {tableName = tableName}).ToList();
        }

        public string GetLocatorByPrimaryKey(IEnumerable<string> primaryKeyColumns)
        {
            return String.Join(" and ", primaryKeyColumns.Select(x => String.Format("{0} = @{0}", x)));            
        }

        public int CountOfRecordsWithPrimaryKey(string tableName, string primaryKeyLocator, IDictionary<string, string> record)
        {
            var parameters = record.AsDynamicParameters();

            var checkIfExists = String.Format("SELECT COUNT(1) FROM {0} WHERE {1}", tableName, primaryKeyLocator);


            return this.connection.Query<int>(checkIfExists, parameters).Single();
        }

        public void InsertRecord(string tableName, IDictionary<string, string> record)
        {
            var columns = string.Join(", ", record.Keys);
            var values = string.Join(", ", record.Keys.Select(x => "@" + x));

            var insert = string.Format("SET IDENTITY_INSERT {0} ON; INSERT INTO {0}({1}) VALUES({2})", tableName, columns, values);

            this.connection.Query<int>(insert, record.AsDynamicParameters());
        }

        public void UpdateRecord(IEnumerable<string> primaryKeyColumns, string primaryKeyLocator, IDictionary<string, string> record, string tableName)
        {
            var set = string.Join(", ", record.Keys.Except(primaryKeyColumns).Select(x => x + " = @" + x));

            var update = string.Format("UPDATE {0} SET {1} WHERE {2}", tableName, set, primaryKeyLocator);

            this.connection.Query<int>(update, record.AsDynamicParameters());
        }
    }
}
