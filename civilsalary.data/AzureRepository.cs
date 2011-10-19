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
            BulkSave(GovernmentsTable, rows);
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

        public void SaveDepartments(ICollection<DepartmentRow> departments)
        {
            BulkSave(DepartmentsTable, departments);
        }

        public void SaveEmployees(ICollection<EmployeeRow> employees)
        {
            BulkSave(EmployeesTable, employees);
        }

        void BulkSave<T>(string entitySetName, ICollection<T> entities) where T : TableServiceEntity
        {
            BulkSave<T>(entitySetName, entities, 100);
        }

        void BulkSave<T>(string entitySetName, ICollection<T> entities, int batchSize) where T : TableServiceEntity
        {
            if(batchSize > 100) throw new ArgumentOutOfRangeException("batchSize");

            if (entities.Count <= batchSize * 2)
            {
                var ctx = CreateContext();
                ctx.AddOrUpdateObjects(entitySetName, entities);
                ctx.SaveChanges(SaveChangesOptions.None);

                return;
            }

            //more than 200, see if its possible to batch
            foreach (var g in entities.GroupBy(e => e.PartitionKey))
            {
                var group = g.ToList();

                if (group.Count > batchSize * 2)
                {
                    //TODO: parallel?
                    foreach (var batch in group.Batch(batchSize, x => x.ToList()))
                    {
                        var ctx = CreateContext();

                        var existing = ctx.LoadObjects<T>(entitySetName, batch.Select(e => Tuple.Create(e.PartitionKey, e.RowKey)));

                        //AddOrUpdate does not work for batch...
                        var joined = from b in batch
                                     join e in existing on new { b.PartitionKey, b.RowKey } equals new { e.PartitionKey, e.RowKey } into existingGroup
                                     from e in existingGroup.DefaultIfEmpty()
                                     select new { toSave = b, existing = e };

                        foreach (var j in joined)
                        {
                            if (j.existing != null)
                            {
                                ctx.AttachTo(entitySetName, j.toSave);
                                ctx.UpdateObject(j.toSave);
                            }
                            else
                            {
                                ctx.AddObject(entitySetName, j.toSave);
                            }
                        }

                        ctx.SaveChanges(SaveChangesOptions.Batch);
                    }
                }
                else
                {
                    var ctx = CreateContext();
                    ctx.AddOrUpdateObjects(entitySetName, group);
                    ctx.SaveChanges(SaveChangesOptions.None);
                }
            }
        }
    }
}
