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
