using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LAN_Connection
{
	class LocalServer
	{
		public IPAddress LocalIP
		{
			get
			{
				try
				{
					NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
					foreach (var intf in interfaces)
					{
						if (intf.OperationalStatus == OperationalStatus.Up)
						{
							var ipAddress = intf.GetIPProperties().UnicastAddresses[0].Address;
							if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
								return ipAddress;

						}
					}
				}
				catch (Exception)
				{
					return null;

				}
				return null;
			}
		}

		public async Task<string> ListenAsync()
		{
			UdpClient client = new UdpClient(8080);
			var result = await client.ReceiveAsync();
			string message = Encoding.UTF8.GetString(result.Buffer);
			Console.WriteLine("服务器接收到信息：" + message);
			return message;
		}
		public async void MulticastAsync()
		{
			IPAddress localIP = LocalIP;
			if (localIP == null)
			{
				Console.WriteLine("Not connect to the local network");
				return;
			}
			string message = $"{localIP}";

			//系统将自动分配最合适的本地地址和端口号
			UdpClient client = new UdpClient();
			//允许发送广播    
			client.EnableBroadcast = true;
			client.Ttl = 1;
			//必须使用组播地址
			IPEndPoint p = new IPEndPoint(IPAddress.Parse("224.0.0.2"), 8001);
			//将发送内容装换为字节数组
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			try
			{
				//向子网发送信息
				await client.SendAsync(bytes, bytes.Length, p);
				Console.WriteLine("服务端发送成功");
			}
			catch (Exception e)
			{
				Console.WriteLine("服务端出错：" + e.Message);
			}
			finally
			{
				client.Close();
			}
		}

		public void Run()
		{
			Task<string> listen = ListenAsync();
			MulticastAsync();
			Console.WriteLine(listen.Result);
		}
	}
}
