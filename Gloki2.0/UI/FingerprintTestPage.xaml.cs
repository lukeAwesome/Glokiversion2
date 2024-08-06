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
using System.Windows.Threading;
using Tyme.Kihama.Sdk.Interfaces.Peripheral;
using Tyme.Kihama.Sdk;
using Gloki2._0.Model;
using System.Drawing;
using System.ServiceModel;
using Tyme.Kihama.Common.Services.DTOs;
using Tyme.Kihama.Common.Services.Enums;
using Tyme.Kihama.Common.Services.Helpers;
using Tyme.Kihama.Sdk.Events.Peripheral;
using Tyme.Kihama.Sdk.Helpers;
using Tyme.Kihama.Common.Services.DTOs.Peripheral;
using Tyme.Kihama.Sdk.Events;
using ErrorEventArgs = Tyme.Kihama.Sdk.Events.ErrorEventArgs;

namespace Gloki2._0.UI
{
	/// <summary>
	/// Interaction logic for FingerprintTestPage.xaml
	/// </summary>
	public partial class FingerprintTestPage : UserControl
	{
		private DispatcherTimer countdownTimer;
		private readonly GloKiManager _gloki = GloKiManager.Instance;
		private readonly IFingerprintScannerManagerProxy _fingerprintScannerManagerProxy = InternalProxyFactory.Create<IFingerprintScannerManagerProxy>();
		private bool shouldStartScan = true;

		public FingerprintTestPage()
		{
			InitializeComponent();
			SubscribeToEvents();

			FingerprintScannerControl.ManagerProxy = _fingerprintScannerManagerProxy;
			FingerprintScannerControl.Source = null;

			StartTest.IsEnabled = false;
			countdownTimer = new DispatcherTimer();
			countdownTimer.Interval = TimeSpan.FromSeconds(1);
			countdownTimer.Tick += CountdownTimer_Tick;

			countdownTimer.Start();
			GetDeviceInformation();
		}

		private int countdownValue = 8;

		private void CountdownTimer_Tick(object sender, EventArgs e)
		{
			countdownValue--;
			countdownLabel.Content = countdownValue.ToString();

			if (countdownValue == 0)
			{
				countdownTimer.Stop();
				StartScan();
				// Countdown completed, do something else
			}
		}

		private void DoneButton_Click(object sender, RoutedEventArgs e)
		{
			UnsubscribeFromEvents();
			FingerprintScannerControl.Dispose();
			//Switcher.Switch(newPage: new DocumentScannerTestPage());
			Switcher.Switch(newPage: new HomeScreen());
		}

		private void SubscribeToEvents()
		{
			// For event handlers 'sender' will be a string with the method name that was invoked - 
			// see the 'OnCompleted' handler for example.

			// You can also use IFingerprintScannerManagerProxy directly if you do not want to use the 
			// FingerprintScannerControl control.

			// == Base ==

			FingerprintScannerControl.OnIsDeviceAvailable += HandleIsDeviceAvailable;
			FingerprintScannerControl.OnGetDeviceInfo += HandleGetDeviceInfo;
			FingerprintScannerControl.OnError += HandleError;
			FingerprintScannerControl.OnCompleted += HandleCompleted;

			// == Fingerprint Scanner ==

			FingerprintScannerControl.OnPlaceFinger += HandlePlaceFinger;
			FingerprintScannerControl.OnRemoveFinger += HandleRemoveFinger;
			FingerprintScannerControl.OnFakeSourceDetected += HandleFakeSourceDetected;
			FingerprintScannerControl.OnFingerprintFrameReceived += HandleOnFingerprintFrameReceived;
			FingerprintScannerControl.OnFingerprintCaptured += HandleOnFingerprintCaptured;

			// FingerprintScannerControl.OnFingerprintCaptured is handled internally by the control
			// to automatically place the incoming image. You are welcome to listen for it should you
			// need to use the image for other purposes as well as the WSQ data.
		}

		private void UnsubscribeFromEvents()
		{
			// == Base ==

			FingerprintScannerControl.OnIsDeviceAvailable -= HandleIsDeviceAvailable;
			FingerprintScannerControl.OnGetDeviceInfo -= HandleGetDeviceInfo;
			FingerprintScannerControl.OnError -= HandleError;
			FingerprintScannerControl.OnCompleted -= HandleCompleted;

			// == Fingerprint Scanner ==

			FingerprintScannerControl.OnPlaceFinger -= HandlePlaceFinger;
			FingerprintScannerControl.OnRemoveFinger -= HandleRemoveFinger;
			FingerprintScannerControl.OnFakeSourceDetected -= HandleFakeSourceDetected;
			FingerprintScannerControl.OnFingerprintFrameReceived -= HandleOnFingerprintFrameReceived;
			FingerprintScannerControl.OnFingerprintCaptured -= HandleOnFingerprintCaptured;
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
				case "StartScan": // For event method
				case "StartScanAsync": // For async method
				case "StopScan":
					//loadingDisplay.Visibility = Visibility.Hidden;
					UpdateControls(description: "Operation complete.",  stopEnabled: true);
					Dispatcher.Invoke(callback: () =>
					{
						DelayNavigation(4000);
					});
					break;
			}
		}

		private void HandlePlaceFinger(object sender, EventArgs e)
		{
			UpdateControls(description: "Place finger on scanner...", stopEnabled: true);
		}

		private void HandleRemoveFinger(object sender, EventArgs e)
		{
			UpdateControls(description: "Remove finger from scanner...", stopEnabled: true);
		}

