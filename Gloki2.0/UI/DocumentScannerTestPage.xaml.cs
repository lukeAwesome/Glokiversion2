using System;
using System.Collections.Generic;
using System.Linq;
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
using Tyme.Kihama.Sdk.Interfaces.Client;
using Tyme.Kihama.Sdk;
using Tyme.Kihama.Sdk.Interfaces.Peripheral;
using System.Drawing;
using Tyme.Kihama.Common.Services.DTOs.Peripheral;
using Tyme.Kihama.Common.Services.Helpers;
using Tyme.Kihama.Sdk.Events.Peripheral;
using Tyme.Kihama.Sdk.Events;
using Tyme.Kihama.Sdk.Helpers;
using System.ServiceModel;
using Tyme.Kihama.Common.Services.DTOs;
using MaterialDesignThemes.Wpf;
using Tyme.Kihama.Sdk.Interfaces;
using Tyme.Kihama.Sdk.Controls;
using Gloki2._0.Model;
using Tyme.Kihama.Common.Services.Enums;

namespace Gloki2._0.UI
{
	/// <summary>
	/// Interaction logic for DocumentScanner.xaml
	/// </summary>
	public partial class DocumentScannerTestPage : UserControl
	{

		private readonly GloKiManager _gloki = GloKiManager.Instance;
		private readonly IClientManagerProxy _managerProxy = ClientProxyFactory.Create<IClientManagerProxy>();
		private readonly IDocumentScannerManagerProxy _documentScannerManagerProxy = InternalProxyFactory.Create<IDocumentScannerManagerProxy>();
		private readonly List<string> _statusList = new List<string>();

		public DocumentScannerTestPage()
		{
			InitializeComponent();
			SubscribeToEvents();
			SkipTestButton.IsEnabled = false;
			DocumentScannerControl.ManagerProxy = _documentScannerManagerProxy;
			DocumentScannerControl.Source = null;

			getDeviceAvailability();
		}

		private void DoneButton_Click(object sender, RoutedEventArgs e)
		{
			UnsubscribeFromEvents();
			Switcher.Switch(newPage: new HomeScreen());
		}

		private void SubscribeToEvents()
		{
			// For event handlers 'sender' will be a string with the method name that was invoked - 
			// see the 'OnCompleted' handler for example.

			// You can also use IDocumentScannerManagerProxy directly if you do not want to use the 
			// DocumentScannerControl control.

			// == Base ==

			DocumentScannerControl.OnIsDeviceAvailable += HandleIsDeviceAvailable;
			DocumentScannerControl.OnGetDeviceInfo += HandleGetDeviceInfo;
			DocumentScannerControl.OnError += HandleError;
			DocumentScannerControl.OnCompleted += HandleCompleted;

			// == Document Scanner ==

			DocumentScannerControl.OnStatus += HandleGetStatus;
			DocumentScannerControl.OnDocumentReady += HandleCheckDocumentReady;

			// DocumentScannerControl.OnDocumentScanCaptured is handled internally by the control
			// to automatically place the primary image.
			DocumentScannerControl.OnDocumentScanCaptured += HandleDocumentCaptured;
		}

		private void UnsubscribeFromEvents()
		{
			// == Base ==

			DocumentScannerControl.OnIsDeviceAvailable -= HandleIsDeviceAvailable;
			DocumentScannerControl.OnGetDeviceInfo -= HandleGetDeviceInfo;
			DocumentScannerControl.OnError -= HandleError;
			DocumentScannerControl.OnCompleted -= HandleCompleted;

			// == Document Scanner ==

			DocumentScannerControl.OnStatus -= HandleGetStatus;
			DocumentScannerControl.OnDocumentReady -= HandleCheckDocumentReady;
			DocumentScannerControl.OnDocumentScanCaptured -= HandleDocumentCaptured;
		}

		#region Proxy Event Handlers

		private void HandleError(object sender, ErrorEventArgs e)
		{
			ShowException(e: e.Exception);
		}

