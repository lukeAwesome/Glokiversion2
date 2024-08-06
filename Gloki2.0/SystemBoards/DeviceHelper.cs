
namespace Gloki2._0.SystemBoards
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Management;
	using System.Reflection;
	using Tyme.Kihama.Common.Services.DTOs;
	using Tyme.Kihama.Common.Services.Helpers;
	using ErrorCode = Tyme.Kihama.Common.Services.Enums.ErrorCode;

	public class DeviceHelper
	{
		public static List<string> GetDevices(
			int vendorId,
			int productId,
			PnpDeviceClass deviceClass = PnpDeviceClass.Unknown,
			string deviceId = "")
		{
			var deviceList = new List<string>();

			try
			{
				var query =
					$"SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE '%VID_{vendorId:X4}&PID_{productId:X4}%'";

				if (deviceClass != PnpDeviceClass.Unknown)
				{
					query += $" AND PNPClass = '{deviceClass.GetDescription()}'";
				}

				using (var searcher = new ManagementObjectSearcher(queryString: query))
				{
					foreach (var device in searcher.Get())
					{
						var productName = device[propertyName: "Name"]?.ToString();

						if (string.IsNullOrWhiteSpace(value: productName))
						{
							continue;
						}

						if (string.IsNullOrWhiteSpace(value: deviceId))
						{
							deviceList.Add(item: productName);

							continue;
						}

						var serialNumber = device[propertyName: "DeviceID"]?.ToString();

						if (string.IsNullOrWhiteSpace(value: serialNumber))
						{
							continue;
						}

						var serialNumberParts = serialNumber.Split('\\');

						var id = serialNumberParts.Length >= 3
							? serialNumberParts[2].Trim()
							: null;

						if (deviceId != id)
						{
							continue;
						}

						deviceList.Add(item: productName);
					}
				}
			}
			catch (Exception e)
			{
				//   
			}

			return deviceList;
		}

		public static string GetDeviceId(
			string deviceName,
			int vendorId,
			int productId,
			PnpDeviceClass deviceClass = PnpDeviceClass.Unknown)
		{
			string deviceId = null;

			try
			{
				var query =
					$"SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE '%VID_{vendorId:X4}&PID_{productId:X4}%' AND Name = '{deviceName}'";

				if (deviceClass != PnpDeviceClass.Unknown)
				{
					query += $" AND PNPClass = '{deviceClass.GetDescription()}'";
				}

				using (var searcher = new ManagementObjectSearcher(queryString: query))
				{
					foreach (var device in searcher.Get())
					{
						var serialNumber = device[propertyName: "DeviceID"]?.ToString();

						if (string.IsNullOrWhiteSpace(value: serialNumber))
						{
							continue;
						}

						var serialNumberParts = serialNumber.Split('\\');
						deviceId = serialNumberParts.Length >= 3
							? serialNumberParts[2].Trim()
							: null;

						break;
					}
				}
			}
			catch (Exception e)
			{
				//   
			}

			return deviceId;
		}

		public static (bool IsSuccess, string Response) ProgramDevice(string comPort, string firmwareFileName)
		{
			try
			{
				var partitionFileName = firmwareFileName.Replace(oldValue: ".bin", newValue: ".partitions.bin");

				var workingDirectory = Path.GetDirectoryName(path: Assembly.GetAssembly(
										   type: typeof(DeviceHelper)).Location)
									   + @"\ESP32Program";

				using (var process = new Process())
				{
					process.StartInfo = new ProcessStartInfo
					{
						WorkingDirectory = workingDirectory,
						FileName = $@"{workingDirectory}\Program.bat",
						Arguments = $"{comPort} \"{firmwareFileName}\" \"{partitionFileName}\"",
						UseShellExecute = false,
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden,
						RedirectStandardOutput = true
					};

					process.Start();
					process.WaitForExit();

					var output = process.StandardOutput.ReadToEnd();
					var outputParts = output.Split('\n');
					var response = outputParts.ElementAt(index: outputParts.Length - 2).Replace(oldValue: "\r", newValue: "");
					var isSuccess = process.ExitCode == 0 &&
									output.ToLowerInvariant().Contains(value: "hard resetting via rts pin");

					return (IsSuccess: isSuccess, Response: response);
				}
			}
			catch (Exception e)
			{
				return (IsSuccess: false, Response: $"ERROR: {e.Message}");
			}
		}

		public static DeviceInfo<T> GetUsbDeviceInfo<T>(int vendorId, int productId,
			PnpDeviceClass deviceClass = PnpDeviceClass.Unknown,
			string firmwareVersion = "", string deviceId = "")
		{
			var deviceInfo = DeviceInfo<T>.Create(isDetected: false, errorCode: ErrorCode.DeviceNotFound);

			try
			{
				var query =
					$"SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE '%VID_{vendorId:X4}&PID_{productId:X4}%'";

				if (deviceClass != PnpDeviceClass.Unknown)
				{
					query += $" AND PNPClass = '{deviceClass.GetDescription()}'";
				}

				using (var searcher = new ManagementObjectSearcher(queryString: query))
				{
					foreach (var device in searcher.Get())
					{
						var serialNumber = device[propertyName: "DeviceID"]?.ToString();

						if (!string.IsNullOrWhiteSpace(value: serialNumber))
						{
							var serialNumberParts = serialNumber.Split('\\');

							serialNumber = serialNumberParts.Length >= 3
								? serialNumberParts[2].Replace(oldValue: "&", newValue: "").Trim()
								: ""; // TODO: Auto-generate a serial number for non-serialised component
						}

						// Check against provided Device ID
						if (!string.IsNullOrWhiteSpace(value: deviceId) && deviceId != serialNumber)
						{
							continue;
						}

						deviceInfo.ErrorCode = ErrorCode.Success;
						deviceInfo.IsDetected = true;
						deviceInfo.Vendor = device[propertyName: "Manufacturer"]?.ToString();
						deviceInfo.Product = device[propertyName: "Name"]?.ToString();
						deviceInfo.FirmwareVersion = firmwareVersion;
						deviceInfo.SerialNumber = serialNumber;

						break; // Only get the first result
					}
				}
			}
			catch (Exception e)
			{
			}

			return deviceInfo;
		}
	}
}
