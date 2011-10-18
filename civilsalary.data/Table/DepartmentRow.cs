﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace civilsalary.data
{
    public sealed class DepartmentRow : AggregateRow, IKeyed
    {
        public string Key { get; set; }
        public string Name { get; set; }
        
        public string GovernmentKey { get; set; }
    }
}
