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
        EmployeeData _current;
        string _government;

        public BaltimoreCountyEmployeeEnumerator(string government)
        {
            _government = government;
        }

        public override EmployeeData Current
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

            _current = new EmployeeData()
            {
                DepartmentName = cols[2].Trim(),
                Row = new EmployeeRow()
                {
                    GovernmentKey = _government,
                    Name = (cols[1] + ", " + cols[0]).Trim().Trim(','),
                    Position = cols[3].Trim(),
                    Salary = double.Parse(cols[4], NumberStyles.AllowCurrencySymbol
                        | NumberStyles.AllowDecimalPoint
                        | NumberStyles.AllowLeadingSign
                        | NumberStyles.AllowParentheses
                        | NumberStyles.AllowThousands)
                }
            };

            _current.Row.EmployeeId = _current.DepartmentName + "|" + _current.Row.Name;

            return true;
        }
    }
}
