using System.Windows;
using System.Windows.Controls;
using Tyme.Kihama.Sdk.Interfaces.Client;
using Tyme.Kihama.Sdk.Interfaces.Peripheral;
using Tyme.Kihama.Sdk;
using System;
using Tyme.Kihama.Common.Services.DTOs.Peripheral;
using Tyme.Kihama.Sdk.Events.Peripheral;
using Tyme.Kihama.Sdk.Events;
using System.Drawing;
using System.Windows.Media;
using Tyme.Kihama.Common.Services.Helpers;
using Tyme.Kihama.Sdk.Helpers;
using Gloki2._0.Model;
using System.Threading.Tasks;
using Tyme.Kihama.Sdk.DTOs.Client;
using Tyme.Kihama.Sdk.Events.Client;
using System.Windows.Media.Imaging;
using System.ServiceModel;
using Tyme.Kihama.Common.Services.DTOs;
using System.Collections.Generic;
using MaterialDesignThemes.Wpf;
using Tyme.Kihama.Sdk.Interfaces;
using Tyme.Kihama.Sdk.Controls;
using Tyme.Kihama.Common.Services.Enums;

namespace Gloki2._0.UI
{
	/// <summary>
	/// Interaction logic for IdentityScannerTestPage.xaml
	/// </summary>
	public partial class IdentityScannerTestPage : UserControl
	{

		private readonly GloKiManager _gloki = GloKiManager.Instance;
		private readonly IClientManagerProxy _managerProxy = ClientProxyFactory.Create<IClientManagerProxy>();
		private readonly List<string> _statusList = new List<string>();
		private readonly IIdentityScannerManagerProxy _identityScannerManagerProxy = InternalProxyFactory.Create<IIdentityScannerManagerProxy>();
		private bool CheckSecurity = true;

		public IdentityScannerTestPage()
		{
			SubscribeToEvents();
			InitializeComponent();
			GetDeviceInformation();
			SkipTestButton.IsEnabled = false;
			GetDeviceReady();
		}

		private async void GetDeviceInformation()
		{
			await AppHelper.ManageSessionAsync();
			var configuration = DeviceConfiguration.Create(false);
			var result = await _identityScannerManagerProxy.GetDeviceInfoAsync(configuration);
			HandleGetDeviceInfo(this, result);
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

			// == Base ==

			_identityScannerManagerProxy.OnIsDeviceAvailable += HandleIsDeviceAvailable;
			_identityScannerManagerProxy.OnGetDeviceInfo += HandleGetDeviceInfo;
			_identityScannerManagerProxy.OnError += HandleError;
			_identityScannerManagerProxy.OnCompleted += HandleCompleted;

			// == Identity Scanner ==

			_identityScannerManagerProxy.OnStatus += HandleGetStatus;
			_identityScannerManagerProxy.OnDocumentReady += HandleCheckDocumentReady;
			_identityScannerManagerProxy.OnIdentityScanCaptured += HandleIdentityScanCaptured;
		}

		private void UnsubscribeFromEvents()
		{
			// == Base ==

			_identityScannerManagerProxy.OnIsDeviceAvailable -= HandleIsDeviceAvailable;
			_identityScannerManagerProxy.OnGetDeviceInfo -= HandleGetDeviceInfo;
			_identityScannerManagerProxy.OnError -= HandleError;
			_identityScannerManagerProxy.OnCompleted -= HandleCompleted;

			// == Identity Scanner ==

			_identityScannerManagerProxy.OnStatus -= HandleGetStatus;
			_identityScannerManagerProxy.OnDocumentReady -= HandleCheckDocumentReady;
			_identityScannerManagerProxy.OnIdentityScanCaptured -= HandleIdentityScanCaptured;
		}

