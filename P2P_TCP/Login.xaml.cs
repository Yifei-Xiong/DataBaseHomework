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
			//thread = new Thread(new ThreadStart(ListenThreadMethod));
			//thread.IsBackground = true;
			//thread.Start();
			myIPAddress = IPAddress.Parse("127.0.0.1");
			for (int i = 0; i <= 100; i++) {
				try {
					tcpListener = new TcpListener(myIPAddress, MyPort);
					tcpListener.Start();
					break;
				}
				catch {
					MyPort++; //已被使用,端口号加1
				}
				if (i == 100) {
					MessageBox.Show("计算机存在问题！");
					this.Close();
				}
			}
		}

        public Login(string UserID,string IP) {
			InitializeComponent();
			textBox_id.Text = UserID;
			textBox_ip.Text = IP;
			//thread = new Thread(new ThreadStart(ListenThreadMethod));
			//thread.IsBackground = true;
			//thread.Start();
			myIPAddress = IPAddress.Parse("127.0.0.1");
			for (int i = 0; i <= 100; i++) {
				try {
					tcpListener = new TcpListener(myIPAddress, MyPort);
					tcpListener.Start();
					break;
				}
				catch {
					MyPort++; //已被使用,端口号加1
				}
				if (i == 100) {
					MessageBox.Show("计算机存在问题！");
					this.Close();
				}
			}
		}

		//Thread thread; //侦听的线程类变量
		TcpListener tcpListener = null;
		IPAddress myIPAddress = null;
		static int MyPort = 37529;

		public byte[] ReadFromTcpClient(TcpClient tcpClient) {
			List<byte> data = new List<byte>();
			NetworkStream netStream = null;
			byte[] bytes = new byte[tcpClient.ReceiveBufferSize]; //字节数组保存接收到的数据
			int n = 0;
			try {
				netStream = tcpClient.GetStream();
				if (netStream.CanRead) {
					do { //文件大小未知
						n = netStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);
						if (n == (int)tcpClient.ReceiveBufferSize) {
							data.AddRange(bytes);
						} //如果bytes被读入数据填满
						else if (n != 0) {
							byte[] bytes1 = new byte[n];
							for (int i = 0; i < n; i++) {
								bytes1[i] = bytes[i];
							}
							data.AddRange(bytes1);
						} //读入的字节数不为0
					} while (netStream.DataAvailable); //是否还有数据
				} //判断数据是否可读
				bytes = data.ToArray();
			}
			catch {
				MessageBox.Show("读数据失败");
				bytes = null;
			}
			finally {
				if (netStream != null) {
					netStream.Close();
				}
				tcpClient.Close();
			}
			return bytes;
		}

		//侦听线程执行的方法
		private string ListenThreadMethod() {
			//IPAddress ip = (IPAddress)Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0);
			var newClient = tcpListener.AcceptTcpClient();
			var receiveByte = ReadFromTcpClient(newClient);
			var messageClass = new IMClassLibrary.SingleChatDataPackage(receiveByte);
			newClient.Close();
			return messageClass.Message;
		}

		private void button_register_Click(object sender, RoutedEventArgs e) {
			TcpClient tcpClient = null;
			NetworkStream networkStream = null;
			try {
				string[] ip = textBox_ip.Text.Split(':');
				tcpClient = new TcpClient();
				IPAddress ServerIP = IPAddress.Parse(ip[0]);
				tcpClient.Connect(ServerIP, int.Parse(ip[1])); //建立与服务器的连接
				networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					IMClassLibrary.LoginDataPackage loginDataPackage = new IMClassLibrary.LoginDataPackage("127.0.0.1:"+MyPort.ToString(), "Server_Reg", textBox_id.Text, sha256(passwordBox.Password)); //初始化登录数据包
					byte[] sendBytes = loginDataPackage.DataPackageToBytes(); //注册数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
				}
			}
			catch {
				MessageBox.Show("无法连接到服务器!");
				return;
			}
			finally {
				if (networkStream != null) {
					networkStream.Close();
				}
				tcpClient.Close();
			}
			string msg = ListenThreadMethod();
			if (msg == "注册成功") {
				MessageBox.Show("注册成功！");
			}
			else {
				MessageBox.Show("注册失败！");
			}
		}

		private void button_login_Click(object sender, RoutedEventArgs e) {
			TcpClient tcpClient;
			IPAddress ServerIP;
			string msg = string.Empty;
			try {
				string[] ip = textBox_ip.Text.Split(':');
				tcpClient = new TcpClient();
				ServerIP = IPAddress.Parse(ip[0]);
				tcpClient.Connect(ServerIP, int.Parse(ip[1])); //建立与服务器的连接

				NetworkStream networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					IMClassLibrary.LoginDataPackage loginDataPackage = new IMClassLibrary.LoginDataPackage("127.0.0.1:" + MyPort.ToString(), "Server_Login", textBox_id.Text, sha256(passwordBox.Password)); //初始化登录数据包
					Byte[] sendBytes = loginDataPackage.DataPackageToBytes(); //登录数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
				}

				msg = ListenThreadMethod();
			}
			catch {
				MessageBox.Show("与服务器连接失败！");
			}
			if (msg == "登录成功") {
				P2PClient client = new P2PClient(textBox_id.Text, tcpListener, MyPort, textBox_ip.Text.Split(':')[1]); //传入用户名&登录端口
				client.Show();
				Close();
			}
			else {
				MessageBox.Show("登录失败！");
			}
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
