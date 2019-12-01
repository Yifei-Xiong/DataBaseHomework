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
	/// SearchGroup.xaml 的交互逻辑
	/// </summary>
	public partial class SearchGroup : Window {
		public SearchGroup() {
			InitializeComponent();
		}
		public SearchGroup(ServerWindow.AllGroup allGroup) {
			InitializeComponent();
			this.allGroup = allGroup;
			for (int i=0; i<allGroup.Count; i++) {
				comboBox.Items.Add(allGroup[i].GroupPort);
			}
		}

		ServerWindow.AllGroup allGroup; //全部群聊与用户
		ServerWindow.GroupMsg currentGroupMsg; //显示在list里的聊天记录
		ServerWindow.GroupMsg searchGroupMsg; //显示在list里的聊天记录

		private void Button2_Copy_Click(object sender, RoutedEventArgs e) {
			textBox_Copy.Clear();
			textBox_Copy1.Clear();
			textBox_Copy2.Clear();
			textBox_Copy3.Clear();
			textBox_Copy5.Clear();
			textBox_Copy6.Clear();
			checkBox.IsChecked = false;
			MsgList.ItemsSource = currentGroupMsg;
		} //清空

		private void Button2_Click(object sender, RoutedEventArgs e) {
			searchGroupMsg = new ServerWindow.GroupMsg();
			for (int i = 0; i < currentGroupMsg.Count; i++) {
				searchGroupMsg.Add(currentGroupMsg[i]);
			} //Copy
			if (checkBox.IsChecked == true) {
				if (textBox_Copy.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].MsgID != textBox_Copy.Text) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				} 

				if (textBox_Copy1.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].MsgTime != textBox_Copy1.Text) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				} 

				if (textBox_Copy2.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].MsgIP != textBox_Copy2.Text) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				}

				if (textBox_Copy3.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].MsgPort != textBox_Copy3.Text) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				} 

				if (textBox_Copy5.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].UserID != textBox_Copy5.Text) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				}

				if (textBox_Copy6.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].Msg != textBox_Copy6.Text) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				}
			} //模糊搜索

			if (checkBox.IsChecked == false) {
				if (textBox_Copy.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].MsgID.IndexOf(textBox_Copy.Text) == -1) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				} 

				if (textBox_Copy1.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].MsgTime.IndexOf(textBox_Copy1.Text) == -1) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				} 

				if (textBox_Copy2.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].MsgIP.IndexOf(textBox_Copy2.Text) == -1) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				}

				if (textBox_Copy3.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].MsgPort.IndexOf(textBox_Copy3.Text) == -1) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				}

				if (textBox_Copy5.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].UserID.IndexOf(textBox_Copy5.Text) == -1) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				}

				if (textBox_Copy6.Text != string.Empty) {
					for (int i = 0; i < searchGroupMsg.Count; i++) {
						if (searchGroupMsg[i].Msg.IndexOf(textBox_Copy6.Text) == -1) {
							searchGroupMsg.Remove(searchGroupMsg[i]);
							i--;
						}
					}
				}
			} //精确搜索
			MsgList.ItemsSource = searchGroupMsg;
		} //查询

		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			int i;
			for(i=0; i<allGroup.Count; i++) {
				if (allGroup[i].GroupPort==(int)comboBox.SelectedItem) {
					MsgList.ItemsSource = allGroup[i].groupMsg;
					currentGroupMsg = allGroup[i].groupMsg;
					break;
				}
			}
			if (i== allGroup.Count) {
				MessageBox.Show("Port Error");
			}
			
		} //选择一个端口
	}
}
