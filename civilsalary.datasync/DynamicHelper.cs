using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace civilsalary.datasync
{
    public static class DynamicJsonHelper
    {
        public static object GetDynamicValue(JToken value)
        {
            if (value is JValue)
                return (value as JValue).Value;
            else if (value is JArray)
                return new DynamicJsonArray(value as JArray);
            else if (value is JObject)
                return new DynamicJsonObject(value as JObject);
            else if (value is JConstructor)
            {
                var objConstructor = (value as JConstructor);
                switch (objConstructor.Name)
                {
                    case "Date":
                        var val = (objConstructor.Children().First() as JValue).Value;
                        return JsonConvert.DeserializeObject<DateTime>(objConstructor.ToString(), new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
                }
            }
            return null;
        }
    }
}
