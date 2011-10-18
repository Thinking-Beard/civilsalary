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
        public static void AddOrUpdateObjects<T>(this TableServiceContext context, string entitySetName, IEnumerable<T> entities) where T : TableServiceEntity
        {
            foreach (var e in entities)
            {
                AddOrUpdateObject<T>(context, entitySetName, e);
            }
        }

        public static T LoadObject<T>(this TableServiceContext context, string entitySetName, string partitionKey, string rowKey) where T : TableServiceEntity
        {
            return context.CreateQuery<T>(entitySetName).Where(e => e.PartitionKey == partitionKey && e.RowKey == rowKey).SingleOrDefault();
        }

        public static void AddOrUpdateObject<T>(this TableServiceContext context, string entitySetName, T entity) where T : TableServiceEntity
        {
            var existingObject = LoadObject<T>(context, entitySetName, entity.PartitionKey, entity.RowKey);

            if (existingObject == null)
            {
                context.AddObject(entitySetName, entity);
            }
            else
            {
                var properties = from p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)
                                 where !string.Equals(p.Name, "RowKey", StringComparison.Ordinal)
                                 && !string.Equals(p.Name, "PartitionKey", StringComparison.Ordinal)
                                 && !string.Equals(p.Name, "Timestamp", StringComparison.Ordinal)
                                 select p;

                foreach (var p in properties)
                {
                    //copy value
                    p.SetValue(existingObject, p.GetValue(entity, null), null);
                }

                //existingObject.Timestamp = entity.Timestamp;
                context.UpdateObject(existingObject);
            }
        }

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
