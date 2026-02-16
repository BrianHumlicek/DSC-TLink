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
				log.LogDebug("TCP listener started, waiting for connections on port {Port}", port);
				do
				{
					log.LogDebug("Waiting for next TCP connection...");
					TcpClient tcpClient = await listener.AcceptTcpClientAsync(shutdownToken);
					try
					{
						if (tcpClient.Connected)
						{
							var remote = tcpClient.Client.RemoteEndPoint;
							log.LogInformation($"Got connection from {remote}");
							log.LogDebug("TCP socket state: Connected={Connected}, Available={Available}, LocalEndPoint={Local}, RemoteEndPoint={Remote}",
								tcpClient.Connected, tcpClient.Available, tcpClient.Client.LocalEndPoint, remote);
							log.LogDebug("TCP socket options: NoDelay={NoDelay}, ReceiveTimeout={RecvTimeout}ms, SendTimeout={SendTimeout}ms, ReceiveBufferSize={RecvBuf}, SendBufferSize={SendBuf}",
								tcpClient.NoDelay, tcpClient.ReceiveTimeout, tcpClient.SendTimeout, tcpClient.ReceiveBufferSize, tcpClient.SendBufferSize);
							ConnectionContext connectionContext = new TcpClientConnectionContext(tcpClient);
							ConnectionHandler connectionHandler = ServiceProvider.CreateITv2ConnectionHandler();
							log.LogDebug("Handing connection from {Remote} to ITv2ConnectionHandler", remote);
							await connectionHandler.OnConnectedAsync(connectionContext);
							log.LogInformation("Connection closed from {Remote}.", remote);
						}
						else if (shutdownToken.IsCancellationRequested)
						{
							log.LogInformation($"Stopped TCP session");
							return;
						}
						else
						{
							log.LogError("TCP connection error: AcceptTcpClient returned disconnected client");
						}
					}
					catch (Exception ex)
					{
						log.LogError(ex, "Exception during session with {Remote}", tcpClient.Client.RemoteEndPoint);
						break;
					}
					finally
					{
						log.LogDebug("Disposing TCP client for {Remote}", tcpClient.Client.RemoteEndPoint);
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
