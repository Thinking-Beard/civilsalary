using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace civilsalary.datasync
{
    public class DynamicJsonObject : DynamicObject
    {
        JObject o;

        public DynamicJsonObject(JObject o)
        {
            this.o = o;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return from child in o.Children().AsJEnumerable()
                   where child.Type == JTokenType.Property
                   select (child as JProperty).Name;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            JToken value;
            if (o.TryGetValue(binder.Name, out value))
            {
                result = DynamicJsonHelper.GetDynamicValue(value);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            o[binder.Name] = JToken.FromObject(value);
            return true;
        }
    }
}
