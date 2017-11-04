using System;

namespace NI.Data.Extensions.Attributes
{
	public class DalcAttributeException : Exception
	{
		public DalcAttributeException(string msg) : base(msg)
		{
		}
	}
}
