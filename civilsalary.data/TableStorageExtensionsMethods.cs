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
        public static void AddOrUpdateObjects(this TableServiceContext context, string entitySetName, IEnumerable<object> entities)
        {
            foreach (var e in entities)
            {
                AddOrUpdateObjectInternal(context, entitySetName, e);
            }
        }

        public static ICollection<T> LoadObjects<T>(this TableServiceContext context, string entitySetName, IEnumerable<Tuple<string,string>> keys) where T : TableServiceEntity
        {
            var existing = new List<T>();

            foreach (var partition in keys.GroupBy(k => k.Item1))
            {
                var filter = new StringBuilder("PartitionKey eq '" + partition.Key + "' and (");
                var first = true;

                foreach (var row in partition)
                {
                    if (!first) filter.Append(" or ");

                    filter.Append("RowKey eq '");
                    filter.Append(row.Item2);
                    filter.Append("'");

                    first = false;
                }

                filter.Append(")");

                var query = context.CreateQuery<T>(entitySetName);

                query.AddQueryOption("$filter", filter.ToString());

                existing.AddRange(query);
            }

            return existing;
        }

        public static T LoadObject<T>(this TableServiceContext context, string entitySetName, string partitionKey, string rowKey) where T : TableServiceEntity
        {
            return context.CreateQuery<T>(entitySetName).Where(e => 1 == 1 && e.PartitionKey == partitionKey && e.RowKey == rowKey).SingleOrDefault();
        }

        public static void AddOrUpdateObject(this TableServiceContext context, string entitySetName, object entity)
        {
            AddOrUpdateObjectInternal(context, entitySetName, entity);
        }

        static void AddOrUpdateObjectInternal(this TableServiceContext context, string entitySetName, object entity)
        {
            context.Detach(entity);
            context.AttachTo(entitySetName, entity, "*");
            context.UpdateObject(entity);
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
