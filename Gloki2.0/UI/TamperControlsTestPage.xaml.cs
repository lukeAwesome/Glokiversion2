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

namespace Gloki2._0.UI
{
	/// <summary>
	/// Interaction logic for TamperControlsTestPage.xaml
	/// </summary>
	public partial class TamperControlsTestPage : UserControl
	{
		public TamperControlsTestPage()
		{
			InitializeComponent();
		}

		private void DoneButton_Click(object sender, RoutedEventArgs e)
		{
			Switcher.Switch(newPage: new HomeScreen());
		}
	}
}
