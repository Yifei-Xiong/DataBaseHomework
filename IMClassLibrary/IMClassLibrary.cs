﻿using System;
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
		public readonly DateTime sendTime; //消息的发送时间
		public string Sender { get; set; }
		public string Receiver { get; set; }
		public int MessageType = 0; //数据包类Type为0
	}

	//登入数据包类
	public class LoginDataPackage : DataPackage {
		public LoginDataPackage(byte[] Bytes) {
			MessageType = 1;
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				LoginDataPackage loginDataPackage = formatter.Deserialize(ms) as LoginDataPackage;
				if (loginDataPackage != null) {
					this.Password = loginDataPackage.Password;
					this.UserID = loginDataPackage.UserID;
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
	public class LogoutDataPackage : DataPackage {
		public LogoutDataPackage(byte[] Bytes) {
			MessageType = 2;
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				LogoutDataPackage logoutDataPackage = formatter.Deserialize(ms) as LogoutDataPackage;
				if (logoutDataPackage != null) {
					this.UserID = logoutDataPackage.UserID;
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
	public class ChatDataPackage : DataPackage {
		public ChatDataPackage (byte[] Bytes) {
			MessageType = 3;
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				ChatDataPackage chatDataPackage = formatter.Deserialize(ms) as ChatDataPackage;
				if (chatDataPackage != null) {
					this.Message = chatDataPackage.Message;
				}
			}
		} //构造函数 字节数组转化为数据包
		public ChatDataPackage(string sender, string receiver, string message) : base(sender, receiver) {
			MessageType = 3;
			this.Message = message;
		} //构造函数 接受发送者,接收者字符串,发送的消息
		public string Message { get; set; } //发送的消息
	}

	//单人聊天数据包类
	public class SingleChatDataPackage : ChatDataPackage {
		public SingleChatDataPackage(byte[] Bytes) : base(Bytes) {
			MessageType = 4;
		} //构造函数 字节数组转化为数据包
		public SingleChatDataPackage(string sender, string receiver, string message) : base(sender,receiver,message) {
			MessageType = 4;
		} //构造函数 接受发送者,接收者字符串,发送的消息
		public static string operator +(string str, SingleChatDataPackage data) {
			return str+data.Message;
		}
	}

	//多人聊天数据包类
	public class MultiChatDataPackage : ChatDataPackage {
		public MultiChatDataPackage (byte[] Bytes) : base(Bytes) {
			MessageType = 5;
		} //构造函数 字节数组转化为数据包
		public MultiChatDataPackage(string sender, string receiver, string message) : base(sender, receiver, message) {
			MessageType = 5;
		} //构造函数 接受发送者,接收者字符串,发送的消息
		public static string operator +(string str, MultiChatDataPackage data) {
			return str + data.Message;
		}
	}

	//更改名称数据包类
	public class ChangeNameDataPackage : DataPackage {
		public ChangeNameDataPackage(byte[] Bytes) {
			MessageType = 6;
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

}