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
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
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
		static readonly byte[] Pbkdf2Salt = Encoding.UTF8.GetBytes("DSC-TLink-Relay-v1");
		const int Pbkdf2Iterations = 100_000;
		const int KeySize = 32;
		const int NonceSize = 12;
		const int TagSize = 16;

		readonly ILogger log;
		readonly CancellationToken shutdownToken;
		readonly ConcurrentDictionary<string, (TcpClient client, NetworkStream stream)> clients = new();
		readonly byte[] encryptionKey;
		TcpListener? listener;
		int clientIdCounter;

		/// <summary>
		/// Commands received from relay clients (Home Assistant) waiting to be
		/// dispatched to the panel.
		/// </summary>
		public ConcurrentQueue<RelayCommand> PendingCommands { get; } = new();

		/// <summary>
		/// Signaled when a new command is enqueued in PendingCommands.
		/// The panel communication loop waits on this to interrupt its blocking read.
		/// </summary>
		public SemaphoreSlim CommandAvailable { get; } = new(0);

		public JsonRelay(ILoggerFactory loggerFactory, CancellationToken shutdownToken, string relaySecret)
		{
			log = loggerFactory.CreateLogger<JsonRelay>();
			this.shutdownToken = shutdownToken;
			encryptionKey = DeriveKey(relaySecret);
		}

		static byte[] DeriveKey(string secret)
		{
			using var pbkdf2 = new Rfc2898DeriveBytes(
				Encoding.UTF8.GetBytes(secret), Pbkdf2Salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
			return pbkdf2.GetBytes(KeySize);
		}

		static byte[] Encrypt(byte[] key, byte[] plaintext)
		{
			byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
			byte[] ciphertext = new byte[plaintext.Length];
			byte[] tag = new byte[TagSize];
			using var aes = new AesGcm(key, TagSize);
			aes.Encrypt(nonce, plaintext, ciphertext, tag);
			// Return nonce + ciphertext + tag
			byte[] result = new byte[NonceSize + ciphertext.Length + TagSize];
			Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
			Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);
			Buffer.BlockCopy(tag, 0, result, NonceSize + ciphertext.Length, TagSize);
			return result;
		}

		static byte[] Decrypt(byte[] key, byte[] data)
		{
			byte[] nonce = new byte[NonceSize];
			Buffer.BlockCopy(data, 0, nonce, 0, NonceSize);
			int ciphertextLen = data.Length - NonceSize - TagSize;
			byte[] ciphertext = new byte[ciphertextLen];
			Buffer.BlockCopy(data, NonceSize, ciphertext, 0, ciphertextLen);
			byte[] tag = new byte[TagSize];
			Buffer.BlockCopy(data, NonceSize + ciphertextLen, tag, 0, TagSize);
			byte[] plaintext = new byte[ciphertextLen];
			using var aes = new AesGcm(key, TagSize);
			aes.Decrypt(nonce, ciphertext, tag, plaintext);
			return plaintext;
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
					clients[clientId] = (tcpClient, stream);
					log.LogInformation("Relay client connected: {ClientId} from {RemoteEndPoint}", clientId, tcpClient.Client.RemoteEndPoint);

					// Start background reader for commands from this client
					_ = Task.Run(() => ClientReadLoop(clientId, stream), shutdownToken);
				}
			}
			catch (OperationCanceledException)
			{
				log.LogInformation("JSON relay stopping");
			}
			finally
			{
				listener.Stop();
				foreach (var (id, (client, stream)) in clients)
				{
					try { stream.Dispose(); } catch { }
					try { client.Dispose(); } catch { }
				}
				clients.Clear();
			}
		}

		async Task ClientReadLoop(string clientId, NetworkStream stream)
		{
			try
			{
				byte[] lengthBuf = new byte[4];
				while (!shutdownToken.IsCancellationRequested)
				{
					await ReadExactly(stream, lengthBuf, 0, 4);
					int length = BinaryPrimitives.ReadInt32BigEndian(lengthBuf);
					if (length <= 0 || length > 1_048_576)
					{
						log.LogDebug("Relay client {ClientId}: invalid frame length {Length}", clientId, length);
						break;
					}
					byte[] payload = new byte[length];
					await ReadExactly(stream, payload, 0, length);

					byte[] plaintext;
					try
					{
						plaintext = Decrypt(encryptionKey, payload);
					}
					catch (CryptographicException)
					{
						log.LogWarning("Relay client {ClientId}: decryption failed (wrong secret?)", clientId);
						break;
					}

					string line = Encoding.UTF8.GetString(plaintext).Trim();
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
							CommandAvailable.Release();
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

		static async Task ReadExactly(NetworkStream stream, byte[] buffer, int offset, int count)
		{
			int totalRead = 0;
			while (totalRead < count)
			{
				int bytesRead = await stream.ReadAsync(buffer.AsMemory(offset + totalRead, count - totalRead));
				if (bytesRead == 0)
					throw new IOException("Connection closed");
				totalRead += bytesRead;
			}
		}

		public void Broadcast(object jsonEvent)
		{
			string json = JsonSerializer.Serialize(jsonEvent, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			byte[] plaintext = Encoding.UTF8.GetBytes(json);
			byte[] encrypted = Encrypt(encryptionKey, plaintext);
			byte[] lengthPrefix = new byte[4];
			BinaryPrimitives.WriteInt32BigEndian(lengthPrefix, encrypted.Length);
			byte[] frame = new byte[4 + encrypted.Length];
			Buffer.BlockCopy(lengthPrefix, 0, frame, 0, 4);
			Buffer.BlockCopy(encrypted, 0, frame, 4, encrypted.Length);

			foreach (var (id, (client, stream)) in clients)
			{
				try
				{
					if (!client.Connected)
					{
						RemoveClient(id);
						continue;
					}
					stream.Write(frame, 0, frame.Length);
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
				try { entry.stream.Dispose(); } catch { }
				try { entry.client.Dispose(); } catch { }
			}
		}

		public void Dispose()
		{
			listener?.Stop();
			foreach (var (id, (client, stream)) in clients)
			{
				try { stream.Dispose(); } catch { }
				try { client.Dispose(); } catch { }
			}
			clients.Clear();
		}
	}
}
