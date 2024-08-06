using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gloki2._0.SystemBoards
{
	public class LogEntry
	{
		public string DateTime { get; set; }

		public string Detail { get; set; }

		#region Factory

		public static LogEntry Create(string dateTime, string detail)
		{
			return new LogEntry
			{
				DateTime = dateTime,
				Detail = detail
			};
		}

		#endregion Factory
	}
}
