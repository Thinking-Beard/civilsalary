using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoogleVisualization
{
    public sealed class QueryResult
    {
        //TODO: errors, warnings, etc...

        internal QueryResult(GoogleDataTable table)
        {
            Table = table;
        }

        public GoogleDataTable Table { get; private set; }
    }
}
