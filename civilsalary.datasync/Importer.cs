using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using civilsalary.data;

namespace civilsalary.datasync
{
    public static class Importer
    {
        public void Import(params EmployeeDataProvider[] providers)
        {
            foreach (var p in providers)
            {
                var employees = p.ToList();
                var departmentGroups = employees.GroupBy(e => e.Department);

                var departments = departmentGroups.Select(d =>
                {
                    var departmentEmployees = d.ToList();

                    return new DepartmentRow()
                    {
                        Government = departmentEmployees[0].Government,
                        Department = d.Key,

                        EmployeeCount = departmentEmployees.Count,

                        GrossPayAvg = departmentEmployees.Average(e => e.GrossPay),
                        GrossPayMin = departmentEmployees.Min(e => e.GrossPay),
                        GrossPayMax = departmentEmployees.Max(e => e.GrossPay),
                        GrossPaySum = departmentEmployees.Sum(e => e.GrossPay),
                        GrossPayMed = departmentEmployees.Median(e => e.GrossPay),

                        SalaryAvg = departmentEmployees.Average(e => e.Salary),
                        SalaryMin = departmentEmployees.Min(e => e.Salary),
                        SalaryMax = departmentEmployees.Max(e => e.Salary),
                        SalarySum = departmentEmployees.Sum(e => e.Salary),
                        SalaryMed = departmentEmployees.Median(e => e.Salary),
                    };

                }).ToList();                
            }
        }
    }
}
