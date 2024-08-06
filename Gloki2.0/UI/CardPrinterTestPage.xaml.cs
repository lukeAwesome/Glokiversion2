using ControlzEx.Standard;
using Gloki2._0.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Tyme.Kihama.Common.Services.DTOs.Peripheral;
using Tyme.Kihama.Sdk.Interfaces.Peripheral;
using Tyme.Kihama.Sdk.Interfaces;
using Tyme.Kihama.Sdk;
using Tyme.Kihama.Common.Services.Enums.Peripheral;
using Tyme.Kihama.Sdk.Events.Peripheral;
using Tyme.Kihama.Common.Services.Helpers;
using Tyme.Kihama.Sdk.Events;
using System.ServiceModel;
using Tyme.Kihama.Common.Services.DTOs;
using Tyme.Kihama.Sdk.Events.System;
using QRCoder;
using Tyme.Kihama.Common.Services.Enums;

namespace Gloki2._0.UI
{
	/// <summary>
	/// Interaction logic for CardPrinterTestPage.xaml
	/// </summary>
	public partial class CardPrinterTestPage : UserControl
	{
		private readonly ObservableCollection<LogEntry> _logEntries = new ObservableCollection<LogEntry>();
		private readonly ILogger _logger;
		private readonly ICardPrinterManagerProxy _cardPrinterManagerProxy = InternalProxyFactory.Create<ICardPrinterManagerProxy>();

		private List<CardPrinterCardHopperStatus> _types = new List<CardPrinterCardHopperStatus>();
		private readonly List<BarcodeType> _barcodeTypes = new List<BarcodeType>();
		private CardPrinterMockConfiguration _mockConfiguration = new CardPrinterMockConfiguration();
		private List<string> _statusList = new List<string>();
		private GloKiManager _grokiManager;

		public CardPrinterTestPage()
		{
			InitializeComponent();
			_grokiManager = GloKiManager.Instance;
			InitializeLogEntryGrid();
			_logEntries.Clear();
			CardPrinterLogEntries.ItemsSource = _logEntries;
			LoadBarcodeTypes();
			GetDetails_CardPrinter();
		}

		private void DoneButton_Click(object sender, RoutedEventArgs e)
		{
			_grokiManager.SetIsTestCompleted(true);
			Switcher.Switch(newPage: new HomeScreen());
		}

		private async void StartTest_Click(object sender, RoutedEventArgs e)
		{
			await InsertCard();
			await ReadToken(false);
			await PrintCard();
			await PresentCard();
		}

		private void InitializeLogEntryGrid()
		{

			var styleSetter = new Setter
			{
				Property = TextBlock.BackgroundProperty,
				Value = Brushes.Transparent
			};

			// Error colour
			var triggerSetter = new Setter
			{
				Property = TextBlock.BackgroundProperty,
				Value = Brushes.Red
			};

			var styleTrigger = new Trigger
			{
				Property = TextBlock.TextProperty,
				Value = "ERROR",
			};

			styleTrigger.Setters.Add(item: triggerSetter);

			// Success colour
			triggerSetter = new Setter
			{
				Property = TextBlock.BackgroundProperty,
				Value = Brushes.Green
			};

			styleTrigger = new Trigger
			{
				Property = TextBlock.TextProperty,
				Value = "SUCCESS",
			};

			styleTrigger.Setters.Add(item: triggerSetter);

			var styleStatus = new Style(targetType: typeof(TextBlock));

			styleStatus.Setters.Add(item: styleSetter);
			styleStatus.Triggers.Add(item: styleTrigger);

			CardPrinterLogEntries.Columns.Add(item: new DataGridTextColumn
			{
				Header = "Timestamp",
				Binding = new Binding(path: "DateTime"),
				IsReadOnly = true,
				Width = new DataGridLength(value: 250, type: DataGridLengthUnitType.Pixel)
			});

			CardPrinterLogEntries.Columns.Add(item: new DataGridTextColumn
			{
				Header = "Detail",
				Binding = new Binding(path: "Detail"),
				IsReadOnly = true,
				Width = new DataGridLength(value: 1, type: DataGridLengthUnitType.Star),
				ElementStyle = styleStatus
			});
		}

