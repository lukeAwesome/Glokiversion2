using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tyme.Kihama.Common.Services.Enums.Peripheral;

namespace Gloki2._0.Model
{
	public class BarcodeType
	{
		public string Name { get; set; }

		public CardPrintBarcodeType Type { get; set; }

		#region Factory

		public static BarcodeType Create(string name, CardPrintBarcodeType type)
		{
			return new BarcodeType
			{
				Name = name,
				Type = type
			};
		}

		#endregion Factory
	}
}
