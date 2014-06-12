using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DataSeeder
{
    static class JsonExtensions
    {
        public static IDictionary<string, string> AsDictionary(this JObject @this)
        {
            return @this.Properties().ToDictionary(x => x.Name, x => x.Value.Value<string>());
        }
    }
}
