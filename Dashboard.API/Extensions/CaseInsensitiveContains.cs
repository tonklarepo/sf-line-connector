using System;

namespace Dashboard.API.Extensions
{
    public class CaseInsensitiveContains
    {
        public static bool CaseInsensitiveUsingContains(string text, string value, 
            StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }
    }
}