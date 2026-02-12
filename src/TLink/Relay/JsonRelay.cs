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
	public class RelayCommand
	{
		public string Type { get; set; } = "";
		public int Partition { get; set; } = 1;
		public string? Code { get; set; }
	}

	public class JsonRelay : IDisposable
	{
		readonly ILogger log;
		readonly CancellationToken shutdownToken;
		readonly ConcurrentDictionary<string, (TcpClient client, StreamWriter writer)> clients = new();
		TcpListener? listener;
		int clientIdCounter;

		/// <summary>
		/// Commands received from relay clients (Home Assistant) waiting to be
		/// dispatched to the panel.
		/// </summary>
		public ConcurrentQueue<RelayCommand> PendingCommands { get; } = new();

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

			_ = Task.Run(() => HeartbeatLoop(), shutdownToken);

			try
			{
				while (!shutdownToken.IsCancellationRequested)
				{
					TcpClient tcpClient = await listener.AcceptTcpClientAsync(shutdownToken);
					string clientId = $"relay-{Interlocked.Increment(ref clientIdCounter)}";
					var stream = tcpClient.GetStream();
					var writer = new StreamWriter(stream) { AutoFlush = true };
					var reader = new StreamReader(stream);
					clients[clientId] = (tcpClient, writer);
					log.LogInformation("Relay client connected: {ClientId} from {RemoteEndPoint}", clientId, tcpClient.Client.RemoteEndPoint);

					// Start background reader for commands from this client
					_ = Task.Run(() => ClientReadLoop(clientId, reader), shutdownToken);
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

		async Task ClientReadLoop(string clientId, StreamReader reader)
		{
			try
			{
				while (!shutdownToken.IsCancellationRequested)
				{
					string? line = await reader.ReadLineAsync();
					if (line == null)
					{
						log.LogDebug("Relay client {ClientId} disconnected (EOF)", clientId);
						break;
					}

					line = line.Trim();
					if (string.IsNullOrEmpty(line)) continue;

					try
					{
						var command = JsonSerializer.Deserialize<RelayCommand>(line, new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						});

						if (command != null && !string.IsNullOrEmpty(command.Type))
						{
							log.LogInformation("Received command from {ClientId}: {Type} partition={Partition}",
								clientId, command.Type, command.Partition);
							PendingCommands.Enqueue(command);
						}
						else
						{
							log.LogDebug("Ignoring empty/invalid command from {ClientId}: {Line}", clientId, line);
						}
					}
					catch (JsonException ex)
					{
						log.LogDebug("Invalid JSON command from {ClientId}: {Line} ({Error})", clientId, line, ex.Message);
					}
				}
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				log.LogDebug("Relay client {ClientId} read error: {Error}", clientId, ex.Message);
			}
			finally
			{
				RemoveClient(clientId);
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

		async Task HeartbeatLoop()
		{
			while (!shutdownToken.IsCancellationRequested)
			{
				await Task.Delay(TimeSpan.FromSeconds(60), shutdownToken);
				Broadcast(new { type = "heartbeat" });
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
