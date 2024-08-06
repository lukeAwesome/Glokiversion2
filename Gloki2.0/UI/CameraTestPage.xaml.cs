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
using Tyme.Kihama.Sdk.Interfaces.Peripheral;
using Tyme.Kihama.Sdk;
using System.ServiceModel;
using Tyme.Kihama.Common.Services.DTOs.Peripheral;
using Tyme.Kihama.Common.Services.DTOs;
using Tyme.Kihama.Common.Services.Helpers;
using Tyme.Kihama.Sdk.Events.Peripheral;
using Tyme.Kihama.Sdk.Events;
using Tyme.Kihama.Sdk.Helpers;
using System.IO;
using Tyme.Kihama.Common.Services.Enums.Peripheral;
using System.Windows.Media.Media3D;

namespace Gloki2._0.UI
{
	/// <summary>
	/// Interaction logic for CameraTestPage.xaml
	/// </summary>
	public partial class CameraTestPage : UserControl
	{

		private readonly GloKiManager _gloki = GloKiManager.Instance;
		private readonly IFaceCameraManagerProxy _faceCameraManagerProxy = InternalProxyFactory.Create<IFaceCameraManagerProxy>();

		public CameraTestPage()
		{
			InitializeComponent();
			Init();
		}

		private async void Init()
		{
			await AppHelper.ManageSessionAsync();
			SubscribeToEvents();
			FaceCameraControl.ManagerProxy = _faceCameraManagerProxy;
			FaceCameraControl.Source = null;
			CapturedImage.Source = null;

			await Task.Delay(3000);
			StartFaceCameraAndGetFaceCameraDetails();
		}

