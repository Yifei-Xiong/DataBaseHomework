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
	abstract class DataPackage {
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
		private readonly DateTime sendTime; //消息的发送时间
		public string Sender { get; set; }
		public string Receiver { get; set; }
	}

	//登入数据包类
	class LoginDataPackage : DataPackage {
		public LoginDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				LoginDataPackage loginDataPackage = formatter.Deserialize(ms) as LoginDataPackage;
				if (loginDataPackage != null) {
					this.Password = loginDataPackage.Password;
					this.UserID = loginDataPackage.UserID;
				}
			}
		} //构造函数 字节数组转化为数据包
		public string UserID { get; set; } //登录用户名
		public string Password { protected get; set; } //登录密码
	}

	//登出数据包类
	class LogoutDataPackage : DataPackage {
		public LogoutDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				LogoutDataPackage logoutDataPackage = formatter.Deserialize(ms) as LogoutDataPackage;
				if (logoutDataPackage != null) {
					this.UserID = logoutDataPackage.UserID;
				}
			}
		} //构造函数 字节数组转化为数据包
		public string UserID { get; set; } //登录用户名
	}

	//聊天数据包类
	class ChatDataPackage : DataPackage {
		public ChatDataPackage (byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				ChatDataPackage chatDataPackage = formatter.Deserialize(ms) as ChatDataPackage;
				if (chatDataPackage != null) {
					this.Message = chatDataPackage.Message;
				}
			}
		} //构造函数 字节数组转化为数据包
		public string Message { get; set; } //发送的消息
	}

	//单人聊天数据包类
	class SingleChatDataPackage : ChatDataPackage {
		public SingleChatDataPackage(byte[] Bytes) : base(Bytes) {

		} //构造函数 字节数组转化为数据包
	}

	//多人聊天数据包类
	class MultiChatDataPackage : ChatDataPackage {
		public MultiChatDataPackage (byte[] Bytes) : base(Bytes) {
			
		} //构造函数 字节数组转化为数据包
	}
}
