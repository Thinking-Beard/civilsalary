using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using civilsalary.data;
using civilsalary.datasync.usa.md.baltimorecounty;
using civilsalary.datasync.usa.md.baltimorecity;

namespace civilsalary.datasync
{
    public static class Importer
    {
        public static void Import(IRepository repository)
        {
            //setup governments
            //TODO: make this not hard coded, maybe reflect providers or something? or a property on provider? I don't know....
            repository.SaveGovernments(new GovernmentRow[] {
                new GovernmentRow()
                {
                    Key = "usa",
                    Name = "United States Federal"
                },
                new GovernmentRow()
                {
                    Key = "usa-md",
                    Name = "Maryland State"
                },
                new GovernmentRow()
                {
                    Key = "usa-md-baltimorecity",
                    Name = "Baltimore City"
                },
                new GovernmentRow()
                {
                    Key = "usa-md-baltimorecounty",
                    Name = "Baltimore County"
                }
            });


            repository.AddParentChildGovernmentAssociation("usa", "usa-md");
            repository.AddParentChildGovernmentAssociation("usa-md", "usa-md-baltimorecity");
            repository.AddParentChildGovernmentAssociation("usa-md", "usa-md-baltimorecounty");

            repository.AddAdjacentGovernmentAssocation("usa-md-baltimorecity", "usa-md-baltimorecounty");

            var providers = new EmployeeDataProvider[] { new EmployeeDataProvider("usa-md-baltimorecounty", typeof(BaltimoreCountyEmployeeEnumerator))
                , new EmployeeDataProvider("usa-md-baltimorecity", typeof(BaltimoreCityEmployeeEnumerator)) };

            foreach (var p in providers)
            {
                var employees = p.ToList();

                var g = repository.LoadGovernment(p.GovernmentKey);

                g.FillAggregates(employees);

                repository.SaveGovernments(g);

                var departmentGroups = employees.GroupBy(e => e.Department);

                var departments = departmentGroups.Select(d =>
                {
                    var departmentEmployees = d.ToList();

                    var department = new DepartmentRow()
                    {
                        GovernmentKey = p.GovernmentKey,
                        Key = d.Key.ToUrlValue(),
                        Name = d.Key
                    };

                    department.FillAggregates(departmentEmployees);

                    return department;
                }).ToList();

                repository.SaveDepartments(departments);

                repository.SaveEmployees(employees);
            }
        }
    }
}
