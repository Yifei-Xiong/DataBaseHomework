using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
	/// SearchUser.xaml 的交互逻辑
	/// </summary>
	public partial class SearchUser : Window {
		public SearchUser() {
			InitializeComponent();
		}
		public SearchUser(ServerWindow.AllUser allUser) {
			InitializeComponent();
			this.allUser = allUser;
			UserList.ItemsSource = allUser; //UserList的数据源
			currentUser = new ServerWindow.AllUser();
		}

		ServerWindow.AllUser allUser; //全部群聊与用户
		ServerWindow.AllUser currentUser; //显示在list里的聊天记录

		private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (UserList.SelectedItems.Count != 0) {
				ServerWindow.UserPw arg = (ServerWindow.UserPw)UserList.SelectedItem;
				textBox.Text = arg.UserID;
				textBox1.Text = arg.Password; 
				textBlock5_Copy.Text = "登录时间: " + arg.LogInTime;
				textBlock5_Copy1.Text = "登录IP: " + arg.LogInIP;
				textBlock5_Copy2.Text = "登录端口: " + arg.LogInPort;
			} //选择一条消息记录
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			if (UserList.SelectedItems.Count == 0) {
				MessageBox.Show("未选择修改的联系人");
			}
			ServerWindow.UserPw arg = new ServerWindow.UserPw();
			arg = (ServerWindow.UserPw)UserList.SelectedItem;
			arg.UserID = textBox.Text;
			arg.Password = textBox1.Text;
			allUser.Remove((ServerWindow.UserPw)UserList.SelectedItem);
			allUser.Add(arg);
			//allUser的修改
			if (currentUser != null && currentUser.Count != 0) {
				currentUser.Remove((ServerWindow.UserPw)UserList.SelectedItem);
				currentUser.Add(arg);
			} //current的修改
		} //修改消息记录

		private void button1_Click(object sender, RoutedEventArgs e) {
			if (UserList.SelectedItems.Count == 0) {
				MessageBox.Show("未选择删除的用户");
			}
			ServerWindow.UserPw arg = (ServerWindow.UserPw)UserList.SelectedItem;
			allUser.Remove(arg);
			currentUser.Remove(arg);
		} //删除消息记录

		private void Button2_Click(object sender, RoutedEventArgs e) {
			currentUser = new ServerWindow.AllUser();
			for (int i = 0; i < allUser.Count; i++) {
				currentUser.Add(allUser[i]);
			} //Copy
			if (checkBox.IsChecked == true) {
				if (textBox_Copy.Text != string.Empty) {
					for (int i = 0; i < currentUser.Count; i++) {
						if (currentUser[i].UserID != textBox_Copy.Text) {
							currentUser.Remove(currentUser[i]);
							i--;
						}
					}
				} 

				if (textBox_Copy1.Text != string.Empty) {
					for (int i = 0; i < currentUser.Count; i++) {
						if (currentUser[i].Password != textBox_Copy1.Text) {
							currentUser.Remove(currentUser[i]);
							i--;
						}
					}
				} 

				if (textBox_Copy2.Text != string.Empty) {
					for (int i = 0; i < currentUser.Count; i++) {
						if (currentUser[i].LogInTime != textBox_Copy2.Text) {
							currentUser.Remove(currentUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy3.Text != string.Empty) {
					for (int i = 0; i < currentUser.Count; i++) {
						if (currentUser[i].LogInIP != textBox_Copy3.Text) {
							currentUser.Remove(currentUser[i]);
							i--;
						}
					}
				} 

				if (textBox_Copy4.Text != string.Empty) {
					for (int i = 0; i < currentUser.Count; i++) {
						if (currentUser[i].LogInPort != textBox_Copy4.Text) {
							currentUser.Remove(currentUser[i]);
							i--;
						}
					}
				}
			} //模糊搜索

			if (checkBox.IsChecked == false) {
				if (textBox_Copy.Text != string.Empty) {
					for (int i = 0; i < currentUser.Count; i++) {
						if (currentUser[i].UserID.IndexOf(textBox_Copy.Text) == -1) {
							currentUser.Remove(currentUser[i]);
							i--;
						}
					}
				} 

				if (textBox_Copy1.Text != string.Empty) {
					for (int i = 0; i < currentUser.Count; i++) {
						if (currentUser[i].Password.IndexOf(textBox_Copy1.Text) == -1) {
							currentUser.Remove(currentUser[i]);
							i--;
						}
					}
				} 

				if (textBox_Copy2.Text != string.Empty) {
					for (int i = 0; i < currentUser.Count; i++) {
						if (currentUser[i].LogInTime.IndexOf(textBox_Copy2.Text) == -1) {
							currentUser.Remove(currentUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy3.Text != string.Empty) {
					for (int i = 0; i < currentUser.Count; i++) {
						if (currentUser[i].LogInIP.IndexOf(textBox_Copy3.Text) == -1) {
							currentUser.Remove(currentUser[i]);
							i--;
						}
					}
				}

				if (textBox_Copy4.Text != string.Empty) {
					for (int i = 0; i < currentUser.Count; i++) {
						if (currentUser[i].LogInPort.IndexOf(textBox_Copy4.Text) == -1) {
							currentUser.Remove(currentUser[i]);
							i--;
						}
					}
				}
			} //精确搜索
			UserList.ItemsSource = currentUser;
		} //查询

		private void Button2_Copy_Click(object sender, RoutedEventArgs e) {
			textBox_Copy.Clear();
			textBox_Copy1.Clear();
			textBox_Copy2.Clear();
			textBox_Copy3.Clear();
			textBox_Copy4.Clear();
			checkBox.IsChecked = false;
			UserList.ItemsSource = allUser;
		} //清空

		private void button1_Copy_Click(object sender, RoutedEventArgs e) {
			byte[] bytes = Encoding.UTF8.GetBytes(textBox2.Text);
			byte[] hash = SHA256Managed.Create().ComputeHash(bytes);
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < hash.Length; i++) {
				builder.Append(hash[i].ToString("X2"));
			}
			textBox2_Copy.Text =  builder.ToString();
		} //加密

	}
}
