using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace civilsalary.data
{
    public sealed class EmployeeRow : TableServiceEntity
    {
        string _government;
        string _id;
        DateTime? _hire;

        public EmployeeRow()
        {
        }

        public string Name { get; set; }
        public string Position { get; set; }
        public double? Salary { get; set; }
        public double? GrossPay { get; set; }
        public DateTime? HireDate
        {
            get
            {
                return _hire;
            }
            set
            {
                if (value != null && value.Value < SecUtility.MinSupportedDateTime) throw new ArgumentOutOfRangeException("value");

                if(value != null) Console.WriteLine(value);

                _hire = value;
            }
        }
        public string DepartmentKey { get; set; }

        public string EmployeeId
        {
            get
            {
                return _id;
            }
            set
            {
                SecUtility.CheckKey(value);

                _id = value;

                RowKey = SecUtility.EscapeKey(value);
            }
        }

        public string GovernmentKey
        {
            get
            {
                return _government;
            }
            set
            {
                SecUtility.CheckKey(value);

                _government = value;

                PartitionKey = SecUtility.EscapeKey(value);
            }
        }
    }
}
