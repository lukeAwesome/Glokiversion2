
namespace Gloki2._0
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Media;
	using System.Windows;
	using Tyme.Kihama.Common.Services.Enums.System;
	using Tyme.Kihama.Sdk.Interfaces.System;
	using Tyme.Kihama.Sdk;
	using System;

	public class AppHelper
	{
		public const string APPLICATION_ID = "4d5a870a-6640-4f97-952e-b4974a6b5fef";

		// Toggle between event-based and task-based methods
		public const bool USE_ASYNC = true;

		public const int DESCRIPTION_FONT_SIZE = 14;

		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj == null)
			{
				yield return null;
			}

			for (var i = 0; i < VisualTreeHelper.GetChildrenCount(reference: depObj); i++)
			{
				var child = VisualTreeHelper.GetChild(reference: depObj, childIndex: i);

				if (child is T dependencyObject)
				{
					yield return dependencyObject;
				}

				foreach (var childOfChild in FindVisualChildren<T>(depObj: child))
				{
					yield return childOfChild;
				}
			}
		}

		public static async Task ManageSessionAsync()
		{
			var systemManagerProxy = InternalProxyFactory.Create<ISystemManagerProxy>();
			var session = await systemManagerProxy.GetSessionIdAsync(
				sessionType: SessionType.Standard,
				applicationId: APPLICATION_ID).ConfigureAwait(continueOnCapturedContext: false);

			if (string.IsNullOrWhiteSpace(value: session))
			{
				await systemManagerProxy.StartSessionAsync(
					sessionType: SessionType.Standard,
					applicationId: APPLICATION_ID).ConfigureAwait(continueOnCapturedContext: false);

				await Task.Delay(millisecondsDelay: 5000)
					.ConfigureAwait(continueOnCapturedContext: false); // Avoid race condition
			}

			var systemSession = await systemManagerProxy.GetSessionIdAsync(
				sessionType: SessionType.System,
				applicationId: APPLICATION_ID).ConfigureAwait(continueOnCapturedContext: false);

			if (string.IsNullOrWhiteSpace(value: systemSession))
			{
				await systemManagerProxy.StartSessionAsync(
					sessionType: SessionType.System,
					applicationId: APPLICATION_ID).ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		public static async Task EndSessionAsync(){
			var systemManagerProxy = InternalProxyFactory.Create<ISystemManagerProxy>();
			var session = await systemManagerProxy.GetSessionIdAsync(
				sessionType: SessionType.Standard,
				applicationId: APPLICATION_ID).ConfigureAwait(continueOnCapturedContext: false);

			if (!string.IsNullOrWhiteSpace(value: session))
			{
				await systemManagerProxy.EndSessionAsync(
					sessionType: SessionType.Standard,
					applicationId: APPLICATION_ID).ConfigureAwait(continueOnCapturedContext: false);

				await Task.Delay(millisecondsDelay: 5000)
					.ConfigureAwait(continueOnCapturedContext: false); // Avoid race condition
			}

			var systemSession = await systemManagerProxy.GetSessionIdAsync(
				sessionType: SessionType.System,
				applicationId: APPLICATION_ID).ConfigureAwait(continueOnCapturedContext: false);

			if (!string.IsNullOrWhiteSpace(value: systemSession))
			{
				await systemManagerProxy.EndSessionAsync(
					sessionType: SessionType.System,
					applicationId: APPLICATION_ID).ConfigureAwait(continueOnCapturedContext: false);
			}
		}


		public static string GetKihamaEnvironment()
		{
			var environment = Environment.GetEnvironmentVariable(
				variable: "KIHAMA_ENVIRONMENT",
				target: EnvironmentVariableTarget.Machine);

			if (string.IsNullOrWhiteSpace(value: environment))
			{
				environment = "Development";
			}

			return environment;
		}
	}
}
