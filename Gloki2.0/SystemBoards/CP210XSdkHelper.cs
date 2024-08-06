
namespace Gloki2._0.SystemBoards
{
	using System;
	using System.Runtime.InteropServices;
	using System.Text;

	public static class CP210XSdkHelper
	{
		#region CP210x Functions

		private const string DLL_MANUFACTURING = "CP210xManufacturing.dll";

		// Manufacturing

		[DllImport(dllName: DLL_MANUFACTURING)]
		public static extern int CP210x_GetNumDevices(out int lpdwNumDevices);
		[DllImport(dllName: DLL_MANUFACTURING)]
		// Flags can be CP210x_RETURN_SERIAL_NUMBER, CP210x_RETURN_DESCRIPTION, CP210x_RETURN_FULL_PATH
		public static extern int CP210x_GetProductString(int dwDeviceIndex, [In, Out] StringBuilder lpvDeviceString, int dwFlags);
		[DllImport(dllName: DLL_MANUFACTURING)]
		public static extern int CP210x_Open(int DeviceIndex, out IntPtr pcyHandle);
		[DllImport(dllName: DLL_MANUFACTURING)]
		public static extern int CP210x_Close(IntPtr cyHandle);
		[DllImport(dllName: DLL_MANUFACTURING)]
		public static extern int CP210x_SetSerialNumber(IntPtr cyHandle, string lpvSerialNumberString, int bSerialNumberStringLength, bool bConvertToUnicode = true);
		[DllImport(dllName: DLL_MANUFACTURING)]
		public static extern int CP210x_Reset(IntPtr cyHandle);

		#endregion CP210x Functions

		#region CP210x Structures

		[StructLayout(layoutKind: LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct CP210X_FIRMWARE
		{
			public short major;
			public short minor;
			public short build;
		}

		#endregion CP210x Structures

		#region CP210x Constants

		// GetProductString() function flags
		public const int CP210x_RETURN_SERIAL_NUMBER = 0x00;
		public const int CP210x_RETURN_DESCRIPTION = 0x01;
		public const int CP210x_RETURN_FULL_PATH = 0x02;

		// GetDeviceVersion() return codes
		public const int CP210x_CP2101_VERSION = 0x01;
		public const int CP210x_CP2102_VERSION = 0x02;
		public const int CP210x_CP2103_VERSION = 0x03;
		public const int CP210x_CP2104_VERSION = 0x04;
		public const int CP210x_CP2105_VERSION = 0x05;
		public const int CP210x_CP2108_VERSION = 0x08;
		public const int CP210x_CP2109_VERSION = 0x09;
		public const int CP210x_CP2102N_QFN28_VERSION = 0x20;
		public const int CP210x_CP2102N_QFN24_VERSION = 0x21;
		public const int CP210x_CP2102N_QFN20_VERSION = 0x22;

		// Return codes									
		public const int CP210x_SUCCESS = 0x00;
		public const int CP210x_DEVICE_NOT_FOUND = 0xFF;
		public const int CP210x_INVALID_HANDLE = 0x01;
		public const int CP210x_INVALID_PARAMETER = 0x02;
		public const int CP210x_DEVICE_IO_FAILED = 0x03;
		public const int CP210x_FUNCTION_NOT_SUPPORTED = 0x04;
		public const int CP210x_GLOBAL_DATA_ERROR = 0x05;
		public const int CP210x_FILE_ERROR = 0x06;
		public const int CP210x_COMMAND_FAILED = 0x08;
		public const int CP210x_INVALID_ACCESS_TYPE = 0x09;

		// Buffer size limits
		//
		// CP2101/2/3/4/9
		public const int CP210x_MAX_DEVICE_STRLEN = 256;
		public const int CP210x_MAX_PRODUCT_STRLEN = 126;
		public const int CP210x_MAX_MANUFACTURER_STRLEN = 45;
		public const int CP210x_MAX_SERIAL_STRLEN = 63;
		public const int CP210x_MAX_MAXPOWER = 250;

		// Mask and Latch value bit definitions
		public const int CP210x_GPIO_0 = 0x0001;
		public const int CP210x_GPIO_1 = 0x0002;
		public const int CP210x_GPIO_2 = 0x0004;
		public const int CP210x_GPIO_3 = 0x0008;
		public const int CP210x_GPIO_4 = 0x0010;
		public const int CP210x_GPIO_5 = 0x0020;
		public const int CP210x_GPIO_6 = 0x0040;
		public const int CP210x_GPIO_7 = 0x0080;
		public const int CP210x_GPIO_8 = 0x0100;
		public const int CP210x_GPIO_9 = 0x0200;
		public const int CP210x_GPIO_10 = 0x0400;
		public const int CP210x_GPIO_11 = 0x0800;
		public const int CP210x_GPIO_12 = 0x1000;
		public const int CP210x_GPIO_13 = 0x2000;
		public const int CP210x_GPIO_14 = 0x4000;
		public const int CP210x_GPIO_15 = 0x8000;

		#endregion CP210x Constants
	}
}
