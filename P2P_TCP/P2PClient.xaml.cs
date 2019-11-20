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
using System.Windows.Threading;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace P2P_TCP {
	/// <summary>
	/// P2PClient.xaml 的交互逻辑
	/// </summary>
	public partial class P2PClient : Window {
		public P2PClient(string ID, TcpListener tcpListener,int MyPort,string LoginPort) {
			InitializeComponent();
			//myIPAddress = (IPAddress)Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0);
			myIPAddress = IPAddress.Parse("127.0.0.1");
			/*
			for (int i = 0; i <= 100; i++) {
				try {
					tcpListener = new TcpListener(myIPAddress, MyPort);
					tcpListener.Start();
					textBlock.Text = "本机IP地址和端口号:" + myIPAddress.ToString() + ":" + MyPort.ToString();
					break;
				}
				catch {
					MyPort++; //已被使用,端口号加1
				}
				if (i == 100) {
					MessageBox.Show("不能建立服务器,可能计算机网络有问题");
					this.Close();
				}
			}
			*/
			textBlock.Text = "本机IP地址和端口号:" + myIPAddress.ToString() + ":" + MyPort.ToString();
			FriendListView.ItemsSource = myFriendIPAndPorts; //FriendListview的数据源
			IPAndPort = myIPAddress.ToString() + ":" + MyPort.ToString();
			ListenerThread = new Thread(new ThreadStart(ListenerthreadMethod));
			ListenerThread.IsBackground = true; //主线程结束后，该线程自动结束
			ListenerThread.Start(); //启动线程
			UserID = ID; //设置用户名
			this.Title = "当前用户ID：" + ID;
			this.MyPort = MyPort;
			this.tcpListener = tcpListener;
			this.LoginPort = LoginPort;
		}

		string UserID; //用户ID
		int MyPort; //本程序侦听准备使用的端口号
		string LoginPort; //登录端口
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

		List<IMClassLibrary.FileDataPackage> FileList = new List<IMClassLibrary.FileDataPackage>(); //接受文件列表
		public class FriendIPAndPorts : ObservableCollection<FriendIPAndPort> { } //定义集合
		FriendIPAndPorts myFriendIPAndPorts = new FriendIPAndPorts();
		private Thread ListenerThread; //接收信息的侦听线程类变量
		private delegate void OneArgDelegate(string arg); //代表无返回值有一个string参数方法
		private delegate void SetList(FriendIPAndPort arg); //代表无返回值 FriendIPAndPort参数方法
		public delegate void ReadDataF(TcpClient tcpClient); //代表无返回值 Tcpclient参数方法

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
			//string s = IPAndPort + "说:" + SendMessageTextBox.Text; //要发送的字符串
			//UnicodeEncoding ascii = new UnicodeEncoding(); //以下将字符串转换为字节数组
			//int n = (ascii.GetBytes(s)).Length;
			//byte[] SendMsg = new byte[n];
			//SendMsg = ascii.GetBytes(s);
			string ip = null; //记录好友端IP
			int port = 0; //记录好友端端口号
			TcpClient tcpClient;
			StateObject stateObject;
			IMClassLibrary.SingleChatDataPackage chatData = null;
			for (int i = 0; i < myIPAndPorts.Length; i++) {
				tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
				stateObject = new StateObject(); ////每次发送建立一个StateObject类对象
				stateObject.tcpClient = tcpClient;
				//stateObject.buffer = SendMsg;
				ip = myIPAndPorts[i].friendIP; //所选好友IP地址字符串
				port = int.Parse(myIPAndPorts[i].friendPort); //所选字符串好友端口号转换为数字
				stateObject.friendIPAndPort = ip + ":" + port.ToString(); //所选好友IP和端口号
				chatData = new IMClassLibrary.SingleChatDataPackage(UserID, IPAndPort, SendMessageTextBox.Text);
				stateObject.buffer = chatData.DataPackageToBytes(); //buffer为发送的数据包的字节数组
				tcpClient.BeginConnect(ip, port, new AsyncCallback(SentCallBackF), stateObject); //异步连接
			} //给选定所有好友发信息
			FriendListBox.Items.Add(IPAndPort + "（本机）（" + chatData.sendTime.ToString() + "）说:" + SendMessageTextBox.Text); //显示已发送的信息
		}

		private void SentCallBackF(IAsyncResult ar) {
			StateObject stateObject = (StateObject)ar.AsyncState;
			TcpClient tcpClient = stateObject.tcpClient; //得到下载使用的类对象
			NetworkStream netStream = null; //下载使用的流对象
			try {
				tcpClient.EndConnect(ar); //结束和下载服务器的连接，如下载错误将产生异常
				netStream = tcpClient.GetStream();
				if (netStream.CanWrite) {
					netStream.Write(stateObject.buffer, 0, stateObject.buffer.Length); //传入要发送的信息
				}
				else {
					MessageBox.Show("暂时无法与" + stateObject.friendIPAndPort + "通讯");
				}
			}
			catch {
				MessageBox.Show("暂时无法与" + stateObject.friendIPAndPort + "通讯");
			}
			finally {
				if (netStream != null) {
					netStream.Close();
				}
				tcpClient.Close();
			}
		} //不在主线程执行

		private void SendFileButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			openFileDialog1.Filter = "所有文件(*.*)|*.*";
			if (openFileDialog1.ShowDialog().Value) {
				string s_FileName = openFileDialog1.FileName;
				string FileExtension = System.IO.Path.GetExtension(s_FileName).ToLower();
				using (FileStream fileStream = File.OpenRead(s_FileName)) {
					if (FriendListView.SelectedItems.Count == 0) {
						MessageBox.Show("请选择好友IP和端口号");
						return;
					} //是否选择发送好友
					FriendIPAndPort[] myIPAndPorts = new FriendIPAndPort[FriendListView.SelectedItems.Count];
					for (int i = 0; i < myIPAndPorts.Length; i++) {
						myIPAndPorts[i] = (FriendIPAndPort)FriendListView.SelectedItems[i];
					} //得到所有要发送的好友IP和端口号
					string ip = null; //记录好友端IP
					int port = 0; //记录好友端端口号
					TcpClient tcpClient;
					StateObject stateObject;
					byte[] bytes = new byte[fileStream.Length];
					fileStream.Read(bytes, 0, bytes.Length);
					IMClassLibrary.FileDataPackage data = null;
					for (int i = 0; i < myIPAndPorts.Length; i++) {
						tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
						stateObject = new StateObject(); ////每次发送建立一个StateObject类对象
						stateObject.tcpClient = tcpClient;
						//stateObject.buffer = SendMsg;
						ip = myIPAndPorts[i].friendIP; //所选好友IP地址字符串
						port = int.Parse(myIPAndPorts[i].friendPort); //所选字符串好友端口号转换为数字
						stateObject.friendIPAndPort = ip + ":" + port.ToString(); //所选好友IP和端口号
						data = new IMClassLibrary.FileDataPackage(UserID, IPAndPort, "发送了文件", bytes, FileExtension);
						stateObject.buffer = data.DataPackageToBytes(); //buffer为发送的数据包的字节数组
						tcpClient.BeginConnect(ip, port, new AsyncCallback(SentCallBackF), stateObject); //异步连接
					} //给选定所有好友发信息
					FriendListBox.Items.Add(IPAndPort + "（本机）("+ data.sendTime.ToString() + "）发送了一个文件"); //显示已发送的信息
				}
			}
		} //发送文件

		/*
		public void DownLoadCallBackF(IAsyncResult ar) {
			TcpClient tcpClient = (TcpClient)ar.AsyncState;
			try {
				tcpClient.EndConnect(ar);
			}
			catch {
				MessageBox.Show("连接失败");
				return;
			}
			byte[] bytes = ReadFromTcpClient(tcpClient);
			FileStream fs = null;
			try {
				fs = new FileStream(SaveFilePath.Text, FileMode.Create); //建立文件
				fs.Write(bytes, 0, bytes.Length);
			}
			catch {
				MessageBox.Show("写入文件失败");
			}
			finally {
				if (fs != null) {
					fs.Close();
				}
			}
		}*/ //下载文件的异步方法

		private void ListenerthreadMethod() {
			TcpClient tcpClient = null; //服务器和客户机连接的 TcpClient类对象
			ReadDataF readDataF = new ReadDataF(readRevMsg); //方法 readRevMsg
			while (true) {
				try {
					tcpClient = tcpListener.AcceptTcpClient(); //阻塞等待客户端的连接
					readDataF.BeginInvoke(tcpClient, null, null); //异步调用方法readRevMsg
				}
				catch {

				} //即使发生错误，如果tcpClient不为null，下次循环将引用其他对象
			}
		} //侦听线程执行的方法

		public void readRevMsg(TcpClient tcpClient) {
			byte[] bytes = ReadFromTcpClient(tcpClient); //获取chatData
			FriendIPAndPort friendIPAndPort = new FriendIPAndPort();
			IMClassLibrary.ChatDataPackage chatData = new IMClassLibrary.ChatDataPackage(bytes);
			string message = string.Empty;
			switch (chatData.MessageType) {
				case 4: //单人聊天数据包
					IMClassLibrary.SingleChatDataPackage chatData1 = new IMClassLibrary.SingleChatDataPackage(bytes);
					friendIPAndPort.friendIP = chatData1.Receiver.Split(':')[0];
					friendIPAndPort.friendPort = chatData1.Receiver.Split(':')[1];
					message = chatData1.Receiver + "（用户ID:"+ chatData1.Sender + "）（"+chatData1.sendTime.ToString() +"）说:" + chatData1.Message;
					break;
				case 5: //多人聊天数据包
					IMClassLibrary.MultiChatDataPackage chatData2 = new IMClassLibrary.MultiChatDataPackage(bytes);
					friendIPAndPort.friendIP = chatData2.Receiver.Split(':')[0];
					friendIPAndPort.friendPort = chatData2.Receiver.Split(':')[1];
					message = chatData2.Receiver + "（用户ID:" + chatData2.SenderID + ",来自群聊" + chatData2.Sender.ToString() + "）（" + chatData2.sendTime.ToString() + "）说:" + chatData2.Message;
					break;
				case 7: //文件传输数据包
					IMClassLibrary.FileDataPackage chatData3 = new IMClassLibrary.FileDataPackage(bytes);
					FileList.Add(chatData3); //加入List中待下载
					friendIPAndPort.friendIP = chatData3.Receiver.Split(':')[0];
					friendIPAndPort.friendPort = chatData3.Receiver.Split(':')[1];
					message = chatData3.Receiver + "（用户ID:" + chatData3.Sender + "）（" + chatData3.sendTime.ToString() + "）给你发了一个文件，请接收";
					break;
				default:
					MessageBox.Show("聊天数据包读取失败");
					return;
			}
			//UnicodeEncoding ascii = new UnicodeEncoding();
			//string s = ascii.GetString(bytes);
			//int i1 = s.IndexOf(":"); //第一个:
			//int i2 = s.IndexOf(":", i1 + 1); //第二个:
			//friendIPAndPort.friendIP = s.Substring(0, i1); //提取IP字符串
			//friendIPAndPort.friendPort = s.Substring(i1 + 1, i2 - i1 - 2); //提取端口字符串
			int k = myFriendIPAndPorts.IndexOf(friendIPAndPort);
			if (k == -1) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetList(SetListViewSource), friendIPAndPort);
			} //未找到该ip与端口号，需要增加
			if(message!=string.Empty) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OneArgDelegate(SetFriendListBox), message); //接受信息在FriendListBox显示
			}
		} //被异步调用的方法

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

		private void SetFriendListBox(string text) {
			FriendListBox.Items.Add(text);
		} //修改FriendListBox的方法

		private void SetListViewSource(FriendIPAndPort arg) {
			myFriendIPAndPorts.Add(arg);
		} //修改FriendListView的方法

		private void AddFriendButton_Click(object sender, RoutedEventArgs e) {
			IPAddress myFriendIpAdress;
			if (IPAddress.TryParse(addFriendIPTextBox.Text, out myFriendIpAdress) == false) {
				MessageBox.Show("IP地址格式不正确！");
				return;
			}
			int myFriendPort;
			if (int.TryParse(addFriendPortTextBox.Text, out myFriendPort) == false) {
				MessageBox.Show("端口号格式不正确！");
				return;
			}
			else {
				if (myFriendPort < 1024 || myFriendPort > 65535) {
					MessageBox.Show("端口号范围不正确！(1024-65535)");
					return;
				}
			}
			FriendIPAndPort friendIPAndPort = new FriendIPAndPort();
			friendIPAndPort.friendIP = addFriendIPTextBox.Text; //IP字符串
			friendIPAndPort.friendPort = addFriendPortTextBox.Text; //端口字符串
			int k = myFriendIPAndPorts.IndexOf(friendIPAndPort);
			if (k == -1) {
				myFriendIPAndPorts.Add(friendIPAndPort);
			} //未找到该ip与端口号，需要增加
			else {
				MessageBox.Show("好友已在列表中");
				return;
			}
			
			TcpClient tcpClient;
			StateObject stateObject;
			tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
			stateObject = new StateObject(); ////每次发送建立一个StateObject类对象
			stateObject.tcpClient = tcpClient;
			//stateObject.buffer = SendMsg;
			stateObject.friendIPAndPort = friendIPAndPort.friendIP + ":" + friendIPAndPort.friendPort; //所选好友IP和端口号
			IMClassLibrary.SingleChatDataPackage chatData = new IMClassLibrary.SingleChatDataPackage(UserID, IPAndPort, "添加您为好友");
			stateObject.buffer = chatData.DataPackageToBytes(); //buffer为发送的数据包的字节数组
			tcpClient.BeginConnect(friendIPAndPort.friendIP, myFriendPort, new AsyncCallback(SentCallBackF), stateObject); //异步连接
			//发送添加好友信息
		}

		private void DeleteFriendButton_Click(object sender, RoutedEventArgs e) {
			if(FriendListView.SelectedItems.Count==0) {
				MessageBox.Show("未选择删除的联系人");
			}
			FriendIPAndPort[] myIPAndPorts = new FriendIPAndPort[FriendListView.SelectedItems.Count];
			for (int i = 0; i < myIPAndPorts.Length; i++) {
				myIPAndPorts[i] = (FriendIPAndPort)FriendListView.SelectedItems[i];
			}
			for (int i = 0; i < myIPAndPorts.Length; i++) {
				myFriendIPAndPorts.Remove(myIPAndPorts[i]);
			}
		} //删除选中联系人

		private void MenuItem_Click(object sender, RoutedEventArgs e) {
			string s = "";
			for (int i = 0; i < myFriendIPAndPorts.Count; i++) {
				s += myFriendIPAndPorts[i].friendIP + ":" + myFriendIPAndPorts[i].friendPort + "\r\n";
			}
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			saveFileDialog1.Filter = "文本文档(*.txt)|*.txt|所有文件(*.*)|*.*";
			if (saveFileDialog1.ShowDialog().Value) {
				FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);
				StreamWriter streamWriter = new StreamWriter(fs, Encoding.UTF8);
				streamWriter.Write(s);
				streamWriter.Flush();
				streamWriter.Close();
				fs.Close();
			}
		} //保存好友列表到文件

		private void MenuItem_Click_1(object sender, RoutedEventArgs e) {
			string s = "";
			foreach (var item in this.FriendListBox.Items) {
				s += item.ToString() + "\r\n";
			}
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			saveFileDialog1.Filter = "文本文档(*.txt)|*.txt|所有文件(*.*)|*.*";
			if (saveFileDialog1.ShowDialog().Value) {
				FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);
				StreamWriter streamWriter = new StreamWriter(fs, Encoding.UTF8);
				streamWriter.Write(s);
				streamWriter.Flush();
				streamWriter.Close();
				fs.Close();
			}
		} //保存消息列表到文件

		private void MenuItem_Click_2(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			openFileDialog1.Filter = "文本文档(*.txt)|*.txt|所有文件(*.*)|*.*";
			if (openFileDialog1.ShowDialog().Value) {
				string s_FileName = openFileDialog1.FileName;
				string FileExtension = System.IO.Path.GetExtension(s_FileName).ToUpper();
				if (FileExtension == ".TXT") {
					using (FileStream fileStream = File.OpenRead(s_FileName)) {
						StreamReader reader = new StreamReader(fileStream);
						string text = reader.ReadToEnd();
						string[] friendData = text.Split('\n');
						FriendIPAndPort friendIPAndPort = new FriendIPAndPort();
						IPAddress myFriendIpAdress; //测试IP格式是否正确
						int myFriendPort; //测试端口格式是否正确
						foreach (string data in friendData) {
							if (data == string.Empty) {
								break; //防止最后一行换行符导致的多读入
							}
							string[] IPAndPort = data.Split(':');
							friendIPAndPort.friendIP = IPAndPort[0]; //IP字符串
							friendIPAndPort.friendPort = IPAndPort[1].Substring(0, IPAndPort[1].Length - 1); //端口字符串，防止读入\r
							if (IPAddress.TryParse(friendIPAndPort.friendIP, out myFriendIpAdress) == false) {
								MessageBox.Show("联系人文件IP地址格式不正确！");
								return;
							}
							if (int.TryParse(friendIPAndPort.friendPort, out myFriendPort) == false) {
								MessageBox.Show("联系人文件端口号格式不正确！");
								return;
							}
							int k = myFriendIPAndPorts.IndexOf(friendIPAndPort);
							if (k == -1) {
								myFriendIPAndPorts.Add(friendIPAndPort); //增加此好友
							}
						}
					}
				}
			}
		} //从文件导入好友列表

		private void MenuItem_Click_3(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			openFileDialog1.Filter = "文本文档(*.txt)|*.txt|所有文件(*.*)|*.*";
			if (openFileDialog1.ShowDialog().Value) {
				string s_FileName = openFileDialog1.FileName;
				string FileExtension = System.IO.Path.GetExtension(s_FileName).ToUpper();
				if (FileExtension == ".TXT") {
					using (FileStream fileStream = File.OpenRead(s_FileName)) {
						byte[] bytes = new byte[fileStream.Length];
						fileStream.Read(bytes, 0, bytes.Length);
						string[] data = Encoding.UTF8.GetString(bytes).Split('\n');
						foreach (string str in data) {
							if (str == string.Empty) {
								break; //防止最后一行换行符导致的多读入
							}
							if (str[str.Length-1] == '\r') {
								if (FriendListBox.Items.IndexOf(str.Substring(0, str.Length - 1)) == -1) {
									FriendListBox.Items.Add(str.Substring(0, str.Length - 1)); //添加消息
								} //忽略'\r'
							}
							else {
								if (FriendListBox.Items.IndexOf(str.Substring(0, str.Length)) == -1) {
									FriendListBox.Items.Add(str.Substring(0, str.Length)); //添加消息
								}
							}
						}
					}
				}
			}
		} //从文件导入消息列表

		private void MenuItem_Logout_Click(object sender, RoutedEventArgs e) {
			Login login = new Login(UserID, "127.0.0.1:"+LoginPort);
			login.Show();
			Close(); //关闭Client窗口
		} //登出

		private void MenuItem_Exit_Click(object sender, RoutedEventArgs e) {
			Close();
		} //退出

		private void MenuItem_About_Search(object sender, RoutedEventArgs e) {
			Search search = new Search();
			search.ShowDialog();
		} //查询窗口

		private void MenuItem_About_Click(object sender, RoutedEventArgs e) {
			About about = new About();
			about.ShowDialog();
		} //关于

		private void CheckReceiveFile_Click(object sender, RoutedEventArgs e) {
			if (FileList.Count==0) {
				MessageBox.Show("目前没有收到文件！");
			} //无文件待保存
			else {
				IMClassLibrary.FileDataPackage data;
				for (int i=0;i< FileList.Count; ++i) {
					data = FileList[i];
					SaveFileDialog saveFileDialog1 = new SaveFileDialog();
					saveFileDialog1.Filter = "接收文件(*" + data.FileExtension + ")|*" + data.FileExtension;
					if (saveFileDialog1.ShowDialog().Value) {
						FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);
						//StreamWriter streamWriter = new StreamWriter(fs, Encoding.UTF8);
						//streamWriter.Write(data.file);
						//streamWriter.Flush();
						//streamWriter.Close();
						fs.Write(data.file, 0, data.file.Length);
						fs.Close();
					}
				}
				FileList.Clear();
			}
		}
	}
}
