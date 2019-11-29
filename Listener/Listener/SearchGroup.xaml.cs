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

		ServerWindow.AllGroup allGroup;

		private void Button2_Copy_Click(object sender, RoutedEventArgs e) {

		} //清空

		private void Button2_Click(object sender, RoutedEventArgs e) {

		} //筛选

		private void button_Click(object sender, RoutedEventArgs e) {

		} //修改

		private void button1_Click(object sender, RoutedEventArgs e) {

		} //删除
	}
}
