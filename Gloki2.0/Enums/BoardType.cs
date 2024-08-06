using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gloki2._0.Enum
{
	using System.ComponentModel;
	public enum BoardType
	{
		[Description(description: "Unknown")]
		Unknown,

		[Description(description: "LCBLITE")]
		LightingControlLite,

		[Description(description: "PCBLITE")]
		PowerSupplyLite,

		[Description(description: "ESBLITE")]
		SensorLite
	}
}
