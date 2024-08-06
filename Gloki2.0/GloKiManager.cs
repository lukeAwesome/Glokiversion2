using Gloki2._0.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Gloki2._0
{
	public sealed class GloKiManager
	{
		private static GloKiManager _glokiInstance;

		// Automation for starting and stopping tests
		private bool _automate = false;
		private bool _testComplete = false;

		public bool TestPass = true;
		public string TestId = "";
		public int TestCycle = 0;
		public int CycleTimes = 0;

		// Overall health indicators for components tested
		// Start off healthy and then deteriorate based on errors encountered.
		public string FingerprintScannerHealth = "NOT TESTED";
		public string CardPrinterHealth = "NOT TESTED";
		public string IdScannerHealth = "NOT TESTED";
		public string DocumentScannerHealth = "NOT TESTED";
		public string CameraHealth = "NOT TESTED";
		public string LightingBoardHealth = "NOT TESTED";
		public string PowerBoardHealth = "NOT TESTED";
		public string TamperBoardHealth = "NOT TESTED";
		public string SensorBoardHealth = "NOT TESTED";

		// Test complete Indicators
		public bool FingerPrintTestComplete = false;
		public bool FirstFingerPrintTestComplete = false;
		public bool CardPrinterTestComplete = false;
		public bool IdScannerTestComplete = false;
		public bool DocumentScannerTestComplete = false;
		public bool ImageTestComplete = false;
		public bool LightingBoardTestComplete = false;
		public bool PowerBoardTestComplete = false;
		public bool SensorBoardTestComplete = false;
		public bool TamperBoardTestComplete = false;
		public String KioskIdentity = "";

		// Lighting Test
		public bool LightingTestFail = false;

		//Lighting Test Checklist
		public bool CardPrinterLightTestComplete = false;
		public bool DocumentScannerLightTestComplete = false;
		public bool TouchScreenLightTestComplete = false;
		public bool FingerprintScannerLightTestComplete = false;
		public bool KioskUnitLightTestComplete = false;
		public bool IdentityScannerLightTestComplete = false;
		public bool FaceCameraLightTestComplete = false;
		public bool POSDeviceLightTestComplete = false;

		//Sensor Test Checklist
		public bool PanelCoverFanTestComplete = false;
		public bool MiddleRearRightFanTestComplete = false;
		public bool MiddleRearLeftFanTestComplete = false;
		public bool FingerPrintScannerFanTestComplete = false;
		public bool BatteryBoxFrontFanTestComplete = false;
		public bool BatteryBoxBackFanTestComplete = false;
		public bool TopLeftFanTestComplete = false;
		public bool TopRightFanTestComplete = false;

		public void ResetTestComponentComplete()
		{
			FingerPrintTestComplete = false;
			CardPrinterTestComplete = false;
			IdScannerTestComplete = false;
			DocumentScannerTestComplete = false;
			ImageTestComplete = true;
			LightingBoardTestComplete = false;
			PowerBoardTestComplete = false;
		}

		// Test types and test events
		private List<InventoryComponent> _inventoryComponents = new List<InventoryComponent>();

		public List<EventEntry> MainResults = new List<EventEntry>();
		public List<EventEntry> TestResults = new List<EventEntry>();
		public List<EventEntry> CardPrinterResults = new List<EventEntry>();
		public List<EventEntry> FingerPrintScannerResults = new List<EventEntry>();
		public List<EventEntry> DocumentScannerResults = new List<EventEntry>();
		public List<EventEntry> IdentityScannerResults = new List<EventEntry>();
		public List<EventEntry> OverallTestResults = new List<EventEntry>();
		public List<EventEntry> LightingBoardResults = new List<EventEntry>();
		public List<EventEntry> TamperBoardResults = new List<EventEntry>();
		public List<EventEntry> SensorBoardResults = new List<EventEntry>();

		// TestType cycles and current test indicator
		public int TestCycles { get; set; }
		public int CurrentCycle { get; set; }

		// Image results from test
		public byte[] FingerprintImage { get; set; }
		public BitmapSource FingerprintImageActualImage { get; set; }
		public byte[] IdentityScannerNormalImage { get; set; }
		public byte[] IdentityScannerUltravioletImage { get; set; }
		public byte[] IdentityScannerInfraredImage { get; set; }
		public byte[] DocumentPrimaryImage { get; set; }
		public byte[] DocumentSecondaryImage { get; set; }

		public List<EventEntry> GetCardPrinterResults()
		{
			return CardPrinterResults;
		}

		public void AddCardPrinterResult(EventEntry value)
		{
			CardPrinterResults.Add(item: value);
		}

		public List<EventEntry> GetFingerPrintScannerResults()
		{
			return FingerPrintScannerResults;
		}

		public void AddFingerPrintScannerResult(EventEntry value)
		{
			FingerPrintScannerResults.Add(item: value);
		}

		public List<EventEntry> GetDocumentScannerResults()
		{
			return DocumentScannerResults;
		}

		public void AddDocumentScannerResult(EventEntry value)
		{
			DocumentScannerResults.Add(item: value);
		}

		public List<EventEntry> GetIdentityScannerResults()
		{
			return IdentityScannerResults;
		}

		public void AddIdentityScannerResult(EventEntry value)
		{
			IdentityScannerResults.Add(item: value);
		}

		public List<EventEntry> GetLightingBoardResults()
		{
			return LightingBoardResults;
		}

		public void AddLightingBoardResult(EventEntry value)
		{
			LightingBoardResults.Add(item: value);
		}

		public List<EventEntry> GetTamperBoardResults()
		{
			return TamperBoardResults;
		}

		public void AddTamperBoardResult(EventEntry value)
		{
			TamperBoardResults.Add(item: value);
		}

		public List<EventEntry> GetSensorBoardResults()
		{
			return SensorBoardResults;
		}

		public void AddSensorBoardResult(EventEntry value)
		{
			SensorBoardResults.Add(item: value);
		}

		public List<EventEntry> GetMainResults()
		{
			return MainResults;
		}

		public void AddMainResult(EventEntry value)
		{
			MainResults.Add(item: value);
		}

		public List<EventEntry> GetTestResults()
		{
			return TestResults;
		}

		public void AddTestResult(EventEntry value)
		{
			TestResults.Add(item: value);
		}


		public List<EventEntry> GetOverallTestResults()
		{
			return OverallTestResults;
		}

		public void AddOverallTestResult(EventEntry value)
		{
			OverallTestResults.Add(item: value);
		}

		public List<InventoryComponent> GetAllConnectedComponents()
		{
			return _inventoryComponents;
		}

		public void AddConnectedComponent(InventoryComponent value)
		{
			_inventoryComponents.Add(item: value);
		}

		private GloKiManager(string kioskId, string kioskString)
		{
			KioskId = kioskId;
			KioskNumber = kioskString;
		}

		public string KioskId { get; set; }
		public string KioskTestTimestampId { get; set; }
		public string KioskNumber { get; set; }
		public string KioskSerial { get; set; }
		public string KioskName { get; set; }

		public static GloKiManager Instance
		{
			get
			{
				if (_glokiInstance == null)
				{
					Create();
				}
				return _glokiInstance;
			}
		}

		public static void Create(string arg1, string arg2)
		{
			if (_glokiInstance != null)
			{
				throw new Exception(message: "Object already created");
			}
			_glokiInstance = new GloKiManager(kioskId: arg1, kioskString: arg2);
		}

		public static void Create()
		{
			if (_glokiInstance != null)
			{
				throw new Exception(message: "Object already created");
			}
			_glokiInstance = new GloKiManager();
		}

		private GloKiManager()
		{
		}

		public void SetIsAutomated(bool isAutomated)
		{
			_automate = isAutomated;
		}

		public bool GetIsAutomated()
		{
			return _automate;
		}

		public void SetKioskTimestampTestID(string kioskTimestamp)
		{
			KioskTestTimestampId = kioskTimestamp;
		}

		public string KioskTimestampTestID()
		{
			return KioskTestTimestampId;
		}

		public void SetKioskTimestamp(string timestamp)
		{
			TestId = timestamp;
		}

		public string GetKioskTimestamp()
		{
			return TestId;
		}


		public void SetIsTestCompleted(bool isTestDone)
		{
			_testComplete = isTestDone;
		}

		public bool GetTestComplete()
		{
			return _testComplete;
		}

		public void ResetTestInstance()
		{
			// Automation for starting and stopping tests
			_automate = false;
			_testComplete = false;
			TestPass = true;
			TestCycle = 0;
			CurrentCycle = 0;

			// Overall Health Indicators for components Tested
			// Start off healthy and then deteriorate based on errors encountered.
			FingerprintScannerHealth = "NOT TESTED";
			CardPrinterHealth = "NOT TESTED";
			IdScannerHealth = "NOT TESTED";
			DocumentScannerHealth = "NOT TESTED";
			CameraHealth = "NOT TESTED";
			LightingBoardHealth = "NOT TESTED";
			PowerBoardHealth = "NOT TESTED";

			// Test Complete Indicators
			FingerPrintTestComplete = false;
			CardPrinterTestComplete = false;
			IdScannerTestComplete = false;
			DocumentScannerTestComplete = false;
			ImageTestComplete = true;
			LightingBoardTestComplete = false;
			PowerBoardTestComplete = false;

			FingerprintImage = null;

			// Test types and test events
			CardPrinterResults = new List<EventEntry>();
			FingerPrintScannerResults = new List<EventEntry>();
			DocumentScannerResults = new List<EventEntry>();
			IdentityScannerResults = new List<EventEntry>();
			OverallTestResults = new List<EventEntry>();
			LightingBoardResults = new List<EventEntry>();
			TamperBoardResults = new List<EventEntry>();
			SensorBoardResults = new List<EventEntry>();
			MainResults = new List<EventEntry>();
			_inventoryComponents = new List<InventoryComponent>();
	}

		private ObservableCollection<LogEntry> _logEntries = new ObservableCollection<LogEntry>();

		public void CreateLogEntry(string detail)
		{

		if(_logEntries == null)
			{
				_logEntries = new ObservableCollection<LogEntry>();
			}

			var logEntry = LogEntry.Create(
				dateTime: DateTime.Now.ToString(format: "yyyy-MM-dd HH:mm:ss.fff"),
				detail: detail);

			_logEntries.Add(item: logEntry);
		}

		public ObservableCollection<LogEntry> GetEventHistory()
		{
			return _logEntries;
		}



	}
}
