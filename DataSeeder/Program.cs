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
        static void Main(string[] args)
        {
            var connectionString = @"Data Source=.\SQLEXPRESS;Initial catalog=seeder;trusted_connection=yes";
            var provider = "System.Data.SqlClient";
            var file = @"..\..\sample1.json";

            var command = new SeedCommand();

            command.Execute(provider, file, connectionString);
        }
    }
}
