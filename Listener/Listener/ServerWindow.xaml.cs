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

    public partial class ServerWindow : Window
    {
        int UserCnt = 1000;
        ArrayList user;

        public ServerWindow() {
            InitializeComponent();
            //////user(ArrayList) Serization
            //textBlock3.Text += ((IPAddress)Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0)).ToString();
            textBlock3.Text += "127.0.0.1";
            GetSerizationUser();
            if (user.Count == 0) {
                UserClass ADMIN = new UserClass("admin", "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918");
                user.Add(ADMIN);
            }
        }

        ~ServerWindow() {
            SerizationUser();
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

        private int nowEnterPort;

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

        private void AcceptClientConnect() {
            IPAddress ip = IPAddress.Parse("127.0.0.1");//服务器端ip
            var myListener = new TcpListener(ip, nowEnterPort);//创建TcpListener实例
            myListener.Start();//start
            var newClient = new TcpClient();
            while (true) {
                try {
                    newClient = myListener.AcceptTcpClient();//等待客户端连接
                }
                catch {
                    return;
                }

                try {
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
                                        ///////////////////////////
                                    } else {
                                        user.Add(newUser);
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
                                    ////////////////////////////
                                    continue;
                                } else {
                                    /////////
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
            var threadAccept = new Thread(AcceptClientConnect);
            threadAccept.Start();
            button_StartServer.Content = "退出";

        }

        private void button_Click(object sender, RoutedEventArgs e) {
            if ((string)button_StartServer.Content == "运行") {
                MessageBox.Show("请先运行客户端");
                return;
            }
			textBlock1.Text += textBox.Text + ", ";
		}
    }
}
