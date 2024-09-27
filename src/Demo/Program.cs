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

using Microsoft.Extensions.Logging;

namespace DSC.TLink.Demo
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			MockServer server = new MockServer();
			server.ServiceProvider.LogFactory = LoggerFactory.Create((configure) =>
			{
				configure.AddProvider(new TextFileLoggerProvider());
				configure.AddConsole();
				configure.SetMinimumLevel(LogLevel.Debug);
			});
			//3072 local
			//3097 on VPN
			Task listenTask = server.TcpListen(3072);
			Console.ReadKey();
			if (!listenTask.IsCompleted)
			{
				server.Stop();
			}
		}
	}
}