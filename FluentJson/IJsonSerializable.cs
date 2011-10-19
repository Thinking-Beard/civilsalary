using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentJson
{
    public interface IJsonSerializable
    {
        void BuildJson(StringBuilder sb);
    }
}
