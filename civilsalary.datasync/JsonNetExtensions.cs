using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace civilsalary.datasync
{
    public static class JsonNetExtensions
    {
        public static dynamic AsDynamic(this JObject target)
        {
            return new DynamicJsonObject(target);
        }

        public static void ReadToProperty(this JsonReader json, string propertyName, int depth)
        {
            while (json.TokenType != JsonToken.PropertyName || json.Depth != depth || !string.Equals(propertyName, (string)json.Value, StringComparison.Ordinal))
            {
                if (!json.Read()) return; //return if at the end...
            }
        }
    }
}
