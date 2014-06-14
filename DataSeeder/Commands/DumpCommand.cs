using System.Data.Common;
using System.IO;
using System.Linq;
using DataSeeder.Sql;
using Newtonsoft.Json.Linq;

namespace DataSeeder.Commands
{
    public class DumpCommand
    {
        private DbConnection connection;
        private SqlHelper sql;

        public void Execute(DumpArgs args)
        {
            var providerFactory = DbProviderFactories.GetFactory(args.Provider);           

            using (this.connection = providerFactory.CreateConnection())
            {
                this.connection.ConnectionString = args.ConnectionString;
                this.connection.Open();

                this.sql = new SqlHelper(this.connection);

                var tableOps = this.sql.GetTableOperations(args.TableName);

                var records = tableOps.GetRecords(args.Condition, args.Columns ?? new string[0]).ToList();

                var result = new JArray();

                foreach (var record in records)
                {
                    var obj = new JObject();

                    foreach (var property in record)
                    {
                        obj.Add(property.Key, new JValue(property.Value));
                    }

                    result.Add(obj);
                }

                var resultObject = new JObject(new JProperty(args.TableName, result));

                File.WriteAllText(args.OutputFile, resultObject.ToString());
            }
        }
    }
}