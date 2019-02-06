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

namespace Listener
{
	/// <summary>
	/// ServerWindow.xaml 的交互逻辑
	/// </summary>
    /// 

    public class UserClass
    {
        public int userId;
        public string name;
        public string password;
        public bool isOnline;
    }

	public partial class ServerWindow : Window
    {
        public ServerWindow()
        {
            InitializeComponent();
            button_StopServer.IsEnabled = false;
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
            var myListener = new TcpListener(ip, 7890);//创建TcpListener实例
            myListener.Start();//start
            var newClient = myListener.AcceptTcpClient();//等待客户端连接
            var newThread = new Thread(AcceptClientConnect);
            newThread.Start();
            while (true)
            {
                try {

                } catch {

                }
            }
        }

		private void button_StartServer_Click(object sender, RoutedEventArgs e) {
            bool canTurnPortToInt = int.TryParse(port.Text, out nowEnterPort);
            if (canTurnPortToInt == false || nowEnterPort > 65536) {
                MessageBox.Show("端口号输入错误");
                return;
            }
            var threadAccept = new Thread(AcceptClientConnect);
            threadAccept.Start();
        }

		private void button_StopServer_Click(object sender, RoutedEventArgs e) {

		}
	}
}
