using System.Collections.Generic;
using Dapper;

static internal class DictionaryExtensions
{
    public static DynamicParameters AsDynamicParameters(this IEnumerable<KeyValuePair<string, string>> record)
    {
        var parameters = new DynamicParameters();

        foreach (var parameter in record)
        {
            parameters.Add(parameter.Key, parameter.Value);
        }
        return parameters;
    }
}