using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;

namespace Listener {
	/// <summary>
	/// ServerWindow.xaml 的交互逻辑
	/// </summary>
	/// 

	public class UserClass {
		public string userId { get; set; }
		public string password { get; set; }
		public bool isOnline { get; set; }
		//public ArrayList message { get; set; }

		public UserClass() { }
		public UserClass(string ID, string Password) {
			userId = ID;
			password = Password;
			isOnline = false;
			//message = new ArrayList();

		}
	}

	public partial class ServerWindow : Window {
		ArrayList user;
		private int nowEnterPort;
		TcpListener myListener = null;
		bool IsPortCanUse = true;
		ArrayList AllGroupPort;

		public ServerWindow() {
			InitializeComponent();
			//////user(ArrayList) Serization
			//textBlock3.Text += ((IPAddress)Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0)).ToString();
			textBlock3.Text += "127.0.0.1";
			user = new ArrayList();
			//GetSerizationUser();
			if (user.Count == 0) {
				UserClass ADMIN = new UserClass("admin", "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918");
				user.Add(ADMIN);
			}
			AllGroupPort = new ArrayList();
		}

		~ServerWindow() {
			if(myListener!=null) {
				myListener.Stop(); //释放此端口
			}
		}

		private void SerizationUser() {
			XmlSerializer ser = new XmlSerializer(typeof(ArrayList));
			MemoryStream mem = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(mem, Encoding.UTF8);
			ser.Serialize(writer, user);
			writer.Close();
			string s = Encoding.UTF8.GetString(mem.ToArray());
			FileStream fs = new FileStream("C:\\USERDATA.txt", FileMode.Create);
			StreamWriter sw = new StreamWriter(fs);
			sw.Write(s);
			sw.Flush();
			sw.Close();
			fs.Close();
		}