		private async void StartFaceCameraAndGetFaceCameraDetails()
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls("Processing...");
				FaceCameraControl.GetCameras();
				var result = await _faceCameraManagerProxy.GetCamerasAsync();
				loadingDisplay.Visibility = Visibility.Hidden;
			}
			catch (Exception exc)
			{
				ShowException(exc);
			}
		}

		public void SubscribeToEvents()
		{
			//// == Base ==

			FaceCameraControl.OnIsDeviceAvailable += HandleIsDeviceAvailable;
			FaceCameraControl.OnGetDeviceInfo += HandleGetDeviceInfo;
			FaceCameraControl.OnError += HandleError;
			FaceCameraControl.OnCompleted += HandleCompleted;

			//// == Face Camera ==

			FaceCameraControl.OnGetCameras += HandleGetCameras;
			FaceCameraControl.OnGetVideoFormats += HandleGetCameraVideoFormats;

			//// FaceCameraControl.OnFrameReceived is handled internally by the control
			//// to automatically place the received frame.

			FaceCameraControl.OnImageCaptured += HandleImageCaptured;
			FaceCameraControl.OnGetProperties += HandleGetProperties;
			FaceCameraControl.OnStartVideoCapture += HandleStartCaptureVideo;

			//// Video will be saved at the location specified when calling StartCaptureVideo()
			FaceCameraControl.OnVideoCaptured += HandleStopCaptureVideo;
		}

		public void UnsubscribeToEvents()
		{
			// == Base ==

			FaceCameraControl.OnIsDeviceAvailable -= HandleIsDeviceAvailable;
			FaceCameraControl.OnGetDeviceInfo -= HandleGetDeviceInfo;
			FaceCameraControl.OnError -= HandleError;
			FaceCameraControl.OnCompleted -= HandleCompleted;

			//// == Face Camera ==

			FaceCameraControl.OnGetCameras -= HandleGetCameras;
			FaceCameraControl.OnGetVideoFormats -= HandleGetCameraVideoFormats;

			//// FaceCameraControl.OnFrameReceived is handled internally by the control
			//// to automatically place the received frame.

			FaceCameraControl.OnImageCaptured -= HandleImageCaptured;
			FaceCameraControl.OnGetProperties -= HandleGetProperties;
			FaceCameraControl.OnStartVideoCapture -= HandleStartCaptureVideo;

			//// Video will be saved at the location specified when calling StartCaptureVideo()
			FaceCameraControl.OnVideoCaptured -= HandleStopCaptureVideo;
		}

		#region Proxy Event Handlers

		private void HandleError(object sender, Tyme.Kihama.Sdk.Events.ErrorEventArgs e) => ShowException(e.Exception);

		private void HandleCompleted(object sender, EventArgs e)
		{
			switch (sender)
			{
				case "GetDeviceInfo":
				case "GetCameras":
				case "GetCameraVideoFormats":
					UpdateControls("Operation complete.");
					Dispatcher.Invoke(() => loadingDisplay.Visibility = Visibility.Hidden);
					break;

				case "Stop":
					HandleStop();
					Dispatcher.Invoke(() => loadingDisplay.Visibility = Visibility.Hidden);
					break;
			}
		}

		private void HandleGetCameras(object sender, FaceCameraDevicesEventsArgs e)
		{
			Dispatcher.Invoke(() => Devices.ItemsSource = e.Devices);

			Task.Delay(1000);
			Dispatcher.Invoke(() => Devices.SelectedIndex = 0);
			Task.Delay(1000);
			GetFormats();

		}

		private async void GetFormats()
		{

			try
			{
				String camera = "";
				//camera = (string)Devices.SelectedItem;
				Dispatcher.Invoke(() => camera = (string)Devices.SelectedItem);

				if (string.IsNullOrWhiteSpace(camera))
				{
					MessageBox.Show("Please select a camera", "Error");

					return;
				}

				UpdateControls("Processing...");

				var configuration = FaceCameraDeviceConfiguration.Create(camera);
				var result = await FaceCameraControl.GetCameraVideoFormatsAsync(configuration);

				HandleGetCameraVideoFormats(this, result);
				UpdateControls("Operation complete.");
				
			}
			catch (Exception exc)
			{
				ShowException(exc);
			}
		}

		private void HandleGetCameraVideoFormats(object sender, FaceCameraVideoFormatsEventsArgs e) =>
			Dispatcher.Invoke(() =>
			{
				Formats.ItemsSource = e.VideoFormats;
				Formats.DisplayMemberPath = "Description";
				Formats.SelectedIndex = 0;

				Task.Delay(1000);
				GetDetails();
				Dispatcher.Invoke(() => loadingDisplay.Visibility = Visibility.Hidden);
			});

		private async void GetDetails()
		{
			try
			{
				var camera = (string)Devices.SelectedItem;

				if (string.IsNullOrWhiteSpace(camera))
				{
					MessageBox.Show("Please select a camera", "Error");

					return;
				}


				var configuration = DeviceConfiguration.Create(camera, false);

				UpdateControls("Processing...");
				var result = await FaceCameraControl.GetDeviceInfoAsync(configuration);

				HandleGetDeviceInfo(this, result);
				UpdateControls("Operation complete.");
				Dispatcher.Invoke(() => loadingDisplay.Visibility = Visibility.Hidden);
			}
			catch (Exception exc)
			{
				ShowException(exc);
			}
		}

		private void HandleImageCaptured(object sender, MediaCapturedEventArgs e) =>
			Dispatcher.Invoke(() =>
			{
				if (e.MediaData == null) return;

				var imageSource = UiHelper.ConvertToBitmapSource((System.Drawing.Bitmap)ImageHelper.ConvertFromBytes(e.MediaData));

				if (imageSource != null)
					CapturedImage.Source = imageSource;

				//// To save to JPEG file use File.WriteAllBytes(@"C:\Captured.jpg", e.MediaData);

				var filename = $"KihamaImage_{DateTime.Now:yyMMddHHmmss}.jpg";

				File.WriteAllBytes($@"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\{filename}",
					e.MediaData);
			});

		private void HandleStop()
		{
			UpdateControls("Operation complete.");
			Dispatcher.Invoke(() =>
			{
				FaceCameraControl.ImageSource = null;
				CapturedImage.Source = null;
			});
		}

		private void HandleStartCaptureVideo(object sender, EventArgs e) =>
			UpdateControls("Recording in progress....");

		private void HandleStopCaptureVideo(object sender, EventArgs e) =>
			UpdateControls("Video has been saved.");

		private void HandleIsDeviceAvailable(object sender, DeviceAvailableEventArgs e)
		{
			switch (e.IsDetected)
			{
				case true when !e.IsInUseByOther:
					UpdateControls("Device is available.");
					return;

				case true when e.IsInUseByOther:
					UpdateControls("Device was found but is currently in use");
					return;

				default:
					UpdateControls("Device not found");
					break;
			}
		}

		private void HandleGetDeviceInfo(object sender, DeviceInfoEventArgs<FaceCameraDeviceMetadata> e) =>
			Dispatcher.Invoke(() =>
			{
				Vendor.Text = $"Vendor: {e.DeviceInfo.Vendor}";

				FingerprintProduct.Text = $"Product: {e.DeviceInfo.Product}";
				SerialNumber.Text = $"Serial Number: {e.DeviceInfo.SerialNumber}";
				Firmware.Text = $"Firmware Version: {e.DeviceInfo.FirmwareVersion}";
				Status.Text = $"Status: {e.DeviceInfo.ErrorCode.GetDescription()}";
				Detected.Text = $"Detected: {e.DeviceInfo.IsDetected}";
			});

		private void HandleGetProperties(object sender, FaceCameraPropertiesEventArgs e) =>
			Dispatcher.Invoke(() =>
			{
				Prompt.Text = "";

				foreach (var property in e.Properties)
				{
					Prompt.Text += $"{property.Property.GetDescription()} - Min={property.Minimum}, Max={property.Maximum}, ";
					Prompt.Text += $"Def={property.Default}, Curr={property.Value} ";
					Prompt.Text += property.CanSetAutomatic != null && (bool)property.CanSetAutomatic ? "(A)\n" : "\n";
				}
			});

		#endregion Proxy Event Handlers

		private void UpdateControls(string description) =>
		   Dispatcher.Invoke(() =>
		   {
			   Prompt.Text = description;
			   Prompt.FontSize = 14;
		   });

		private void ShowException(Exception e)
		{
			var message = e.Message;

			if (e is FaultException<KihamaErrorMessage> ex)
			{
				message = !string.IsNullOrWhiteSpace(ex.Detail?.SourceMessage)
					? ex.Detail?.SourceMessage
					: ex.Detail?.ErrorCode.GetDescription();
			}

			UpdateControls(message);
		}


		private void DoneButton_Click(object sender, RoutedEventArgs e)
		{
			UnsubscribeToEvents();
			Switcher.Switch(newPage: new HomeScreen());
		}

		private async void StartCamera_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// To cater for a minimum video requirement of 600x600, select a minimum resolution of 800x600 for 4:3 aspect ratio
				// or 1280x720 for 16:9 aspect ratio.

				UpdateControls("Processing...");

				var camera = (string)Devices.SelectedItem;

				if (string.IsNullOrWhiteSpace(camera))
				{
					MessageBox.Show("Please select a camera", "Error");

					return;
				}

				Prompt.Text = "Started";

				var format = (FaceCameraVideoFormat)Formats.SelectedItem;
				var width = format.Width;
				var height = format.Height;
				var gcd = ImageHelper.GCD(width, height);

				var configuration = FaceCameraDeviceConfiguration.Create(deviceName: camera, formatType: format.Type,
					mediaWidth: format.Width, mediaHeight: format.Height, orientation: FaceCameraOrientation.Portrait);

				if (width / gcd == 4 && height / gcd == 3) // 4:3
				{
					FaceCameraControl.Width = 232;
					CapturedImage.Width = 232;
				}
				else // 16:9
				{
					FaceCameraControl.Width = 174;
					CapturedImage.Width = 174;
				}

				// The async call will only start the camera feed
				// The FaceCameraControl.OnFrameReceived event must still be subscribed to for events as
				// frame will be streamed continuously
				await FaceCameraControl.StartAsync(configuration);
			}
			catch (Exception exc)
			{
				ShowException(exc);
			}
		}

		private async void Capture_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				FaceCameraControl.CaptureImage();

				var result = await FaceCameraControl.CaptureImageAsync();

				HandleImageCaptured(this, result);
			}
			catch (Exception exc)
			{
				ShowException(exc);
			}
		}

		private async void StopCameraButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				UpdateControls("Processing...");

				Prompt.Text = "Stopped";

				await FaceCameraControl.StopAsync();
				HandleStop();
			}
			catch (Exception exc)
			{
				ShowException(exc);
			}
		}

		private async void CaptureImage_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				FaceCameraControl.CaptureImage();
				var result = await FaceCameraControl.CaptureImageAsync();

				HandleImageCaptured(this, result);
			}
			catch (Exception exc)
			{
				ShowException(exc);
			}
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			Init();
		}
	}
}
