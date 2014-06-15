using System;
using PowerArgs;

namespace DataSeeder
{
    public class Args
    {
        [ArgRequired]
        [ArgPosition(0)]
        public string Action { get; set; }

        [ArgDescription("Show usage")]
        public bool Help { get; set; }

        [ArgActionMethod]
        public static void Seed(SeedArgs a)
        {
        }

        [ArgActionMethod]
        public static void Dump(DumpArgs a)
        {
        }
    }

    public class SeedArgs : ConnectionRelatedActionArgs
    {
        [ArgRequired]
        [ArgPosition(3)]
        [ArgShortcut("-f")]
        [ArgExistingFile]
        public string InputFile { get; set; }
    }

    public class DumpArgs : ConnectionRelatedActionArgs
    {
        [ArgRequired]
        [ArgPosition(3)]
        public string TableName { get; set; }

        [ArgShortcut("-w")]
        public string Condition { get; set; }

        [ArgShortcut("-s")]        
        public string[] Columns { get; set; }

        [ArgRequired]
        [ArgShortcut("-f")]
        public string OutputFile { get; set; }

        [ArgRequired]
        [DefaultValue(true)]
        public bool Append { get; set; }
    }

    public class ConnectionRelatedActionArgs
    {
        [ArgRequired]
        [ArgPosition(1)]
        [ArgShortcut("-p")]
        public string Provider { get; set; }

        [ArgRequired]
        [ArgPosition(2)]
        [ArgShortcut("-c")]
        public string ConnectionString { get; set; }
    }
}