using System;

namespace Gloki2._0.SystemBoards
{
	public class UserFriendlyException : Exception
	{
		public UserFriendlyException(string message) : base(message: message)
		{
		}
	}
}
