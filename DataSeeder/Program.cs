using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json.Linq;
using NLog;

namespace DataSeeder
{
    class Program
    {
        private static DbConnection connection;
        private static readonly Logger Log = LogManager.GetLogger("Seeder");

        static void Main(string[] args)
        {
            var connectionString = @"Data Source=.\SQLEXPRESS;Initial catalog=seeder;trusted_connection=yes";
            var provider = "System.Data.SqlClient";
            var file = @"..\..\sample1.json";

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

        private static void SeedTable(JProperty table)
        {
            Log.Info("Seeding table {0}", table.Name);

            var primaryKeyColumns = connection.Query<string>(@"select col.COLUMN_NAME from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE col on col.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
where tc.TABLE_NAME = @tableName and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'", new {tableName = table.Name}).ToList();

            var primaryKeyLocator = GetLocatorByPrimaryKey(primaryKeyColumns);

            var records = table.Value.Value<JArray>();            

            foreach (JObject record in records)
            {
                var checkIfExists = string.Format("SELECT COUNT(1) FROM {0} WHERE {1}", table.Name, primaryKeyLocator);

                var parameters = BuildParametersObject(record);

                var count = ((IEnumerable<int>) SqlMapper.Query<int>(connection, checkIfExists, parameters)).Single();

                if (count == 0)
                {
                    Log.Debug("Inserting new record");

                    var columns = string.Join(", ", record.Properties().Select(x => x.Name));
                    var values = string.Join(", ", record.Properties().Select(x => "@" + x.Name));

                    var insert = string.Format("SET IDENTITY_INSERT {0} ON; INSERT INTO {0}({1}) VALUES({2})", table.Name, columns, values);

                    SqlMapper.Query<int>(connection, insert, parameters);
                }
                else
                {
                    Log.Debug("Updating existing record");

                    var set = string.Join(", ", record.Properties().Select(x=>x.Name).Except(primaryKeyColumns).Select(x => x + " = @" + x));

                    var update = string.Format("UPDATE {0} SET {1} WHERE {2}", table.Name, set, primaryKeyLocator);

                    SqlMapper.Query<int>(connection, update, parameters);
                }
            }
        }

        private static dynamic BuildParametersObject(JObject record)
        {
            var parameters = new ExpandoObject();

            foreach (var property in record.Properties())
            {
                ((IDictionary<string, object>) parameters)[property.Name] = property.Value.Value<string>();
            }

            return parameters;
        }

        private static string GetLocatorByPrimaryKey(IEnumerable<string> primaryKeyColumns)
        {
            return string.Join(" and ", primaryKeyColumns.Select(x => string.Format("{0} = @{0}", x)));
            ;
        }
    }
}
