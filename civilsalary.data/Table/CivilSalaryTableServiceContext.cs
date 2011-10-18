using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace civilsalary.data
{
    internal sealed class CivilSalaryTableServiceContext : TableServiceContext
    {
        const string DepartmentsTable = "Departments";
        const string EmployeesTable = "Employees";

        public IQueryable<DepartmentRow> Departments
        {
            get
            {
                return CreateQuery<DepartmentRow>(DepartmentsTable);
            }
        }

        public IQueryable<EmployeeRow> Employees
        {
            get
            {
                return CreateQuery<EmployeeRow>(EmployeesTable);
            }
        }

        public void DeleteDepartments(string government)
        {
            var departments = from d in Departments
                              where d.PartitionKey == SecUtility.EscapeKey(government)
                              select d;

            foreach (var d in departments)
            {
                this.DeleteObject(d);
            }
        }
    }
}
