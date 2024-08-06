using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Gloki2._0.Enum;
using Gloki2._0.SystemBoards;
using Newtonsoft.Json;

namespace Gloki2._0.UI
{
	/// <summary>
	/// Interaction logic for SensorTestPage.xaml
	/// </summary>
	/// 

	public partial class SensorTestPage : UserControl
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

		public SensorTestPage()
		{
			InitializeComponent();
			try { 
			InitSensorFunction();
				BBFFan.IsChecked = true;
				TLFan.IsChecked = true;
				PanelFan.IsChecked = true;
				MRFan.IsChecked = true;
			} catch (Exception ex) { }
		}

		private void DoneButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new HomeScreen());
		}

		private void InitSensorFunction()
		{
			int Length;
			byte[] DataToSend = new byte[1000];
			byte[] EndDataToSend = new byte[1000];
			byte[] bytes = new byte[1000];
			byte[] bytes2 = new byte[200];
			byte[] bytes3 = new byte[1000];
			Crypto myCrypto = new Crypto();
			char[] RelayOutputs = new char[13];
			string Ouputs;


			// Resolve interace deviec
			cp210x myCP210x = new cp210x();
			Int32 retVal;
			UInt16[] latch = new UInt16[8];

			//------------------------------------------------------------------
			// Select the main processor interface
			retVal = myCP210x.Open(Cp201xDeviceNum, ref Cp201xHandle);
			retVal = myCP210x.ReadLatch(Cp201xHandle, latch);
			//MCU CTRL2 HIGH
			//retVal = myCP210x.WriteLatch(Cp201xHandle, 0x40, 0x40);
			//MCU CTRL1 LOW
			//retVal = myCP210x.WriteLatch(Cp201xHandle, 0x20, 0x20);
			//Task.Delay(400).Wait();
			//MCU CTRL2 LOW
			retVal = myCP210x.WriteLatch(Cp201xHandle, 0x40, 0x00);
			//MCU CTRL1 HIGH
			retVal = myCP210x.WriteLatch(Cp201xHandle, 0x20, 0x20);

			Task.Delay(500).Wait();
			retVal = myCP210x.Close(Cp201xHandle);

			System.IO.Ports.SerialPort SensorBoardESP32 = new System.IO.Ports.SerialPort();
			SensorBoardESP32.PortName = GetDeviceComPort<String>(BoardType.SensorLite);
			SensorBoardESP32.BaudRate = Convert.ToInt32(BoardRateConstant);

			SensorBoardESP32.Open();
			if (SensorBoardESP32.IsOpen)
			{
				myHDLC.yahdlc_get_data_resetRx();
			}
			else
			{

			}


			SensorJSONCommands SensorCommand = new SensorJSONCommands()
			{
				ID = "100",
				Cmd = "S_Fan",
			};

			SensorCommand.ID = PacketOutCounter.ToString();
			//Ouputs = RelayOutputs.ToString();
			// RelayOutputs = RelayOutputs[0].ToString() + RelayOutputs[1].ToString();
			//--------------------------------- FAN 1
			Ouputs = "1";
			//--------------------------------- FAN 2
			Ouputs += "1";
			//--------------------------------- FAN 3
			Ouputs += "1";
			//--------------------------------- FAN 4
			Ouputs += "1";
			//--------------------------------- FAN 5
			Ouputs += "0";
			//--------------------------------- FAN 6
			Ouputs += "0";
			//--------------------------------- FAN 7
			Ouputs += "0";
			//--------------------------------- FAN 8
			Ouputs += "0";

			SensorCommand.Fan = Ouputs;//RelayOutputs.ToString();
			PacketOutCounter++;

			string stringjson = JsonConvert.SerializeObject(SensorCommand);
			bytes = Encoding.ASCII.GetBytes(stringjson);
			//=============================================================================
			// Update Command: Header
			Length = 0;
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes3[Length++] = bytes[i];
			}
			//-----------------------------------------------------------------
			// Encrypt Payload
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
			//+++++++++++++++++++++++++++++++++++++++++++++++
			for (int i = 0; i < HDLCDataOutLength; i++)
			{
				// DataToSend[i].Equals(Destination[i]);
				DataToSend[i] = (byte)HDLCDataOut[i];
			}
			if (SensorBoardESP32.IsOpen)
			{
				SensorBoardESP32.Write(DataToSend, 0, HDLCDataOutLength);
				HDLCPacketOutCounter++;
			}
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
