using System;
using System.Collections.Generic;
using System.Reflection;
using NI.Data.Extensions.Attributes;

namespace NI.Data.Extensions
{
	public static class DataRowDalcMapperExtension
	{
		public static T LoadEntity<T>(this DataRowDalcMapper dbMgr, params object[] pk) where T : class, new()
		{
			return MappingHelper.GetObjectDalcMapper<T>(dbMgr, MappingType.Load).Load(pk);
		}

		public static T LoadEntity<T>(this DataRowDalcMapper dbMgr, Query q) where T : class, new()
		{
			return MappingHelper.GetObjectDalcMapper<T>(dbMgr, MappingType.Load).Load(q);
		}

		public static IEnumerable<T> LoadAllEntities<T>(this DataRowDalcMapper dbMgr, Query q) where T : class, new()
		{
			return MappingHelper.GetObjectDalcMapper<T>(dbMgr, MappingType.Load).LoadAll(q);
		}

		public static void AddEntity<T>(this DataRowDalcMapper dbMgr, T entity) where T : class, new()
		{
			var objectDalcMapper = MappingHelper.GetObjectDalcMapper<T>(dbMgr, MappingType.Insert);
			MakeSureColumnsHaveAllowedValues(entity);
			objectDalcMapper.Add(entity);
		}

		public static bool UpdateEntity<T>(this DataRowDalcMapper dbMgr, T entity) where T : class, new()
		{
			var objectDalcMapper = MappingHelper.GetObjectDalcMapper<T>(dbMgr, MappingType.Update);
			MakeSureColumnsHaveAllowedValues(entity);
			try
			{
				objectDalcMapper.Update(entity);
				return true;
			}
			catch (System.Data.DBConcurrencyException)
			{
				return false;
			}
		}

		public static void DeleteEntity<T>(this DataRowDalcMapper dbMgr, T entity) where T : class, new()
		{
			MappingHelper.GetObjectDalcMapper<T>(dbMgr, MappingType.Update).Delete(entity); //any mapping type can be used here
		}

		private static void MakeSureColumnsHaveAllowedValues(object entity)
		{
			var properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var property in properties)
			{
				var allowedValuesAttribute = property.GetCustomAttribute<AllowedValuesAttribute>();
				if (allowedValuesAttribute == null)
					continue;

				var propertyValue = property.GetValue(entity);
				if (!allowedValuesAttribute.Values.Contains(propertyValue))
				{
					var tableName = entity.GetType().GetCustomAttribute<DbTableAttribute>()?.Name;
					var columnName = Attribute.IsDefined(property, typeof(DbColumnAttribute)) ? MappingHelper.PascalCaseToSnakeCase(property.Name) : "";
					throw new Exception($"Value is not allowed for {tableName}.{columnName}: {propertyValue}");
				}
			}
		}
	}
}