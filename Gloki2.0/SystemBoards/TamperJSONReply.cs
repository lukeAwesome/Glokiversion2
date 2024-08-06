using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gloki2._0.SystemBoards
{
	public class TamperJSONReply
	{
		public string ID { get; set; }
		public string Cmd { get; set; }
		public string SN { get; set; }
		public string Version { get; set; }
		public string Health { get; set; }
		public string Temp { get; set; }
		public string AmbLight { get; set; }
		public string WhiteLight { get; set; }
	}
}
