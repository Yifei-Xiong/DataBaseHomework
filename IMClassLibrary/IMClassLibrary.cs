using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace IMClassLibrary
{
	//数据包类
	[Serializable]
	public abstract class DataPackage {
		public DataPackage() {
			sendTime = DateTime.Now;
		}
		public byte[] DataPackageToBytes() {
			using (MemoryStream ms = new MemoryStream()) {
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(ms, this);
				return ms.GetBuffer();
			}
		} //数据包转化为字节数组
		public DataPackage(string sender, string receiver) {
			this.Sender = sender;
			this.Receiver = receiver;
			sendTime = DateTime.Now;
		} //构造函数 接受发送者与接收者字符串
		public DateTime sendTime { get; set; } //消息的发送时间
		public string Sender { get; set; }
		public string Receiver { get; set; }
		public int MessageType = 0; //数据包类Type为0
	}

	//登入数据包类
	[Serializable]
	public class LoginDataPackage : DataPackage {
		public LoginDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				LoginDataPackage loginDataPackage = formatter.Deserialize(ms) as LoginDataPackage;
				if (loginDataPackage != null) {
					this.Password = loginDataPackage.Password;
					this.UserID = loginDataPackage.UserID;
					this.Sender = loginDataPackage.Sender;
					this.Receiver = loginDataPackage.Receiver;
					this.sendTime = loginDataPackage.sendTime;
					this.MessageType = loginDataPackage.MessageType;
				}
			}
		} //构造函数 字节数组转化为数据包
		public LoginDataPackage(string sender, string receiver, string userID, string password) : base(sender,receiver) {
			MessageType = 1;
			this.UserID = userID;
			this.Password = password;
		} //构造函数 接受发送者,接收者字符串,登录用户名与密码
		public string UserID { get; set; } //登录用户名
		public string Password { get; set; } //登录密码
	}

	//登出数据包类
	[Serializable]
	public class LogoutDataPackage : DataPackage {
		public LogoutDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				LogoutDataPackage logoutDataPackage = formatter.Deserialize(ms) as LogoutDataPackage;
				if (logoutDataPackage != null) {
					this.UserID = logoutDataPackage.UserID;
					this.UserID = logoutDataPackage.UserID;
					this.Sender = logoutDataPackage.Sender;
					this.Receiver = logoutDataPackage.Receiver;
					this.sendTime = logoutDataPackage.sendTime;
					this.MessageType = logoutDataPackage.MessageType;
				}
			}
		} //构造函数 字节数组转化为数据包
		public LogoutDataPackage(string sender, string receiver, string userID) : base(sender, receiver) {
			MessageType = 2;
			this.UserID = userID;
		} //构造函数 接受发送者,接收者字符串,登出用户名
		public string UserID { get; set; } //登出用户名
	}

	//聊天数据包类
	[Serializable]
	public class ChatDataPackage : DataPackage {
		public ChatDataPackage () { }
		public ChatDataPackage (byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				ms.Position = 0;
				ChatDataPackage chatDataPackage = formatter.Deserialize(ms) as ChatDataPackage;
				if (chatDataPackage != null) {
					this.Message = chatDataPackage.Message;
					this.Sender = chatDataPackage.Sender;
					this.Receiver = chatDataPackage.Receiver;
					this.sendTime = chatDataPackage.sendTime;
					this.MessageType = chatDataPackage.MessageType;
					this.SenderID = chatDataPackage.SenderID;
				}
			}
		} //构造函数 字节数组转化为数据包
		public ChatDataPackage(string sender, string receiver, string message) : base(sender, receiver) {
			MessageType = 3;
			this.Message = message;
		} //构造函数 接受发送者,接收者字符串,发送的消息
		public string Message { get; set; } //发送的消息
		public string SenderID { get; set; } //发送者的ID
	}

	//单人聊天数据包类
	[Serializable]
	public class SingleChatDataPackage : ChatDataPackage {
		public SingleChatDataPackage(byte[] Bytes) : base(Bytes) {
		} //构造函数 字节数组转化为数据包
		public SingleChatDataPackage(string sender, string receiver, string message) : base(sender,receiver,message) {
			MessageType = 4;
		} //构造函数 接受发送者,接收者字符串,发送的消息
		public static string operator +(string str, SingleChatDataPackage data) {
			return str+data.Message;
		}
	}

	//多人聊天数据包类
	[Serializable]
	public class MultiChatDataPackage : ChatDataPackage {
		public MultiChatDataPackage (byte[] Bytes) : base(Bytes) {
		} //构造函数 字节数组转化为数据包
		public MultiChatDataPackage(string sender, string receiver, string message) : base(sender, receiver, message) {
			MessageType = 5;
		} //构造函数 接受发送者,接收者字符串,发送的消息
		public static string operator +(string str, MultiChatDataPackage data) {
			return str + data.Message;
		}
	}

	//更改名称数据包类
	[Serializable]
	public class ChangeNameDataPackage : DataPackage {
		public ChangeNameDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				if (formatter.Deserialize(ms) is ChangeNameDataPackage changeNameDataPackage) {
					this.Name = changeNameDataPackage.Name;
				}
			}
		} //构造函数 字节数组转化为数据包
		public ChangeNameDataPackage(string sender, string receiver, string name) : base(sender, receiver) {
			MessageType = 6;
			this.Name = name;
		} //构造函数 接受发送者,接收者字符串,用户名称
		public string Name { get; set; } //用户名称
	}

	//文件数据包类
	[Serializable]
	public class FileDataPackage : ChatDataPackage {
		public FileDataPackage(string sender, string receiver, string message, byte[] file,string ext) : base(sender, receiver, message) {
			this.file = file;
			FileExtension = ext;
			MessageType = 7;
		} //构造函数
		public FileDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				ms.Position = 0;
				FileDataPackage fileDataPackage = formatter.Deserialize(ms) as FileDataPackage;
				if (fileDataPackage != null) {
					this.Message = fileDataPackage.Message;
					this.Sender = fileDataPackage.Sender;
					this.Receiver = fileDataPackage.Receiver;
					this.sendTime = fileDataPackage.sendTime;
					this.MessageType = fileDataPackage.MessageType;
					this.SenderID = fileDataPackage.SenderID;
					this.file = fileDataPackage.file;
					this.FileExtension = fileDataPackage.FileExtension;
				}
			}
		} //构造函数 字节数组转化为数据包
		public byte[] file; //文件流
		public string FileExtension { get; set; } //文件后缀
	}

}
