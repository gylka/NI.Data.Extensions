using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NI.Data.Extensions.Attributes;

namespace NI.Data.Extensions
{
	public class MappingHelper
	{
		private static readonly object Lock = new object(); 
		private static readonly Dictionary<Type, Dictionary<MappingType, ObjectDalcMapperParameters>> TypeObjectDalcMapperParametersCache = new Dictionary<Type, Dictionary<MappingType, ObjectDalcMapperParameters>>();
		
		public static ObjectDalcMapper<T> GetObjectDalcMapper<T>(DataRowDalcMapper dbManager, MappingType mappingType) where T : class, new()
		{
			ObjectDalcMapperParameters parameters = null;
			lock (Lock)
			{
				// cache should be populated with property-to-column dictionaries for given type for all mapping types at once
				if (TypeObjectDalcMapperParametersCache.ContainsKey(typeof(T)))
					parameters = TypeObjectDalcMapperParametersCache[typeof(T)][mappingType];
			}
			if(parameters != null)
				return new ObjectDalcMapper<T>(dbManager, parameters.TableName, parameters.PropertyToColumnMapping);
			
			// if not found ObjectDalcMapperParameters - creating for every mapping type all at once
			var attributes = Attribute.GetCustomAttributes(typeof(T));
			var tableName = attributes.OfType<DbTableAttribute>().FirstOrDefault()?.Name;
			if (string.IsNullOrWhiteSpace(tableName))
				throw new DalcAttributeException("Domain entity class must have table name attribute specified");

			var entityProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var mappingTypeToObjectDalcMapperParameters = new Dictionary<MappingType, ObjectDalcMapperParameters>();
			Dictionary<string, string> currentPropertyToColumnMap = null;
			foreach (var enumeratorMappingValue in Enum.GetValues(typeof(MappingType)).Cast<MappingType>())
			{
				var propertyToColumnMap = GetPropertyToColumnMap(entityProperties, enumeratorMappingValue);
				if (enumeratorMappingValue == mappingType)
					currentPropertyToColumnMap = propertyToColumnMap;
				mappingTypeToObjectDalcMapperParameters[enumeratorMappingValue] = new ObjectDalcMapperParameters(tableName, propertyToColumnMap);
			}

			lock (Lock)
			{
				if (!TypeObjectDalcMapperParametersCache.ContainsKey(typeof(T)))
					TypeObjectDalcMapperParametersCache[typeof(T)] = mappingTypeToObjectDalcMapperParameters;
				return new ObjectDalcMapper<T>(dbManager, tableName, currentPropertyToColumnMap);
			}
		}

		private static Dictionary<string, string> GetPropertyToColumnMap(PropertyInfo[] typeProperties, MappingType mappingType)
		{
			var dalcMappedProperties = typeProperties.Where(property => IsModelPropertyMeetsMappingRequirements(property, mappingType)).ToArray();
			return dalcMappedProperties.ToDictionary(GetPropertyColumnName, p => p.Name);
		}

		private static bool IsModelPropertyMeetsMappingRequirements(PropertyInfo property, MappingType mappingType)
		{
			var dbColumnAttribute = property.GetCustomAttribute<DbColumnAttribute>();
			if (dbColumnAttribute == null)		//not a db column
				return false;
			switch (mappingType)
			{
				case MappingType.Load:
					return true;
				case MappingType.Insert:
					return dbColumnAttribute.Writable;
				case MappingType.Update:
					return dbColumnAttribute.Writable && !property.IsDefined(typeof(NoUpdateAttribute));
				default:
					throw new ArgumentOutOfRangeException(nameof(mappingType), mappingType, null);
			}
		}

		private static string GetPropertyColumnName(PropertyInfo propertyInfo)
			=> propertyInfo.GetCustomAttribute<DbColumnAttribute>()?.Name ?? PascalCaseToSnakeCase(propertyInfo.Name);

		public static string PascalCaseToSnakeCase(string str)
			=> str == null ? null : Regex.Replace(str, "[A-Z]", "_$0").ToLower().TrimStart('_');
	}

	internal class ObjectDalcMapperParameters
	{
		public readonly string TableName;
		public readonly Dictionary<string, string> PropertyToColumnMapping;

		public ObjectDalcMapperParameters(string tableName, Dictionary<string, string> propertyToColumnMapping)
		{
			TableName = tableName;
			PropertyToColumnMapping = propertyToColumnMapping;
		}
	}
}