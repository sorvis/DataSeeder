using System.Data.Common;
using System.IO;
using DataSeeder.Sql;
using Newtonsoft.Json.Linq;
using NLog;

namespace DataSeeder.Commands
{
    internal class SeedCommand
    {
        private static readonly Logger Log = LogManager.GetLogger("Seeder");

        private DbConnection connection;
        private SqlHelper sql;

        public void Execute(SeedArgs args)
        {
            var providerFactory = DbProviderFactories.GetFactory(args.Provider);

            var inputData = JObject.Parse(File.ReadAllText(args.InputFile));

            using (this.connection = providerFactory.CreateConnection())
            {
                this.connection.ConnectionString = args.ConnectionString;
                this.connection.Open();

                this.sql = new SqlHelper(this.connection);

                foreach (var table in inputData.Properties())
                {
                    SeedTable(table);
                }
            }
        }

        public void SeedTable(JProperty table)
        {
            Log.Info("Seeding table {0}", table.Name);

            var tableOps = this.sql.GetTableOperations(table.Name);          

            var records = table.Value.Value<JArray>();

            foreach (JObject record in records)
            {
                var parameters = record.AsDictionary();

                var count = tableOps.CountOfRecordsWithPrimaryKey(parameters);

                if (count == 0)
                {
                    Log.Debug("Inserting new record");

                    tableOps.InsertRecord(parameters);
                }
                else
                {
                    Log.Debug("Updating existing record");

                    tableOps.UpdateRecord(parameters);
                }
            }
        }
    }
}