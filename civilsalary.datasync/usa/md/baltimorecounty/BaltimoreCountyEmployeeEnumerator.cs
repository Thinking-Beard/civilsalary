using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using civilsalary.data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.pdf.parser;
using System.Globalization;

namespace civilsalary.datasync.usa.md.baltimorecounty
{
    public sealed class BaltimoreCountyEmployeeEnumerator : EmployeeDataProviderEnumerator
    {
        StringReader _sr;
        EmployeeRow _current;
        
        public override EmployeeRow Current
        {
            get 
            {
                if (IsDisposed) throw new ObjectDisposedException(null);

                return _current; 
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_sr != null)
                {
                    _sr.Dispose();
                    _sr = null;
                }
            }

            base.Dispose(disposing);
        }

        public override bool MoveNext()
        {
            if (IsDisposed) throw new ObjectDisposedException(null);

            if (_sr == null)
            {
                var path = (new FileInfo("..\\..\\..\\civilsalary.datasync\\usa\\md\\baltimorecounty\\salaries110630.pdf")).FullName;
                var reader = new PdfReader(path);
                var sb = new StringBuilder();

                for (var page = 1; page <= reader.NumberOfPages; page++)
                {
                    var strategy = new TableTextExtractionStrategy();
                    sb.AppendLine(PdfTextExtractor.GetTextFromPage(reader, page, strategy));
                }

                _sr = new StringReader(sb.ToString());

                var throwAway = _sr.ReadLine();
                throwAway = _sr.ReadLine();
                throwAway = _sr.ReadLine();
            }

            var current = _sr.ReadLine();

            if (current == null) return false;

            var cols = current.Split('\t');

            _current = new EmployeeRow()
            {
                Government = "usa-md-baltimorecounty",
                Name = cols[1] + ", " + cols[0],
                Department = cols[2],
                Position = cols[3],
                Salary = double.Parse(cols[4], NumberStyles.AllowCurrencySymbol 
                    | NumberStyles.AllowDecimalPoint 
                    | NumberStyles.AllowLeadingSign 
                    | NumberStyles.AllowParentheses 
                    | NumberStyles.AllowThousands)
            };

            _current.ExternalId = _current.Department + "|" + _current.Name;

            return true;
        }
    }
}
