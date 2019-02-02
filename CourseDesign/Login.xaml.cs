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

namespace CourseDesign {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class Login : Window {
		public Login() {
			InitializeComponent();
		}

		public Login(string UserID) {
			InitializeComponent();
			textBox_id.Text = UserID;
		}

		private void button_register_Click(object sender, RoutedEventArgs e) {

		}

		private void button_login_Click(object sender, RoutedEventArgs e) {
			ClientWindow clientWindow = new ClientWindow(textBox_id.Text); //传入用户名
			clientWindow.Show();
			Close();
		}

		private void button_about_Click(object sender, RoutedEventArgs e) {
			About about = new About();
			about.ShowDialog();
		}
	}
}
