using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gloki2._0.Model
{
	public class EventEntry
	{
		public int TestCycle { get; set; }
		public string Description { get; set; }
		public bool IsPassed { get; set; }
		public string ResultCode { get; set; }

		public EventEntry(int testCycle, string description, bool isPassed, string resultCode)
		{
			TestCycle = testCycle;
			Description = description;
			IsPassed = isPassed;
			ResultCode = resultCode;
		}
	}
}
