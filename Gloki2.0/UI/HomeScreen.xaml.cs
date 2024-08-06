using System;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Tyme.Kihama.Common.Services.DTOs.System;
using Tyme.Kihama.Common.Services.Helpers;
using Tyme.Kihama.Sdk;
using Tyme.Kihama.Sdk.Events.System;
using Tyme.Kihama.Sdk.Interfaces.System;
using QRCoder;
using System.Windows.Data;
using Gloki2._0.Model;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Tyme.Kihama.Sdk.Helpers;
using Tyme.GlobalKiosk.Management.ApiClient.Api;
using KioskType = Tyme.Kihama.Common.Services.Enums.KioskType;
using Tyme.GlobalKiosk.Management.ApiClient.Model;
using OAuthFlow = Tyme.GlobalKiosk.Management.ApiClient.Client.Auth.OAuthFlow;
using Tyme.GlobalKiosk.Management.ApiClient.Client;
using System.Drawing.Imaging;
using System.Drawing;
using System;
using ControlzEx.Standard;
using System.Windows.Controls.Primitives;
using LogEntry = Gloki2._0.Model.LogEntry;
using Gloki2._0.Enum;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace Gloki2._0.UI
{
	/// <summary>
	/// Interaction logic for HomeScreen.xaml
	/// </summary>
	public partial class HomeScreen : UserControl
	{
		private readonly ISystemManagerProxy _systemManagerProxy = InternalProxyFactory.Create<ISystemManagerProxy>();
		private GloKiManager _grokiManager;
		private SystemMockConfiguration _mockConfiguration = new SystemMockConfiguration();
		private String QRCodeData = "";
		private String TestId = "";

		private int BoardRateConstant = 115200;
		private bool EncryptionLayerOn = false;
		public SystemBoards.hdlc myHDLC = new SystemBoards.hdlc();
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

		public HomeScreen()
		{
			InitializeComponent();
			_grokiManager = GloKiManager.Instance;
			InitializeLogEntryGrid();
			InitalizeMainPageData();
			DisplayEventEntries();
			LogEntries.ItemsSource = _logEntries;

			try
			{
				InitLightingFunctions();
				InitSensorFunction();
			} 
			catch (Exception ex) { }


			if (_grokiManager.GetTestComplete()){

				//Evaluate The Results and display them on screen// 
				SetupResultsView();
				SetupFingerprintScannerResultsPage();
				SetupCardPrinterResultsPage();
				SetupDocumentScannerResultsPage();
				SetupSensorBoardResultsPage();
				SetupIdentityScannerResultsPage();
				SetupLightingBoardResultsPage();
				SetupTamperBoardResultsPage();

				SetupComponentHealthIndicators();
				CreateKMSTelephone();

				bool? isChecked = ToggleTestMode.IsChecked;
				if (!isChecked.Value)
				{
				// Assembly Test
				}

				_grokiManager.SetIsTestCompleted(false);

			}
		}

		private void InitalizeMainPageData()
		{
			KioskSerial.Text = "Device Serial number : " + GetSystemSerialNumber();
			OperatingSystem.Text = "Windows Version number : " + GetOSVersion();
			UpTime.Text = "Kiosk Uptime : " + GetUpTime();
			SetupDetailsScreen();
		}

		private async void SetupDetailsScreen()
		{
				await AppHelper.ManageSessionAsync();
			await GetKioskAvailability();
			await GetKioskInformation();
			TestId = DateTime.Now.Ticks.ToString();
			QRCodeData += "|" + TestId;
			BitmapSource qrCodeBitmap = GenerateQRCode(QRCodeData, 300);
			qrCodeImage.Source = qrCodeBitmap;
			loadingDisplay.Visibility = Visibility.Hidden;
		}

		private async Task GetKioskAvailability()
		{
			try
			{
				await AppHelper.ManageSessionAsync();
				var result = await _systemManagerProxy.GetDeviceAvailabilityStatusAsync();

				HandleGetDeviceAvailabilityStatus(sender: this, e: result);
			}
			catch (Exception exc)
			{
			}
		}

		private async Task GetKioskInformation()
		{
			try
			{

				var result = await _systemManagerProxy.GetDeploymentConfigurationAsync();

				var environment = AppHelper.GetKihamaEnvironment();

				if (environment.ToLowerInvariant() == "production" &&
					(result.DeploymentId == "00000000-0000-0000-0000-000000000000" ||
					 result.KioskNumber.StartsWith(value: "GK99-") ||
					 string.IsNullOrWhiteSpace(value: result.KioskNumber)))
				{
					DeploymentStatus.Text = "THIS KIOSK IS NOT DEPLOYED";
				}
				else
				{
					DeploymentStatus.Text = "KIOSK DEPLOYED";
				}

				//Add Data to qrCode
				QRCodeData += "|" + result.DeviceName + "|" + result.DeviceName + "|" + result.KioskNumber;

				DeploymentId.Text = $"Deployment ID: {result.DeploymentId}";
				CreateLogEntry($"Deployment ID: {result.DeploymentId}");

				KioskNumer.Text = $"Kiosk Number: {result.KioskNumber}";
				CreateLogEntry($"Kiosk Number: {result.KioskNumber}");

				KioskType.Text = $"Kiosk Type: {result.KioskType.GetDescription()}";
				CreateLogEntry($"Kiosk Type: {result.KioskType.GetDescription()}");

				DeviceName.Text = $"Device Name: {result.DeviceName}";
				CreateLogEntry($"Device Name: {result.DeviceName}");

				DeviceId.Text = $"Device ID: {result.DeviceIdentifier}";
				CreateLogEntry($"Device ID: {result.DeviceIdentifier}");

				OperatingSystemName.Text = $"Operating System: {result.DeviceOperatingSystem}";
				CreateLogEntry($"Operating System: {result.DeviceOperatingSystem}");

				DeviceMacAddress.Text = $"MAC Address: {result.DeviceNetworkMacAddress}";
				CreateLogEntry($"MAC Address: {result.DeviceNetworkMacAddress}");

				Organization.Text = $"Organisation: {result.OrganisationName}";
				CreateLogEntry($"Organisation: {result.OrganisationName}");

				CountryCode.Text = $"Country Code: {result.OrganisationCountryCode}";
				CreateLogEntry($"Country Code: {result.OrganisationCountryCode}");
				//Details.Content += $"Commercial Partner: {result.OrganisationCommercialPartner}\n";
				//Details.Content += $"Presence Category: {result.OrganisationPresenceCategory}\n";
				//Details.Content += $"Site: {result.OrganisationSiteName}\n";
				CreateLogEntry($"Site: {result.OrganisationSiteName}");
				//Details.Content += $"Sit Type: {result.OrganisationSiteType}\n";
				//Details.Content += $"Site Reference: {result.OrganisationSitePartnerReference}\n";

				if (result.OrganisationSiteAddress != null && result.OrganisationSiteAddress.Count > 0)
				{
					for (var i = 0; i < result.OrganisationSiteAddress.Count; i++)
					{
						var address = result.OrganisationSiteAddress[index: i];
						//Details.Content += $"Site Address Part {i + 1}: {address.Key} - {address.Value}\n";
					}
				}

				if (result.OrganisationSiteProperties == null || result.OrganisationSiteProperties.Count <= 0)
				{
					return;
				}

				for (var i = 0; i < result.OrganisationSiteProperties.Count; i++)
				{
					var property = result.OrganisationSiteProperties[index: i];
					//Details.Content += $"Site Property {i + 1}: {property.Key} - {property.Value}\n";
				}
			}
			catch (Exception exc)
			{
			}
		}


		/// <summary>
		/// Device availability Handler
		/// </summary>
		/// <param name="nextPage"></param>
		private void HandleGetDeviceAvailabilityStatus(object sender, DeviceAvailabilityEventArgs e)
		{

			// This only display a subset of all information returned (use as required).
			Dispatcher.Invoke(callback: () =>
			{
				var cardPrinterRibbonRemaining = e.CardPrinterHealthStatus?.RibbonRemaining == null
					? "Unknown"
					: e.CardPrinterHealthStatus.RibbonRemaining.ToString();

				var cardPrinterAvailable = e.IsCardPrinterAvailable ? "Yes" : "No";
				CardPrinterButton.IsEnabled = e.IsCardPrinterAvailable ? true : false; 

				var documentScannerAvailable = e.IsDocumentScannerAvailable ? "Yes" : "No";
				DocumentScannerButton.IsEnabled = e.IsDocumentScannerAvailable ? true : false;

				var faceCamerasAvailable = e.AreFaceCamerasAvailable ? "Yes" : "No";
				CameraButton.IsEnabled = e.AreFaceCamerasAvailable ? true : false;

				var fingerprintScannerAvailable = e.IsFingerprintScannerAvailable ? "Yes" : "No";
				FingerprintScannerButton.IsEnabled = e.IsFingerprintScannerAvailable ? true : false;

				var identityScannerAvailable = e.IsIdentityScannerAvailable ? "Yes" : "No";
				IdentityScannerButton.IsEnabled = e.IsIdentityScannerAvailable ? true : false;

				var lightingControlAvailable = e.IsLightingControlBoardAvailable ? "Yes" : "No";
				LightingControlButton.IsEnabled = e.IsLightingControlBoardAvailable ? true : false;

				var powerSupplyAvailable = e.IsPowerSupplyBoardAvailable ? "Yes" : "No";
				PowerControlButton.IsEnabled = e.IsPowerSupplyBoardAvailable ? true : false;

				var routerAvailable = e.IsRouterAvailable ? "Yes" : "No";

				var sensorAvailable = e.IsSensorBoardAvailable ? "Yes" : "No";
				SensorControlButton.IsEnabled = e.IsSensorBoardAvailable ? true : false;

				var tamperAvailable = e.IsTamperBoardAvailable ? "Yes" : "No";
				TamperControlsButton.IsEnabled = e.IsTamperBoardAvailable ? true : false;

				QRCodeData += cardPrinterAvailable + "|" + documentScannerAvailable + "|" + faceCamerasAvailable + "|" + fingerprintScannerAvailable + "|" + identityScannerAvailable + "|" + lightingControlAvailable;

				CardPrinterStatusLabel.Text = $"Card printer available: {cardPrinterAvailable}";
				CardPrinterRibbonStatusLabel.Text = $"Card printer ribbon remaining: {cardPrinterRibbonRemaining}";
				DocumentScannerStatusLabel.Text = $"Document scanner available: {documentScannerAvailable}";
				CameraStatusLabel.Text = $"Face cameras available: {faceCamerasAvailable}";
				FingerprintScannerStatusLabel.Text = $"Fingerprint scanner available: {fingerprintScannerAvailable}";
				IdentityScannerStatusLabel.Text = $"Identity scanner available: {identityScannerAvailable}";
				LightingStatusLabel.Text = $"Lighting control board available: {lightingControlAvailable}";
				PowerStatusLabel.Text = $"Power supply board available: {powerSupplyAvailable}";
				RouterStatusLabel.Text = $"Router available: {routerAvailable}";
				SensorStatusLabel.Text = $"Sensor board available: {sensorAvailable}";
				TamperStatusLabel.Text = $"Tamper board available: {tamperAvailable}";

			});
		}

		/// <summary>
		/// Get and Display Device Information
		/// </summary>
		/// <param name="nextPage"></param>

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetSystemTimes(out long lpIdleTime, out long lpKernelTime, out long lpUserTime);

		private string GetSystemSerialNumber()
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
			try
			{
				foreach (ManagementObject mo in searcher.Get())
				{
					return mo["SerialNumber"].ToString();
				}
				return "";
			}
			catch
			{
				return "";
			}
		}

		private string GetOSVersion()
		{
			return Environment.OSVersion.ToString();
		}

		private TimeSpan GetUpTime()
		{
			long idleTime, kernelTime, userTime;
			if (GetSystemTimes(out idleTime, out kernelTime, out userTime))
			{
				long ticks = Environment.TickCount * 10000;
				return TimeSpan.FromTicks(ticks - idleTime - kernelTime);
			}
			return TimeSpan.Zero;
		}

		/// <summary>
		/// Get the information for deploy information
		/// </summary>
		/// <param name="nextPage"></param>



		/// <summary>
		/// Navigation Params
		/// </summary>
		/// <param name="nextPage"></param>
		public void Navigate(UserControl nextPage) => Content = nextPage;


		public void Navigate(UserControl nextPage, object state)
		{
			Content = nextPage;

			if (nextPage is ISwitchable s)
			{
				s.UtilizeState(state: state);
			}
			else
			{
				throw new ArgumentException(message: $"NextPage is not ISwitchable - {nextPage.Name}");
			}
		}

		/// <summary>
		/// Create Bitmap
		/// </summary>
		/// <param name="nextPage"></param>
		public BitmapSource GenerateQRCode(string content, int size)
		{
			QRCodeGenerator qrGenerator = new QRCodeGenerator();
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
			QRCode qrCode = new QRCode(qrCodeData);
			Bitmap qrCodeImage = qrCode.GetGraphic(size);

			return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				qrCodeImage.GetHbitmap(),
				IntPtr.Zero,
				System.Windows.Int32Rect.Empty,
				BitmapSizeOptions.FromWidthAndHeight(size, size)
			);
		}

		/// <summary>
		/// Button Actions
		/// </summary>
		/// <param name=""></param>
		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			QRCodeData = "";
			loadingDisplay.Visibility = Visibility.Visible;
			InitalizeMainPageData();
		}

		private void FingerprintScannerButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new FingerprintTestPage());
		}

		private void DocumentScannerButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new DocumentScannerTestPage());
		}

		private void IdentityScannerButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new IdentityScannerTestPage());
		}

		private void CardPrinterButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new CardPrinterTestPage());
		}

		private void SensorControlButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new SensorTestPage());
		}

		private void CameraButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new CameraTestPage());
		}

		private void PowerControlButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new PowerControlsTestPage());
		}

		private void LightingControlButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new LightingControlsTestPage());
		}

		private void TamperControlsButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new TamperControlsTestPage());
		}

		private void InitializeLogEntryGrid()
		{

			LogEntries.Columns.Add(item: new DataGridTextColumn
			{
				Header = "DateTime",
				Binding = new Binding(path: "DateTime"),
				IsReadOnly = true,
				Width = new DataGridLength(value: 250, type: DataGridLengthUnitType.Pixel)
			});

			LogEntries.Columns.Add(item: new DataGridTextColumn
			{
				Header = "Detail",
				Binding = new Binding(path: "Detail"),
				IsReadOnly = true,
				Width = new DataGridLength(value: 1, type: DataGridLengthUnitType.Star),
			});
		}

		private readonly ObservableCollection<LogEntry> _logEntries = new ObservableCollection<LogEntry>();

		private void CreateLogEntry(string detail)
		{
			var logEntry = LogEntry.Create(
				dateTime: DateTime.Now.ToString(format: "yyyy-MM-dd HH:mm:ss.fff"),
				detail: detail);

			Dispatcher.Invoke(callback: () =>
			{
				_grokiManager.CreateLogEntry(detail: detail);
				_logEntries.Add(item: logEntry);

				LogEntries.ScrollIntoView(item: LogEntries.Items[index: LogEntries.Items.Count - 1]);
			});
		}

		private void DisplayEventEntries(){

		//var EventList = _grokiManager.GetEventHistory();

		//	foreach (LogEntry itemObject in EventList)
		//	{
		//		Dispatcher.Invoke(callback: () =>
		//	{
		//		_logEntries.Add(item: itemObject);
		//	});
		//	}
		}

		private void StartTestButton_Click(object sender, RoutedEventArgs e)
		{
			_grokiManager.SetIsAutomated(true);
			_grokiManager.SetKioskTimestamp(TestId);
			_grokiManager.SetKioskTimestampTestID(TestId);
			Switcher.Switch(newPage: new FingerprintTestPage());
		}

		/// <summary>
		/// This is logic for Automated Testing
		/// </summary>
		/// 


		/// <summary>
		/// This is logic for Automated Test Persistance and Test Retention
		/// </summary>
		/// 

		public void SetupResultsView()
		{
			if (_grokiManager.TestPass)
			{
				TestIndicator.Fill = new SolidColorBrush(Colors.Green);
			}
			else
			{
				TestIndicator.Fill = new SolidColorBrush(Colors.Red);
			}

			kiosktestresult.Content = _grokiManager.TestPass;
			cyclecount.Content = _grokiManager.TestCycle;

			if (_grokiManager.FingerprintImage != null)
			{
				FingerImage.Source = UiHelper.ConvertToBitmapSource((Bitmap)ImageHelper.ConvertFromBytes(_grokiManager.FingerprintImage));
				FingerImageReviewPage.Source = UiHelper.ConvertToBitmapSource((Bitmap)ImageHelper.ConvertFromBytes(_grokiManager.FingerprintImage));
			}
		}


		private void SetupComponentHealthIndicators()
		{
			if (_grokiManager.FingerPrintTestComplete)
			{
				FingerprintHealthIndicator.Content = _grokiManager.FingerprintScannerHealth;
			}

			if (_grokiManager.CardPrinterTestComplete)
			{
				CardPrinterHealthIndicator.Content = _grokiManager.CardPrinterHealth;
			}

			if (_grokiManager.IdScannerTestComplete)
			{
				IdentityScannerHealthIndicator.Content = _grokiManager.IdScannerHealth;
			}

			if (_grokiManager.DocumentScannerTestComplete)
			{
				DocumentScannerHealthIndicator.Content = _grokiManager.DocumentScannerHealth;
			}

			if (_grokiManager.LightingBoardTestComplete)
			{
				LightingHealthIndicator.Content = _grokiManager.LightingBoardHealth;
			}

			if (_grokiManager.TamperBoardTestComplete)
			{
				TamperHealthIndicator.Content = _grokiManager.TamperBoardHealth;
			}

			if (_grokiManager.SensorBoardTestComplete)
			{
				SensorHealthIndicator.Content = _grokiManager.SensorBoardHealth;
			}
		}

		private void SetupFingerprintScannerResultsPage()
		{
			FingerprintEventReview.ItemsSource = _grokiManager.FingerPrintScannerResults;
			if (_grokiManager.FingerprintScannerHealth != "Operational")
			{
				FingerprintOverviewIndicator.Background = new SolidColorBrush(Colors.Red);
			}
		}

		private void SetupTamperBoardResultsPage()
		{
			TamperBoardEventReview.ItemsSource = _grokiManager.TamperBoardResults;
			if (_grokiManager.TamperBoardHealth != "Operational")
			{
				TamperBoardOverviewIndicator.Background = new SolidColorBrush(Colors.Red);
			}
		}

		private void SetupLightingBoardResultsPage()
		{
			LightingBoardEventReview.ItemsSource = _grokiManager.LightingBoardResults;
			if (_grokiManager.LightingBoardHealth != "Operational")
			{
				LightingBoardOverviewIndicator.Background = new SolidColorBrush(Colors.Red);
			}
		}

		private void SetupIdentityScannerResultsPage()
		{
			IdentityScannerEventReview.ItemsSource = _grokiManager.IdentityScannerResults;
			if (_grokiManager.IdScannerHealth != "Operational")
			{
				IdentityScannerOverviewIndicator.Background = new SolidColorBrush(Colors.Red);
			}
		}

		private void SetupSensorBoardResultsPage()
		{
			SensorBoardEventReview.ItemsSource = _grokiManager.SensorBoardResults;
			if (_grokiManager.SensorBoardHealth != "Operational")
			{
				SensorOverviewIndicator.Background = new SolidColorBrush(Colors.Red);
			}
		}

		private void SetupDocumentScannerResultsPage()
		{
			DocumentScannerEventReview.ItemsSource = _grokiManager.DocumentScannerResults;
			if (_grokiManager.DocumentScannerHealth != "Operational")
			{
				DocumentScannerOverviewIndicator.Background = new SolidColorBrush(Colors.Red);
			}
		}

		private void SetupCardPrinterResultsPage()
		{
			CardPrinterEventReview.ItemsSource = _grokiManager.CardPrinterResults;
			if (_grokiManager.CardPrinterHealth != "Operational")
			{
				CardPrinterOverviewIndicator.Background = new SolidColorBrush(Colors.Red);
			}
		}



		//https://prd-gkm-api.tymeinnovation.com/api/QualityAssurance/evaluations POST
		//https://prd-gkm-api.tymeinnovation.com/api/QualityAssurance/results POST
		//private async Task PostQAKioskDataAsync()
		//{
		//	var client = new IntegrationClient(isTestMode: true, baseUrl: "https://prd-gkm-api.tymeinnovation.com/");
		//	var insertEvaluationResult = await client.CreateEvaluationAsync(criteria: new GlobalKiosk.Integrations.QualityAssurance.Evaluations.Criterions.EvaluationCriteria(
		//		name: "Integration test",
		//		started: DateTime.UtcNow,
		//		startReason: "Integration test",
		//		kioskId: Guid.Parse("45d137b3-11a3-4bc3-b5a6-48a09f94d7e3")));

		//	var insertResultResult = await client.CreateResultAsync(criteria: new GlobalKiosk.Integrations.QualityAssurance.Evaluations.Criterions.ResultCriteria(
		//		evaluationId: Guid.Parse(insertEvaluationResult.RecordId),
		//		ended: DateTime.UtcNow,
		//		endReason: "End of integration testing",
		//		hasPassed: _grokiManager.TestPass,
		//		failureReason: null));
		//}



		private async void CreateKMSTelephone(){

			string Basepath = "https://dev-gkm-api.tymeinnovation.com";

			GlobalConfiguration.Instance = new Tyme.GlobalKiosk.Management.ApiClient.Client.Configuration
			{
			//Use Slack creds
				BasePath = Basepath,
				OAuthTokenUrl = $"{Basepath}/token/",
				OAuthClientId = "1n8bhnlk05hq135vh5qs1gj9id",
				OAuthClientSecret = "b84bmmjmi5hol88sdjs5fbrjtgbus3s4omfs6cj7o3n99orpdt1",
				OAuthFlow = OAuthFlow.APPLICATION
			};

			var KioskAssociations = new RelationshipsApi();
			try
			{
				CreateLogEntry("Get Kiosk Associations: Start");
				var result = await KioskAssociations.ApiRelationshipsKioskComponentAssociationsGetWithHttpInfoAsync(kioskNumber: _grokiManager.KioskNumber, isFilteringByActive: true, pageSize: 100).ConfigureAwait(false);
				CreateLogEntry($"Kiosk Associations: {result}");
				foreach (var association in result.Data.Items)
				{
					//association.Kiosk.Id;
					//var group = () association.Component.ComponentGroup.Value; 
				}
			}
			catch (Exception ex)
			{
				CreateLogEntry("Get Kiosk Associations: Fail");
			}
			

			//This is end part for Posting KMS transmitMessage
			var KioskMessage = new KioskMessagingApi();
			var messageCriteria = new TransmitKioskMessageCriteria(messageTypeName: "TransmitPayload", kioskId: new Guid(), componentTypeId: new Guid(),userName: "GLOKI TEST APPLICATION") ;

			try
			{
				CreateLogEntry("Kiosk Transmit Payloads: Start");
				var Messagresult = await KioskMessage.ApiKioskMessagingTransmitKioskMessagePostWithHttpInfoAsync(transmitKioskMessageCriteria: messageCriteria).ConfigureAwait(false);
				CreateLogEntry($"Kiosk Associations: {Messagresult}");
			}
			catch (Exception ex)
			{
				CreateLogEntry("Kiosk Transmit Payloads: Fail");
			}
			//follow PerformEndSessionAndKMSInteraction

		}

		// call KMS at the end... 
		private async void DoApICallToKMS(){
		//Get Kiosk Data
			//var client = new IntegrationClient(isTestMode: true, baseUrl: "https://prd-gkm-api.tymeinnovation.com/");
			//var performKioskEventResultUpdate = await client.TransmitKioskMessageAsync(new Guid(), "TransmitPayload", new Guid(), new Guid(), "Gloki Test Application", "", new Guid(), null);
		}

		async void PerformEndSessionAndKMSInteraction(){
		//Disable test button and show loading
		//AppHelper.EndSession()
		//Init Kms Result capture.
		//Wait 2.5 minutes
		//Hid loading and enable test button.
		}

		private void StackPanel_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
		{
		}

		private void GetKioskTestCertificate(){
		
		}

		private void PowerControlButton_Click_1(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new PowerControlsTestPage());
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
			SystemBoards.Crypto myCrypto = new SystemBoards.Crypto();
			System.IO.Ports.SerialPort LightingBoardESP32 = new System.IO.Ports.SerialPort();
			LightingBoardESP32.PortName = GetDeviceComPort<String>(BoardType.LightingControlLite);
			LightingBoardESP32.BaudRate = Convert.ToInt32(BoardRateConstant);
			LightingBoardESP32.Open();

			SystemBoards.LightingJSONCommands LightingCommand = new SystemBoards.LightingJSONCommands()
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
			LightingCommand = new SystemBoards.LightingJSONCommands()
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
			LightingCommand = new SystemBoards.LightingJSONCommands()
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
			LightingCommand = new SystemBoards.LightingJSONCommands()
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
			LightingCommand = new SystemBoards.LightingJSONCommands()
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
			LightingCommand = new SystemBoards.LightingJSONCommands()
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

		private void InitSensorFunction()
		{
			int Length;
			byte[] DataToSend = new byte[1000];
			byte[] EndDataToSend = new byte[1000];
			byte[] bytes = new byte[1000];
			byte[] bytes2 = new byte[200];
			byte[] bytes3 = new byte[1000];
			SystemBoards.Crypto myCrypto = new SystemBoards.Crypto();
			char[] RelayOutputs = new char[13];
			string Ouputs;


			// Resolve interace deviec
			SystemBoards.cp210x myCP210x = new SystemBoards.cp210x();
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


			SystemBoards.SensorJSONCommands SensorCommand = new SystemBoards.SensorJSONCommands()
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

		private void MarketingScreenTestButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new MarketingScreenTestPage());
		}

        public static string GetDeviceComPort<T>(BoardType boardType)
		{
			try
			{
				var deviceInfo = SystemBoards.DeviceHelper.GetUsbDeviceInfo<T>(
					vendorId: 4292,
					productId: 60000,
					deviceClass: SystemBoards.PnpDeviceClass.Ports,
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
