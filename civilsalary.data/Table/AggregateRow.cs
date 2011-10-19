using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace civilsalary.data
{
    public abstract class AggregateRow : TableServiceEntity
    {
        public int EmployeeCount { get; set; }
        public double? SalarySum { get; set; }
        public double? SalaryMax { get; set; }
        public double? SalaryMin { get; set; }
        public double? SalaryAvg { get; set; }
        public double? SalaryMed { get; set; }
        public double? GrossPaySum { get; set; }
        public double? GrossPayMax { get; set; }
        public double? GrossPayMin { get; set; }
        public double? GrossPayAvg { get; set; }
        public double? GrossPayMed { get; set; }
    }

    public static class AggregateRowExtensions
    {
        public static void FillAggregates(this AggregateRow row, ICollection<EmployeeRow> employees)
        {
            row.EmployeeCount = employees.Count;

            row.GrossPayAvg = employees.Average(e => e.GrossPay);
            row.GrossPayMin = employees.Min(e => e.GrossPay);
            row.GrossPayMax = employees.Max(e => e.GrossPay);
            row.GrossPaySum = employees.Sum(e => e.GrossPay);
            row.GrossPayMed = employees.Median(e => e.GrossPay);

            row.SalaryAvg = employees.Average(e => e.Salary);
            row.SalaryMin = employees.Min(e => e.Salary);
            row.SalaryMax = employees.Max(e => e.Salary);
            row.SalarySum = employees.Sum(e => e.Salary);
            row.SalaryMed = employees.Median(e => e.Salary);
        }
    }
}
