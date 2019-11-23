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
	/// Search2.xaml 的交互逻辑
	/// </summary>
	public partial class Search2 : Window {
		public Search2() {
			InitializeComponent();
		}
		public Search2(P2PClient.AllMsg allMsg) {
			InitializeComponent();
			this.allMsg = allMsg;
			//for (int i = 0; i < allMsg.Count; i++) {
			//	this.allMsg.Add(allMsg[i]);
			//}
			UserList.ItemsSource = this.allMsg; //UserList的数据源
		}
		P2PClient.AllMsg allMsg;

		private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (UserList.SelectedItems.Count != 0) {
				P2PClient.Msg msg = (P2PClient.Msg)UserList.SelectedItem;
				textBlock.Text = "消息序号: " + msg.MsgID;
				textBlock1.Text = "发送时间: " + msg.MsgTime;
				textBox.Text = msg.UserIP;
				textBox1.Text = msg.UserPort;
				textBox2.Text = msg.UserName;
				textBlock4.Text = "消息类型" + msg.IsGroup;
				textBox3.Text = msg.ChatMsg;
			} //选择一条消息记录
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			for (int i = 0; i < allMsg.Count; i++) {
				if (allMsg[i].MsgID==((P2PClient.Msg)UserList.SelectedItem).MsgID) {
					P2PClient.Msg msg = new P2PClient.Msg();
					msg = allMsg[i];
					//msg.MsgID = allMsg[i].MsgID;
					//msg.MsgTime = allMsg[i].MsgTime;
					//msg.IsGroup = allMsg[i].IsGroup;
					msg.UserIP = textBox.Text;
					msg.UserPort = textBox1.Text;
					msg.UserName = textBox2.Text;
					msg.ChatMsg = textBox3.Text;
					allMsg[i] = msg;
					break;
				}
			}
		} //修改消息记录

		private void button1_Click(object sender, RoutedEventArgs e) {
			if (UserList.SelectedItems.Count == 0) {
				MessageBox.Show("未选择删除的联系人");
			}
			P2PClient.Msg msg = (P2PClient.Msg)UserList.SelectedItem;
			allMsg.Remove(msg);
			for (int i = 0; i < allMsg.Count; i++) {
				P2PClient.Msg msg1 = new P2PClient.Msg();
				msg1 = allMsg[i];
				msg1.MsgID = (i + 1).ToString();
				allMsg[i] = msg1;
			}
		} //删除消息记录
	}
}
