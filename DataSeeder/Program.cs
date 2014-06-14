using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DataSeeder.Commands;
using Newtonsoft.Json.Linq;
using NLog;
using PowerArgs;

namespace DataSeeder
{
    class Program
    {
        static void Main(string[] cmdArgs)
        {            
            try
            {                
                var args = PowerArgs.Args.ParseAction<Args>(cmdArgs);

                if (args.Args.Help)
                {
                    PowerArgs.ArgUsage.GetStyledUsage<Args>().Write();
                }
                else
                {

                    var commandTypeName = string.Format("{0}.Commands.{1}Command", typeof (Program).Namespace, args.Args.Action.ToCamelCase());

                    var command = Activator.CreateInstance(Type.GetType(commandTypeName));

                    ((dynamic) command).Execute((dynamic) args.ActionArgs);                    
                }
            }
            catch (ArgException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                
                PowerArgs.ArgUsage.GetStyledUsage<Args>().Write();
            }
        }
    }
}
