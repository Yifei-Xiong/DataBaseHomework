using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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

namespace Listener
{
    internal class People
    {
        public int id; //账号 开始于1000
        public string name; //昵称
        public string password; //密码
        public bool is_online; //是否在线;
        public TcpClient tcpclient; //通信的tcpclient对象
        public IPEndPoint ip;
    };

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            EndButton.IsEnabled = false;
            var listenerIP_t = Dns.GetHostAddresses("");
            localIpAddress = listenerIP_t[0];
        }

        private IPAddress localIpAddress;
        private TcpListener tcpListener;
        private ArrayList User;
        private int listenerPort;
        private UdpClient receiveUdpClient;

        //点击开始
        private void Start()
        {
            var sp = portText.Text;
            //获得输入的port
            var port = int.Parse(sp);
            var serverIPEndPoint = new IPEndPoint(localIpAddress, port);

            //建立监听的Udp，来监听login和logout
            receiveUdpClient = new UdpClient(serverIPEndPoint);
            var threadReceive = new Thread(ReceiveMessage);
            threadReceive.Start();

            //改变按钮状态
            StartButton.IsEnabled = false;
            EndButton.IsEnabled = true;

            //聊天端口，尝试其他所有非port端口
            listenerPort = port + 1;
            for ( ; listenerPort != port; )
            {
                bool can_link = true;
                try
                {
                    tcpListener = new TcpListener(localIpAddress, listenerPort);
                }
                catch
                {
                    //在listenerPort下建立连接失败，尝试新的端口
                    listenerPort++;
                    if (listenerPort > 65536)
                        listenerPort = 1;
                    can_link = false;
                }

                if (can_link)
                    break;
            }

            //如果尝试完所有端口
            if (listenerPort == port)
            {
                MessageBox.Show("无可用监听端口");
                End();
                return;
            }

            //否则成功建立监听
            tcpListener.Start();
            var threadAccept = new Thread(ListenClientConnect);
            threadAccept.Start();
        }

        //接收客户端连接,LOGIN
        private void ListenClientConnect()
        {
            TcpClient newClient = null;
            while (true)
            {
                try
                {
                    newClient = tcpListener.AcceptTcpClient();
                }
                catch
                {
                    break;
                }

                bool is_peo = false;
                foreach (People peo in User)
                {
                    if (peo.ip == newClient.Client.RemoteEndPoint)
                    {
                        peo.tcpclient = newClient;
                        is_peo = true;
                        break;
                    }
                }
                if (is_peo == false) 
                {
                    MessageBox.Show("发现未知的人连接");
                }
            }
        }

        //点击结束按钮
        private void End()
        {
            foreach(People user in User)
            {
                user.tcpclient.Close();
            }
            tcpListener.Stop();

            StartButton.IsEnabled = true;
            EndButton.IsEnabled = false;
        }

        //对于login和logout消息进行处理
        private void ReceiveMessage()
        {
            //任选可用端口
            var remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    //处理Udp接收到的消息
                    //格式为
                    //LOGIN:id,password,ip,port
                    //LOGOUT:id
                    var loginInfo = receiveUdpClient.Receive(ref remoteIPEndPoint);
                    var loginMessage = Encoding.Unicode.GetString(loginInfo, 0, loginInfo.Length);
                    var MessageItem = loginMessage.Split(',');
                    switch (MessageItem[0])
                    {
                        case "LOGIN":
                            var Id = int.Parse(MessageItem[1]) - 1000;
                            var Password = MessageItem[2];
                            var Ip = MessageItem[3];
                            var Port = MessageItem[4].TrimEnd(';');
                            var ipAddress = IPAddress.Parse(Ip);
                            var ClientIPEndPoint = new IPEndPoint(ipAddress, int.Parse(Port));
                            var sendUpUDP = new UdpClient(0);
                            string message;
                            var nowUser = (People)User[Id];
                            if (nowUser.password == Password)
                            {
                                // Connect
                                UserConnect(Id, ClientIPEndPoint);
                                message = "SUCCESS LOGIN:" + localIpAddress + listenerPort;
                            }
                            else
                            {
                                message = "UNSECCESS LOGIN";
                            }

                            var messageBytes = Encoding.Unicode.GetBytes(message);
                            sendUpUDP.Send(messageBytes, messageBytes.Length, ClientIPEndPoint);
                            sendUpUDP.Close();

                            break;

                        case "LOGOUT":
                            //logout
                            var id2 = int.Parse(MessageItem[1].TrimEnd(';')) - 1000;
                            var nowUser2 = (People) User[id2];
                            nowUser2.is_online = false;
                            nowUser2.ip = null;
                            nowUser2.tcpclient = null;
                            break;
                    }
                }
                catch
                {
                    break;
                }
            }

            MessageBox.Show("服务终止");
        }

        //当有用户登录时
        private void UserConnect(int id, IPEndPoint ip)
        {
            var nowUser = (People) User[id];
            nowUser.is_online = true;
            nowUser.ip = ip;
        }

    }
}
