using System;

namespace NI.Data.Extensions.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DbTableAttribute : Attribute
	{
		public string Name { get; }

		public DbTableAttribute(string name)
		{
			Name = name;
		}
	}
}