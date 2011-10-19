using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace civilsalary.data
{
    public static class StringExtensions
    {
        public static string ToUrlValue(this string s)
        {
            var urlValue = Regex.Replace(s.ToLowerInvariant(), @"[^a-z0-9\-]", "-");

            while (urlValue.Contains("--"))
            {
                urlValue = urlValue.Replace("--", "-");
            }

            return urlValue;
        }
    }
}
