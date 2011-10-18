using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace civilsalary.data
{
    public sealed class EmployeeRow : TableServiceEntity
    {
        public EmployeeRow()
        {
        }

        public string Name { get; set; }

        public string Position { get; set; }

        public decimal? Salary { get; set; }

        public string ExternalId { get; set; }

        public decimal? GrossPay { get; set; }

        public DateTime? HireDate { get; set; }

        public string Department { get; set; }

        public string Government { get; set; }
    }
}
