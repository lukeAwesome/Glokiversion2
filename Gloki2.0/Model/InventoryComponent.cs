using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gloki2._0.Model
{
	public class InventoryComponent
	{
		public bool IsDetected { get; set; }
		public bool IsHealthy { get; set; }
		public string Vendor { get; set; }
		public string Product { get; set; }
		public string SerialNumber { get; set; }
		public string FirmwareVersion { get; set; }
		public string ComponentFeedback { get; set; }

		public InventoryComponent(bool isDetected, bool isHealthy, string vendor, string product, string serialNumber, string firmwareVersion, string componentFeedback)
		{
			IsDetected = isDetected;
			IsHealthy = isHealthy;
			Vendor = vendor;
			Product = product;
			SerialNumber = serialNumber;
			FirmwareVersion = firmwareVersion;
			ComponentFeedback = componentFeedback;
		}
	}
}
