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
        public string Key { get; set; }
        public string Name { get; set; }
    }
}
