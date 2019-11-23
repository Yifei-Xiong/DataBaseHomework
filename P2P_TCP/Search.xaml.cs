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

		private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}
	}

	//UserData的定义：
	class UserData : INotifyPropertyChanged //通知接口
		{
		public event PropertyChangedEventHandler PropertyChanged;
		private string _uin;
		private string _pass;
		private int _id;
		public string uin
		{
			get { return _uin; }
			set
			{
				_uin = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("uin"));
			}
		}
		public string pass
		{
			get { return _pass; }
			set
			{
				_pass = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("pass"));
			}
		}
		public int id
		{
			get { return _id; }
			set
			{
				_id = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("id"));
			}
		}
		public UserData(int id, string uin, string pass) {
			_id = id;
			_uin = uin;
			_pass = pass;
		}
	}

}
