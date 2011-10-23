using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq.Expressions;

namespace GoogleVisualization
{
    public static class GoogleVisualizationExtensions
    {
        //TODO: move this some place more generic?
        public static IDictionary<string, string> SplitDictionary(this string s, string itemSeparator, string keyValueSeparator)
        {
            var dic = new Dictionary<string, string>();

            var items = s.Split(new string[] { itemSeparator} , StringSplitOptions.RemoveEmptyEntries);

            foreach (var i in items)
            {
                var pair = i.Split(new string[] { keyValueSeparator }, 2, StringSplitOptions.None);

                dic[pair[0]] = pair[1];
            }

            return dic;
        }

        public static int GetColumnIndex<T>(Expression<Func<T,object>> columnExpression)
        {
            return GetColumnsIndicies(typeof(T), ExpressionToMemberName(columnExpression)).Single();
        }

        private static string ExpressionToMemberName<T>(Expression<Func<T, object>> expression)
        {
            var unary = expression.Body as UnaryExpression;
            var member = expression.Body as MemberExpression;

            if (unary != null) member = (MemberExpression)unary.Operand;

            return member.Member.Name;
        }

        public static ICollection<int> GetColumnsIndicies<T>(params Expression<Func<T,object>>[] columnExpressions)
        {
            return GetColumnsIndicies(typeof(T), columnExpressions.Select(e => ExpressionToMemberName(e)).ToArray());
        }

        static ICollection<int> GetColumnsIndicies(Type type, params string[] columnNames)
        {
            var viewData = GetViewDataDictionaryForType(type);
            var metadata = ModelMetadata.FromStringExpression(string.Empty, viewData);

            return (from p in metadata.Properties.Select((property, index) => new { property, index })
                    join n in columnNames.Select((name, index) => new { name, index }) on p.property.PropertyName equals n.name
                    orderby n.index
                    select p.index).ToList();   
        }

        static ViewDataDictionary GetViewDataDictionaryForType(Type t)
        {
            var viewDataType = typeof(ViewDataDictionary<>).MakeGenericType(t);
            return (ViewDataDictionary)Activator.CreateInstance(viewDataType);
        }

        public static GoogleDataTable ToGoogleDataTable(this IQueryable query)
        {
            return ToGoogleDataTable(query, null);
        }

        public static GoogleDataTable ToGoogleDataTable<T>(this IEnumerable<T> data, object additionalTableProperties)
        {
            return ToGoogleDataTable(data, typeof(T), additionalTableProperties);
        }

        public static GoogleDataTable ToGoogleDataTable(this IQueryable query, object additionalTableProperties)
        {
            return ToGoogleDataTable(query, query.ElementType, additionalTableProperties);
        }

        public static GoogleDataTable ToGoogleDataTable(this IEnumerable data, Type elementType, object additionalTableProperties)
        {
            var t = new GoogleDataTable();

            if (additionalTableProperties != null)
            {
                var rv = new RouteValueDictionary(additionalTableProperties);

                t.Custom.Merge(rv);
            }

            var viewData = GetViewDataDictionaryForType(elementType);
            var metadata = ModelMetadata.FromStringExpression(string.Empty, viewData);

            foreach (var p in metadata.Properties)
            {
                t.Columns.Add(new GoogleDataColumn()
                {
                    DataType = GoogleValue.GetColumnDataType(p.ModelType),
                    Id = p.PropertyName,
                    Label = p.GetDisplayName()
                });
            }

            foreach (var item in data)
            {
                var row = new GoogleDataRow();
                t.Rows.Add(row);

                viewData.Model = item;

                foreach (var p in metadata.Properties)
                {
                    var cell = new GoogleDataCell();

                    cell.DataType = GoogleValue.GetColumnDataType(p.ModelType);
                    cell.Value = viewData.Eval(p.PropertyName);

                    if (cell.Value == null)
                    {
                        cell.Formatted = p.NullDisplayText;
                    }
                    else if (!string.IsNullOrWhiteSpace(p.DisplayFormatString))
                    {
                        cell.Formatted = viewData.Eval(p.PropertyName, p.DisplayFormatString);
                    }

                    row.Cells.Add(cell);
                }
            }

            return t;
        }
    }

}
