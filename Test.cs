using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Opal
{
	enum TestProtocol { Ping, TCP, HTTP }

	class Test
	{
		public string Name { get; }
		public string Address { get; }
		public TestProtocol Protocol { get; }
		public int Port { get; }

		public Test(string name, string address, string protocol)
		{
			Name = name;
			Address = address;

			Protocol =
				protocol == "ping" ? TestProtocol.Ping :
				protocol == "tcp" ? TestProtocol.TCP :
				protocol == "http" ? TestProtocol.HTTP :
				throw new ArgumentException();

			Port = -1;
			if (Protocol == TestProtocol.TCP)
			{
				var separator = Address.LastIndexOf(':');
				Port = int.Parse(Address.Substring(separator + 1));
				Address = Address.Substring(0, separator);
			}	
		}

		public static Test FromJson(JObject json) =>
			new Test((string)json["name"], (string)json["address"], (string)json["protocol"]);

		public bool Run()
		{
			var result = false;

			if (Protocol == TestProtocol.Ping)
			{
				using var ping = new Ping();
				try { result = ping.Send(Address, 2000).Status == IPStatus.Success; }
				catch { }
			}

			if (Protocol == TestProtocol.TCP)
			{
				using var tcp = new TcpClient();
				try { result = tcp.ConnectAsync(Address, Port).Wait(2000); }
				catch { }
			}

			if (Protocol == TestProtocol.HTTP)
			{
				using var http = new HttpClient();
				http.Timeout = TimeSpan.FromMilliseconds(2000);
				HttpResponseMessage response = null;
				try { response = http.GetAsync(Address).Result; }
				catch { }
				if (response != null && 
					(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized))
					result = true;
			}

			return result;
		}
	}
}
