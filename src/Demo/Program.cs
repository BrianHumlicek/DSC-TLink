// {LICENSE HEADER.txt}

//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.

//  You should have received a copy of the GNU General Public License
//	along with this program.  If not, see <https://www.gnu.org/licenses/>.

using DSC.TLink.Relay;
using Microsoft.Extensions.Logging;

namespace DSC.TLink.Demo
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: Demo <integrationId> <encryptionKey> [options]");
				Console.WriteLine("  integrationId    - Your integration account ID (e.g. 2032000000)");
				Console.WriteLine("  encryptionKey    - 32-char hex key matching [851][701] (e.g. 356523BF0473E03EB76E52157578C45C)");
				Console.WriteLine("  --port <port>    - TCP listen port for panel (default: 3072)");
				Console.WriteLine("  --relay-port <p> - TCP relay port for Home Assistant (default: 3078)");
				Console.WriteLine("  --relay-ip <ip>  - IP address to bind relay to (default: 0.0.0.0)");
				Console.WriteLine("  --debug          - Enable debug logging (shows sequence numbers, raw data)");
				Console.WriteLine("  --trace          - Enable trace logging (most verbose)");
				return;
			}

			string integrationId = args[0];
			string encryptionKey = args[1];
			int port = 3072;
			int relayPort = 3078;
			string relayIp = "0.0.0.0";
			LogLevel logLevel = LogLevel.Information;

			for (int i = 2; i < args.Length; i++)
			{
				switch (args[i].ToLower())
				{
					case "--port" when i + 1 < args.Length:
						if (int.TryParse(args[++i], out int p)) port = p;
						break;
					case "--relay-port" when i + 1 < args.Length:
						if (int.TryParse(args[++i], out int rp)) relayPort = rp;
						break;
					case "--relay-ip" when i + 1 < args.Length:
						relayIp = args[++i];
						break;
					case "--debug":
						logLevel = LogLevel.Debug;
						break;
					case "--trace":
						logLevel = LogLevel.Trace;
						break;
				}
			}

			MockServer server = new MockServer();
			server.ServiceProvider.IntegrationId = integrationId;
			server.ServiceProvider.EncryptionKey = encryptionKey;
			server.ServiceProvider.LogFactory = LoggerFactory.Create((configure) =>
			{
				configure.AddProvider(new TextFileLoggerProvider());
				configure.AddSimpleConsole(options =>
				{
					options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
				});
				configure.SetMinimumLevel(logLevel);
			});

			var relay = new JsonRelay(server.ServiceProvider.LogFactory, server.shutdownToken);
			server.ServiceProvider.Relay = relay;

			Console.CancelKeyPress += (sender, e) =>
			{
				e.Cancel = true;
				server.Stop();
			};
			Task relayTask = relay.StartListening(relayPort, relayIp);
			Task listenTask = server.TcpListenUntilStopped(port);
			await Task.WhenAny(listenTask, relayTask);
		}
	}
}