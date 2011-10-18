using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace civilsalary.data
{
    public static class RequestCache
    {
        public static T LoadValue<T>(object key)
        {
            return LoadValue<T>(new HttpContextWrapper(HttpContext.Current), key);
        }

        public static void SaveValue(object key, object value)
        {
            SaveValue(new HttpContextWrapper(HttpContext.Current), key, value);
        }

        public static T LoadValue<T>(this HttpContextBase context, object key)
        {
            //check context cache
            if (context != null && context.Items.Contains(key))
            {
                return (T)context.Items[key];
            }

            return default(T);
        }

        public static void SaveValue(this HttpContextBase context, object key, object value)
        {
            if (context != null)
            {
                context.Items[key] = value;
            }
        }
    }
}
