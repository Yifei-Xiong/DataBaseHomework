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
using System.Windows.Shapes;
using System.Collections.ObjectModel; //ObservableCollection
using System.ComponentModel; //INotifyPropertyChanged

namespace P2P_TCP {
	/// <summary>
	/// Search.xaml 的交互逻辑
	/// </summary>

	//private static ObservableCollection<UserData> List = new ObservableCollection<UserData>();
	public partial class Search : Window {
		public Search() {
			InitializeComponent();
			//UserList.ItemsSource = List;
		}
		public Search(P2PClient.FriendIPAndPorts allContacts) {
			InitializeComponent();
			this.allContacts = allContacts;
			//for (int i = 0; i < allContacts.Count; i++) {
			//	this.allContacts.Add(allContacts[i]);
			//}
			ContactList.ItemsSource = allContacts; //UserList的数据源
			SearchContacts = new P2PClient.FriendIPAndPorts();
		}

		P2PClient.FriendIPAndPorts allContacts; //ref
		P2PClient.FriendIPAndPorts SearchContacts; //Search

		private void ContactList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (ContactList.SelectedItems.Count != 0) {
				P2PClient.FriendIPAndPort arg = (P2PClient.FriendIPAndPort)ContactList.SelectedItem;
				textBox.Text = arg.friendIP;
				textBox1.Text = arg.friendPort;
				textBox2.Text = arg.friendID;
				textBlock5_Copy.Text = "消息类型: " + arg.IsGroup;
			} //选择一条消息记录
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			if (ContactList.SelectedItems.Count == 0) {
				MessageBox.Show("未选择修改的联系人");
			}
			P2PClient.FriendIPAndPort arg = new P2PClient.FriendIPAndPort();
			arg = (P2PClient.FriendIPAndPort)ContactList.SelectedItem;
			arg.friendIP = textBox.Text;
			arg.friendPort = textBox1.Text;
			arg.friendID = textBox2.Text;
			allContacts.Remove((P2PClient.FriendIPAndPort)ContactList.SelectedItem);
			allContacts.Add(arg);
			//allContacts的修改
			if (SearchContacts!=null && SearchContacts.Count!=0) {
				SearchContacts.Remove((P2PClient.FriendIPAndPort)ContactList.SelectedItem);
				SearchContacts.Add(arg);
			} //Search的修改
		} //修改消息记录

		private void button1_Click(object sender, RoutedEventArgs e) {
			if (ContactList.SelectedItems.Count == 0) {
				MessageBox.Show("未选择删除的联系人");
			}
			P2PClient.FriendIPAndPort arg = (P2PClient.FriendIPAndPort)ContactList.SelectedItem;
			allContacts.Remove(arg);
			SearchContacts.Remove(arg);
		} //删除消息记录

		private void Button2_Click(object sender, RoutedEventArgs e) {
			SearchContacts = new P2PClient.FriendIPAndPorts();
			for (int i = 0; i < allContacts.Count; i++) {
				SearchContacts.Add(allContacts[i]);
			} //Copy

			if (textBox_Copy.Text != string.Empty) {
				for (int i = 0; i < SearchContacts.Count; i++) {
					if (SearchContacts[i].friendIP != textBox_Copy.Text) {
						SearchContacts.Remove(SearchContacts[i]);
						i--;
					}
				}
			} //friendIP

			if (textBox_Copy1.Text != string.Empty) {
				for (int i = 0; i < SearchContacts.Count; i++) {
					if (SearchContacts[i].friendPort != textBox_Copy1.Text) {
						SearchContacts.Remove(SearchContacts[i]);
						i--;
					}
				}
			} //friendPort

			if (textBox_Copy2.Text != string.Empty) {
				for (int i = 0; i < SearchContacts.Count; i++) {
					if (SearchContacts[i].friendID != textBox_Copy2.Text) {
						SearchContacts.Remove(SearchContacts[i]);
						i--;
					}
				}
			} //friendID
			ContactList.ItemsSource = SearchContacts;
		} //查询

		private void Button2_Copy_Click(object sender, RoutedEventArgs e) {
			textBox_Copy.Clear();
			textBox_Copy1.Clear();
			textBox_Copy2.Clear();
			checkBox.IsChecked = true;
			checkBox1.IsChecked = true;
			ContactList.ItemsSource = allContacts;
		} //清空
	}
}
