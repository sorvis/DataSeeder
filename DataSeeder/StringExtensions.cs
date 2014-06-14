namespace DataSeeder
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string s)
        {
            return s.Substring(0, 1) + s.Substring(1);
        }
    }
}