
namespace Gloki2._0.SystemBoards
{
	using System;
	using System.Text;
	using System.Runtime.InteropServices;

	public class cp210x
	{
		/*        [DllImport("CP210xRuntime.dll", EntryPoint = "CP210xRT_GetDeviceProductString",
                        CharSet = CharSet.Ansi)]
                public static extern int NewgetDeviceProductString(
                        [In, Out] ref IntPtr deviceHandle,
                        [In, Out] ref StringBuilder Product,
                        [Out]  IntPtr Length,
                        [In] bool ConvertToASCII);

                [DllImport("CP210xRuntime.dll", EntryPoint = "CP210xRT_GetDeviceSerialNumber",
                CharSet = CharSet.Ansi)]
                public static extern int NewgetDeviceSerialNumber(
                [In, Out] ref IntPtr deviceHandle,
                [In, Out] ref StringBuilder Product,
                [In, Out] ref byte Length,
                [In, Out] bool ConvertToASCII);

                */
		[DllImport("CP210xManufacturing.dll")]
		private static extern Int32 CP210x_GetNumDevices(ref Int32 numOfDevices);
		public Int32 GetNumDevices(ref Int32 numOfDevices)
		{
			return CP210x_GetNumDevices(ref numOfDevices);
		}

		[DllImport("CP210xManufacturing.dll")]
		private static extern Int32 CP210x_Open(Int32 deviceNum, ref IntPtr handle);
		public Int32 Open(Int32 deviceNum, ref IntPtr handle)
		{
			return CP210x_Open(deviceNum, ref handle);
		}

		[DllImport("CP210xManufacturing.dll")]
		private static extern Int32 CP210x_Close(IntPtr handle);
		public Int32 Close(IntPtr handle)
		{
			return CP210x_Close(handle);
		}

		[DllImport("CP210xManufacturing.dll")]
		private static extern Int32 CP210x_GetPartNumber(IntPtr handle, Byte[] lpbPartNum);
		public Int32 GetPartNumber(IntPtr handle, Byte[] lpbPartNum)
		{
			return CP210x_GetPartNumber(handle, lpbPartNum);
		}
		//----------------------------------------------------------------------------------------
		[DllImport("CP210xRuntime.dll")]
		private static extern Int32 CP210xRT_WriteLatch(IntPtr handle, UInt16 mask, UInt16 latch);
		public Int32 WriteLatch(IntPtr handle, UInt16 mask, UInt16 latch)
		{
			return CP210xRT_WriteLatch(handle, mask, latch);
		}
		//----------------------------------------------------------------------------------------
		[DllImport("CP210xRuntime.dll")]
		private static extern Int32 CP210xRT_ReadLatch(IntPtr handle, UInt16[] lpLatch);
		public Int32 ReadLatch(IntPtr handle, UInt16[] lpLatch)
		{
			return CP210xRT_ReadLatch(handle, lpLatch);
		}
		//----------------------------------------------------------------------------------------
		[DllImport("CP210xRuntime.dll", CallingConvention = CallingConvention.StdCall)]
		private static extern Int32 CP210xRT_GetPartNumber(IntPtr cyHandle, out IntPtr lpbPartNum);
		public Int32 GetPartNumberRT(IntPtr cyHandle, out IntPtr lpbPartNum)
		{
			return CP210xRT_GetPartNumber(cyHandle, out lpbPartNum);
		}
		//----------------------------------------------------------------------------------------
		[DllImport("CP210xRuntime.dll")]
		private static extern Int32 CP210xRT_GetDeviceProductString(IntPtr cyHandle,
								[In, Out] StringBuilder Product,
								out IntPtr lpbLength,
								bool bConvertToASCII = true
								);
		public Int32 GetDeviceProductString(IntPtr cyHandle,
								[In, Out] StringBuilder Product,
						 out IntPtr lpbLength,
						 bool bConvertToASCII = true
						 )
		{
			return CP210xRT_GetDeviceProductString(cyHandle, Product, out lpbLength, bConvertToASCII);
		}
		//----------------------------------------------------------------------------------------
		//----------------------------------------------------------------------------------------
		//       [DllImport("CP210xManufacturing.dll")]
		[DllImport("CP210xManufacturing.dll")]
		private static extern Int32 CP210x_GetDeviceSerialNumber(IntPtr cyHandle,
								[In, Out] StringBuilder SerialNumber,
								out IntPtr lpbLength,
								bool bConvertToASCII = true
								);
		public Int32 GetDeviceSerialNumber(IntPtr cyHandle,
						 [In, Out] StringBuilder SerialNumber,
						 out IntPtr lpbLength,
						 bool bConvertToASCII = true
						 )
		{
			return CP210x_GetDeviceSerialNumber(cyHandle, SerialNumber, out lpbLength, bConvertToASCII);
		}
		//----------------------------------------------------------------------------------------
		[DllImport("CP210xRuntime.dll", CallingConvention = CallingConvention.StdCall)]
		private static extern Int32 CP210xRT_GetDeviceInterfaceString(IntPtr cyHandle,
								[In, Out] StringBuilder IFString,
								//                                out IntPtr lpProduct,
								out IntPtr lpbLength,
								bool bConvertToASCII = true
								);
		public Int32 GetDeviceInterfaceString(IntPtr cyHandle,
						 [In, Out] StringBuilder IFString,
						 out IntPtr lpbLength,
						 bool bConvertToASCII = true
						 )
		{
			return CP210xRT_GetDeviceInterfaceString(cyHandle, IFString, out lpbLength, bConvertToASCII);
		}

		[DllImport("CP210xManufacturing.dll")]
		private static extern Int32 CP210x_GetDeviceVid(IntPtr handle, out IntPtr IntPtr_Vid);
		public Int32 getDeviceVid(IntPtr handle, out IntPtr IntPtr_Vid)
		{
			return CP210x_GetDeviceVid(handle, out IntPtr_Vid);
		}
		[DllImport("CP210xManufacturing.dll")]
		private static extern Int32 CP210x_GetDevicePid(IntPtr handle, out IntPtr IntPtr_Pid);
		public Int32 getDevicePid(IntPtr handle, out IntPtr IntPtr_Pid)
		{
			return CP210x_GetDevicePid(handle, out IntPtr_Pid);
		}
		/*        [DllImport(CP210xManu, EntryPoint = "CP210x_GetDeviceVid")]
                public static extern int getDeviceVid(
                [In, Out] ref IntPtr deviceHandle,
                [In, Out] ref ushort Vid);

                [DllImport(CP210xManu, EntryPoint = "CP210x_GetDevicePid")]
                public static extern int getDevicePid(
                [In, Out] IntPtr deviceHandle,
                [In, Out] ref ushort Pid);*/

	}
}
