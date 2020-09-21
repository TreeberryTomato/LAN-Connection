using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LAN_Connection
{
	class LocalClient
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
		
		public async Task<string> ReceiveAsync()
		{
			//在本机指定的端口接收
			UdpClient client = new UdpClient(8001);
			IPEndPoint remove = null;
			//不知道为啥用224.0.0.1就会出现套接字无法连接主机的错误，也许是防火墙的问题？
			//加入组播，注意必须使用组播地址范围内的地址
			client.JoinMulticastGroup(IPAddress.Parse("224.0.0.2"));
			client.Ttl = 1;

			//接收从远程主机发送过来的信息
			try
			{
				var result = await client.ReceiveAsync();
				Byte[] bytes = result.Buffer;
				string data = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
				Console.WriteLine("客户端收到信息："+data);
				client.Close();
				return data;
			}
			catch(Exception e)
			{
				Console.WriteLine("接收端错误：" + e.Message);
				client.Close();
				return null;
			}
		}
	
		public async void ReplyAsync(string targetIP)
		{
			Byte[] message = Encoding.UTF8.GetBytes(LocalIP.ToString());

			Byte[]ip = Encoding.UTF8.GetBytes(targetIP);
			IPAddress HostIP = IPAddress.Parse(targetIP);
			IPEndPoint host = new IPEndPoint(HostIP, 8080);

			UdpClient client = new UdpClient();
			client.Ttl = 1;
			await client.SendAsync(message, message.Length, host);

		}
		public async void RunAsync()
		{
			string receive = await ReceiveAsync();
			Console.WriteLine("客户端接收到："+receive);
			ReplyAsync(receive);
		}
	}
}