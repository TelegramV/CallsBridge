using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace CallsBridge
{
	public class Bridge : WebSocketBehavior
	{
		public IPEndPoint Endpoint;
		public UdpClient UdpClient;

		protected override void OnMessage(MessageEventArgs e)
		{
			var data = e.RawData;

			if (Endpoint == null)
			{
				var ip = new IPAddress(data.Take(4).ToArray());
				var port = BitConverter.ToUInt16(data.Skip(4).ToArray());
				Endpoint = new IPEndPoint(ip, port);
				Console.WriteLine("New bridge to " + Endpoint);
				UdpClient = new UdpClient();
				Task.Run(async () =>
				{
					while (UdpClient != null) {
						try
						{
							Console.WriteLine("listening..." + Endpoint);
							var result = await UdpClient.ReceiveAsync();
							Console.WriteLine("Received " + result.Buffer.Length + " bytes from " + result.RemoteEndPoint);
							Console.ForegroundColor = ConsoleColor.Yellow;
							Console.WriteLine(BitConverter.ToString(result.Buffer));
							Console.ForegroundColor = ConsoleColor.White;

							Send(result.Buffer);
						} catch(Exception e)
						{
						}
					}
					Console.WriteLine("no more!", Endpoint);
				});
				return;
			}
			Console.WriteLine("Forwarding " + data.Length + " bytes to " + Endpoint);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(e.Data);
			Console.ForegroundColor = ConsoleColor.White;
			UdpClient.Send(data, data.Length, Endpoint);
		}

		protected override void OnClose(CloseEventArgs e)
		{
			Console.WriteLine("Closing bridge to " + Endpoint);
			UdpClient.Close();
			UdpClient.Dispose();
			UdpClient = null;
		}
	}

	public class Program
	{
		static void Main(string[] args)
		{
			var wssv = new WebSocketServer("ws://127.0.0.1:1488");
			wssv.AddWebSocketService<Bridge>("/");
			wssv.Start();
			Console.ReadKey(true);
			wssv.Stop();
		}
	}
}