		private void HandleCompleted(object sender, EventArgs e)
		{
			switch (sender)
			{
				case "GetDeviceInfo":
				case "GetStatus":
				case "ScanDocument":
				case "EjectPaper":
				case "Calibrate":
					loadingDisplay.Visibility = Visibility.Hidden;
					UpdateControls(description: "Operation complete.", true);
					break;
			}
		}

		private void HandleDocumentCaptured(object sender, DocumentScanCapturedEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			Dispatcher.Invoke(callback: () =>
			{
				// NOTE: Primary image is downward-facing and secondary image is upward facing.

				BitmapSource imageSource;

				if (DocumentScannerControl.Source == null && e.PrimaryImage != null)
				{
					imageSource =
						UiHelper.ConvertToBitmapSource(
							bitmap: (Bitmap)ImageHelper.ConvertFromBytes(imageBytes: e.PrimaryImage));

					if (imageSource != null)
					{
						DocumentScannerControl.Source = imageSource;
					}
				}

				if (e.SecondaryImage == null)
				{
					return;
				}

				imageSource =
					UiHelper.ConvertToBitmapSource(
						bitmap: (Bitmap)ImageHelper.ConvertFromBytes(imageBytes: e.SecondaryImage));

				if (imageSource != null)
				{
					SecondaryImage.Source = imageSource;
				}

				_gloki.AddDocumentScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
				description: "Document Scan Success",
				isPassed: true,
				resultCode: ErrorCode.Success.GetDescription()));
				// To save to JPEG file use File.WriteAllBytes(@"C:\Captured.jpg", e.PrimaryImage);
			});
		}

		private void HandleCheckDocumentReady(object sender, DocumentReadyEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			if (e.IsDocumentReady)
			{
				UpdateControls(description: "Document is ready.",  true);

				return;
			}

			UpdateControls(description: "Document is not ready.",  true);
		}

		private void HandleIsDeviceAvailable(object sender, DeviceAvailableEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			switch (e.IsDetected)
			{
				case true when !e.IsInUseByOther:
					UpdateControls(
						description: "Device is available.", true);
					break;

				case true when e.IsInUseByOther:
					UpdateControls(
						description: "Device was found but is currently in use",  true);
					break;

				default:
					UpdateControls(
						description: "Device not found", true);
					break;
			}
		}

		private void HandleGetDeviceInfo(object sender, DeviceInfoEventArgs<DocumentScannerDeviceMetadata> e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			Dispatcher.Invoke(callback: () =>
			{
				Vendor.Text = $"Vendor: {e.DeviceInfo.Vendor}";
				FingerprintProduct.Text = $"Product: {e.DeviceInfo.Product}";
				SerialNumber.Text = $"Serial Number: {e.DeviceInfo.SerialNumber}";
				Firmware.Text = $"Firmware Version: {e.DeviceInfo.FirmwareVersion}";
				Status.Text = $"Status: {e.DeviceInfo.ErrorCode.GetDescription()}";
				Detected.Text = $"Detected: {e.DeviceInfo.IsDetected}";
			});
		}

		private void HandleGetStatus(object sender, DocumentScannerStatusEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			_gloki.AddDocumentScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
			description: "GetDeviceDetails",
			isPassed: true,
			resultCode: ErrorCode.Success.GetDescription()));

			Dispatcher.Invoke(callback: () =>
			{
				if (!_statusList.Contains(item: $"{e.Status.GetDescription()}\n"))
				{
					_statusList.Add(item: $"{e.Status.GetDescription()}\n");
				}

				Prompt.Text = string.Join(separator: "", values: _statusList);
			});
		}

		#endregion Proxy Event Handlers

		private void ShowException(Exception e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			var message = e.Message;

			if (e is FaultException<KihamaErrorMessage> ex)
			{
				message = !string.IsNullOrWhiteSpace(value: ex.Detail?.SourceMessage)
					? ex.Detail?.SourceMessage
					: ex.Detail?.ErrorCode.GetDescription();
			}

			UpdateControls(description: message, stopEnabled: true);
		}

		private void UpdateControls(string description, bool stopEnabled)
		{
			Dispatcher.Invoke(callback: () =>
			{
				Prompt.Text = description;
				//Prompt.Foreground = new SolidColorBrush(color: color);
				Prompt.FontSize = AppHelper.DESCRIPTION_FONT_SIZE;
				//Prompt.IsEnabled = stopEnabled;
			});
		}

		private async void StartScan(){
			try
			{
				_statusList.Clear();

				DocumentScannerControl.Source = null;
				SecondaryImage.Source = null;

				var configuration = DocumentScannerScanConfiguration.Create(
					shouldAutoEnhance: true);

				_gloki.AddDocumentScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
				description: "Document Scan Success",
				isPassed: true,
				resultCode: ErrorCode.Success.GetDescription()));

				_gloki.AddMainResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
				description: "Document Scan Success",
				isPassed: true,
				resultCode: ErrorCode.Success.GetDescription()));

				Prompt.Text = "...";

				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				await AppHelper.ManageSessionAsync();
				var result = await DocumentScannerControl.ScanDocumentAsync(configuration: configuration);

				HandleDocumentCaptured(sender: this, e: result);
				UpdateControls(description: "Operation complete.", true);
				_gloki.DocumentScannerHealth = "Operational";

				DelayNavigation(4000);
			}
			catch (Exception exc)
			{
				SkipTestButton.IsEnabled = true;
				ShowException(e: exc);
			}
		}

		private void StartTest_Click(object sender, RoutedEventArgs e)
		{
			StartScan();
		}

		private async void getDeviceAvailability(){
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...",false);

				var configuration = DeviceConfiguration.Create(shouldDetectOnly: true);

				await AppHelper.ManageSessionAsync();

				var result = await DocumentScannerControl.IsDeviceAvailableAsync(configuration: configuration);

				HandleIsDeviceAvailable(sender: this, e: result);
			}
			catch (Exception exc)
			{
				_gloki.DocumentScannerHealth = "Non Operational";
				ShowException(e: exc);
			}

			try
			{
				Prompt.Text = "Processing...";

				var configuration = DeviceConfiguration.Create(shouldDetectOnly: true);

				await AppHelper.ManageSessionAsync();
				var result = await DocumentScannerControl.GetDeviceInfoAsync(configuration: configuration);

				HandleGetDeviceInfo(sender: this, e: result);
				UpdateControls(description: "Operation complete.", true);


				if(_gloki.GetIsAutomated())
				{
					DelayStartScan(2000);
				}
			}
			catch (Exception exc)
			{
				SkipTestButton.IsEnabled = true;
				ShowException(e: exc);
			}
		}

		private async void DelayNavigation(int milliSecondsValue)
		{
			await Task.Delay(milliSecondsValue);
			navigate();
		}

		private async void DelayStartScan(int milliSecondsValue)
		{
			_gloki.DocumentScannerTestComplete = true;
			await Task.Delay(milliSecondsValue);
			StartScan();
		}

		private void navigate()
		{
			if (_gloki.GetIsAutomated())
			{
				UnsubscribeFromEvents();
				DocumentScannerControl.Dispose();
				Switcher.Switch(newPage: new IdentityScannerTestPage());
			}
		}

		private void InitiateDeviceReset_Click(object sender, RoutedEventArgs e)
		{
			_gloki.TestPass = false;
			//get date time of skip
			_gloki.DocumentScannerHealth = "Evaluation Skipped";
			_gloki.AddMainResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
			description: "Document Scan Skipped",
			isPassed: false,
			resultCode: ErrorCode.DeviceError.GetDescription()));

			_gloki.AddDocumentScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
			description: "Document Scan Skipped",
			isPassed: false,
			resultCode: ErrorCode.DeviceError.GetDescription()));
			//get componet information
			//set componet to failed
			//Set issue as skipped
			DelayNavigation(500);

		}

		private void CancelReset_Click(object sender, RoutedEventArgs e)
		{
			DeviceSkipConfimationView.Visibility = Visibility.Hidden;
		}

		private void SkipTestButton_Click(object sender, RoutedEventArgs e)
		{
			DeviceSkipConfimationView.Visibility = Visibility.Visible;
		}
	}
}
