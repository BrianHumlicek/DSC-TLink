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

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace DSC.TLink.Relay
{
	public class JsonRelay : IDisposable
	{
		readonly ILogger log;
		readonly CancellationToken shutdownToken;
		readonly ConcurrentDictionary<string, (TcpClient client, StreamWriter writer)> clients = new();
		TcpListener? listener;
		int clientIdCounter;

		public JsonRelay(ILoggerFactory loggerFactory, CancellationToken shutdownToken)
		{
			log = loggerFactory.CreateLogger<JsonRelay>();
			this.shutdownToken = shutdownToken;
		}

		public async Task StartListening(int port, string bindAddress = "0.0.0.0")
		{
			IPAddress address = IPAddress.Parse(bindAddress);
			listener = new TcpListener(address, port);
			listener.Start();
			log.LogInformation("JSON relay listening on {Address}:{Port}", bindAddress, port);

			try
			{
				while (!shutdownToken.IsCancellationRequested)
				{
					TcpClient tcpClient = await listener.AcceptTcpClientAsync(shutdownToken);
					string clientId = $"relay-{Interlocked.Increment(ref clientIdCounter)}";
					var writer = new StreamWriter(tcpClient.GetStream()) { AutoFlush = true };
					clients[clientId] = (tcpClient, writer);
					log.LogInformation("Relay client connected: {ClientId} from {RemoteEndPoint}", clientId, tcpClient.Client.RemoteEndPoint);
				}
			}
			catch (OperationCanceledException)
			{
				log.LogInformation("JSON relay stopping");
			}
			finally
			{
				listener.Stop();
				foreach (var (id, (client, writer)) in clients)
				{
					try { writer.Dispose(); } catch { }
					try { client.Dispose(); } catch { }
				}
				clients.Clear();
			}
		}

		public void Broadcast(object jsonEvent)
		{
			string json = JsonSerializer.Serialize(jsonEvent, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			foreach (var (id, (client, writer)) in clients)
			{
				try
				{
					if (!client.Connected)
					{
						RemoveClient(id);
						continue;
					}
					writer.WriteLine(json);
				}
				catch (Exception)
				{
					log.LogDebug("Relay client disconnected: {ClientId}", id);
					RemoveClient(id);
				}
			}
		}

		void RemoveClient(string clientId)
		{
			if (clients.TryRemove(clientId, out var entry))
			{
				try { entry.writer.Dispose(); } catch { }
				try { entry.client.Dispose(); } catch { }
			}
		}

		public void Dispose()
		{
			listener?.Stop();
			foreach (var (id, (client, writer)) in clients)
			{
				try { writer.Dispose(); } catch { }
				try { client.Dispose(); } catch { }
			}
			clients.Clear();
		}
	}
}