		private void CreateLogEntry(string detail)
		{
			var logEntry = LogEntry.Create(
				dateTime: DateTime.Now.ToString(format: "yyyy-MM-dd HH:mm:ss.fff"),
				detail: detail);

			Dispatcher.Invoke(callback: () =>
			{
				_logEntries.Add(item: logEntry);

				CardPrinterLogEntries.ScrollIntoView(item: CardPrinterLogEntries.Items[index: CardPrinterLogEntries.Items.Count - 1]);
			});
		}

		private void SubscribeToEvents()
		{
			// For event handlers 'sender' will be a string with the method name that was invoked - 
			// see the 'OnCompleted' handler for example.

			// == Base ==

			_cardPrinterManagerProxy.OnIsDeviceAvailable += HandleIsDeviceAvailable;
			_cardPrinterManagerProxy.OnGetDeviceInfo += HandleGetDeviceInfo;
			_cardPrinterManagerProxy.OnError += HandleError;
			_cardPrinterManagerProxy.OnCompleted += HandleCompleted;

			// == Card Printer ==

			_cardPrinterManagerProxy.OnGetStatus += HandleGetStatus;
			_cardPrinterManagerProxy.OnGetHopperStatus += HandleGetHopperStatus;
			_cardPrinterManagerProxy.OnGetFlipperStatus += HandleGetStatus;
			_cardPrinterManagerProxy.OnTokenRead += HandleReadCardToken;
			_cardPrinterManagerProxy.OnPresentCard += HandlePresentCard;
			_cardPrinterManagerProxy.OnPresentCardTimeout += HandleCardTaken;
			_cardPrinterManagerProxy.OnCheckCardTaken += HandleCardTaken;

			// Other events (bin card, retract card, etc.) are not subscribed to but are available should they be required.
		}

		private void UnsubscribeFromEvents()
		{
			// == Base ==

			_cardPrinterManagerProxy.OnIsDeviceAvailable -= HandleIsDeviceAvailable;
			_cardPrinterManagerProxy.OnGetDeviceInfo -= HandleGetDeviceInfo;
			_cardPrinterManagerProxy.OnError -= HandleError;
			_cardPrinterManagerProxy.OnCompleted -= HandleCompleted;

			// == Card Printer ==

			_cardPrinterManagerProxy.OnGetStatus -= HandleGetStatus;
			_cardPrinterManagerProxy.OnGetHopperStatus -= HandleGetHopperStatus;
			_cardPrinterManagerProxy.OnGetFlipperStatus -= HandleGetStatus;
			_cardPrinterManagerProxy.OnTokenRead -= HandleReadCardToken;
			_cardPrinterManagerProxy.OnPresentCard -= HandlePresentCard;
			_cardPrinterManagerProxy.OnPresentCardTimeout -= HandleCardTaken;
			_cardPrinterManagerProxy.OnCheckCardTaken -= HandleCardTaken;
		}

		private void LoadCardTypes()
		{
			CardTypes.ItemsSource = _types;
			CardTypes.SelectedItem = _types.First();
			CardTypes.DisplayMemberPath = "CardType";
		}

		private void LoadBarcodeTypes()
		{
			_barcodeTypes.Clear();

			foreach (var barcodeType in System.Enum.GetValues(enumType: typeof(CardPrintBarcodeType)).Cast<CardPrintBarcodeType>())
			{
				_barcodeTypes.Add(item: BarcodeType.Create(name: barcodeType.GetDescription(), type: barcodeType));
			}

			BarcodeTypes.ItemsSource = _barcodeTypes;
			BarcodeTypes.SelectedItem = _barcodeTypes.First();
			BarcodeTypes.DisplayMemberPath = "Name";
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
				case "GetHopperStatus":
				case "GetFlipperStatus":
				case "InsertCard":
				case "PrintCard":
				case "RetractCard":
				case "BinCard":
				case "EjectCard":
				case "Reboot":
					loadingDisplay.Visibility = Visibility.Hidden;
					UpdateControls(description: "Operation complete.", true);
					break;
			}
		}

