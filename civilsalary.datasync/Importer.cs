using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using civilsalary.data;
using civilsalary.datasync.usa.md.baltimorecounty;
using civilsalary.datasync.usa.md.baltimorecity;
using System.Data.Services.Client;

namespace civilsalary.datasync
{
    public static class Importer
    {
        const string Key_US = "us";
        const string Key_US_MD = "us/md";
        const string Key_US_MD_BaltimoreCity = "us/md/baltimore-city";
        const string Key_US_MD_BaltimoreCounty = "us/md/baltimore-county";

        public static void Import(IRepository repository)
        {
            //setup governments
            //TODO: make this not hard coded, maybe reflect providers or something? or a property on provider? I don't know....
            repository.SaveGovernments(new GovernmentRow[] {
                new GovernmentRow()
                {
                    Key = Key_US,
                    Name = "United States Federal"
                },
                new GovernmentRow()
                {
                    Key = Key_US_MD,
                    Name = "Maryland State"
                },
                new GovernmentRow()
                {
                    Key = Key_US_MD_BaltimoreCity,
                    Name = "Baltimore City"
                },
                new GovernmentRow()
                {
                    Key = Key_US_MD_BaltimoreCounty,
                    Name = "Baltimore County"
                }
            });


            repository.AddParentChildGovernmentAssociation(Key_US, Key_US_MD);
            repository.AddParentChildGovernmentAssociation(Key_US_MD, Key_US_MD_BaltimoreCity);
            repository.AddParentChildGovernmentAssociation(Key_US_MD, Key_US_MD_BaltimoreCounty);

            repository.AddAdjacentGovernmentAssocation(Key_US_MD_BaltimoreCity, Key_US_MD_BaltimoreCounty);

            //var providers = new EmployeeDataProvider[] { new EmployeeDataProvider(Key_US_MD_BaltimoreCounty, typeof(BaltimoreCountyEmployeeEnumerator))
            //    , new EmployeeDataProvider(Key_US_MD_BaltimoreCity, typeof(BaltimoreCityEmployeeEnumerator)) };

            var providers = new EmployeeDataProvider[] { new EmployeeDataProvider(Key_US_MD_BaltimoreCity, typeof(BaltimoreCityEmployeeEnumerator)) };

            foreach (var p in providers)
            {
                var employeeData = p.ToList();
                var employees = employeeData.Select(d => d.Row).ToList();

                var g = repository.LoadGovernment(p.GovernmentKey);

                g.FillAggregates(employees);

                repository.SaveGovernments(g);

                var departmentGroups = employeeData.GroupBy(e => e.DepartmentKey);

                var departments = departmentGroups.Select(dept =>
                {
                    var departmentEmployeeData = dept.ToList();

                    var department = new DepartmentRow()
                    {
                        GovernmentKey = p.GovernmentKey,
                        Key = dept.Key,
                        Name = departmentEmployeeData[0].DepartmentName
                    };

                    var departmentEmployees = departmentEmployeeData.Select(data => data.Row).ToList();

                    department.FillAggregates(departmentEmployees);

                    foreach (var employee in departmentEmployees)
                    {
                        employee.DepartmentKey = department.Key;
                    }

                    return department;
                }).ToList();

                repository.SaveDepartments(departments);

                repository.SaveEmployees(employees);
            }
        }
    }
}
