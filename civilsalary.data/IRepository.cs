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
        GovernmentRow LoadGovernment(string key);
        void SaveDepartments(ICollection<DepartmentRow> departments, SaveChangesOptions options);
        void SaveEmployees(ICollection<EmployeeRow> employees, SaveChangesOptions options);
    }

    public static class IRepositoryExtensions
    {
        public static void SaveGovernments(this IRepository repository, params GovernmentRow[] governments)
        {
            repository.SaveGovernments(governments);
        }
    }
}
