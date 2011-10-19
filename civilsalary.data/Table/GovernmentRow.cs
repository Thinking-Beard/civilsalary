using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.WindowsAzure.StorageClient;

namespace civilsalary.data
{
    public sealed class GovernmentRow : AggregateRow, IKeyed
    {
        string _key;
        
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

                PartitionKey = SecUtility.EscapeKey(value);
                RowKey = string.Empty;
            }
        }

        public string Name { get; set; }
    }
}
