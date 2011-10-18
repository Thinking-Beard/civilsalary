using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace civilsalary.data
{
    public interface IRepository
    {
        IQueryable<GovernmentRow> LoadGovernments();
        void SaveGovernments(IEnumerable<GovernmentRow> governmentRow);
        void AddParentChildGovernmentAssociation(string parentKey, string childKey);
        void AddAdjacentGovernmentAssocation(string keyX, string keyY);
        GovernmentRow LoadGovernment(string key);
        void SaveDepartments(IEnumerable<DepartmentRow> departments);
        void SaveEmployees(IEnumerable<EmployeeRow> employees);
    }

    public static class IRepositoryExtensions
    {
        public static void SaveGovernments(this IRepository repository, params GovernmentRow[] governments)
        {
            repository.SaveGovernments(governments);
        }
    }
}
