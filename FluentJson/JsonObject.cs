using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Web;

namespace FluentJson
{
    public sealed class JsonObject : IHtmlString, IJsonSerializable
    {
        Dictionary<string, object> _properties = new Dictionary<string,object>();
        
        private JsonObject()
        {
        }

        public JsonObject AddProperty(string name, params Action<JsonObject, int>[] childBuilders)
        {
            var children = new List<JsonObject>();

            for (var i = 0; i < childBuilders.Length; i++)
            {
                var childBuilder = childBuilders[i];
                var json = JsonObject.Create();
                childBuilder(json, i);
                children.Add(json);
            }

            _properties.Add(name, children);

            return this;
        }

        public JsonObject AddProperty(string name, Action<JsonObject> childBuilder)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (childBuilder == null) throw new ArgumentNullException("childBuilder");

            var json = JsonObject.Create();
            childBuilder(json);
            _properties.Add(name, json);
            return this;
        }

        public JsonObject AddProperty(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            _properties.Add(name, value);
            return this;
        }

        public static JsonObject Create()
        {
            return new JsonObject();
        }

        public string ToJson()
        {
            return new CustomJsonSerializer().Serialize(_properties);
        }

        string IHtmlString.ToHtmlString()
        {
            return ToJson();
        }

        public override string ToString()
        {
            return ToJson();
        }

        void IJsonSerializable.BuildJson(StringBuilder sb)
        {
            new CustomJsonSerializer().Serialize(_properties, sb);
        }
    }
}