		private void HandlePresentCard(object sender, EventArgs e)
		{
			UpdateControls(description: "Waiting for user to take card...", true);
			CreateLogEntry("Waiting for user to take card.");
			_grokiManager.AddCardPrinterResult(value: new EventEntry(testCycle: _grokiManager.CurrentCycle, description: "Waiting for user to take card.", isPassed: true, resultCode: ErrorCode.Success.GetDescription()));
		}

		private void HandleCardTaken(object sender, CardTakenEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			if (e.IsCardTaken)
			{
				UpdateControls(description: "The card was taken.", true);
				_grokiManager.AddCardPrinterResult(value: new EventEntry(testCycle: _grokiManager.CurrentCycle,description: "The card was taken", isPassed: true,resultCode: ErrorCode.Success.GetDescription()));
				CreateLogEntry("The card was taken.");

				return;
			}

			UpdateControls(description: "The card was not taken.", true);
			CreateLogEntry("The card was not taken.");
			_grokiManager.AddCardPrinterResult(value: new EventEntry(testCycle: _grokiManager.CurrentCycle, description: "The card was not taken.", isPassed: true, resultCode: ErrorCode.Success.GetDescription()));
		}

		private void HandleGetDeviceAvailability(object sender, DeviceAvailabilityEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			if (!e.IsCardPrinterAvailable ||
				e.CardPrinterHealthStatus?.HopperStatusCodes == null ||
				e.CardPrinterHealthStatus.HopperStatusCodes.Count == 0)
			{
				return;
			}

			Dispatcher.Invoke(callback: () =>
			{
				_types = e.CardPrinterHealthStatus.HopperStatusCodes;

				LoadCardTypes();
			});
		}

		private void HandleReadCardToken(object sender, TokenReadEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;
			UpdateControls(description: $"Token = {e.Token}", true);
			CreateLogEntry($"Token = {e.Token}");
		}

		private void HandleGetStatus(object sender, CardPrinterStatusEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			Dispatcher.Invoke(callback: () =>
			{
				if (e.Statuses == null || !e.Statuses.Any())
				{
					_grokiManager.CardPrinterHealth = "Non Operational";
					return;
				}

				_statusList.Add(item: "");
				_statusList.Add(item: $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]");

				foreach (var status in e.Statuses)
				{
					CreateLogEntry(status.GetDescription());
					_statusList.Add(item: status.GetDescription());
				}

				//StatusList.Items.Refresh();

				//// Scroll to bottom 
				//var border = (Border)VisualTreeHelper.GetChild(reference: StatusList, childIndex: 0);
				//var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(reference: border, childIndex: 0);
				//scrollViewer.ScrollToBottom();
			});
		}

		private void HandleIsDeviceAvailable(object sender, DeviceAvailableEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			switch (e.IsDetected)
			{
				case true when !e.IsInUseByOther:
					UpdateControls(description: "Device is available.", true);
					CreateLogEntry("Device is available.");
					_grokiManager.CardPrinterHealth = "Operational";
					_grokiManager.AddCardPrinterResult(value: new EventEntry(testCycle: _grokiManager.CurrentCycle,
description: "Device is available",
isPassed: true,
resultCode: ErrorCode.Success.GetDescription()));
					break;

				case true when e.IsInUseByOther:
					UpdateControls(description: "Device was found but is currently in use", true);
					CreateLogEntry("Device was found but is currently in use");
					_grokiManager.AddCardPrinterResult(value: new EventEntry(testCycle: _grokiManager.CurrentCycle,
description: "Device was found but is currently in use",
isPassed: false,
resultCode: ErrorCode.Success.GetDescription()));
					break;

				default:
					UpdateControls(description: "Device not found", true);
					CreateLogEntry("Device not found");
					_grokiManager.CardPrinterHealth = "Non Operational";
					_grokiManager.AddCardPrinterResult(value: new EventEntry(testCycle: _grokiManager.CurrentCycle,
description: "Device not found",
isPassed: false,
resultCode: ErrorCode.Success.GetDescription()));
					break;
			}
		}

