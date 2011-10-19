using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Services.Client;

namespace civilsalary.data
{
    public interface IRepository
    {
        IQueryable<GovernmentRow> LoadGovernments();
        void SaveGovernments(ICollection<GovernmentRow> governmentRow);
        void AddParentChildGovernmentAssociation(string parentKey, string childKey);
        void AddAdjacentGovernmentAssocation(string keyX, string keyY);
        GovernmentRow LoadGovernment(string governmentKey);
        void SaveDepartments(ICollection<DepartmentRow> departments);
        void SaveEmployees(ICollection<EmployeeRow> employees);
        DepartmentRow LoadDepartment(string governmentKey, string departmentKey);
    }

    public static class IRepositoryExtensions
    {
        public static void SaveGovernments(this IRepository repository, params GovernmentRow[] governments)
        {
            repository.SaveGovernments(governments);
        }
    }
}
