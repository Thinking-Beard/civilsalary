using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GoogleVisualization
{
    public sealed class Query : ICloneable
    {
        //TODO: this is a pretty hacked way to do the parsing, maybe use their JavaCC implementation 
        //and port to C#? maybe this is good enough for our needs? Is there an ANTLR port to .NET?

        //select	Selects which columns to return, and in what order. If omitted, all of the table's columns are returned, in their default order.
        //where	Returns only rows that match a condition. If omitted, all rows are returned.
        //group by	Aggregates values across rows.
        //pivot	Transforms distinct values in columns into new columns.
        //order by	Sorts rows by values in columns.
        //limit	Limits the number of returned rows.
        //offset	Skips a given number of first rows.
        //label	Sets column labels.
        //format	Formats the values in certain columns using given formatting patterns.
        //options	Sets additional options.

        const string select = "select";
        const string where = "where";
        const string groupBy = "group by";
        const string pivot = "pivot";
        const string orderBy = "order by";
        const string limit = "limit";
        const string offset = "offset";
        const string label = "label";
        const string format = "format";
        const string options = "options";

        static readonly string[] _clausesInOrder = new string[] { select, where, groupBy, pivot, orderBy, limit, offset, label, format, options };

        static readonly string _clausePattern = @"(?<!`){0} ";
        static readonly Regex _whereRegex = new Regex(@"(?<operand1>.+)\s+(?<operator>\<=|<|>|>=|=|!=|<>|starts with|contains|ends with|like)\s+(?<operand2>.+)", RegexOptions.Compiled);

        public static Query Parse(string query)
        {
            var q = new Query();
            var lastStart = query.Length;

            //iterate in reverse because need to know where next clause ends to pull body
            for (var i = _clausesInOrder.Length - 1; i >= 0; i--)
            {
                var clause = _clausesInOrder[i];
                var parsed = ParseClause(clause, query, 0, ref lastStart);

                switch(clause)
                {
                    case select:
                        q.Select = parsed;
                        break;
                    case where:
                        q.Where = parsed;
                        break;
                    case groupBy:
                        q.GroupBy = parsed;
                        break;
                    case pivot:
                        q.Pivot = parsed;
                        break;
                    case orderBy:
                        q.OrderBy = parsed;
                        break;
                    case limit:
                        q.Limit = ParseNullableInt32(parsed);
                        break;
                    case offset:
                        q.Offset = ParseNullableInt32(parsed);
                        break;
                    case label:
                        q.Label = parsed;
                        break;
                    case format:
                        q.Format = parsed;
                        break;
                    case options:
                        q.Options = parsed;
                        break;
                    
                }
            }

            return q;
        }

        static int? ParseNullableInt32(string value)
        {
            int parsed;

            if (!string.IsNullOrWhiteSpace(value))
            {
                if (int.TryParse(value, out parsed))
                {
                    return parsed;
                }
                else
                {
                    //TODO: add to errors/warnings collections?
                    throw new FormatException(string.Format("'{0}' is not a valid integer.", value));
                }
            }

            return null;
        }

        static string ParseClause(string clauseKeyword, string query, int start, ref int length)
        {
            var originalLength = length;
            var regex = new Regex(string.Format(_clausePattern, clauseKeyword));
            var match = regex.Match(query, start, originalLength);

            if (match != null && match.Success)
            {
                length = match.Index;
                var clauseStart = match.Index + match.Length;
                var clauseLength = originalLength - start - clauseStart;
                return query.Substring(clauseStart, clauseLength).Trim();
            }

            return null;
        }

        public QueryResult Execute(IQueryable data, object additionalProperties)
        {
            data = Apply(data);

            //TODO: status, warnings, errors, etc...

            return new QueryResult(data.ToGoogleDataTable(additionalProperties));
        }

        public IQueryable Apply(IQueryable data)
        {
            if (!string.IsNullOrWhiteSpace(Select)
                || !string.IsNullOrWhiteSpace(Label)
                || !string.IsNullOrWhiteSpace(Options)
                || !string.IsNullOrWhiteSpace(Format)
                || !string.IsNullOrWhiteSpace(Pivot)
                || !string.IsNullOrWhiteSpace(GroupBy))
            {
                throw new NotSupportedException("One or more clauses in this query are not supported.");
            }

            if (!string.IsNullOrWhiteSpace(Where))
            {
                if (Where.Contains(" and ") || Where.Contains(" or ") || Where.Contains(" not ")) throw new NotSupportedException("Multiple criteria in the where clause are not supported.");

                var match = _whereRegex.Match(Where);

                if (!match.Success)
                {
                    throw new FormatException("The 'Where' clause is in an invalid format.");
                }

                var left = match.Groups["operand1"].Value;
                var right = match.Groups["operand2"].Value;
                var op = match.Groups["operator"].Value;

                //HACK: assuming left is property and right is constant
                var propertyName = left;
                var constantValue = right.Trim().Trim('\'', '"');

                switch (op)
                {
                    case "contains":
                        data = data.WherePropertyContains(propertyName, constantValue, StringComparison.OrdinalIgnoreCase);
                        break;
                    case "like":
                    case "starts with":
                    case "ends with":
                    case "<=":
                    case "<":
                    case ">":
                    case ">=":
                    case "=":
                    case "!=":
                    case "<>":
                        throw new NotSupportedException(string.Format("The operator '{0}' is not yet supported.", op));
                }
            }

            if (!string.IsNullOrWhiteSpace(OrderBy))
            {
                if (OrderBy.Contains(",")) throw new NotSupportedException("Multiple column ordering not supported.");

                var desc = OrderBy.EndsWith("desc");
                var columnName = OrderBy.Replace(" asc", string.Empty).Replace(" desc", string.Empty).Trim(' ', '`');

                if (desc)
                {
                    data = data.OrderByDescending(columnName);
                }
                else
                {
                    data = data.OrderBy(columnName);
                }
            }


            //Offset and Limit have to be in this order...
            if (Offset != null)
            {
                data = data.Skip(Offset.Value);
            }

            if (Limit != null)
            {
                data = data.Take(Limit.Value);
            }

            return data;
        }

        private Query()
        {
        }

        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public string Where { get; set; }
        public string OrderBy { get; set; }
        public string Select { get; set; }
        public string Label { get; set; }
        public string Options { get; set; }
        public string GroupBy { get; set; }
        public string Pivot { get; set; }
        public string Format { get; set; }

        public Query Clone()
        {
            return new Query()
            {
                Limit = Limit,
                Offset = Offset,
                Where = Where,
                OrderBy = OrderBy,
                Select = Select,
                Label = Label,
                Options = Options,
                GroupBy = GroupBy,
                Pivot = Pivot,
                Format = Format
            };
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
