using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using Dapper;
using Newtonsoft.Json.Linq;
using NLog;

namespace DataSeeder
{
    internal class SeedCommand
    {
        private static readonly Logger Log = LogManager.GetLogger("Seeder");

        private DbConnection connection;
        private SqlHelper sql;

        public void Execute(string provider, string file, string connectionString)
        {
            var providerFactory = DbProviderFactories.GetFactory(provider);

            var inputData = JObject.Parse(File.ReadAllText(file));

            using (connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                this.sql = new SqlHelper(connection);

                foreach (var table in inputData.Properties())
                {
                    SeedTable(table);
                }
            }
        }

        public void SeedTable(JProperty table)
        {
            Log.Info("Seeding table {0}", table.Name);

            var primaryKeyColumns = this.sql.GetPrimaryKeyColumns(table.Name);

            var primaryKeyLocator = this.sql.GetLocatorByPrimaryKey(primaryKeyColumns);

            var records = table.Value.Value<JArray>();

            foreach (JObject record in records)
            {
                var parameters = record.AsDictionary();

                var count = this.sql.CountOfRecordsWithPrimaryKey(table.Name, primaryKeyLocator, record.AsDictionary());

                if (count == 0)
                {
                    Log.Debug("Inserting new record");

                    this.sql.InsertRecord(table.Name, parameters);
                }
                else
                {
                    Log.Debug("Updating existing record");

                    this.sql.UpdateRecord(primaryKeyColumns, primaryKeyLocator, parameters, table.Name);
                }
            }
        }
    }
}