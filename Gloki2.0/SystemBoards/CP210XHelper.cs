
namespace Gloki2._0.SystemBoards
{
	using System;
	using System.Runtime.ExceptionServices;
	using System.Text;
	using System.Threading.Tasks;
	public static class CP210XHelper
	{
		#region Initialisation / Shutdown

		[HandleProcessCorruptedStateExceptions]
		public static IntPtr OpenDevice(int deviceIndex)
		{
			try
			{
				var resultCode = CP210XSdkHelper.CP210x_Open(DeviceIndex: deviceIndex, pcyHandle: out var handlePtr);

				return resultCode != CP210XSdkHelper.CP210x_SUCCESS ? IntPtr.Zero : handlePtr;
			}
			catch (Exception e)
			{
				return IntPtr.Zero;
			}
		}

		[HandleProcessCorruptedStateExceptions]
		public static bool CloseDevice(IntPtr handle)
		{
			try
			{
				var resultCode = CP210XSdkHelper.CP210x_Close(cyHandle: handle);

				return resultCode == CP210XSdkHelper.CP210x_SUCCESS;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		[HandleProcessCorruptedStateExceptions]
		public static bool ResetDevice(IntPtr handle)
		{
			try
			{
				var resultCode = CP210XSdkHelper.CP210x_Reset(cyHandle: handle);

				return resultCode == CP210XSdkHelper.CP210x_SUCCESS;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		#endregion Initialisation / Shutdown

		#region Get Information

		[HandleProcessCorruptedStateExceptions]
		public static int GetNumberOfDevices()
		{
			try
			{
				var resultCode = CP210XSdkHelper.CP210x_GetNumDevices(lpdwNumDevices: out var numberOfDevices);

				if (resultCode != CP210XSdkHelper.CP210x_SUCCESS)
				{
					return -1;
				}

				return numberOfDevices;
			}
			catch (Exception e)
			{
				return -1;
			}
		}

		[HandleProcessCorruptedStateExceptions]
		public static string GetSerialNumber(int deviceIndex)
		{
			try
			{
				var serialNumber = new StringBuilder();
				var resultCode = CP210XSdkHelper.CP210x_GetProductString(
					dwDeviceIndex: deviceIndex,
					lpvDeviceString: serialNumber,
					dwFlags: CP210XSdkHelper.CP210x_RETURN_SERIAL_NUMBER);

				return resultCode != CP210XSdkHelper.CP210x_SUCCESS ? "" : serialNumber.ToString();
			}
			catch (Exception e)
			{
				return "";
			}
		}

		public static int GetDeviceIndex(string deviceId)
		{
			var devices = GetNumberOfDevices();

			if (devices <= 0)
			{
				return -1;
			}

			var deviceIndex = -1;

			for (var i = 0; i < devices; i++)
			{
				var serialNumber = CP210XHelper.GetSerialNumber(deviceIndex: i);

				if (!string.Equals(
						a: deviceId,
						b: serialNumber,
						comparisonType: StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}

				deviceIndex = i;

				break;
			}

			return deviceIndex;
		}

		#endregion Get Information

		#region Instructions

		[HandleProcessCorruptedStateExceptions]
		public static bool SetSerialNumber(IntPtr handle, string serialNumber)
		{
			try
			{
				var resultCode = CP210XSdkHelper.CP210x_SetSerialNumber(
					cyHandle: handle,
					lpvSerialNumberString: serialNumber,
					bSerialNumberStringLength: serialNumber.Length,
					bConvertToUnicode: true);

				return resultCode == CP210XSdkHelper.CP210x_SUCCESS;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		#endregion Instructions
	}
}
