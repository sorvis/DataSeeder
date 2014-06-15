using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using DataSeeder.Sql;
using Newtonsoft.Json.Linq;
using NLog;

namespace DataSeeder.Commands
{
    public class DumpCommand
    {
        private static readonly Logger Log = LogManager.GetLogger("Dump");

        private DbConnection connection;
        private SqlHelper sql;
        private TableOperations tableOps;

        public void Execute(DumpArgs args)
        {
            var providerFactory = DbProviderFactories.GetFactory(args.Provider);

            using (this.connection = providerFactory.CreateConnection())
            {
                this.connection.ConnectionString = args.ConnectionString;
                this.connection.Open();

                Log.Info("Dumping table {0}", args.TableName);

                this.sql = new SqlHelper(this.connection);

                this.tableOps = this.sql.GetTableOperations(args.TableName);              

                var records = this.tableOps.GetRecords(args.Condition, args.Columns ?? new string[0]).ToList();

                JObject outputFile;
                JArray result;

                if (args.Append && File.Exists(args.OutputFile))
                {
                    outputFile = JObject.Parse(File.ReadAllText(args.OutputFile));

                    var temp = outputFile.Property(args.TableName);

                    if (temp == null)
                    {
                        result = new JArray();

                        outputFile.Add(new JProperty(args.TableName, result));
                    }
                    else
                    {
                        result = (JArray) temp.Value;
                    }
                }
                else
                {
                    outputFile =  new JObject();
                    result = new JArray();

                    outputFile.Add(args.TableName, result);
                }
                
                foreach (var record in records)
                {
                    JObject outputEntry;

                    outputEntry = new JObject();

                    foreach (var property in record)
                    {
                        outputEntry.Add(property.Key, new JValue(property.Value));
                    }

                    Merge(result, outputEntry);
                }               

                File.WriteAllText(args.OutputFile, outputFile.ToString());
            }
        }

        private void Merge(JArray result, JObject outputEntry)
        {
            var existing =  result.FirstOrDefault(x => this.tableOps.IsPrimaryKeyEqual(outputEntry.AsDictionary(), ((JObject) x).AsDictionary()));

            if (existing != null)
            {
                existing.Replace(outputEntry);
            }
            else
            {
                result.Add(outputEntry);
            }
        }
    }
}