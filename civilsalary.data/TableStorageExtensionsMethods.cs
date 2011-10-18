using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Reflection;

namespace civilsalary.data
{
    static class TableStorageExtensionsMethods
    {
        public static bool CreateTableIfNotExist<T>(this CloudTableClient tableStorage, string entityName) where T : TableServiceEntity, new()
        {
            bool result = tableStorage.CreateTableIfNotExist(entityName);

            // Execute conditionally for development storage only
            if (tableStorage.BaseUri.IsLoopback)
            {
                InitializeTableSchemaFromEntity(tableStorage,
                    entityName, new T());
            }
            return result;
        }

        private static void InitializeTableSchemaFromEntity(CloudTableClient tableStorage, string entityName, TableServiceEntity entity)
        {
            TableServiceContext context = tableStorage.GetDataServiceContext();
            DateTime now = DateTime.UtcNow;
            entity.PartitionKey = Guid.NewGuid().ToString();
            entity.RowKey = Guid.NewGuid().ToString();

            Array.ForEach(
                entity.GetType().GetProperties(BindingFlags.Public |
                BindingFlags.Instance),
                p =>
                {
                    if ((p.Name != "PartitionKey") &&
                        (p.Name != "RowKey") && (p.Name != "Timestamp"))
                    {
                        if (p.PropertyType == typeof(string))
                        {
                            p.SetValue(entity, Guid.NewGuid().ToString(),
                                null);
                        }
                        else if (p.PropertyType == typeof(DateTime))
                        {
                            p.SetValue(entity, now, null);
                        }
                    }
                });

            context.AddObject(entityName, entity);
            context.SaveChangesWithRetries();
            context.DeleteObject(entity);
            context.SaveChangesWithRetries();
        }
    }
}
