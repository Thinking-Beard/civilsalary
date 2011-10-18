using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace civilsalary.data
{
    public sealed class AzureRepository : IRepository
    {
        public const string DepartmentsTable = "Departments";
        public const string EmployeesTable = "Employees";
        public const string GovernmentsTable = "Governments";

        TableServiceContext CreateContext()
        {
            throw new NotImplementedException();
        }

        public IQueryable<GovernmentRow> LoadGovernments()
        {
            throw new NotImplementedException();
        }

        public void SaveGovernments(IEnumerable<GovernmentRow> rows)
        {
            var ctx = CreateContext();

            ctx.AddOrUpdateObjects(GovernmentsTable, rows);

            ctx.SaveChanges();
        }

        public void AddParentChildGovernmentAssociation(string parentKey, string childKey)
        {
            throw new NotImplementedException();
        }

        public void AddAdjacentGovernmentAssocation(string keyX, string keyY)
        {
            throw new NotImplementedException();
        }

        public GovernmentRow LoadGovernment(string key)
        {
            var ctx = CreateContext();

            return ctx.LoadObject<GovernmentRow>(GovernmentsTable, key, string.Empty);
        }

        public void SaveDepartments(IEnumerable<DepartmentRow> departments)
        {
            var ctx = CreateContext();

            ctx.AddOrUpdateObjects(DepartmentsTable, departments);

            ctx.SaveChanges();
        }

        public void SaveEmployees(IEnumerable<EmployeeRow> employees)
        {
            var ctx = CreateContext();

            ctx.AddOrUpdateObjects(EmployeesTable, employees);

            ctx.SaveChanges();
        }
    }
}
