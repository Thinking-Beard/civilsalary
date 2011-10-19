using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentJson;

namespace GoogleVisualization
{
    //http://code.google.com/apis/chart/interactive/docs/reference.html#dataparam


//{
//  cols: [{id: 'A', label: 'NEW A', type: 'string'},
//         {id: 'B', label: 'B-label', type: 'number'},
//         {id: 'C', label: 'C-label', type: 'date'}
//        ],
//  rows: [{c:[{v: 'a'}, {v: 1.0, f: 'One'}, {v: new Date(2008, 1, 28, 0, 31, 26), f: '2/28/08 12:31 AM'}]},
//         {c:[{v: 'b'}, {v: 2.0, f: 'Two'}, {v: new Date(2008, 2, 30, 0, 31, 26), f: '3/30/08 12:31 AM'}]},
//         {c:[{v: 'c'}, {v: 3.0, f: 'Three'}, {v: new Date(2008, 3, 30, 0, 31, 26), f: '4/30/08 12:31 AM'}]}
//        ],
//  p: {foo: 'hello', bar: 'world!'}
//}

//'boolean' - JavaScript boolean value ('true' or 'false'). Example value: v:'true'
//'number' - JavaScript number value. Example values: v:7 , v:3.14, v:-55
//'string' - JavaScript string value. Example value: v:'hello'
//'date' - JavaScript Date object (zero-based month), with the time truncated. Example value: v:new Date(2008, 0, 15)
//'datetime' - JavaScript Date object including the time. Example value: v:new Date(2008, 0, 15, 14, 30, 45)
//'timeofday' - Array of three numbers and an optional fourth, representing hour (0 indicates midnight), minute, second, and optional millisecond. Example values: v:[8, 15, 0], v: [6, 12, 1, 144]

    public enum ColumnDataType
    {
        Boolean,
        Number,
        String,
        Date,
        DateTime,
        TimeOfDay
    }

    public abstract class GoogleObject
    {
        public GoogleObject()
        {
            Custom = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Custom { get; private set; }

        public string ToJson()
        {
            return ToJsonObject().ToJson();
        }

        internal JsonObject ToJsonObject()
        {
            var json = JsonObject.Create();

            if (Custom != null && Custom.Count > 0)
            {
                json.AddProperty("p", Custom);
            }

            SetJsonProperties(json);

            return json;
        }

        protected abstract void SetJsonProperties(JsonObject json);
    }

    public sealed class GoogleDataTable : GoogleObject
    {
        public GoogleDataTable()
        {
            Columns = new List<GoogleDataColumn>();
            Rows = new List<GoogleDataRow>();
        }

        public ICollection<GoogleDataColumn> Columns { get; private set; }
        public ICollection<GoogleDataRow> Rows { get; private set; }

        protected override void SetJsonProperties(JsonObject json)
        {
            json.AddProperty("cols", Columns.Select(c => c.ToJsonObject()).ToList());
            json.AddProperty("rows", Rows.Select(r => r.ToJsonObject()).ToList());
        }
    }

    public sealed class GoogleDataColumn : GoogleObject
    {
        public GoogleDataColumn()
        {
            DataType = ColumnDataType.String;
        }

        public string Id { get; set; }
        public string Label { get; set; }
        public ColumnDataType DataType { get; set; }
        public string Pattern { get; set; }

        protected override void SetJsonProperties(JsonObject json)
        {
            string type;

            switch(DataType)
            {
                case ColumnDataType.Boolean:
                    type = "boolean";
                    break;
                case ColumnDataType.Date:
                    type = "date";
                    break;
                case ColumnDataType.DateTime:
                    type = "datetime";
                    break;
                case ColumnDataType.Number:
                    type = "number";
                    break;
                case ColumnDataType.String:
                    type = "string";
                    break;
                case ColumnDataType.TimeOfDay:
                    type = "timeofday";
                    break;
                default:
                    throw new InvalidOperationException("DataType is not valid.");
            }

            json.AddProperty("type", type);
            if (!string.IsNullOrWhiteSpace(Id)) json.AddProperty("id", Id);
            if (!string.IsNullOrWhiteSpace(Label)) json.AddProperty("label", Label);
            if (!string.IsNullOrWhiteSpace(Pattern)) json.AddProperty("pattern", Pattern);
        }
    }

    public sealed class GoogleDataRow : GoogleObject
    {
        public GoogleDataRow()
        {
            Cells = new List<GoogleDataCell>();
        }

        public ICollection<GoogleDataCell> Cells { get; private set; }

        protected override void SetJsonProperties(JsonObject json)
        {
            json.AddProperty("c", Cells.Select(c => c.ToJsonObject()));
        }
    }

    public sealed class GoogleDataCell : GoogleObject
    {
        public ColumnDataType DataType { get; set; }
        public object Value { get; set; }
        public string Formatted { get; set; }

        protected override void SetJsonProperties(JsonObject json)
        {
            json.AddProperty("v", new GoogleValue(DataType, Value));
            if (Formatted != null) json.AddProperty("f", Formatted);
        }
    }
}
