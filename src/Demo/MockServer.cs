// DSC TLink - a communications library for DSC Powerseries NEO alarm panels
// Copyright (C) 2024 Brian Humlicek
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace DSC.TLink.Demo
{
	public class MockServer
	{
		ILogger _log;
		ILogger log => _log ??= ServiceProvider.LogFactory.CreateLogger<MockServer>();
		public MockServer()
		{
			ServiceProvider.shutdownTokenGetter = () => shutdownToken;
		}
		internal  MockServiceProvider ServiceProvider { get; } = new MockServiceProvider();
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		public CancellationToken shutdownToken => cancellationTokenSource.Token;
		public async Task TcpListenUntilStopped(int port)
		{
			await TcpListen(port);
		}
		public async Task TcpListen(int port)
		{
			using (TcpListener listener = TcpListener.Create(port))
			{
				log.LogInformation($"Starting TCP session on port {port}");
				listener.Start();
				do
				{
					TcpClient tcpClient = await listener.AcceptTcpClientAsync(shutdownToken);
					try
					{
						if (tcpClient.Connected)
						{
							log.LogInformation($"Got connection from {tcpClient.Client.RemoteEndPoint}");
							ConnectionContext connectionContext = new TcpClientConnectionContext(tcpClient);
							ConnectionHandler connectionHandler = ServiceProvider.CreateITv2ConnectionHandler();
							await connectionHandler.OnConnectedAsync(connectionContext);
							log.LogInformation("Connection closed.");
						}
						else if (shutdownToken.IsCancellationRequested)
						{
							log.LogInformation($"Stopped TCP session");
							return;
						}
						else
						{
							log.LogError($"TCP connection error");
						}
					}
					catch (Exception ex)
					{
						log.LogError(ex, "Exception during session");
						break;
					}
					finally
					{
						tcpClient.Dispose();
					}
				} while (shutdownToken.CanBeCanceled);
				log.LogInformation("Ending TCP session");
			}
		}
		public void Stop()
		{
			log.LogInformation($"Stopping...");
			cancellationTokenSource?.Cancel();
		}
	}
}
