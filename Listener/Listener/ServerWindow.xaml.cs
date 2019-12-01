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
using System.Collections.ObjectModel;
using System.Windows.Threading;
using MySql.Data.MySqlClient;

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
		//bool IsPortCanUse = true;
		public struct UserPw {
			public string UserID { get; set; }
			public string Password { get; set; }
			public string Online { get; set; }
			public string LogInTime { get; set; }
			public string LogInIP { get; set; }
			public string LogInPort { get; set; }
		}
		public struct MessageInfo {
			public string MsgIP { get; set; }
			public string MsgPort { get; set; }
			public string UserID { get; set; }
			public string Msg { get; set; }
			public string MsgTime { get; set; }
			public string MsgID { get; set; }
		} //单条消息
		public struct UserInfo {
			public string UserIP { get; set; }
			public string UserPort { get; set; }
			public string UserID { get; set; }
			public string LastMsgTime { get; set; }
			public int MsgTimes { get; set; }
		} //单个用户
		public class AllUser : ObservableCollection<UserPw> { }
		public class GroupMsg : ObservableCollection<MessageInfo> { } //定义一个群组的聊天数据集合 
		public class GroupUser : ObservableCollection<UserInfo> { } //定义一个群组的成员集合 
        [Serializable]
        public class GroupInfo {
			public GroupInfo() {
				this.groupMsg = new GroupMsg();
				this.groupUser = new GroupUser();
			}
			public GroupMsg groupMsg { get; set; }
			public GroupUser groupUser { get; set; }
			public int GroupPort { get; set; }
		}
        [Serializable]
        public class AllGroup : ObservableCollection<GroupInfo> { } //定义所有群组的集合 
		AllUser allUser;
		AllGroup allGroup;
		ArrayList AllGroupPort;
		private delegate void SetGroupMsg(MessageInfo arg, int port);
		private delegate void SetGroupUser(UserInfo arg, int port);
		private delegate void SetGroupUser2(string arg, int port); //modify
		private delegate void SetAllGroup(GroupInfo arg);
		private delegate void SetUser(UserPw arg);
		private void SetGroupMsgSource(MessageInfo arg, int port) {
			int i;
			GroupInfo gp;
			for (i=0; i< allGroup.Count; i++) {
				if (allGroup[i].GroupPort == port) {
					gp = allGroup[i];
					gp.groupMsg.Add(arg);
					break;
				}
			}
			if (i== allGroup.Count) {
				MessageBox.Show("异步消息修改错误!");
			}
		}
		private void SetGroupUserSource(UserInfo arg, int port) {
			int i;
			GroupInfo gp;
			for (i = 0; i < allGroup.Count; i++) {
				if (allGroup[i].GroupPort == port) {
					gp = allGroup[i];
					if (gp.groupUser.IndexOf(arg)==-1) {
						gp.groupUser.Add(arg);
					}
					else {
						gp.groupUser.Remove(arg);
						arg.LastMsgTime = DateTime.Now.ToString();
						arg.MsgTimes++;
						gp.groupUser.Add(arg);
					}
					break;
				}
			}
			if (i == allGroup.Count) {
				MessageBox.Show("异步用户修改错误!");
			}
		}

		private void SetGroupUserSource2(string arg, int port) {
			int i;
			GroupInfo gp;
			for (i = 0; i < allGroup.Count; i++) {
				if (allGroup[i].GroupPort == port) {
					gp = allGroup[i];
					int j;
					for (j = 0; j < gp.groupUser.Count; i++) {
						if (gp.groupUser[j].UserID == arg) {
							var vr = new UserInfo();
							//var vr = allGroup[i].groupUser[j];
							vr.LastMsgTime = DateTime.Now.ToString();
							vr.MsgTimes = gp.groupUser[j].MsgTimes+1;
							vr.UserID = gp.groupUser[j].UserID;
							vr.UserIP = gp.groupUser[j].UserIP;
							vr.UserPort = gp.groupUser[j].UserPort;
							gp.groupUser.RemoveAt(j);
							//allGroup[i].groupUser[j] = vr;////
							gp.groupUser.Add(vr);
							return;
						}
					}
					if (j == allGroup.Count) {
						MessageBox.Show("异步用户修改查询错误!");
					}
					break;
				}
			}
			if (i == allGroup.Count) {
				MessageBox.Show("异步用户修改错误!");
			}
		}

		private void SetAllGroupSource(GroupInfo arg) {
			allGroup.Add(arg);
		}
		private void SetUserSource(UserPw arg) {
			int i;
			for (i=0; i<allUser.Count; i++) {
				if (allUser[i].Password==arg.Password) {
					break;
				}
			}
			if (i == allUser.Count) {
				allUser.Add(arg);
			}
			else {
				UserPw up = new UserPw();
				up = allUser[i];
				up.LogInIP = arg.LogInIP;
				up.LogInPort = arg.LogInPort;
				up.LogInTime = arg.LogInTime;
				up.Online = "在线";
				allUser.RemoveAt(i);
				allUser.Add(up);
			}
		}

		public ServerWindow() {
			InitializeComponent();
			//////user(ArrayList) Serization
			//textBlock3.Text += ((IPAddress)Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0)).ToString();
			textBlock3.Text += "127.0.0.1";
			user = new ArrayList();
			allUser = new AllUser();
			//GetSerizationUser();
			if (user.Count == 0) {
				UserClass ADMIN = new UserClass("admin", "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918");
				user.Add(ADMIN);
				UserPw admin = new UserPw();
				admin.UserID = "admin";
				admin.Password = "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918";
				allUser.Add(admin);
			}
			allGroup = new AllGroup();
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
									UserPw up = new UserPw();
									up.UserID = LogIn.UserID;
									up.Password = LogIn.Password;
									bool CanResiger = true;
									foreach (UserPw up1 in allUser) {
										if (up1.UserID == newUser.userId) {
											CanResiger = false;
											break;
										}
									}
									if (CanResiger == false) {
										SendMessageTo(LogIn.Sender.Split(':')[0], LogIn.Sender.Split(':')[1], "注册失败");
									}
									else {
										user.Add(newUser);
										this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetUser(SetUserSource), up);
										//allUser.Add(up);
										SendMessageTo(LogIn.Sender.Split(':')[0], LogIn.Sender.Split(':')[1], "注册成功");
										///////////////////////////
									}
									continue;
								}
								bool SuccessLogin = false;
								//for (int i = 0; i < user.Count; ++i) {
								//	var nowUser = (UserClass)user[i];
								//	if (LogIn.UserID == nowUser.userId && LogIn.Password == nowUser.password) {
								//		SuccessLogin = true;
								//		nowUser.isOnline = true;
								//	}
								//	user[i] = nowUser;
								//}
								for (int i = 0; i < allUser.Count; ++i) {
									UserPw up2 = allUser[i];
									if (LogIn.UserID == up2.UserID && LogIn.Password == up2.Password) {
										SuccessLogin = true;
									}
								}
								if (SuccessLogin == false) {
									SendMessageTo(LogIn.Sender.Split(':')[0], LogIn.Sender.Split(':')[1], "登录失败");
									continue;
								}
								else {
									SendMessageTo(LogIn.Sender.Split(':')[0], LogIn.Sender.Split(':')[1], "登录成功");
									UserPw up = new UserPw();
									up.UserID = LogIn.UserID;
									up.Password = LogIn.Password;
									up.LogInIP = LogIn.Sender.Split(':')[0];
									up.LogInPort = LogIn.Sender.Split(':')[1];
									up.LogInTime = DateTime.Now.ToString();
									this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetUser(SetUserSource), up);
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
			//IsPortCanUse = true;
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
			textBox.Clear(); //清空输入框
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
			try {
				myListener.Start(); //start
			}
			catch {
				MessageBox.Show("无法在该端口建立群组，该端口已被占用");
				return;
			}
			var newClient = new TcpClient();
			//var group = new ArrayList();
			GroupInfo groupInfo = new GroupInfo();
			groupInfo.GroupPort = nowEnterPort;
			//allGroup.Add(groupInfo);////
			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetAllGroup(SetAllGroupSource), groupInfo);
			while (true) {
				try {
					newClient = myListener.AcceptTcpClient();//等待客户端连接
				}
				catch {
					if (newClient == null) {
						MessageBox.Show("无法在该端口建立群组，该端口已被占用, Error 401");
						return;
					}	
				}

				try {
					byte[] receiveBytes = ReadFromTcpClient(newClient);
					var userMessage = new IMClassLibrary.SingleChatDataPackage(receiveBytes);

					MessageInfo msg = new MessageInfo();
					msg.MsgID = (groupInfo.groupMsg.Count + 1).ToString();
					msg.MsgTime = userMessage.sendTime.ToString();
					msg.MsgIP = userMessage.Receiver.Split(':')[0];
					msg.MsgPort = userMessage.Receiver.Split(':')[1];
					msg.UserID = userMessage.Sender;
					msg.Msg = userMessage.Message;
					//groupInfo.groupMsg.Add(msg);////
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetGroupMsg(SetGroupMsgSource), msg, nowEnterPort);

					bool isNewUser = true;
					for (int i = 0; i < groupInfo.groupUser.Count; ++i) {
						var t = groupInfo.groupUser[i];
						if (t.UserID == userMessage.Sender) {
							TcpClient tcpClient;
							StateObject stateObject;
							for (int j = 0; j < groupInfo.groupUser.Count; ++j) {
								if (i == j) {
									this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetGroupUser(SetGroupUserSource), t, nowEnterPort);
									continue;
								}  //找到发送者
								   //var send = (GroupUserStruct)group[j];
								var send = groupInfo.groupUser[j];
								tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
								stateObject = new StateObject(); ////每次发送建立一个StateObject类对象
								stateObject.tcpClient = tcpClient;
								//stateObject.buffer = SendMsg;
								stateObject.friendIPAndPort = send.UserIP+":"+send.UserPort; //所选好友IP和端口号
								var chatData = new IMClassLibrary.MultiChatDataPackage(nowEnterPort.ToString(), userMessage.Receiver, userMessage.Message);
								chatData.SenderID = userMessage.Sender;
								chatData.MessageType = 5;
								stateObject.buffer = chatData.DataPackageToBytes(); //buffer为发送的数据包的字节数组
								//string[] SplitStr = send.IP.Split(':');
								tcpClient.BeginConnect(send.UserIP, int.Parse(send.UserPort), new AsyncCallback(SentCallBackF), stateObject); //异步连接
							}
							//se.LastMsgTime = DateTime.Now.ToString();
							//se.MsgTimes++;
							isNewUser = false;
							//groupInfo.groupUser[i] = se;////
							break;
						}
					}
					if (isNewUser == true) {
						//var newGroupuserStruct = new GroupUserStruct();
						//newGroupuserStruct.userID = userMessage.Sender;
						//newGroupuserStruct.IP = userMessage.Receiver;
						//group.Add(newGroupuserStruct);
						UserInfo info = new UserInfo();
						info.UserID = userMessage.Sender;
						info.UserIP = userMessage.Receiver.Split(':')[0];
						info.UserPort = userMessage.Receiver.Split(':')[1];
						info.MsgTimes = 1;
						info.LastMsgTime = DateTime.Now.ToString();
						//groupInfo.groupUser.Add(info);////
						this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetGroupUser(SetGroupUserSource), info, nowEnterPort);
					}
				}
				catch {
					MessageBox.Show("无法在该端口建立群组，该端口已被占用，Error 556");
					return;
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

		private void MenuItem_Click_2(object sender, RoutedEventArgs e) {
			SearchUser searchUser = new SearchUser(allUser);
			searchUser.ShowDialog();
		} //查询用户信息

		private void MenuItem_Click_3(object sender, RoutedEventArgs e) {
			SearchGroup searchGroup = new SearchGroup(allGroup);
			searchGroup.ShowDialog();
			//ChatDataSync(allMsg);
			//SQLDocker = allGroup;
		} //查询群组信息

		private void MenuItem_Click_4(object sender, RoutedEventArgs e) {
			SearchUserInGroup searchUserInGroup = new SearchUserInGroup(allGroup);
			searchUserInGroup.ShowDialog();
		} //查询群组的用户

		private void MenuItem_Click_Sync1(object sender, RoutedEventArgs e) {
			SQLDocker_user = allUser;
		} //同步用户信息至数据库

		private void MenuItem_Click_Sync2(object sender, RoutedEventArgs e) {
			SQLDocker_group = allGroup;
		} //同步群组信息至数据库

		private void MenuItem_Click_Sync3(object sender, RoutedEventArgs e) {
			allUser = SQLDocker_user;
		} //同步数据库至用户信息

		private void MenuItem_Click_Sync4(object sender, RoutedEventArgs e) {
			allGroup = SQLDocker_group;
		}

		private MySqlConnection connection;

		public void InitSQLDocker() {
			connection = new MySqlConnection("server=106.14.44.67;user=root;password=0000;database=serverdb1;");
			connection.Open();
		}

		AllGroup SQLDocker_group
        {
			get
			{
				if (connection == null || connection.State != System.Data.ConnectionState.Open)
					InitSQLDocker();
				MySqlCommand sql = new MySqlCommand("SELECT * FROM allgroup", connection);
				MySqlDataReader reader = sql.ExecuteReader();
				AllGroup result = new AllGroup();
				while (reader.Read()) {
					using (MemoryStream ms = new MemoryStream(Serialize(reader[0]))) {
						IFormatter formatter = new BinaryFormatter();
						result = formatter.Deserialize(ms) as AllGroup;
					}
				}
				reader.Close();
				return result;
			}
			set
			{
				if (connection == null || connection.State != System.Data.ConnectionState.Open)
					InitSQLDocker();
				var query = new MySqlCommand("DELETE FROM allgroup", connection);
				query.ExecuteNonQuery();
				MySqlCommand sql = new MySqlCommand("INSERT INTO allgroup(GroupInfo) " + "VALUES(\"" + Serialize(allGroup) + "\")", connection);
				sql.ExecuteNonQuery();

			}
		}

        AllUser SQLDocker_user
        {
			get
			{
				if (connection == null || connection.State != System.Data.ConnectionState.Open)
					InitSQLDocker();
				MySqlCommand sql = new MySqlCommand("SELECT * FROM alluser", connection);
				MySqlDataReader reader = sql.ExecuteReader();
				AllUser result = new AllUser();
				while (reader.Read()) {
					UserPw up = new UserPw();
					up.UserID = reader[0].ToString();
					up.Password = reader[1].ToString();
					up.Online = reader[2].ToString();
					up.LogInTime = reader[3].ToString();
					up.LogInIP = reader[4].ToString();
					up.LogInPort = reader[5].ToString();
					result.Add(up);
				}
				reader.Close();
				return result;
			}
			set
			{
				if (connection == null || connection.State != System.Data.ConnectionState.Open)
					InitSQLDocker();
				var query = new MySqlCommand("DELETE FROM alluser", connection);
				query.ExecuteNonQuery();
				foreach (UserPw up in value) {
					MySqlCommand sql = new MySqlCommand("INSERT INTO alluser(UserID,Pwd,Ol,LogInTime,LogInIP,LogInPort) "
						+ "VALUES(\"" + up.UserID + "\", \"" + up.Password + "\", \"" + up.Online + "\", \"" + up.LogInTime + "\", \"" + up.LogInIP + "\", \"" + up.LogInPort + "\")", connection);
					sql.ExecuteNonQuery();
				}
			}
		}

		public static byte[] Serialize(object data) {
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream rems = new MemoryStream();
			formatter.Serialize(rems, data);
			return rems.GetBuffer();
		} //序列化

		public static object Deserialize(byte[] data) {
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream rems = new MemoryStream(data);
			data = null;
			return formatter.Deserialize(rems);
		} //反序列化

	}
}