		private void HandleFakeSourceDetected(object sender, EventArgs e)
		{
			UpdateControls(description: "Could not detect a live finger", stopEnabled: true);
		}

		private void HandleOnFingerprintFrameReceived(object sender, MediaCapturedEventArgs e)
		{
			if (e.MediaData == null)
			{
				_gloki.FingerprintScannerHealth = "Non Operational";
				return;
			}

			Dispatcher.Invoke(callback: () =>
			{
				var imageSource =
					UiHelper.ConvertToBitmapSource(
						bitmap: (Bitmap)ImageHelper.ConvertFromBytes(imageBytes: e.MediaData));

				if (imageSource != null)
				{
					fingerprintresult.Source = imageSource;
					FingerprintScannerControl.Source = imageSource;

					_gloki.FirstFingerPrintTestComplete = true;
					_gloki.FingerprintImage = e.MediaData;
					_gloki.FingerprintImageActualImage = imageSource;
				}

				_gloki.FingerprintScannerHealth = "Operational";

				_gloki.AddFingerPrintScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
				description: "Fingerptint Scan Success",
				isPassed: true,
				resultCode: ErrorCode.Success.GetDescription()));
			});
		}

		private void HandleOnFingerprintCaptured(object sender, FingerprintCapturedEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			Dispatcher.Invoke(callback: () =>
			{
				Prompt.Text = $"NFIQ Score: {e.NfiqScore}";
			});

			// To save to JPEG file use File.WriteAllBytes(@"C:\Captured.jpg", e.Image);

			// To save WSQ data to file sample below ...
			if (e.WsqData == null)
			{
				return;
			}

			// var wsqDataBytes = (byte[])(Array) e.WsqData;
			//
			// File.WriteAllBytes(path: @"C:\Captured.wsq", bytes: wsqDataBytes);
		}

		private void HandleIsDeviceAvailable(object sender, DeviceAvailableEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			switch (e.IsDetected)
			{
				case true when !e.IsInUseByOther:
					UpdateControls(description: "Device is available.", stopEnabled: true);
					return;

				case true when e.IsInUseByOther:
					UpdateControls(
						description: "Device was found but is currently in use",
						stopEnabled: true);
					return;

				default:
					UpdateControls(description: "Device not found", stopEnabled: true);
					break;
			}
		}

		private void HandleGetDeviceInfo(object sender, DeviceInfoEventArgs<FingerprintScannerDeviceMetadata> e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;


			_gloki.AddFingerPrintScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
				description: "GetDeviceDetails",
				isPassed: true,
				resultCode: ErrorCode.Success.GetDescription()));

			_gloki.CreateLogEntry($"Fingerprint Scanner Vendor: {e.DeviceInfo.Vendor}");
			_gloki.CreateLogEntry($"Fingerprint Scanner Product: {e.DeviceInfo.Product}");
			_gloki.CreateLogEntry($"Fingerprint Scanner Serial Number: {e.DeviceInfo.SerialNumber}");
			_gloki.CreateLogEntry($"Fingerprint Scanner Status: {e.DeviceInfo.ErrorCode.GetDescription()}");
			_gloki.CreateLogEntry($"Fingerprint Scanner Detected: {e.DeviceInfo.IsDetected}");

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

		#endregion Proxy Event Handlers


		private void ShowException(Exception e)
		{

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

		private async void StartScan(){
		loadingDisplay.Visibility = Visibility.Visible;
			try
			{
				Prompt.Text = "Scan Started...";
				FingerprintScannerControl.Source = null;
				FingerprintScannerControl.ImageSource = null;

				UpdateControls(description: "Processing...",stopEnabled: false);

				_gloki.AddFingerPrintScannerResult(value: new EventEntry(testCycle: _gloki.CurrentCycle,
				description: "Fingerprint Scan Start",
				isPassed: true,
				resultCode: ErrorCode.Success.GetDescription()));

				// If not specified, the default WSQ canvas size of 512x512 pixels will be used.

				var configuration = FingerprintScannerScanConfiguration.Create(
					scanTimeoutMilliseconds: 6000,
					captureTimeoutMilliseconds: 4000,
					isLivenessCheckEnabled: true,
					shouldInvertColor: true);

				await AppHelper.ManageSessionAsync();

				// The async call will only start the capture process.
				// The FingerprintScannerControl.OnFingerprintCaptured event must still be subscribed to for events due
				// to the hardware's SDK.

				StartTest.IsEnabled = true;
				await FingerprintScannerControl.StartScanAsync(configuration: configuration);
			}
			catch (Exception exc)
			{
				ShowException(e: exc);
			}

		}

		private async void GetDeviceInformation(){
			await AppHelper.ManageSessionAsync();
			var configuration = DeviceConfiguration.Create(false);
			var result = await _fingerprintScannerManagerProxy.GetDeviceInfoAsync(configuration);
			HandleGetDeviceInfo(this,result);
		}

		private void StartTest_Click(object sender, RoutedEventArgs e)
		{
			StartTest.IsEnabled = false;
			StartScan();
		}


		private async void DelayNavigation(int milliSecondsValue)
		{
			_gloki.FingerPrintTestComplete = true;
			await Task.Delay(milliSecondsValue);
			navigate();
		}

		private void navigate(){
			if(_gloki.GetIsAutomated()){
				UnsubscribeFromEvents();
				FingerprintScannerControl.Dispose();
				Switcher.Switch(newPage: new DocumentScannerTestPage());
			}
		}
	}
}
