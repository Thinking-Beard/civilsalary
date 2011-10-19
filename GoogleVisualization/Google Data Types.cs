using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentJson;
using System.Web;

namespace GoogleVisualization
{
    sealed class GoogleValue : IJsonSerializable
    {
        ColumnDataType _type;
        object _value;

        public GoogleValue(ColumnDataType dataType, object value)
        {
            _type = dataType;
            _value = value;
        }

        public void BuildJson(StringBuilder sb)
        {
            if (_value == null)
            {
                sb.Append("null");
                return;
            }

            switch (_type)
            {
                case ColumnDataType.Boolean:
                    var b = _value as bool?;
                    if (b == null) throw new InvalidCastException();
                    if (b.Value)
                    {
                        sb.Append("'true'");
                    }
                    else
                    {
                        sb.Append("'false'");
                    }
                    break;
                case ColumnDataType.Number:
                    if (_value.GetType().IsPrimitive)
                    {
                        if (_value is bool || _value is char) throw new InvalidCastException();
                        sb.Append(_value);
                    }
                    break;
                case ColumnDataType.String:
                    var s = _value as string;
                    if (s == null)
                    {
                        var c = _value as char?;
                        if(c == null) throw new InvalidCastException();
                        sb.Append(HttpUtility.JavaScriptStringEncode(new string(c.Value, 1), true));
                        break;
                    }
                    sb.Append(HttpUtility.JavaScriptStringEncode(s, true));
                    break;
                //'date' - JavaScript Date object (zero-based month), with the time truncated. Example value: v:new Date(2008, 0, 15)
                case ColumnDataType.Date:
                    var d = _value as DateTime?;
                    if (d == null) throw new InvalidCastException();
                    sb.AppendFormat("new Date({0},{1},{2})", d.Value.Year, d.Value.Month - 1, d.Value.Day);
                    break;
                //'datetime' - JavaScript Date object including the time. Example value: v:new Date(2008, 0, 15, 14, 30, 45)
                case ColumnDataType.DateTime:
                    var dt = _value as DateTime?;
                    if (dt == null) throw new InvalidCastException();
                    sb.AppendFormat("new Date({0},{1},{2},{3},{4},{5})", dt.Value.Year, dt.Value.Month - 1, dt.Value.Day, dt.Value.Hour, dt.Value.Minute, dt.Value.Second);
                    break;
                //'timeofday' - Array of three numbers and an optional fourth, representing hour (0 indicates midnight), minute, second, and optional millisecond. Example values: v:[8, 15, 0], v: [6, 12, 1, 144]
                case ColumnDataType.TimeOfDay:
                    var tod = _value as DateTime?;
                    if (tod == null) throw new InvalidCastException();
                    sb.AppendFormat("[{0},{1},{2},{3}]", tod.Value.Hour, tod.Value.Minute, tod.Value.Second, tod.Value.Millisecond);
                    break;
                default:
                    throw new InvalidOperationException("DataType is not valid.");
            }
        }

        internal static ColumnDataType GetColumnDataType(Type type)
        {
            var nullable = Nullable.GetUnderlyingType(type);

            if (nullable != null) type = nullable;

            switch (type.Name)
            {
                case "String":
                case "Char":
                    return ColumnDataType.String;
                case "Int16":
                case "Int32":
                case "Int64":
                case "Double":
                case "Single":
                case "Float":
                case "Decimal":
                    return ColumnDataType.Number;
                case "DateTime":
                    return ColumnDataType.DateTime;
                default:
                    throw new NotSupportedException("Column type not supported.");
            }
        }
    }


}
