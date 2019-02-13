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

namespace Listener
{
	/// <summary>
	/// ServerWindow.xaml 的交互逻辑
	/// </summary>
    /// 

    public class UserClass
    {
        public string userId;
        public string name;
        public string password;
        public bool isOnline;
        public ArrayList message;
    }

	public partial class ServerWindow : Window
    {
        public ServerWindow()
        {
            InitializeComponent();
            //////user(ArrayList) Serization
            GetSerizationUser();
        }

        ~ ServerWindow() {
            SerizationUser();
        }

        ArrayList user;
        private void SerizationUser() {
            XmlSerializer ser = new XmlSerializer(typeof(ArrayList));
            MemoryStream mem = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mem, Encoding.Default);
            ser.Serialize(writer, user);
            writer.Close();
            string s = Encoding.Default.GetString(mem.ToArray());
            FileStream fs = new FileStream("C:\\all.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(s);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        private void GetSerizationUser() {
            StreamReader sr = new StreamReader("C:\\all.txt");
            string s = sr.ReadLine();
            XmlSerializer mySerializer = new XmlSerializer(typeof(ArrayList));
            StreamReader mem2 = new StreamReader(new MemoryStream(Encoding.Default.GetBytes(s)), Encoding.Default);
            ArrayList myObject = (ArrayList)mySerializer.Deserialize(mem2);
        }

        private int nowEnterPort;

        private void AcceptClientConnect()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");//服务器端ip
            var myListener = new TcpListener(ip, nowEnterPort);//创建TcpListener实例
            myListener.Start();//start
            var newClient = myListener.AcceptTcpClient();//等待客户端连接
            myListener.Stop();
            var newThread = new Thread(AcceptClientConnect);
            newThread.Start();
            int index = -1;
            while (true)
            {
                try {
                    NetworkStream clientStream = newClient.GetStream();
                    if (index >= 0) {
                        var nUser = (UserClass)user[index];
                        if (nUser.message.Count > 0) {
                            foreach (IMClassLibrary.DataPackage message in nUser.message) {
                                var info = message.DataPackageToBytes();
                                clientStream.Write(info, 0, info.Length);
                            }
                            nUser.message.Clear();
                            continue;
                        }
                    }

                    byte[] receiveBytes = new byte[10000];
                    clientStream.Read(receiveBytes, 0, 10000);
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
                    string processUser = "";
                    switch (type) {
                        case 1: {
                                var LogIn = new IMClassLibrary.LoginDataPackage(receiveBytes);
                                bool SuccessLogin = false;
                                foreach (UserClass nowUser in user) {
                                    if (LogIn.UserID == nowUser.userId && LogIn.Password == nowUser.password) {
                                        SuccessLogin = true;
                                        processUser = nowUser.userId;
                                        index = int.Parse(processUser) - 1000;
                                        nowUser.isOnline = true;
                                    }
                                }
                                if (SuccessLogin == false) 
                                    return;
                            }
                            break;
                        case 2: {
                                var LogOut = new IMClassLibrary.LogoutDataPackage(receiveBytes);
                                if (LogOut.UserID != processUser) {
                                    MessageBox.Show("登出用户名非正确");
                                    continue;
                                }
                                foreach (UserClass nowUser in user) {
                                    if (nowUser.userId == processUser)
                                        nowUser.isOnline = false;
                                }
                            }
                            break;
                        case 3: {
                                MessageBox.Show("发送了父类聊天数据包");
                            }
                            break;
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
                        case 5: {

                            }
                            break;
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
                    }
                }
                catch {
                    break;
                }
            }
        }

		private void button_StartServer_Click(object sender, RoutedEventArgs e) {
            bool canTurnPortToInt = int.TryParse(port.Text, out nowEnterPort);
            if (canTurnPortToInt == false || nowEnterPort > 65535 || nowEnterPort < 1024) {
                MessageBox.Show("端口号输入错误");
                return;
            }
            var threadAccept = new Thread(AcceptClientConnect);
            threadAccept.Start();
        }
	}
}
