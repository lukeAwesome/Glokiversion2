using Gloki2._0.Enum;
using Gloki2._0.SystemBoards;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gloki2._0.UI
{
	/// <summary>
	/// Interaction logic for LightingControlsTestPage.xaml
	/// </summary>
	/// 

	public partial class LightingControlsTestPage : UserControl
	{

		private int BoardRateConstant = 115200;
		private bool EncryptionLayerOn = false;
		public hdlc myHDLC = new hdlc();
		private int PacketOutCounter;
		private int HDLCPacketOutCounter;
		private int HDLCPacketInCounter;
		private string Text;
		private int[] DataOut = new int[800];
		private int[] HDLCDataIn = new int[2000];
		private int[] HDLCDataOut = new int[2000];
		private int HDLCDataInLength;
		private int HDLCDataOutLength;
		private int[] HDLCTestData = new int[200];
		private int HDLCTestDataLen;
		private int HDLCTestState;
		private int HDLCTestingFlag;
		private int HDLCTestingTimeout;
		private int HDLCEncTestFlag;
		private int ProcessorNumber;
		private int CertPayloadTimeout;
		private int MAX_CERT_PAYLOAD = 70;//30;
		private bool IncommingCertMessage_Flag = false;
		private int IncommingCertMessage_Sequnce = 0;
		private int IncommingCertMessage_Result = 0;
		private int IncommingCertMessage_ExpectedSequnce = 0;
		private bool SendCertFlag = false;
		private int SendCertState = 0;
		private int CertEngineIndex, CertEngineFileLength;
		private bool SendCertTheEnd;

		private IntPtr Cp201xHandle;// = IntPtr.Zero;
		private Int32 Cp201xDeviceNum;
		private string TamperPortName;
		private string TamperApplicationFileName;
		private string fileContent;// = string.Empty;
		private String ESPPortName;
		private string ESPApplicationFileName1;
		private string ESPApplicationFileName2;
		private int ESPProgramExitCode;

		public LightingControlsTestPage()
		{
			InitializeComponent();
			try { 
			InitLightingFunctions();
			CPLight.IsChecked = true;
			DSLight.IsChecked = true;
			ISLight.IsChecked = true;
			FCLight.IsChecked = true;
			FSLight.IsChecked = true;
			TSLight.IsChecked = true;
			} catch(Exception ex){ }

		}

		private void DoneButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new HomeScreen());
		}

		private void PrepareHDLCFrame(int StationAddress)
		{
			unsafe
			{
				int Result;
				int test;
				int Length;
				int RecoveredLength;
				int[] RecoveredData = new int[512];
				int[] SourceData = new int[512];
				int[] Destination = new int[512];
				byte[] DataToSend = new byte[512];
				yahdlc_control_t MyControlDataTx;
				yahdlc_control_t MyControlDataRx;
				Yahdlc_state_t Mystate;

				for (int i = 0; i < HDLCDataInLength; i++)
				{
					SourceData[i] = HDLCDataIn[i];
				}

				fixed (int* P = SourceData)
				{
					fixed (int* DestPntr = Destination)
					{
						fixed (int* RecoveredPntr = RecoveredData)
						{
							Result = myHDLC.yahdlc_frame_stx_etx_data(StationAddress, &MyControlDataTx, P, HDLCDataInLength, DestPntr, &Length);
							for (int i = 0; i < Length; i++)
							{
								HDLCDataOut[i] = Destination[i];
							}
							HDLCDataOutLength = Length;
						}
					}
				}
			}
		}

		private void InitLightingFunctions()
		{
			int Length;
			byte[] DataToSend = new byte[300];
			byte[] EndDataToSend = new byte[300];
			byte[] bytes = new byte[200];
			Crypto myCrypto = new Crypto();
			System.IO.Ports.SerialPort LightingBoardESP32 = new System.IO.Ports.SerialPort();
			LightingBoardESP32.PortName = GetDeviceComPort<String>(BoardType.LightingControlLite);
			LightingBoardESP32.BaudRate = Convert.ToInt32(BoardRateConstant);
			LightingBoardESP32.Open();

			LightingJSONCommands LightingCommand = new LightingJSONCommands()
			{
				ID = "100",
				Cmd = "Ch_1",
				Act = "ON",
				Pat = "1",
			};
			LightingCommand.ID = PacketOutCounter.ToString();
			PacketOutCounter++;
			string stringjson = JsonConvert.SerializeObject(LightingCommand);
			bytes = Encoding.ASCII.GetBytes(stringjson);
			//-----------------------------------------------------------------
			if (EncryptionLayerOn == true)
			{
				int ReturnLength = myCrypto.FrameCreate(bytes, EndDataToSend, bytes.Length);
				//-----------------------------------------------------------------
				for (int i = 0; i < ReturnLength; i++)
				{
					HDLCDataIn[i] = EndDataToSend[i]; //(int)bytes[i];
				}
				HDLCDataInLength = ReturnLength;
				PrepareHDLCFrame(0xFE);
			}
			else
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					HDLCDataIn[i] = bytes[i]; //(int)bytes[i];
				}
				HDLCDataInLength = bytes.Length;
				PrepareHDLCFrame(0xFE);
			}

			for (int i = 0; i < HDLCDataOutLength; i++)
			{
				// DataToSend[i].Equals(Destination[i]);
				DataToSend[i] = (byte)HDLCDataOut[i];
			}

			/////////OPEN LIGHT PORT/////////

			//////LIGHT CHANEL 1
			if (LightingBoardESP32.IsOpen)
			{
				LightingBoardESP32.Write(DataToSend, 0, HDLCDataOutLength);
				HDLCPacketOutCounter++;
			}
			Task.Delay(1000).Wait();
			/////LIGHT CHANNEL 2
			LightingCommand = new LightingJSONCommands()
			{
				ID = "100",
				Cmd = "Ch_2",
				Act = "ON",
				Pat = "1",
			};
			stringjson = JsonConvert.SerializeObject(LightingCommand);
			bytes = Encoding.ASCII.GetBytes(stringjson);
			//-----------------------------------------------------------------
			if (EncryptionLayerOn == true)
			{
				int ReturnLength = myCrypto.FrameCreate(bytes, EndDataToSend, bytes.Length);
				//-----------------------------------------------------------------
				for (int i = 0; i < ReturnLength; i++)
				{
					HDLCDataIn[i] = EndDataToSend[i]; //(int)bytes[i];
				}
				HDLCDataInLength = ReturnLength;
				PrepareHDLCFrame(0xFE);
			}
			else
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					HDLCDataIn[i] = bytes[i]; //(int)bytes[i];
				}
				HDLCDataInLength = bytes.Length;
				PrepareHDLCFrame(0xFE);
			}

			for (int i = 0; i < HDLCDataOutLength; i++)
			{
				// DataToSend[i].Equals(Destination[i]);
				DataToSend[i] = (byte)HDLCDataOut[i];
			}
			if (LightingBoardESP32.IsOpen)
			{
				LightingBoardESP32.Write(DataToSend, 0, HDLCDataOutLength);
				HDLCPacketOutCounter++;
			}
			Task.Delay(1000).Wait();

			/////LIGHT CHANNEL 3
			LightingCommand = new LightingJSONCommands()
			{
				ID = "100",
				Cmd = "Ch_3",
				Act = "ON",
				Pat = "1",
			};
			stringjson = JsonConvert.SerializeObject(LightingCommand);
			bytes = Encoding.ASCII.GetBytes(stringjson);
			//-----------------------------------------------------------------
			if (EncryptionLayerOn == true)
			{
				int ReturnLength = myCrypto.FrameCreate(bytes, EndDataToSend, bytes.Length);
				//-----------------------------------------------------------------
				for (int i = 0; i < ReturnLength; i++)
				{
					HDLCDataIn[i] = EndDataToSend[i]; //(int)bytes[i];
				}
				HDLCDataInLength = ReturnLength;
				PrepareHDLCFrame(0xFE);
			}
			else
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					HDLCDataIn[i] = bytes[i]; //(int)bytes[i];
				}
				HDLCDataInLength = bytes.Length;
				PrepareHDLCFrame(0xFE);
			}

			for (int i = 0; i < HDLCDataOutLength; i++)
			{
				// DataToSend[i].Equals(Destination[i]);
				DataToSend[i] = (byte)HDLCDataOut[i];
			}
			if (LightingBoardESP32.IsOpen)
			{
				LightingBoardESP32.Write(DataToSend, 0, HDLCDataOutLength);
				HDLCPacketOutCounter++;
			}
			Task.Delay(1000).Wait();

			/////LIGHT CHANNEL 4
			LightingCommand = new LightingJSONCommands()
			{
				ID = "100",
				Cmd = "Ch_4",
				Act = "ON",
				Pat = "1",
			};
			stringjson = JsonConvert.SerializeObject(LightingCommand);
			bytes = Encoding.ASCII.GetBytes(stringjson);
			//-----------------------------------------------------------------
			if (EncryptionLayerOn == true)
			{
				int ReturnLength = myCrypto.FrameCreate(bytes, EndDataToSend, bytes.Length);
				//-----------------------------------------------------------------
				for (int i = 0; i < ReturnLength; i++)
				{
					HDLCDataIn[i] = EndDataToSend[i]; //(int)bytes[i];
				}
				HDLCDataInLength = ReturnLength;
				PrepareHDLCFrame(0xFE);
			}
			else
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					HDLCDataIn[i] = bytes[i]; //(int)bytes[i];
				}
				HDLCDataInLength = bytes.Length;
				PrepareHDLCFrame(0xFE);
			}

			for (int i = 0; i < HDLCDataOutLength; i++)
			{
				// DataToSend[i].Equals(Destination[i]);
				DataToSend[i] = (byte)HDLCDataOut[i];
			}
			if (LightingBoardESP32.IsOpen)
			{
				LightingBoardESP32.Write(DataToSend, 0, HDLCDataOutLength);
				HDLCPacketOutCounter++;
			}
			Task.Delay(1000).Wait();

			/////LIGHT CHANNEL 5
			LightingCommand = new LightingJSONCommands()
			{
				ID = "100",
				Cmd = "Ch_5",
				Act = "ON",
				Pat = "1",
			};
			stringjson = JsonConvert.SerializeObject(LightingCommand);
			bytes = Encoding.ASCII.GetBytes(stringjson);
			//-----------------------------------------------------------------
			if (EncryptionLayerOn == true)
			{
				int ReturnLength = myCrypto.FrameCreate(bytes, EndDataToSend, bytes.Length);
				//-----------------------------------------------------------------
				for (int i = 0; i < ReturnLength; i++)
				{
					HDLCDataIn[i] = EndDataToSend[i]; //(int)bytes[i];
				}
				HDLCDataInLength = ReturnLength;
				PrepareHDLCFrame(0xFE);
			}
			else
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					HDLCDataIn[i] = bytes[i]; //(int)bytes[i];
				}
				HDLCDataInLength = bytes.Length;
				PrepareHDLCFrame(0xFE);
			}

			for (int i = 0; i < HDLCDataOutLength; i++)
			{
				// DataToSend[i].Equals(Destination[i]);
				DataToSend[i] = (byte)HDLCDataOut[i];
			}
			if (LightingBoardESP32.IsOpen)
			{
				LightingBoardESP32.Write(DataToSend, 0, HDLCDataOutLength);
				HDLCPacketOutCounter++;
			}
			Task.Delay(1000).Wait();

			/////LIGHT CHANNEL 6
			LightingCommand = new LightingJSONCommands()
			{
				ID = "100",
				Cmd = "Ch_6",
				Act = "ON",
				Pat = "1",
			};
			stringjson = JsonConvert.SerializeObject(LightingCommand);
			bytes = Encoding.ASCII.GetBytes(stringjson);
			//-----------------------------------------------------------------
			if (EncryptionLayerOn == true)
			{
				int ReturnLength = myCrypto.FrameCreate(bytes, EndDataToSend, bytes.Length);
				//-----------------------------------------------------------------
				for (int i = 0; i < ReturnLength; i++)
				{
					HDLCDataIn[i] = EndDataToSend[i]; //(int)bytes[i];
				}
				HDLCDataInLength = ReturnLength;
				PrepareHDLCFrame(0xFE);
			}
			else
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					HDLCDataIn[i] = bytes[i]; //(int)bytes[i];
				}
				HDLCDataInLength = bytes.Length;
				PrepareHDLCFrame(0xFE);
			}

			for (int i = 0; i < HDLCDataOutLength; i++)
			{
				// DataToSend[i].Equals(Destination[i]);
				DataToSend[i] = (byte)HDLCDataOut[i];
			}
			if (LightingBoardESP32.IsOpen)
			{
				LightingBoardESP32.Write(DataToSend, 0, HDLCDataOutLength);
				HDLCPacketOutCounter++;
				Task.Delay(1000).Wait();
			}
			LightingBoardESP32.Close();
		}

		public static string GetDeviceComPort<T>(BoardType boardType)
		{
			try
			{
				var deviceInfo = DeviceHelper.GetUsbDeviceInfo<T>(
					vendorId: 4292,
					productId: 60000,
					deviceClass: PnpDeviceClass.Ports,
					deviceId: SystemBoards.EnumHelper.GetDescription(boardType));

				if (!deviceInfo.IsDetected || string.IsNullOrWhiteSpace(value: deviceInfo.Product))
				{
					return "";
				}

				// Product name e.g. Silicon Labs CP210x USB to UART Bridge (COM3)
				var match = new Regex(pattern: "\\((COM\\d+)\\)$").Match(input: deviceInfo.Product);

				if (!match.Success || string.IsNullOrWhiteSpace(value: match.Value))
				{
					return "";
				}

				return match.Value.Trim('(', ')');
			}
			catch (Exception e)
			{
				return "";
			}
		}
	}
}
