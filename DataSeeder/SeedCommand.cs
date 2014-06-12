using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using Dapper;
using Newtonsoft.Json.Linq;
using NLog;

namespace DataSeeder
{
    internal class SeedCommand
    {
        private static DbConnection connection;
        private static readonly Logger Log = LogManager.GetLogger("Seeder");

        public void Execute(string provider, string file, string connectionString)
        {
            var providerFactory = DbProviderFactories.GetFactory(provider);

            var inputData = JObject.Parse(File.ReadAllText(file));

            using (connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                foreach (var table in inputData.Properties())
                {
                    SeedTable(table);
                }
            }
        }

        public void SeedTable(JProperty table)
        {
            Log.Info("Seeding table {0}", table.Name);

            var primaryKeyColumns = connection.Query<string>(@"select col.COLUMN_NAME from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE col on col.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
where tc.TABLE_NAME = @tableName and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'", new { tableName = table.Name }).ToList();

            var primaryKeyLocator = GetLocatorByPrimaryKey(primaryKeyColumns);

            var records = table.Value.Value<JArray>();

            foreach (JObject record in records)
            {
                var checkIfExists = String.Format("SELECT COUNT(1) FROM {0} WHERE {1}", table.Name, primaryKeyLocator);

                var parameters = BuildParametersObject(record);

                var count = ((IEnumerable<int>)SqlMapper.Query<int>(connection, checkIfExists, parameters)).Single();

                if (count == 0)
                {
                    Log.Debug("Inserting new record");

                    var columns = String.Join(", ", record.Properties().Select(x => x.Name));
                    var values = String.Join(", ", record.Properties().Select(x => "@" + x.Name));

                    var insert = String.Format("SET IDENTITY_INSERT {0} ON; INSERT INTO {0}({1}) VALUES({2})", table.Name, columns, values);

                    SqlMapper.Query<int>(connection, insert, parameters);
                }
                else
                {
                    Log.Debug("Updating existing record");

                    var set = String.Join(", ", record.Properties().Select(x => x.Name).Except(primaryKeyColumns).Select(x => x + " = @" + x));

                    var update = String.Format("UPDATE {0} SET {1} WHERE {2}", table.Name, set, primaryKeyLocator);

                    SqlMapper.Query<int>(connection, update, parameters);
                }
            }
        }

        public dynamic BuildParametersObject(JObject record)
        {
            var parameters = new ExpandoObject();

            foreach (var property in record.Properties())
            {
                ((IDictionary<string, object>)parameters)[property.Name] = property.Value.Value<string>();
            }

            return parameters;
        }

        public string GetLocatorByPrimaryKey(IEnumerable<string> primaryKeyColumns)
        {
            return String.Join(" and ", primaryKeyColumns.Select(x => String.Format("{0} = @{0}", x)));            
        }
    }
}