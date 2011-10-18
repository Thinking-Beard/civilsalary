using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace civilsalary.data
{
    public sealed class DepartmentRow : TableServiceEntity
    {
        public string Department { get; set; }
        public string Government { get; set; }
        public int EmployeeCount { get; set; }
        public decimal? SalarySum { get; set; }
        public decimal? SalaryMax { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryAvg { get; set; }
        public decimal? SalaryMed { get; set; }
        public decimal? GrossPaySum { get; set; }
        public decimal? GrossPayMax { get; set; }
        public decimal? GrossPayMin { get; set; }
        public decimal? GrossPayAvg { get; set; }
        public decimal? GrossPayMed { get; set; }
    }
}
