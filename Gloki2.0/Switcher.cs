namespace Gloki2._0
{
	using System;
	using System.Windows.Controls;
	public static class Switcher
	{
		public static MainWindow PageSwitcher;

		public static void Switch(UserControl newPage)
		{
			PageSwitcher.Navigate(nextPage: newPage);
		}

		public static void Switch(UserControl newPage, object state)
		{
			PageSwitcher.Navigate(nextPage: newPage, state: state);
		}

	}
}
