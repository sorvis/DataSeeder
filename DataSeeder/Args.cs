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
    }

    public class SeedArgs
    {
        [ArgRequired]
        [ArgPosition(1)]
        [ArgShortcut("-p")]
        public string Provider { get; set; }

        [ArgRequired]
        [ArgPosition(2)]
        [ArgShortcut("-c")]
        public string ConnectionString { get; set; }

        [ArgRequired]
        [ArgPosition(3)]
        [ArgShortcut("-f")]
        [ArgExistingFile]
        public string InputFile { get; set; }
    }
}