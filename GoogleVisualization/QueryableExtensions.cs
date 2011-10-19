using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace GoogleVisualization
{
    public static class QueryableExtensions
    {
        public static int Count(this IQueryable queryable)
        {
            return (int) queryable.Provider.Execute(Expression.Call(
                    typeof(Queryable),
                    "Count",
                    new Type[] { queryable.ElementType },
                    queryable.Expression
                ));
        }

        public static IQueryable WherePropertyContains(this IQueryable queryable, string propertyName, string value, StringComparison comparison)
        {
            var type = queryable.ElementType;
            var property = type.GetProperty(propertyName);
            var parameter = Expression.Parameter(type, "p");

            var expression = Expression.GreaterThanOrEqual(
                Expression.Call(
                    Expression.MakeMemberAccess(
                        parameter,
                        property),
                    "IndexOf",
                    null,
                    Expression.Constant(value),
                    Expression.Constant(comparison)),
                Expression.Constant(0));

            var lambda = Expression.Lambda(expression, parameter);

            return queryable.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Where", new Type[] { type }, queryable.Expression, Expression.Quote(lambda)));
        }

        public static IQueryable OrderBy(this IQueryable queryable, string propertyName)
        {
            return OrderByInternal(queryable, propertyName, "OrderBy");
        }

        public static IQueryable OrderByDescending(this IQueryable queryable, string propertyName)
        {
            return OrderByInternal(queryable, propertyName, "OrderByDescending");
        }

        static IQueryable OrderByInternal(IQueryable queryable, string propertyName, string methodName)
        {
            var type = queryable.ElementType;
            var property = type.GetProperty(propertyName);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);

            return queryable.Provider.CreateQuery(Expression.Call(typeof(Queryable), methodName, new Type[] { type, property.PropertyType }, queryable.Expression, Expression.Quote(orderByExp)));
        }

        public static IQueryable Skip(this IQueryable queryable, int count)
        {
            return queryable.Provider.CreateQuery(Expression.Call(
                    typeof(Queryable),
                    "Skip",
                    new Type[] { queryable.ElementType },
                    queryable.Expression,
                    Expression.Constant(count)
                ));
        }

        public static IQueryable Take(this IQueryable queryable, int count)
        {
            return queryable.Provider.CreateQuery(Expression.Call(
                    typeof(Queryable),
                    "Take",
                    new Type[] { queryable.ElementType },
                    queryable.Expression,
                    Expression.Constant(count)
                ));
        }
    }
}