		private void GetSerizationUser() {
			try {
				StreamReader sr = new StreamReader("C:\\USERDATA.txt");
				string s = sr.ReadLine();
				XmlSerializer mySerializer = new XmlSerializer(typeof(ArrayList));
				StreamReader mem2 = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(s)), Encoding.UTF8);
				ArrayList myObject = (ArrayList)mySerializer.Deserialize(mem2);
				user = myObject;
			}
			catch {
				user = new ArrayList();
				return;
			}
		}

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

		public void SendMessageTo(string IP, string port, string message) {
			TcpClient tcpClient;
			StateObject stateObject;
			tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
			stateObject = new StateObject(); ////每次发送建立一个StateObject类对象
			stateObject.tcpClient = tcpClient;
			//stateObject.buffer = SendMsg;
			stateObject.friendIPAndPort = IP + ":" + port; //所选好友IP和端口号
			var chatData = new IMClassLibrary.SingleChatDataPackage("Server", "Server", message);
			stateObject.buffer = chatData.DataPackageToBytes(); //buffer为发送的数据包的字节数组
			tcpClient.BeginConnect(IP, int.Parse(port), new AsyncCallback(SentCallBackF), stateObject);
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

		private void AcceptClientConnect() {
			//IPAddress ip = (IPAddress)Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0);//服务器端ip
			IPAddress ip = IPAddress.Parse("127.0.0.1");
			try {
				myListener = new TcpListener(ip, nowEnterPort);//创建TcpListener实例
				myListener.Start();//start
			}
			catch {
				MessageBox.Show("TcpListener创建失败，请更改端口号或检查计算机网络！");
				Close();
				return;
			}
			var newClient = new TcpClient();
			while (true) {
				try {
					newClient = myListener.AcceptTcpClient();//等待客户端连接
				}
				catch {
					if (newClient == null)
						return;
				}

				try {
					var IP = newClient.Client.RemoteEndPoint.ToString();
					byte[] receiveBytes = ReadFromTcpClient(newClient);
					int type = 0;
					using (MemoryStream ms = new MemoryStream(receiveBytes)) {
						IFormatter formatter = new BinaryFormatter();
						var DataPackage = formatter.Deserialize(ms) as IMClassLibrary.DataPackage;
						if (DataPackage == null) {
							MessageBox.Show("接收数据非数据包");
							continue;
						}
						type = DataPackage.MessageType;
					}
					if (type == 0) {
						MessageBox.Show("数据包非法");
						continue;
					}
					switch (type) {
						case 1: {
								var LogIn = new IMClassLibrary.LoginDataPackage(receiveBytes);
								if (LogIn.Receiver == "Server_Reg") {
									var newUser = new UserClass(LogIn.UserID, LogIn.Password);
									bool CanResiger = true;
									foreach (UserClass nowUser in user) {
										if (nowUser.userId == newUser.userId) {
											CanResiger = false;
											break;
										}
									}
									if (CanResiger == false) {
										SendMessageTo(LogIn.Sender.Split(':')[0], LogIn.Sender.Split(':')[1], "注册失败");
									}
									else {
										user.Add(newUser);
										SendMessageTo(LogIn.Sender.Split(':')[0], LogIn.Sender.Split(':')[1], "注册成功");
										///////////////////////////
									}
									continue;
								}
								bool SuccessLogin = false;
								for (int i = 0; i < user.Count; ++i) {
									var nowUser = (UserClass)user[i];
									if (LogIn.UserID == nowUser.userId && LogIn.Password == nowUser.password) {
										SuccessLogin = true;
										nowUser.isOnline = true;
									}
									user[i] = nowUser;
								}
								if (SuccessLogin == false) {
									SendMessageTo(LogIn.Sender.Split(':')[0], LogIn.Sender.Split(':')[1], "登录失败");
									continue;
								}
								else {
									SendMessageTo(LogIn.Sender.Split(':')[0], LogIn.Sender.Split(':')[1], "登录成功");
									continue;
								}
							}
							break;


						case 2: {
								var LogOut = new IMClassLibrary.LogoutDataPackage(receiveBytes);
								for (int i = 0; i < user.Count; ++i) {
									var t = (UserClass)user[i];
									if (t.userId == LogOut.UserID) {
										t.isOnline = false;
										user[i] = t;
										break;
									}
								}
							}
							break;


						case 3: {
								MessageBox.Show("发送了父类聊天数据包");
							}
							break;


						/*
                        case 4: {
                                var message = new IMClassLibrary.SingleChatDataPackage(receiveBytes);
                                var receiver = message.Receiver;
                                foreach (UserClass nowUser in user) {
                                    if (receiver == nowUser.userId) {
                                        nowUser.message.Add(message);
                                        break;
                                    }
                                }
                            }
                            break;
                        */
						case 5: {
								var message = new IMClassLibrary.SingleChatDataPackage(receiveBytes);
								var receiver = message.Receiver;
								foreach (UserClass nowUser in user) {
									if (receiver == nowUser.userId) {
										//nowUser.message.Add(message);
										break;
									}
								}
							}
							break;


							/*
                            case 6: {
                                    var message = new IMClassLibrary.ChangeNameDataPackage(receiveBytes);
                                    var receiver = message.Receiver;
                                    foreach (UserClass nowUser in user) {
                                        if (receiver == nowUser.userId)
                                            nowUser.name = message.Name;
                                        else
                                            nowUser.message.Add(message);
                                    }
                                }
                                break;
                            */
					}
				}
				catch {
					break;
				}
			}
		}

		private void button_StartServer_Click(object sender, RoutedEventArgs e) {
			if ((string)button_StartServer.Content == "退出") {
				this.Close();
			}

			bool canTurnPortToInt = int.TryParse(portText.Text, out nowEnterPort);
			if (canTurnPortToInt == false || nowEnterPort > 65535 || nowEnterPort < 1024) {
				MessageBox.Show("端口号输入错误");
				return;
			}
			button_StartServer.Content = "退出";
			IsPortCanUse = true;
			var threadAccept = new Thread(AcceptClientConnect);
			threadAccept.IsBackground = true;
			threadAccept.Start();
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			if ((string)button_StartServer.Content == "运行") {
				MessageBox.Show("请先运行客户端");
				return;
			}
			var nowEnterPort = 0;
			bool canTurnPortToInt = int.TryParse(textBox.Text, out nowEnterPort);
			if (canTurnPortToInt == false || nowEnterPort > 65535 || nowEnterPort < 1024) {
				MessageBox.Show("端口号输入错误");
				return;
			}
			foreach (int str in AllGroupPort) {
				if (str == nowEnterPort) {
					MessageBox.Show("已经添加过该端口");
					return;
				}
			}
			AllGroupPort.Add(nowEnterPort);
			textBlock1.Text += textBox.Text + ", ";
			nowTextBoxText = textBox.Text;
			var newThread = new Thread(GroupPortListener);
			newThread.IsBackground = true;
			newThread.Start();
		}

		public struct GroupUserStruct {
			public string userID;
			public string IP;
			public IPAddress IpAddress;
		}

		public class StateObject {
			public TcpClient tcpClient = null;
			public NetworkStream netstream = null;
			public byte[] buffer;
			public string friendIPAndPort = null; //记录好友的IP和端口号
		}

		string nowTextBoxText;

		public void GroupPortListener() {
			//IPAddress ip = (IPAddress)Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0);//服务器端ip
			IPAddress ip = IPAddress.Parse("127.0.0.1");
			var nowEnterPort = 0;
			bool canTurnPortToInt = int.TryParse(nowTextBoxText, out nowEnterPort);
			var myListener = new TcpListener(ip, nowEnterPort);//创建TcpListener实例
			myListener.Start();//start
			var newClient = new TcpClient();
			var group = new ArrayList();
			while (true) {
				try {
					newClient = myListener.AcceptTcpClient();//等待客户端连接
				}
				catch {
					if (newClient == null)
						return;
				}

				try {
					byte[] receiveBytes = ReadFromTcpClient(newClient);
					var userMessage = new IMClassLibrary.SingleChatDataPackage(receiveBytes);
					bool isNewUser = true;
					for (int i = 0; i < group.Count; ++i) {
						var t = (GroupUserStruct)group[i];
						if (t.IP == userMessage.Receiver && t.userID == userMessage.Sender) {
							TcpClient tcpClient;
							StateObject stateObject;
							for (int j = 0; j < group.Count; ++j) {
								if (i == j)
									continue;
								var send = (GroupUserStruct)group[j];
								tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
								stateObject = new StateObject(); ////每次发送建立一个StateObject类对象
								stateObject.tcpClient = tcpClient;
								//stateObject.buffer = SendMsg;
								stateObject.friendIPAndPort = send.IP; //所选好友IP和端口号
								var chatData = new IMClassLibrary.MultiChatDataPackage(nowEnterPort.ToString(), userMessage.Receiver, userMessage.Message);
								chatData.SenderID = userMessage.Sender;
								chatData.MessageType = 5;
								stateObject.buffer = chatData.DataPackageToBytes(); //buffer为发送的数据包的字节数组
								string[] SplitStr = send.IP.Split(':');
								tcpClient.BeginConnect(SplitStr[0], int.Parse(SplitStr[1]), new AsyncCallback(SentCallBackF), stateObject); //异步连接
							}
							isNewUser = false;
							break;
						}
					}
					if (isNewUser == true) {
						var newGroupuserStruct = new GroupUserStruct();
						newGroupuserStruct.userID = userMessage.Sender;
						newGroupuserStruct.IP = userMessage.Receiver;
						group.Add(newGroupuserStruct);
					}
				}
				catch {
					break;
				}
			}
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e) {
			string s = "";
			foreach (UserClass nowUser in user) {
				s += nowUser.userId + " " + nowUser.password + "\r\n";
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
		}

		private void MenuItem_Click_1(object sender, RoutedEventArgs e) {
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
							if (str[str.Length - 1] == '\r') {
								AddUser(str.Substring(0, str.Length - 1));
							} //忽略'\r'
							else {
								AddUser(str);
							}
						}
					}
				}
			}
		}

		private void AddUser(string str) {
			string[] Info = str.Split(' ');
			for (int i = 0; i < user.Count; ++i) {
				var t = (UserClass)user[i];
				if (t.userId == Info[0]) {
					t.password = Info[1]; //密码覆盖
					return;
				}
			}
			var newUser = new UserClass(Info[0], Info[1]);
			user.Add(newUser);
		}

		private void MenuItem_Exit_Click(object sender, RoutedEventArgs e) {
			Close();
		} //退出

		private void MenuItem_About_Click(object sender, RoutedEventArgs e) {
			About about = new About();
			about.ShowDialog();
		} //关于

	}
}
