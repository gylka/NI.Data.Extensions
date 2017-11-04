using System;
using System.Collections.Generic;

namespace NI.Data.Extensions.Attributes
{
	/// <summary>
	/// Enumerates all possible values of the database table column.
	/// Makes sense if only the column is writable
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class AllowedValuesAttribute : Attribute
	{
		public HashSet<object> Values { get; }

		public AllowedValuesAttribute(params object[] values)
		{
			Values = new HashSet<object>(values);

			//null and DbNull.Value are interchangable
			if (Values.Contains(null) || Values.Contains(DBNull.Value))
			{
				Values.Add(null);
				Values.Add(DBNull.Value);
			}
		}
	}
}