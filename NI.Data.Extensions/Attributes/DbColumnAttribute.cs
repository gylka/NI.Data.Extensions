using System;

namespace NI.Data.Extensions.Attributes
{
	/// <summary>
	/// States that the marked property gets the corresponing value from results of DB requests by a repository.
	/// The property value will also be used in Insert/Update operations, if Writable = true
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class DbColumnAttribute : Attribute
	{
		/// <summary>
		/// Name of column. By default, name is snake case of a model property name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Must be true, if the field value can be set by the repositories' Update/Insert operations
		/// </summary>
		public bool Writable { get; set; }

		public DbColumnAttribute(string name = null)
		{
			Name = name;
		}
	}
}