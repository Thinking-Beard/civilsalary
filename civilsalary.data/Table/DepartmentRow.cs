using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace civilsalary.data
{
    public sealed class DepartmentRow : AggregateRow, IKeyed
    {
        string _key;
        string _government;

        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                SecUtility.CheckKey(value);

                _key = value;

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
        
        public string Name { get; set; }
    }
}