		private void HandleGetDeviceInfo(object sender, DeviceInfoEventArgs<CardPrinterDeviceMetadata> e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			_grokiManager.AddCardPrinterResult(value: new EventEntry(testCycle: _grokiManager.CurrentCycle,
			description: "GetCardPrinterDetails",
			isPassed: e.DeviceInfo.IsDetected,
			resultCode: ErrorCode.Success.GetDescription()));

			Dispatcher.Invoke(callback: () =>
			{
				Vendor.Text = $"Vendor: {e.DeviceInfo.Vendor}";
				FingerprintProduct.Text = $"Product: {e.DeviceInfo.Product}";
				SerialNumber.Text = $"Serial Number: {e.DeviceInfo.SerialNumber}";
				Firmware.Text = $"Firmware Version: {e.DeviceInfo.FirmwareVersion}";
				Status.Text = $"Status: {e.DeviceInfo.ErrorCode.GetDescription()}";
				Detected.Text = $"Detected: {e.DeviceInfo.IsDetected}";

				if (e.DeviceInfo.Metadata == null)
				{
					return;
				}

				RemainingRibbon.Text = $"Ribbon Remaining: {e.DeviceInfo.Metadata.CardPrinterRibbonRemaining}";

				foreach (var statusCode in e.DeviceInfo.Metadata.CardPrinterHopperStatusCodes)
				{
					CreateLogEntry($"{statusCode.CardType} - {statusCode.HopperStatus.GetDescription()}");
				}

				_types = e.DeviceInfo.Metadata.CardPrinterHopperStatusCodes;

				LoadCardTypes();
			});
		}

		private void HandleGetHopperStatus(object sender, CardPrinterHopperStatusEventArgs e)
		{
			loadingDisplay.Visibility = Visibility.Hidden;

			Dispatcher.Invoke(callback: () =>
			{
				if (e.StatusCodes == null || !e.StatusCodes.Any())
				{
					CreateLogEntry("Hopper status is not available.");
					_grokiManager.AddCardPrinterResult(value: new EventEntry(testCycle: _grokiManager.CurrentCycle, description: "Hopper status is not available.", isPassed: false, resultCode: ErrorCode.Success.GetDescription()));
					return;
				}

				Prompt.Text = "";

				foreach (var statusCode in e.StatusCodes)
				{
					CreateLogEntry($"{statusCode.CardType} - {statusCode.HopperStatus.GetDescription()}");
				}
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
				//Prompt.IsEnabled = stopEnabled;
			});
		}

		#region Button Events

		private void LoadTypes_OnClick(object sender, RoutedEventArgs e)
		{
			GetDetails_CardPrinter();
		}

		private async void ReadToken_OnClick(object sender, RoutedEventArgs e)
		{
			await ReadToken(true);
		}

		private async Task ReadToken(bool shouldBin)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				var item = (CardPrinterCardHopperStatus)CardTypes.SelectedItem;
				var configuration = CardPrinterReadTokenConfiguration.Create(
					cardType: item.CardType, shouldBinCardOnFailure: shouldBin, shouldProvideRawToken: true);

				await AppHelper.ManageSessionAsync();

				var (data, status) = await _cardPrinterManagerProxy.ReadCardTokenAsync(configuration: configuration);

				HandleReadCardToken(sender: this, e: data);
				HandleGetStatus(sender: this, e: status);

