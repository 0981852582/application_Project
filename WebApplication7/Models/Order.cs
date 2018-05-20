using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
static class Program
{
    private static class AccessorCache
    {
        private static readonly Hashtable accessors = new Hashtable();

        private static readonly Hashtable callSites = new Hashtable();

        private static CallSite<Func<CallSite, object, object>> GetCallSiteLocked(
            string name)
        {
            var callSite = (CallSite<Func<CallSite, object, object>>)callSites[name];
            return callSite;
        }

        internal static Func<dynamic, object> GetAccessor(string name)
        {
            Func<dynamic, object> accessor = (Func<dynamic, object>)accessors[name];
            if (accessor == null)
            {
                lock (accessors)
                {
                    accessor = (Func<dynamic, object>)accessors[name];
                    if (accessor == null)
                    {
                        if (name.IndexOf('.') >= 0)
                        {
                            string[] props = name.Split('.');
                            CallSite<Func<CallSite, object, object>>[] arr
                                = Array.ConvertAll(props, GetCallSiteLocked);
                            accessor = target =>
                            {
                                object val = (object)target;
                                for (int i = 0; i < arr.Length; i++)
                                {
                                    var cs = arr[i];
                                    val = cs.Target(cs, val);
                                }
                                return val;
                            };
                        }
                        else
                        {
                            var callSite = GetCallSiteLocked(name);
                            accessor = target =>
                            {
                                return callSite.Target(callSite, (object)target);
                            };
                        }
                        accessors[name] = accessor;
                    }
                }
            }
            return accessor;
        }
    }

    public static IOrderedEnumerable<dynamic> OrderBy(
        this IEnumerable<dynamic> source,
        string property)
    {
        return Enumerable.OrderBy<dynamic, object>(
            source,
            AccessorCache.GetAccessor(property),
            Comparer<object>.Default);
    }

    public static IOrderedEnumerable<dynamic> OrderByDescending(
        this IEnumerable<dynamic> source,
        string property)
    {
        return Enumerable.OrderByDescending<dynamic, object>(
            source,
            AccessorCache.GetAccessor(property),
            Comparer<object>.Default);
    }

    public static IOrderedEnumerable<dynamic> ThenBy(
        this IOrderedEnumerable<dynamic> source,
        string property)
    {
        return Enumerable.ThenBy<dynamic, object>(
            source,
            AccessorCache.GetAccessor(property),
            Comparer<object>.Default);
    }

    public static IOrderedEnumerable<dynamic> ThenByDescending(
        this IOrderedEnumerable<dynamic> source,
        string property)
    {
        return Enumerable.ThenByDescending<dynamic, object>(
            source,
            AccessorCache.GetAccessor(property),
            Comparer<object>.Default);
    }
    private static LambdaExpression GenerateSelector<TEntity>(String propertyName, out Type resultType) where TEntity : class
    {
        var parameter = Expression.Parameter(typeof(TEntity), "Entity");
        PropertyInfo property;
        Expression propertyAccess;
        if (propertyName.Contains('.'))
        {
            String[] childProperties = propertyName.Split('.');
            property = typeof(TEntity).GetProperty(childProperties[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            propertyAccess = Expression.MakeMemberAccess(parameter, property);
            for (int i = 1; i < childProperties.Length; i++)
            {
                property = property.PropertyType.GetProperty(childProperties[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
            }
        }
        else
        {
            property = typeof(TEntity).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            propertyAccess = Expression.MakeMemberAccess(parameter, property);
        }
        resultType = property.PropertyType;
        return Expression.Lambda(propertyAccess, parameter);
    }
    private static MethodCallExpression GenerateMethodCall<TEntity>(IQueryable<TEntity> source, string methodName, String fieldName) where TEntity : class
    {
        Type type = typeof(TEntity);
        Type selectorResultType;
        LambdaExpression selector = GenerateSelector<TEntity>(fieldName, out selectorResultType);
        MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName,
                        new Type[] { type, selectorResultType },
                        source.Expression, Expression.Quote(selector));
        return resultExp;
    }

    public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
    {
        MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "OrderBy", fieldName);
        return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
    }

    public static IOrderedQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
    {
        MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "OrderByDescending", fieldName);
        return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
    }
    public static IOrderedQueryable<TEntity> ThenBy<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
    {
        MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "ThenBy", fieldName);
        return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
    }
    public static IOrderedQueryable<TEntity> ThenByDescending<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
    {
        MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "ThenByDescending", fieldName);
        return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
    }
    public static IOrderedQueryable<TEntity> OrderUsingSortExpression<TEntity>(this IQueryable<TEntity> source, string sortExpression) where TEntity : class
    {
        String[] orderFields = sortExpression.Split(',');
        IOrderedQueryable<TEntity> result = null;
        for (int currentFieldIndex = 0; currentFieldIndex < orderFields.Length; currentFieldIndex++)
        {
            String[] expressionPart = orderFields[currentFieldIndex].Trim().Split(' ');
            String sortField = expressionPart[0];
            Boolean sortDescending = (expressionPart.Length == 2) && (expressionPart[1].Equals("DESC", StringComparison.OrdinalIgnoreCase));
            if (sortDescending)
            {
                result = currentFieldIndex == 0 ? source.OrderByDescending(sortField) : result.ThenByDescending(sortField);
            }
            else
            {
                result = currentFieldIndex == 0 ? source.OrderBy(sortField) : result.ThenBy(sortField);
            }
        }
        return result;
    }
}