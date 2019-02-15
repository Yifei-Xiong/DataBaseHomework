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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;

namespace P2P_TCP {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			myIPAddress = (IPAddress)Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0);
			MyPort++;
			//一台计算机如果生成多个P2P终端,端口号应不同
			for (int i = 0; i < 51; i++) {
				try {
					tcpListener = new TcpListener(myIPAddress, MyPort);
					tcpListener.Start();
					textBlock.Text = "本机IP地址和端口号:" + myIPAddress.ToString() + ":" + MyPort.ToString();
					break;
				}
				catch {
					MyPort++; //已被使用,端口号加1
				}
				if (i == 50) {
					MessageBox.Show("不能建立服务器,可能计算机网络有问题");
					this.Close();
				}
			}
			FriendListView.ItemsSource = myFriendIPAndPorts; //FriendListview的数据源
			IPAndPort = myIPAddress.ToString() + ":" + MyPort.ToString();
		}

		static int MyPort = 1499; //本程序侦听准备使用的端口号,为静态变量
		IPAddress myIPAddress = null; //本程序侦听使用的IP地址
		TcpListener tcpListener = null; //接收信息的侦听类对象,检查是否有信息
		string IPAndPort; //记录本地IP和端口号

		public class StateObject {
			public TcpClient tcpClient = null;
			public NetworkStream netstream = null;
			public byte[] buffer;
			public string friendIPAndPort = null; //记录好友的IP和端口号
		}

		public struct FriendIPAndPort {
			public string friendIP { get; set; }
			public string friendPort { get; set; }
		}

		public class FriendIPAndPorts : ObservableCollection<FriendIPAndPort> { } //定义集合
		FriendIPAndPorts myFriendIPAndPorts = new FriendIPAndPorts();

		private void SendMessageButton_Click(object sender, RoutedEventArgs e) {
			if (SendMessageTextBox.Text == "") {
				MessageBox.Show("发送信息不能为空");
				return;
			} //是否输入了发送信息
			if (FriendListView.SelectedItems.Count == 0) {
				MessageBox.Show("请选择好友IP和端口号");
				return;
			} //是否选择发送好友
			FriendIPAndPort[] myIPAndPorts = new FriendIPAndPort[FriendListView.SelectedItems.Count];
			for (int i = 0; i < myIPAndPorts.Length; i++) {
				myIPAndPorts[i] = (FriendIPAndPort)FriendListView.SelectedItems[i];
			} //得到所有要发送的好友IP和端口号
			string s = IPAndPort + "说:" + SendMessageTextBox.Text; //要发送的字符串
			UnicodeEncoding ascii = new UnicodeEncoding(); //以下将字符串转换为字节数组
			int n = (ascii.GetBytes(s)).Length;
			byte [] SendMsg = new byte[n];
			SendMsg = ascii.GetBytes(s);
			string ip = null; //记录好友端IP
			int port = 0; //记录好友端端口号
			TcpClient tcpClient;
			StateObject stateObject;
			for(int i=0;i< myIPAndPorts.Length; i++) {
				tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
				stateObject = new StateObject(); ////每次发送建立一个StateObject类对象
				stateObject.tcpClient = tcpClient;
				stateObject.buffer = SendMsg;
				ip = myIPAndPorts[i].friendIP; //所选好友IP地址字符串
				port = int.Parse(myIPAndPorts[i].friendPort); //所选字符串好友端口号转换为数字
				stateObject.friendIPAndPort = ip + ":" + port.ToString(); //所选好友IP和端口号
				tcpClient.BeginConnect(ip, port, new AsyncCallback(SentCallBackF), stateObject); //异步连接
			} //给选定所有好友发信息
		}

		private void SentCallBackF(IAsyncResult ar) {
			StateObject stateObject = (StateObject)ar.AsyncState;
			TcpClient tcpClient = stateObject.tcpClient; //得到下载使用的类对象
			NetworkStream netStream = null; //下载使用的流对象
			try {
				tcpClient.EndConnect(ar); //结束和下载服务器的连接，如下载错误将产生异常
				netStream = tcpClient.GetStream();
				if(netStream.CanWrite) {
					netStream.Write(stateObject.buffer,0,stateObject.buffer.Length);
				} else {
					MessageBox.Show("发送到-" + stateObject.friendIPAndPort + "-失败");
				}
			} catch {
				MessageBox.Show("发送到-" + stateObject.friendIPAndPort + "-失败");
			} finally {
				if(netStream !=null) {
					netStream.Close();
				}
				tcpClient.Close();
			}
		} //不在主线程执行
	}
}