		private void StartTest_Click(object sender, RoutedEventArgs e)
		{
			StartScan();
		}

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
				case "Calibrate":
					loadingDisplay.Visibility = Visibility.Hidden;
					UpdateControls(description: "Operation complete.", true);
					break;
			}
		}

		private void HandleIdentityScanCaptured(object sender, IdentityScanCapturedEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			Dispatcher.Invoke(callback: () =>
			{
				// To save to JPEG file use File.WriteAllBytes(@"C:\Captured.jpg", e.NormalImage);

				if ((bool)CheckSecurity)
				{
					Prompt.Text = e.DidSecurityCheckPass
						? "Ultraviolet security PASSED!"
						: "Ultraviolet security FAILED!";
				}

				BitmapSource imageSource;

				if (e.NormalImage != null)
				{
					_gloki.IdScannerHealth = "Operational";
					imageSource =
						UiHelper.ConvertToBitmapSource(
							bitmap: (Bitmap)ImageHelper.ConvertFromBytes(imageBytes: e.NormalImage));

					if (imageSource != null)
					{
						_gloki.IdScannerHealth = "Operational";
						NormalImage.Source = imageSource;
					}
				}

				if (e.InfraredImage != null)
				{
					_gloki.IdScannerHealth = "Operational";
					imageSource =
						UiHelper.ConvertToBitmapSource(
							bitmap: (Bitmap)ImageHelper.ConvertFromBytes(imageBytes: e.InfraredImage));

					if (imageSource != null)
					{
						_gloki.IdScannerHealth = "Operational";
						InfraredImage.Source = imageSource;
					}
				}

				if (e.UltravioletImage == null)
				{
					_gloki.IdScannerHealth = "Non Operational";
					return;
				}

				imageSource =
					UiHelper.ConvertToBitmapSource(
						bitmap: (Bitmap)ImageHelper.ConvertFromBytes(imageBytes: e.UltravioletImage));

				if (imageSource != null)
				{
					_gloki.IdScannerHealth = "Operational";
					UltravioletImage.Source = imageSource;
				}

				_gloki.IdScannerHealth = "Operational";
			});
		}

		private void HandleCheckDocumentReady(object sender, DocumentReadyEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			if (e.IsDocumentReady)
			{
				UpdateControls(description: "Document is ready.", true);
				_gloki.AddIdentityScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
				description: "Document is ready",
				isPassed: true,
				resultCode: ErrorCode.Success.GetDescription()));
				return;
				;
			}

			UpdateControls(description: "Document is not ready.", true);
			_gloki.IdScannerHealth = "Non Operational";
		}

		private void HandleIsDeviceAvailable(object sender, DeviceAvailableEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			switch (e.IsDetected)
			{
				case true when !e.IsInUseByOther:
					UpdateControls(description: "Device is available.", true);
					_gloki.AddIdentityScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle, description: "Device is available", isPassed: true, resultCode: ErrorCode.Success.GetDescription()));
					return;

				case true when e.IsInUseByOther:
					UpdateControls(description: "Device was found but is currently in use",
						 true);
					_gloki.AddIdentityScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle, description: "Device was found but is currently in use", isPassed: true, resultCode: ErrorCode.Success.GetDescription()));
					return;

				default:
					UpdateControls(description: "Device not found", true);
					_gloki.AddIdentityScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle, description: "Device not found", isPassed: false,resultCode: ErrorCode.Success.GetDescription()));
					break;
			}
		}

		private void HandleGetDeviceInfo(object sender, DeviceInfoEventArgs<IdentityScannerDeviceMetadata> e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			Dispatcher.Invoke(callback: () =>
			{
				Vendor.Text = $"Vendor: {e.DeviceInfo.Vendor}";
				Product.Text = $"Product: {e.DeviceInfo.Product}";
				SerialNumber.Text = $"Serial Number: {e.DeviceInfo.SerialNumber}";
				Firmware.Text = $"Firmware Version: {e.DeviceInfo.FirmwareVersion}";
				Status.Text = $"Status: {e.DeviceInfo.ErrorCode.GetDescription()}";
				Detected.Text = $"Detected: {e.DeviceInfo.IsDetected}";
			});
		}

		private void HandleGetStatus(object sender, DocumentScannerStatusEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			Dispatcher.Invoke(callback: () =>
			{
				if (!_statusList.Contains(item: $"{e.Status.GetDescription()}\n"))
				{
					_statusList.Add(item: $"{e.Status.GetDescription()}\n");
				}

				Prompt.Text = string.Join(separator: "", values: _statusList);
			});
		}


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
				Prompt.IsEnabled = stopEnabled;
			});
		}

		private async void StartScan()
		{
			try
			{
				_statusList.Clear();

				NormalImage.Source = null;
				InfraredImage.Source = null;
				UltravioletImage.Source = null;

				var configuration = IdentityScannerScanConfiguration.Create(
					shouldCaptureUltraviolet: true,
					shouldAutoEnhance: true,
					shouldCheckSecurity: true);

				configuration.ShouldCaptureInfrared = true;

				Prompt.Text = "...";

				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				await AppHelper.ManageSessionAsync();
				var result = await _identityScannerManagerProxy.ScanDocumentAsync(configuration: configuration);


				_gloki.AddIdentityScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle, description: "Document is ready", isPassed: true, resultCode: ErrorCode.Success.GetDescription()));

				HandleIdentityScanCaptured(sender: this, e: result);
				UpdateControls(description: "Operation complete.", true);

				DelayNavigation(5000);
			}
			catch (Exception exc)
			{
				ShowException(e: exc);
				SkipTestButton.IsEnabled = true;
			}
		}

		private async void GetDeviceReady()
		{
			try
			{
				_statusList.Clear();
				loadingDisplay.Visibility = Visibility.Visible;

				UpdateControls(
					description: "Please place your identity document on the scanner...", false);

				Prompt.Text = "...";

				// Set a timeout, if need be, to give the user time to place the document on the scanner
				var configuration = DocumentReadyConfiguration.Create(checkTimeoutMilliseconds: 3000);

				await AppHelper.ManageSessionAsync();
				var result = await _identityScannerManagerProxy.CheckDocumentReadyAsync(configuration: configuration);

				HandleCheckDocumentReady(sender: this, e: result);

			}
			catch (Exception exc)
			{
				ShowException(e: exc);
				SkipTestButton.IsEnabled = true;
			}
			finally
			{

				if (_gloki.GetIsAutomated())
				{
					DelayStartScan(4000);
				}
			}
		}

		private async void DelayStartScan(int milliSecondsValue)
		{
			_gloki.IdScannerTestComplete = true;
			await Task.Delay(milliSecondsValue);
			StartScan();
		}

		private async void DelayNavigation(int milliSecondsValue)
		{
			await Task.Delay(milliSecondsValue);
			navigate();
		}

		private void navigate()
		{
			if (_gloki.GetIsAutomated())
			{
				UnsubscribeFromEvents();
				Switcher.Switch(newPage: new CardPrinterTestPage());
			}
		}

		private void InitiateDeviceReset_Click(object sender, RoutedEventArgs e)
		{
			_gloki.TestPass = false;
			//get date time of skip
			_gloki.IdScannerHealth = "Evaluation Skipped";
			_gloki.AddMainResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
			description: "Identity Scan Skipped",
			isPassed: false,
			resultCode: ErrorCode.DeviceError.GetDescription()));

			_gloki.AddIdentityScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
			description: "Identity Scan Skipped",
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
