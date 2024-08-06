using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gloki2._0.SystemBoards
{

	public static class CommunicationHelper
	{
		// Silicon Labs CP2102N
		public static readonly int VENDOR_ID = 4292;
		public static readonly int PRODUCT_ID = 60000;
		public static readonly int PORT_BAUD_RATE = 115000;
		public static readonly int PORT_DATA_BITS = 8;
		public static readonly Parity PORT_PARITY = Parity.None;

		private static readonly object _commandLock = new object();

		public static SerialPort OpenDeviceComPort(string port)
		{
			try
			{
				var serialPort = new SerialPort(
					portName: port,
					baudRate: PORT_BAUD_RATE,
					parity: PORT_PARITY,
					dataBits: PORT_DATA_BITS);

				serialPort.Open();

				return serialPort;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		public static bool CloseDeviceComPort(string port)
		{
			try
			{
				var serialPort = new SerialPort(
					portName: port,
					baudRate: PORT_BAUD_RATE,
					parity: PORT_PARITY,
					dataBits: PORT_DATA_BITS);

				return CloseDeviceComPort(port: serialPort);
			}
			catch (Exception e)
			{
				return false;
			}
		}

		public static bool CloseDeviceComPort(SerialPort port)
		{
			try
			{
				port.Close();

				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		public static bool IsDeviceComPortOpen(string port)
		{
			try
			{
				var serialPort = new SerialPort(
					portName: port,
					baudRate: PORT_BAUD_RATE,
					parity: PORT_PARITY,
					dataBits: PORT_DATA_BITS);

				return serialPort.IsOpen;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		public static string ExecuteDeviceCommand(string port, string command)
		{
			lock (_commandLock)
			{
				try
				{
					var serialPort = OpenDeviceComPort(port: port);
					var receivedEof = false;
					var data = "";

					void DataReceiverHandler(object sender, SerialDataReceivedEventArgs e)
					{
						try
						{
							var openPort = (SerialPort)sender;
							data = openPort.ReadLine();
							receivedEof = true;
						}
						catch (Exception ex)
						{
							//
						}
					}

					serialPort.DataReceived += DataReceiverHandler;

					serialPort.WriteLine(text: command);

					// Safeguard against infinite loop
					var loopCount = 0;
					const int loopMaxCount = 250; // 2.5 seconds max wait time

					while (!receivedEof && loopCount < loopMaxCount)
					{
						Task.Delay(millisecondsDelay: 10).Wait();
						loopCount++;
					}

					serialPort.DataReceived -= DataReceiverHandler;

					CloseDeviceComPort(port: serialPort);

					return data;
				}
				catch (Exception e)
				{
					return null;
				}
			}
		}
	}
}
