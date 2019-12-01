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

namespace Listener {
	/// <summary>
	/// SearchUserInGroup.xaml 的交互逻辑
	/// </summary>
	public partial class SearchUserInGroup : Window {
		public SearchUserInGroup() {
			InitializeComponent();
		}
		public SearchUserInGroup(ServerWindow.AllGroup allGroup) {
			InitializeComponent();
			this.allGroup = allGroup;
			for (int i = 0; i < allGroup.Count; i++) {
				comboBox.Items.Add(allGroup[i].GroupPort);
			}
		}

		ServerWindow.AllGroup allGroup; //全部群聊与用户
		ServerWindow.GroupUser currentGroupUser; //显示在list里的聊天记录
		ServerWindow.GroupUser searchGroupUser; //显示在list里的聊天记录

		private void Button2_Copy_Click(object sender, RoutedEventArgs e) {
			textBox_Copy2.Clear();
			textBox_Copy3.Clear();
			textBox_Copy5.Clear();
			textBox_Copy.Clear();
			textBox_Copy1.Clear();
			checkBox.IsChecked = false;
			UserList.ItemsSource = currentGroupUser;
		} //清空

		private void Button2_Click(object sender, RoutedEventArgs e) {
			searchGroupUser = new ServerWindow.GroupUser();
			for (int i = 0; i < currentGroupUser.Count; i++) {
				searchGroupUser.Add(currentGroupUser[i]);
			} //Copy
			if (checkBox.IsChecked == true) {
				if (textBox_Copy2.Text != string.Empty) {
					for (int i = 0; i < searchGroupUser.Count; i++) {
						if (searchGroupUser[i].UserIP != textBox_Copy2.Text) {
							searchGroupUser.Remove(searchGroupUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy3.Text != string.Empty) {
					for (int i = 0; i < searchGroupUser.Count; i++) {
						if (searchGroupUser[i].UserPort != textBox_Copy3.Text) {
							searchGroupUser.Remove(searchGroupUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy5.Text != string.Empty) {
					for (int i = 0; i < searchGroupUser.Count; i++) {
						if (searchGroupUser[i].UserID != textBox_Copy5.Text) {
							searchGroupUser.Remove(searchGroupUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy.Text != string.Empty) {
					for (int i = 0; i < searchGroupUser.Count; i++) {
						if (searchGroupUser[i].LastMsgTime != textBox_Copy.Text) {
							searchGroupUser.Remove(searchGroupUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy1.Text != string.Empty) {
					for (int i = 0; i < searchGroupUser.Count; i++) {
						if (searchGroupUser[i].MsgTimes.ToString() != textBox_Copy1.Text) {
							searchGroupUser.Remove(searchGroupUser[i]);
							i--;
						}
					}
				}

			} //模糊搜索

			if (checkBox.IsChecked == false) {
				if (textBox_Copy2.Text != string.Empty) {
					for (int i = 0; i < searchGroupUser.Count; i++) {
						if (searchGroupUser[i].UserIP.IndexOf(textBox_Copy2.Text) == -1) {
							searchGroupUser.Remove(searchGroupUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy3.Text != string.Empty) {
					for (int i = 0; i < searchGroupUser.Count; i++) {
						if (searchGroupUser[i].UserPort.IndexOf(textBox_Copy3.Text) == -1) {
							searchGroupUser.Remove(searchGroupUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy5.Text != string.Empty) {
					for (int i = 0; i < searchGroupUser.Count; i++) {
						if (searchGroupUser[i].UserID.IndexOf(textBox_Copy5.Text) == -1) {
							searchGroupUser.Remove(searchGroupUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy.Text != string.Empty) {
					for (int i = 0; i < searchGroupUser.Count; i++) {
						if (searchGroupUser[i].LastMsgTime.IndexOf(textBox_Copy.Text) == -1) {
							searchGroupUser.Remove(searchGroupUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy1.Text != string.Empty) {
					for (int i = 0; i < searchGroupUser.Count; i++) {
						if (searchGroupUser[i].MsgTimes.ToString().IndexOf(textBox_Copy1.Text) == -1) {
							searchGroupUser.Remove(searchGroupUser[i]);
							i--;
						}
					}
				}
			} //精确搜索
			UserList.ItemsSource = searchGroupUser;
		} //查询

		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			int i;
			for (i = 0; i < allGroup.Count; i++) {
				if (allGroup[i].GroupPort == (int)comboBox.SelectedItem) {
					UserList.ItemsSource = allGroup[i].groupUser;
					currentGroupUser = allGroup[i].groupUser;
					break;
				}
			}
			if (i == allGroup.Count) {
				MessageBox.Show("Port Error");
			}

		} //选择一个端口

		private void button_Click(object sender, RoutedEventArgs e) {
			if (UserList.SelectedItems.Count == 0) {
				MessageBox.Show("未选择修改的用户");
			}
			ServerWindow.UserInfo arg = new ServerWindow.UserInfo();
			arg = (ServerWindow.UserInfo)UserList.SelectedItem;
			arg.UserIP = textBox.Text;
			arg.UserPort = textBox_Copy4.Text;
			arg.UserID = textBox_Copy6.Text;
			currentGroupUser.Remove((ServerWindow.UserInfo)UserList.SelectedItem);
			currentGroupUser.Add(arg);
			if (searchGroupUser != null && searchGroupUser.Count != 0) {
				searchGroupUser.Remove((ServerWindow.UserInfo)UserList.SelectedItem);
				searchGroupUser.Add(arg);
			}
		} //修改消息记录

		private void button1_Click(object sender, RoutedEventArgs e) {
			if (UserList.SelectedItems.Count == 0) {
				MessageBox.Show("未选择删除的用户");
			}
			ServerWindow.UserInfo arg = (ServerWindow.UserInfo)UserList.SelectedItem;
			currentGroupUser.Remove(arg);
			searchGroupUser.Remove(arg);
		} //删除消息记录

		private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (UserList.SelectedItems.Count != 0) {
				ServerWindow.UserInfo arg = (ServerWindow.UserInfo)UserList.SelectedItem;
				textBox.Text = arg.UserIP;
				textBox_Copy4.Text = arg.UserPort;
				textBox_Copy6.Text = arg.UserID;
				textBlock3_Copy3.Text = "发言次数: " + arg.MsgTimes.ToString();
				textBlock5_Copy3.Text = "最后活跃时间: " + arg.LastMsgTime;
			} //选择一条消息记录
		}
	}
}