using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

namespace P2P_TCP {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class Login : Window {
		public Login() {
			InitializeComponent();
			thread = new Thread(new ThreadStart(ListenThreadMethod));
			thread.IsBackground = true;
			thread.Start();
		}

		public Login(string UserID) {
			InitializeComponent();
			textBox_id.Text = UserID;
			thread = new Thread(new ThreadStart(ListenThreadMethod));
			thread.IsBackground = true;
			thread.Start();
		}

		Thread thread; //侦听的线程类变量

		//侦听线程执行的方法
		private void ListenThreadMethod() {
			TcpListener server = null;

		}

		private void button_register_Click(object sender, RoutedEventArgs e) {
			
			string[] ip = textBox_ip.Text.Split(':');
			TcpClient tcpClient = new TcpClient();
			IPAddress ServerIP = IPAddress.Parse(ip[0]);
			tcpClient.Connect(ServerIP, int.Parse(ip[1])); //建立与服务器的连接

			NetworkStream networkStream = tcpClient.GetStream();
			if (networkStream.CanWrite) {
				IMClassLibrary.LoginDataPackage loginDataPackage = new IMClassLibrary.LoginDataPackage(textBox_id.Text, "Server_Reg", textBox_id.Text, sha256(passwordBox.Password)); //初始化登录数据包
				Byte[] sendBytes = loginDataPackage.DataPackageToBytes(); //注册数据包转化为字节数组
				networkStream.Write(sendBytes, 0, sendBytes.Length);
			}
			
			MessageBox.Show("注册成功！");
			P2PClient client = new P2PClient(textBox_id.Text); //传入用户名
			client.Show();
			Close();
		}

		private void button_login_Click(object sender, RoutedEventArgs e) {
			
			string[] ip = textBox_ip.Text.Split(':');
			TcpClient tcpClient = new TcpClient();
			IPAddress ServerIP = IPAddress.Parse(ip[0]);
			tcpClient.Connect(ServerIP, int.Parse(ip[1])); //建立与服务器的连接

			NetworkStream networkStream = tcpClient.GetStream();
			if (networkStream.CanWrite) {
				IMClassLibrary.LoginDataPackage loginDataPackage = new IMClassLibrary.LoginDataPackage(textBox_id.Text, "Server_Login", textBox_id.Text, sha256(passwordBox.Password)); //初始化登录数据包
				Byte[] sendBytes = loginDataPackage.DataPackageToBytes(); //登录数据包转化为字节数组
				networkStream.Write(sendBytes, 0, sendBytes.Length);
			}
			
			P2PClient client = new P2PClient(textBox_id.Text); //传入用户名
			client.Show();
			textBox_id.Text = sha256(passwordBox.Password);
			Close();
		}

		private void button_about_Click(object sender, RoutedEventArgs e) {
			About about = new About();
			about.ShowDialog();
		}

		public string sha256(string data) {
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			byte[] hash = SHA256Managed.Create().ComputeHash(bytes);
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < hash.Length; i++) {
				builder.Append(hash[i].ToString("X2"));
			}
			return builder.ToString();
		}
	}
}
