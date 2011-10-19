using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using System.Configuration;
using System.Data.Services.Client;

namespace civilsalary.data
{
    public sealed class AzureRepository : IRepository
    {
        public const string DepartmentsTable = "Departments";
        public const string EmployeesTable = "Employees";
        public const string GovernmentsTable = "Governments";
        public const string GovernmentAssocationsTable = "GovernmentAssociations";

        CloudStorageAccount _account;

        static AzureRepository()
        {
            CloudStorageAccount.SetConfigurationSettingPublisher((key, publisher) =>
            {
                //TODO: azure environment

                var connectionString = ConfigurationManager.ConnectionStrings[key];

                if (connectionString != null)
                {
                    publisher(connectionString.ConnectionString);
                }

                publisher(ConfigurationManager.AppSettings[key]);
            });
        }

        public AzureRepository()
        {
            _account = CloudStorageAccount.FromConfigurationSetting("CivilSalaryData");

            var tableClient = new CloudTableClient(_account.TableEndpoint.AbsoluteUri, _account.Credentials);

            tableClient.CreateTableIfNotExist<GovernmentRow>(GovernmentsTable);
            tableClient.CreateTableIfNotExist<GovernmentAssociationRow>(GovernmentAssocationsTable);
            tableClient.CreateTableIfNotExist<EmployeeRow>(EmployeesTable);
            tableClient.CreateTableIfNotExist<DepartmentRow>(DepartmentsTable);
        }

        TableServiceContext CreateContext()
        {
            return new TableServiceContext(_account.TableEndpoint.AbsoluteUri, _account.Credentials);
        }

        public IQueryable<GovernmentRow> LoadGovernments()
        {
            var ctx = CreateContext();

            return ctx.CreateQuery<GovernmentRow>(GovernmentsTable);
        }

        public void SaveGovernments(ICollection<GovernmentRow> rows)
        {
            var ctx = CreateContext();

            ctx.AddOrUpdateObjects(GovernmentsTable, rows);

            ctx.SaveChanges();
        }

        public void AddParentChildGovernmentAssociation(string parentKey, string childKey)
        {
            var ctx = CreateContext();

            ctx.AddOrUpdateObjects(GovernmentAssocationsTable, new GovernmentAssociationRow[] 
            {
                new GovernmentAssociationRow() { Association = "Parent", Key1 = parentKey, Key2 = childKey },
                new GovernmentAssociationRow() { Association = "Child", Key2 = parentKey, Key1 = childKey }
            });

            ctx.SaveChanges();
        }

        public void AddAdjacentGovernmentAssocation(string keyX, string keyY)
        {
            var ctx = CreateContext();

            ctx.AddOrUpdateObjects(GovernmentAssocationsTable, new GovernmentAssociationRow[] 
            {
                new GovernmentAssociationRow() { Association = "Adjacent", Key1 = keyX, Key2 = keyY },
                new GovernmentAssociationRow() { Association = "Adjacent", Key2 = keyX, Key1 = keyY }
            });

            ctx.SaveChanges();
        }

        public GovernmentRow LoadGovernment(string key)
        {
            var ctx = CreateContext();

            return ctx.LoadObject<GovernmentRow>(GovernmentsTable, SecUtility.EscapeKey(key), string.Empty);
        }

        public void SaveDepartments(ICollection<DepartmentRow> departments, SaveChangesOptions options)
        {
            var ctx = CreateContext();

            ctx.AddOrUpdateObjects(DepartmentsTable, departments);

            ctx.SaveChanges(options);
        }

        public void SaveEmployees(ICollection<EmployeeRow> employees, SaveChangesOptions options)
        {
            var ctx = CreateContext();

            foreach (var e in employees)
            {
                ctx.AddOrUpdateObject(EmployeesTable, e);
                ctx.SaveChanges();
            }

            //ctx.AddOrUpdateObjects(EmployeesTable, employees);

            //ctx.SaveChanges(options);
        }
    }
}
