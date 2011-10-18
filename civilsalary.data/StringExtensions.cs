using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace civilsalary.data
{
    public static class StringExtensions
    {
        public static string ToUrlValue(this string s)
        {
            return s.Replace(' ', '-').ToLowerInvariant();
        }
    }
}
