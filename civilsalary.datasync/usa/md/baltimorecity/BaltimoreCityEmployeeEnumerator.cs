﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using civilsalary.data;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace civilsalary.datasync.usa.md.baltimorecity
{
    public sealed class BaltimoreCityEmployeeEnumerator : EmployeeDataProviderEnumerator
    {
        const int DataDepth = 3;

        JsonReader _reader;
        dynamic _view;
        EmployeeRow _current;

        public override EmployeeRow Current
        {
            get { return _current; }
        }

        int ColumnIndex(string columnName)
        {
            var columns = _view.columns;

            for (var i = 0; i < columns.length; i++)
            {
                if (columns[i].fieldName == columnName) return i;
            }

            return -1;
        }

        public override bool MoveNext()
        {
            if (IsDisposed) throw new ObjectDisposedException(null);

            if (_reader == null)
            {
                var wc = new WebClient();
                //var json = wc.DownloadString("http://data.baltimorecity.gov/api/views/ijfz-2v3c/rows.json");
                //var json = wc.DownloadString("file:///C:/Users/PTying/Documents/GitHub/civilsalary/civilsalary.datasync/usa/md/baltimorecity/rows.json");

                var uri = new Uri((new FileInfo("..\\..\\..\\civilsalary.datasync\\usa\\md\\baltimorecity\\rows.json")).FullName);
                var json = wc.DownloadString(uri);

                _reader = new JsonTextReader(new StringReader(json));
                _reader.ReadToProperty("meta", 1);
                _reader.ReadToProperty("view", 3);
                _reader.Read(); //start view object...

                _view = JObject.Load(_reader).AsDynamic();

                _reader.ReadToProperty("data", 1);
                _reader.Read(); //step in to data
                
            }

            if (_reader.Read() && _reader.Depth == DataDepth)
            {
                var currentValues = JArray.Load(_reader);

                _current = new EmployeeRow()
                {
                    Government = "usa-md-baltimorecity",
                    Name = (string)currentValues[ColumnIndex("name")],
                    ExternalId = (string) currentValues[ColumnIndex("id")],
                    Department = (string) currentValues[ColumnIndex("agency")],
                    Position = (string) currentValues[ColumnIndex("jobtitle")],
                    Salary = double.Parse((string)currentValues[ColumnIndex("annualsalary")], NumberStyles.AllowCurrencySymbol 
                        | NumberStyles.AllowDecimalPoint 
                        | NumberStyles.AllowLeadingSign 
                        | NumberStyles.AllowParentheses 
                        | NumberStyles.AllowThousands),
                    GrossPay = double.Parse((string)currentValues[ColumnIndex("grosspay")], NumberStyles.AllowCurrencySymbol 
                        | NumberStyles.AllowDecimalPoint 
                        | NumberStyles.AllowLeadingSign 
                        | NumberStyles.AllowParentheses 
                        | NumberStyles.AllowThousands),
                    HireDate = DateTime.Parse((string)currentValues[ColumnIndex("hiredate")])
                };

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