				_grokiManager.CardPrinterHealth = "Operational";
			}
			catch (Exception exc)
			{
				_grokiManager.CardPrinterHealth = "Non Operational";
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void InsertCard_OnClick(object sender, RoutedEventArgs e)
		{
			await InsertCard();
		}

		private async Task InsertCard()
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				var item = (CardPrinterCardHopperStatus)CardTypes.SelectedItem;
				var configuration = CardPrinterInsertCardConfiguration.Create(cardType: item.CardType);

				await AppHelper.ManageSessionAsync();

				var result = await _cardPrinterManagerProxy.InsertCardAsync(configuration: configuration);

				UpdateControls(description: "Operation complete.", true);
				HandleGetStatus(sender: this, e: result);
				_grokiManager.CardPrinterHealth = "Operational";
			}
			catch (Exception exc)
			{
				_grokiManager.CardPrinterHealth = "Non Operational";
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void DispenseCard_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				var item = (CardPrinterCardHopperStatus)CardTypes.SelectedItem;
				var configuration = CardPrinterDispenseCardConfiguration.Create(
					cardType: item.CardType,
					timeoutMilliseconds: 20000,
					shouldRetractCardOnTimeout: true,
					shouldBinCardOnTimeout: true);

				await AppHelper.ManageSessionAsync();

				var (data, status) = await _cardPrinterManagerProxy.DispenseCardAsync(configuration: configuration);

				HandleCardTaken(sender: this, e: data);
				HandleGetStatus(sender: this, e: status);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void Print_OnClick(object sender, RoutedEventArgs e)
		{
			await PrintCard();
		}

		private async Task PrintCard()
		{
			try
			{

				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);
				CreateLogEntry("Printing On Card...");

				// NOTE: Due to limitations in the hardware SDK's advanced printing for text, the font size
				// cannot be set using normal pixel size. The font size is determined by the size of the frame
				// (frameWidth and frameHeight) which will auto-size the text to fit in the frame. Increasing
				// the frameHeight will increase the font size.

				var printSide = CardPrintSide.Front;

				var fullNameText = CardPrinterTextConfiguration.Create(
					printSide: printSide,
					text: "Test Test",
					leftPosition: 70,
					topPosition: 460,
					frameWidth: 650,
					frameHeight: 40,
					fontStyle: CardPrintFontStyle.Bold);

				var accountNumberText = CardPrinterTextConfiguration.Create(
					printSide: printSide,
					text: "0123456789",
					leftPosition: 70,
					topPosition: 510,
					frameWidth: 350,
					frameHeight: 40,
					fontStyle: CardPrintFontStyle.Bold);

				var branchCodeText = CardPrinterTextConfiguration.Create(
					printSide: printSide,
					text: "0123456",
					leftPosition: 450,
					topPosition: 510,
					frameWidth: 350,
					frameHeight: 40,
					fontStyle: CardPrintFontStyle.Bold);

				var configuration = CardPrinterPrintCardConfiguration.Create(
					texts: new List<CardPrinterTextConfiguration> { fullNameText, accountNumberText, branchCodeText });

				await AppHelper.ManageSessionAsync();

				var result = await _cardPrinterManagerProxy.PrintCardAsync(configuration: configuration);

				UpdateControls(description: "Operation complete.", true);
				HandleGetStatus(sender: this, e: result);

				_grokiManager.CardPrinterHealth = "Operational";
			}
			catch (Exception exc)
			{
				_grokiManager.CardPrinterHealth = "Non Operational";
				_logger?.Error(e: exc);
				ShowException(e: exc);
				CreateLogEntry("Device not found");
			}
		}

		private async void Present_OnClick(object sender, RoutedEventArgs e)
		{
			await PresentCard();
		}

		private async Task PresentCard()
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				var configuration = CardPrinterPresentCardConfiguration.Create(
					timeoutMilliseconds: 20000,
					shouldRetractCardOnTimeout: true,
					shouldBinCardOnTimeout: true);

				await AppHelper.ManageSessionAsync();

				var (data, status) = await _cardPrinterManagerProxy.PresentCardAsync(configuration: configuration);

				HandleCardTaken(sender: this, e: data);
				HandleGetStatus(sender: this, e: status);

				_grokiManager.CardPrinterHealth = "Operational";
			}
			catch (Exception exc)
			{
				_grokiManager.CardPrinterHealth = "Non Operational";
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void CardTaken_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				await AppHelper.ManageSessionAsync();

				var (data, status) = await _cardPrinterManagerProxy.CheckCardTakenAsync();

				HandleCardTaken(sender: this, e: data);
				HandleGetStatus(sender: this, e: status);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void Retract_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				await AppHelper.ManageSessionAsync();
				var result = await _cardPrinterManagerProxy.RetractCardAsync();

				UpdateControls(description: "Operation complete.", true);
				HandleGetStatus(sender: this, e: result);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void Bin_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				await AppHelper.ManageSessionAsync();

				var result = await _cardPrinterManagerProxy.BinCardAsync();

				UpdateControls(description: "Operation complete.", true);
				HandleGetStatus(sender: this, e: result);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void Eject_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				await AppHelper.ManageSessionAsync();

				var result = await _cardPrinterManagerProxy.EjectCardAsync();

				UpdateControls(description: "Operation complete.", true);
				HandleGetStatus(sender: this, e: result);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void Reboot_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				await AppHelper.ManageSessionAsync();

				await _cardPrinterManagerProxy.RebootAsync();

				loadingDisplay.Visibility = Visibility.Hidden;
				UpdateControls(description: "Operation complete.", true);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void Status_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				await AppHelper.ManageSessionAsync();

				var result = await _cardPrinterManagerProxy.GetStatusAsync();

				UpdateControls(description: "Operation complete.", true);
				HandleGetStatus(sender: this, e: result);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void HopperStatus_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				Prompt.Text = "Processing...";

				await AppHelper.ManageSessionAsync();

				var result = await _cardPrinterManagerProxy.GetHopperStatusAsync();

				UpdateControls(description: "Operation complete.", true);
				HandleGetHopperStatus(sender: this, e: result);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void FlipperStatus_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				await AppHelper.ManageSessionAsync();

				var result = await _cardPrinterManagerProxy.GetFlipperStatusAsync();

				UpdateControls(description: "Operation complete.", true);
				HandleGetStatus(sender: this, e: result);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void CheckAvailable_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;
				UpdateControls(description: "Processing...", false);

				var configuration = DeviceConfiguration.Create(shouldDetectOnly: true);

				await AppHelper.ManageSessionAsync();

				var result = await _cardPrinterManagerProxy.IsDeviceAvailableAsync(configuration: configuration);

				HandleIsDeviceAvailable(sender: this, e: result);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
		}

		private async void GetDetails_CardPrinter()
		{
			try
			{
				loadingDisplay.Visibility = Visibility.Visible;

				Prompt.Text = "Processing...";

				var configuration = DeviceConfiguration.Create(shouldDetectOnly: false);

				await AppHelper.ManageSessionAsync();

				var result = await _cardPrinterManagerProxy.GetDeviceInfoAsync(configuration: configuration);

				HandleGetDeviceInfo(sender: this, e: result);
				UpdateControls(description: "Operation complete.", true);
			}
			catch (Exception exc)
			{
				_logger?.Error(e: exc);
				ShowException(e: exc);
			}
			finally
			{

				if (_grokiManager.GetIsAutomated())
				{
					DelayStartScan(2000);
				}
			}
		}

		#endregion Button Events

		private async void DelayStartScan(int milliSecondsValue)
		{
			await Task.Delay(milliSecondsValue);
			await ReadToken(true);
			await Task.Delay(milliSecondsValue);
			await ReadToken(false);
			await Task.Delay(milliSecondsValue);
			await PrintCard();
			await Task.Delay(milliSecondsValue);
			await PresentCard();

			DelayNavigation(milliSecondsValue);
		}

		private async void DelayNavigation(int milliSecondsValue)
		{
			_grokiManager.CardPrinterTestComplete = true;
			_grokiManager.SetIsTestCompleted(true);
			await Task.Delay(milliSecondsValue);
			navigate();
		}

		private void navigate()
		{
			if (_grokiManager.GetIsAutomated())
			{
				UnsubscribeFromEvents();
				Switcher.Switch(newPage: new HomeScreen());
			}
		}

    }
}
