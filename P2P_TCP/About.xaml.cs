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
using System.Windows.Shapes;

namespace P2P_TCP {
	/// <summary>
	/// About.xaml 的交互逻辑
	/// </summary>
	public partial class About : Window {
		public About() {
			InitializeComponent();
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
