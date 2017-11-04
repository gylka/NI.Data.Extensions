using System;

namespace NI.Data.Extensions.Attributes
{
	/// <summary>
	/// States that the value of the marked property will be used in Insert operations,
	/// and will NOT be used in Update operations
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class NoUpdateAttribute : Attribute
	{
	}
}