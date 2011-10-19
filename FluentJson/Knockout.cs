using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace FluentJson
{
    public static class Knockout
    {
        public static JsonObject AddObservableArray<T>(this JsonObject json, string name, IEnumerable<T> array)
        {
            return AddObservableArray(json, name, array.Cast<object>());
        }

        public static JsonObject AddObservableArray(this JsonObject json, string name, IEnumerable<object> array)
        {
            if (json == null) throw new ArgumentNullException("json");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (array == null) throw new ArgumentNullException("array");

            json.AddProperty(name, new KnockoutObservableArray(array));

            return json;
        }

        public static JsonObject AddObservable(this JsonObject json, string name, object value)
        {
            if (json == null) throw new ArgumentNullException("json");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            json.AddProperty(name, new KnockoutObservable(value));

            return json;
        }

        class KnockoutObservableArray : IJsonSerializable
        {
            readonly IEnumerable<object> _array;

            public KnockoutObservableArray(IEnumerable<object> array)
            {
                _array = array;
            }

            public void BuildJson(StringBuilder sb)
            {
                var serializer = new CustomJsonSerializer();
                sb.Append("ko.observableArray(");
                serializer.Serialize(_array, sb);
                sb.Append(")");
            }
        }

        class KnockoutObservable : IJsonSerializable
        {
            readonly object _value;

            public KnockoutObservable(object value) { _value = value; }

            public void BuildJson(StringBuilder sb)
            {
                var serializer = new CustomJsonSerializer();
                sb.Append("ko.observable(");
                serializer.Serialize(_value, sb);
                sb.Append(")");
            }
        }
    }
}
