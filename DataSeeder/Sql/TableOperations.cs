using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Dapper;
using Newtonsoft.Json.Linq;

namespace DataSeeder.Sql
{
    public class TableOperations
    {
        private readonly DbConnection connection;

        public string TableName { get; private set; }

        public ColumnInfo[] ColumnInfos { get; private set; }

        public string[] PrimaryKeyColumns { get; private set; }

        public string PrimaryKeyLocator { get; private set; }

        public TableOperations(DbConnection connection, string tableName, ColumnInfo[] columnInfos, string[] primaryKeyColumns)
        {
            this.TableName = tableName;
            this.ColumnInfos = columnInfos;
            this.PrimaryKeyColumns = primaryKeyColumns;
            this.connection = connection;

            this.PrimaryKeyLocator = this.GetLocatorByPrimaryKey();
        }

        public string GetLocatorByPrimaryKey()
        {
            return string.Join(" and ", this.PrimaryKeyColumns.Select(x => string.Format((string) "{0} = @{0}", (object) x)));
        }

        public int CountOfRecordsWithPrimaryKey(IDictionary<string, string> record)
        {
            var parameters = record.AsDynamicParameters();

            var checkIfExists = string.Format("SELECT COUNT(1) FROM {0} WHERE {1}", this.TableName, this.PrimaryKeyLocator);

            return this.connection.Query<int>(checkIfExists, parameters).Single();
        }

        public void InsertRecord( IDictionary<string, string> record)
        {
            var columns = string.Join(", ", record.Keys);
            var values = string.Join(", ", record.Keys.Select(x => "@" + x));

            var insert = string.Format("SET IDENTITY_INSERT {0} ON; INSERT INTO {0}({1}) VALUES({2})", this.TableName, columns, values);

            this.connection.Query<int>(insert, record.AsDynamicParameters());
        }

        public void UpdateRecord(IDictionary<string, string> record)
        {
            var set = string.Join(", ", record.Keys.Except(this.PrimaryKeyColumns).Select(x => x + " = @" + x));

            var update = string.Format("UPDATE {0} SET {1} WHERE {2}", this.TableName, set, this.PrimaryKeyLocator);

            this.connection.Query<int>(update, record.AsDynamicParameters());
        }

        public IEnumerable<IDictionary<string, object>> GetRecords(string condition, string[] columns)
        {
            var columnExpression = columns.Length == 0 ? "*" : string.Join(", ", columns);

            var sql = "SELECT " + columnExpression + " FROM " + this.TableName;

            if (!string.IsNullOrWhiteSpace(condition))
            {
                sql += " WHERE " + condition;
            }

            var records = this.connection.Query(sql);

            return records.Select(x => (IDictionary<string, object>) x);
        }

        public bool IsPrimaryKeyEqual(IDictionary<string, string> a, IDictionary<string, string> b)
        {
            return this.PrimaryKeyColumns.All(x => object.Equals(a[x], b[x]));
        }
    }
}