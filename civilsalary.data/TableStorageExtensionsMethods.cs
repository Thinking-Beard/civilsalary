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
        public static void AddOrUpdateObjects<T>(this TableServiceContext context, string entitySetName, ICollection<T> entities) where T : TableServiceEntity
        {
            var existing = LoadObjects<T>(context, entitySetName, entities.Select(e => Tuple.Create(e.PartitionKey, e.RowKey)), true);

            var joined = from entity in entities
                         join exist in existing on new { entity.PartitionKey, entity.RowKey } equals new { exist.PartitionKey, exist.RowKey } into joinedExisting
                         from exist in joinedExisting.DefaultIfEmpty()
                         select new { entity, existing = exist };

            foreach (var j in joined)
            {
                AddOrUpdateObjectInternal<T>(context, entitySetName, j.entity, j.existing);
            }
        }

        public static ICollection<T> LoadObjects<T>(this TableServiceContext context, string entitySetName, IEnumerable<Tuple<string,string>> keys, bool detach) where T : TableServiceEntity
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

            if (detach)
            {
                context.DetachAll(existing);
            }

            return existing;
        }

        public static T LoadObject<T>(this TableServiceContext context, string entitySetName, string partitionKey, string rowKey, bool detach) where T : TableServiceEntity
        {
            var entity = context.CreateQuery<T>(entitySetName).Where(e => 1 == 1 && e.PartitionKey == partitionKey && e.RowKey == rowKey).SingleOrDefault();

            if (detach)
            {
                context.Detach(entity);
            }

            return entity;
        }

        public static void AddOrUpdateObject<T>(this TableServiceContext context, string entitySetName, T entity) where T : TableServiceEntity
        {
            var existingObject = LoadObject<T>(context, entitySetName, entity.PartitionKey, entity.RowKey, true);

            AddOrUpdateObjectInternal<T>(context, entitySetName, entity, existingObject);
        }

        static void AddOrUpdateObjectInternal<T>(this TableServiceContext context, string entitySetName, T entity, T existingEntity) where T : TableServiceEntity
        {
			if (existingEntity == null)
            {
                context.AddObject(entitySetName, entity);
            }
            else
            {
	            context.Detach(entity);
	            context.AttachTo(entitySetName, entity, "*");
	            context.UpdateObject(entity);
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

        public static void DetachAll(this TableServiceContext context, IEnumerable<object> entities)
        {
            foreach (var e in entities)
            {
                context.Detach(e);
            }
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
