using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentJson;
using System.Web;
using Newtonsoft.Json;

namespace GoogleVisualization
{
    [JsonConverter(typeof(GoogleValue.Converter))]
    sealed class GoogleValue
    {
        class Converter : JsonConverter<GoogleValue>
        {
            public override void WriteJson(JsonWriter writer, GoogleValue value, JsonSerializer serializer)
            {
                if (value._value == null)
                {
                    writer.WriteValue((string)null);
                    return;
                }

                switch (value._type)
                {
                    case ColumnDataType.Boolean:
                        var b = value._value as bool?;
                        if (b == null) throw new InvalidCastException();
                        if (b.Value)
                        {
                            writer.WriteValue("true");
                        }
                        else
                        {
                            writer.WriteValue("false");
                        }
                        break;
                    case ColumnDataType.Number:
                        if (value._value.GetType().IsPrimitive)
                        {
                            if (value._value is bool || value._value is char) throw new InvalidCastException();
                            writer.WriteValue(value._value);
                        }
                        break;
                    case ColumnDataType.String:
                        var s = value._value as string;
                        if (s == null)
                        {
                            var c = value._value as char?;
                            if (c == null) throw new InvalidCastException();
                            writer.WriteValue(new string(c.Value, 1));
                            break;
                        }
                        writer.WriteValue(s);
                        break;
                    //'date' - JavaScript Date object (zero-based month), with the time truncated. Example value: v:new Date(2008, 0, 15)
                    case ColumnDataType.Date:
                        var d = value._value as DateTime?;
                        if (d == null) throw new InvalidCastException();
                        writer.WriteStartConstructor("Date");
                        writer.WriteValue(d.Value.Year);
                        writer.WriteValue(d.Value.Month - 1);
                        writer.WriteValue(d.Value.Day);
                        writer.WriteEndConstructor();
                        break;
                    //'datetime' - JavaScript Date object including the time. Example value: v:new Date(2008, 0, 15, 14, 30, 45)
                    case ColumnDataType.DateTime:
                        var dt = value._value as DateTime?;
                        if (dt == null) throw new InvalidCastException();
                        writer.WriteStartConstructor("Date");
                        writer.WriteValue(dt.Value.Year);
                        writer.WriteValue(dt.Value.Month - 1);
                        writer.WriteValue(dt.Value.Day);
                        writer.WriteValue(dt.Value.Hour);
                        writer.WriteValue(dt.Value.Minute);
                        writer.WriteValue(dt.Value.Second);
                        writer.WriteEndConstructor();
                        break;
                    //'timeofday' - Array of three numbers and an optional fourth, representing hour (0 indicates midnight), minute, second, and optional millisecond. Example values: v:[8, 15, 0], v: [6, 12, 1, 144]
                    case ColumnDataType.TimeOfDay:
                        var tod = value._value as DateTime?;
                        if (tod == null) throw new InvalidCastException();
                        serializer.Serialize(writer, new int[] { tod.Value.Hour, tod.Value.Minute, tod.Value.Second, tod.Value.Millisecond });
                        break;
                    default:
                        throw new InvalidOperationException("DataType is not valid.");
                }
            }

            public override GoogleValue ReadJson(JsonReader reader, GoogleValue existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        ColumnDataType _type;
        object _value;

        public GoogleValue(ColumnDataType dataType, object value)
        {
            _type = dataType;
            _value = value;
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
