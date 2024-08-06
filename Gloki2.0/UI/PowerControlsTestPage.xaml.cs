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
using Tyme.Kihama.Common.Services.DTOs.SystemBoard;
using Tyme.Kihama.Sdk;
using Tyme.Kihama.Sdk.Interfaces.SystemBoard;

namespace Gloki2._0.UI
{
	/// <summary>
	/// Interaction logic for PowerControlsTestPage.xaml
	/// </summary>
	public partial class PowerControlsTestPage : UserControl
	{
		private readonly IPowerSupplyManagerProxy _powerSupplyManagerProxy = InternalProxyFactory.Create<IPowerSupplyManagerProxy>();

		public PowerControlsTestPage()
		{
			InitializeComponent();
		}

		private void DoneButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new HomeScreen());
		}

		private async Task RunKihamaPowerOffFunctionAsync()
		{

			var configuration = PowerSupplyRelayStateConfiguration.Create(
					  isMiniPcOn: false, // Protects against shutting off the mini PC
					  isDocumentScannerOn: true,
					  isUserDisplayOn: true,
					  isMarketingDisplayOn: true,
					  isPeripheralUsbHubOn: true,
					  isSystemBoardUsbHubOn: true,
					  isRouterOn: true, // Protects against shutting off the router
					  isCardPrinterOn: true,
					  isPosDeviceOn: true,
					  isIdentityScannerOn: true,
					  isInverterOn: true, // Protects against shutting off the entire power board
					  isSensorAndLightingControlBoardOn: true,
			isTamperBoardOn: true);

			await AppHelper.ManageSessionAsync();

			await _powerSupplyManagerProxy.SetRelayStatusAsync(configuration: configuration);
		}

		private void StartTest_Click(object sender, RoutedEventArgs e)
		{
			RunKihamaPowerOffFunctionAsync();
		}

		private void StartTest_Copy_Click(object sender, RoutedEventArgs e)
		{
			RunKihamaPowerOffForRelay5FunctionAsync();
			Task.Delay(5000).Wait();
			RunKihamaPowerONForAllRelaysFunctionAsync();
		}

		private async Task RunKihamaPowerOffForRelay5FunctionAsync()
		{

			var configuration = PowerSupplyRelayStateConfiguration.Create(
					  isMiniPcOn: true, // Protects against shutting off the mini PC
					  isDocumentScannerOn: true,
					  isUserDisplayOn: true,
					  isMarketingDisplayOn: true,
					  isPeripheralUsbHubOn: true,
					  isSystemBoardUsbHubOn: true,
					  isRouterOn: true, // Protects against shutting off the router
					  isCardPrinterOn: true,
					  isPosDeviceOn: true,
					  isIdentityScannerOn: true,
					  isInverterOn: true, // Protects against shutting off the entire power board
					  isSensorAndLightingControlBoardOn: true,
			isTamperBoardOn: true);

			await AppHelper.ManageSessionAsync();

			await _powerSupplyManagerProxy.SetRelayStatusAsync(configuration: configuration);
		}

		private async Task RunKihamaPowerONForAllRelaysFunctionAsync()
		{

			var configuration = PowerSupplyRelayStateConfiguration.Create(
					  isMiniPcOn: true, // Protects against shutting off the mini PC
					  isDocumentScannerOn: true,
					  isUserDisplayOn: true,
					  isMarketingDisplayOn: true,
					  isPeripheralUsbHubOn: true,
					  isSystemBoardUsbHubOn: true,
					  isRouterOn: true, // Protects against shutting off the router
					  isCardPrinterOn: true,
					  isPosDeviceOn: true,
					  isIdentityScannerOn: true,
					  isInverterOn: true, // Protects against shutting off the entire power board
					  isSensorAndLightingControlBoardOn: true,
			isTamperBoardOn: true);

			await AppHelper.ManageSessionAsync();

			await _powerSupplyManagerProxy.SetRelayStatusAsync(configuration: configuration);
		}
	}
}
