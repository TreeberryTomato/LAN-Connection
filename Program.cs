using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System;
using System.Net.Sockets;
using System.Net.NetworkInformation;
namespace LAN_Connection
{
	class Program
	{ 
		static void Main(string[] args)
		{
			
			LocalServer server = new LocalServer();
			LocalClient client = new LocalClient();
			client.RunAsync();
			server.Run();
			
		}
	}
}
