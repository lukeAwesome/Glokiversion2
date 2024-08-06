using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tyme.Kihama.Common.Services.DTOs.System;
using Tyme.Kihama.Common.Services.Helpers;
using Tyme.Kihama.Sdk;
using Tyme.Kihama.Sdk.Events.System;
using Tyme.Kihama.Sdk.Interfaces;
using Tyme.Kihama.Sdk.Interfaces.System;
using QRCoder;
using System.Drawing;
using System.Windows.Media.Imaging;
using Gloki2._0.UI;

namespace Gloki2._0
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		private readonly ISystemManagerProxy _systemManagerProxy = InternalProxyFactory.Create<ISystemManagerProxy>();
		private SystemMockConfiguration _mockConfiguration = new SystemMockConfiguration();
		private String QRCodeData = "";
		private String TestId = "";

		public MainWindow()
		{
		///This was a mistake and is not where the code goes
			InitializeComponent();

			Switcher.PageSwitcher = this;
			Switcher.Switch(newPage: new HomeScreen());

		}

		private void InitalizeMainPageData()
		{
			KioskSerial.Text = "Device Serial number : " + GetSystemSerialNumber();
			OperatingSystem.Text = "Windows Version number : " + GetOSVersion();
			UpTime.Text = "Kiosk Uptime : " + GetUpTime();
			SetupDetailsScreen();
		}

		private async void SetupDetailsScreen(){
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
				KioskNumer.Text = $"Kiosk Number: {result.KioskNumber}";
				KioskType.Text = $"Kiosk Type: {result.KioskType.GetDescription()}";
				DeviceName.Text = $"Device Name: {result.DeviceName}";
				DeviceId.Text = $"Device ID: {result.DeviceIdentifier}";
				OperatingSystemName.Text = $"Operating System: {result.DeviceOperatingSystem}";
				DeviceMacAddress.Text = $"MAC Address: {result.DeviceNetworkMacAddress}";
				Organization.Text = $"Organisation: {result.OrganisationName}";
				CountryCode.Text = $"Country Code: {result.OrganisationCountryCode}";
				//Details.Content += $"Commercial Partner: {result.OrganisationCommercialPartner}\n";
				//Details.Content += $"Presence Category: {result.OrganisationPresenceCategory}\n";
				//Details.Content += $"Site: {result.OrganisationSiteName}\n";
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
				var documentScannerAvailable = e.IsDocumentScannerAvailable ? "Yes" : "No";
				var faceCamerasAvailable = e.AreFaceCamerasAvailable ? "Yes" : "No";
				var fingerprintScannerAvailable = e.IsFingerprintScannerAvailable ? "Yes" : "No";
				var identityScannerAvailable = e.IsIdentityScannerAvailable ? "Yes" : "No";
				var lightingControlAvailable = e.IsLightingControlBoardAvailable ? "Yes" : "No";
				var powerSupplyAvailable = e.IsPowerSupplyBoardAvailable ? "Yes" : "No";
				var routerAvailable = e.IsRouterAvailable ? "Yes" : "No";
				var sensorAvailable = e.IsSensorBoardAvailable ? "Yes" : "No";
				var tamperAvailable = e.IsTamperBoardAvailable ? "Yes" : "No";

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
			try { 
			foreach (ManagementObject mo in searcher.Get())
			{
				return mo["SerialNumber"].ToString();
			}
			return "";
			}
			catch {
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
	}
}
