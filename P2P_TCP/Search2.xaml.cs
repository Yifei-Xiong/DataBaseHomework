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
            UserList.ItemsSource = allMsg; //UserList的数据源
            SearchMsg = new P2PClient.AllMsg();
        }

		P2PClient.AllMsg allMsg; //ref
        P2PClient.AllMsg SearchMsg; //Search

        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (UserList.SelectedItems.Count != 0) {
				P2PClient.Msg msg = (P2PClient.Msg)UserList.SelectedItem;
				textBlock.Text = "消息序号: " + msg.MsgID;
				textBlock1.Text = "发送时间: " + msg.MsgTime;
				textBox.Text = msg.UserIP;
				textBox1.Text = msg.UserPort;
				textBox2.Text = msg.UserName;
				textBlock4.Text = "消息类型: " + msg.IsGroup;
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
			if (SearchMsg!=null) {
				for (int i = 0; i < SearchMsg.Count; i++) {
					if (SearchMsg[i].MsgID == ((P2PClient.Msg)UserList.SelectedItem).MsgID) {
						P2PClient.Msg msg = new P2PClient.Msg();
						msg = SearchMsg[i];
						//msg.MsgID = allMsg[i].MsgID;
						//msg.MsgTime = allMsg[i].MsgTime;
						//msg.IsGroup = allMsg[i].IsGroup;
						msg.UserIP = textBox.Text;
						msg.UserPort = textBox1.Text;
						msg.UserName = textBox2.Text;
						msg.ChatMsg = textBox3.Text;
						SearchMsg[i] = msg;
						break;
					}
				}
			}
        } //修改消息记录

		private void button1_Click(object sender, RoutedEventArgs e) {
			if (UserList.SelectedItems.Count == 0) {
				MessageBox.Show("未选择删除的消息记录");
			}
			P2PClient.Msg msg = (P2PClient.Msg)UserList.SelectedItem;
			allMsg.Remove(msg);
            SearchMsg.Remove(msg);
			for (int i = 0; i < allMsg.Count; i++) {
				P2PClient.Msg msg1 = new P2PClient.Msg();
				msg1 = allMsg[i];
				msg1.MsgID = (i + 1).ToString();
				allMsg[i] = msg1;
			}
		} //删除消息记录

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            SearchMsg = new P2PClient.AllMsg();
            for (int i = 0; i < allMsg.Count; i++)
            {
                SearchMsg.Add(allMsg[i]);
            } //Copy

			if (checkBox2.IsChecked == true) {
				if (textBox_Copy.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].MsgID.IndexOf(textBox_Copy.Text) == -1) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //MsgID

				if (textBox_Copy1.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].MsgTime.IndexOf(textBox_Copy1.Text) == -1) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //MsgTime

				if (textBox_Copy2.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].UserIP.IndexOf(textBox_Copy2.Text) == -1) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //UserIP

				if (textBox_Copy3.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].UserPort.IndexOf(textBox_Copy3.Text) == -1) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //UserPort

				if (textBox_Copy5.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].UserName.IndexOf(textBox_Copy5.Text) == -1) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //UserName

				if (textBox_Copy6.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].ChatMsg.IndexOf(textBox_Copy6.Text) == -1) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //ChatMsg

				if (checkBox.IsChecked == false) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].IsGroup != "群组聊天") {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //IsGroup

				if (checkBox1.IsChecked == false) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].IsGroup != "个人聊天") {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //IsFrirnd
			} //精确搜索

			if (checkBox2.IsChecked == true) {
				if (textBox_Copy.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].MsgID != textBox_Copy.Text) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //MsgID

				if (textBox_Copy1.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].MsgTime != textBox_Copy1.Text) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //MsgTime

				if (textBox_Copy2.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].UserIP != textBox_Copy2.Text) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //UserIP

				if (textBox_Copy3.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].UserPort != textBox_Copy3.Text) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //UserPort

				if (textBox_Copy5.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].UserName != textBox_Copy5.Text) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //UserName

				if (textBox_Copy6.Text != string.Empty) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].ChatMsg != textBox_Copy6.Text) {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //ChatMsg

				if (checkBox.IsChecked == false) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].IsGroup != "群组聊天") {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //IsGroup

				if (checkBox1.IsChecked == false) {
					for (int i = 0; i < SearchMsg.Count; i++) {
						if (SearchMsg[i].IsGroup != "个人聊天") {
							SearchMsg.Remove(SearchMsg[i]);
							i--;
						}
					}
				} //IsFrirnd
			} //模糊搜索

			UserList.ItemsSource = SearchMsg;
        }

        private void Button2_Copy_Click(object sender, RoutedEventArgs e)
        {
            textBox_Copy.Clear();
            textBox_Copy1.Clear();
            textBox_Copy2.Clear();
            textBox_Copy3.Clear();
            textBox_Copy5.Clear();
            textBox_Copy6.Clear();
			checkBox.IsChecked = true;
			checkBox1.IsChecked = true;
			UserList.ItemsSource = allMsg;
        } //清空
    }
}
