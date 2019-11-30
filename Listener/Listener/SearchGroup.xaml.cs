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

		private void Button2_Copy_Click(object sender, RoutedEventArgs e) {

		} //清空

		private void Button2_Click(object sender, RoutedEventArgs e) {

		} //筛选

		private void button_Click(object sender, RoutedEventArgs e) {

		} //修改

		private void button1_Click(object sender, RoutedEventArgs e) {

		} //删除

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
