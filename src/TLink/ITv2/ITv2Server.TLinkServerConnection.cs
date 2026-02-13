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

using DSC.TLink.ITv2.Enumerations;
using DSC.TLink.ITv2.Messages;
using DSC.TLink.Relay;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DSC.TLink.ITv2
{
	public partial class ITv2Server : ITlinkServerConnection
	{
		ITv2Session itv2Session;
		bool ITlinkServerConnection.Active => !shutdownToken.IsCancellationRequested;

		void ITlinkServerConnection.EnsureServerConnectionReset()
		{
			//throw new NotImplementedException();
		}

		// When a relay is configured, use a short read timeout so we can check for
		// pending commands frequently. This reduces arm/disarm latency from ~30s to ~2s.
		const int RelayPollTimeoutMs = 2000;

		// Send a keepalive poll to the panel if no messages have been exchanged
		// for this long. Keeps the TCP connection alive across NAT/firewalls.
		const int HeartbeatIntervalSeconds = 30;
		DateTime lastPanelActivity = DateTime.UtcNow;

		async Task ITlinkServerConnection.ReceiveCommand()
		{
			// With a relay, poll every 2s so commands are dispatched quickly.
			// Without a relay, use the default long timeout (300s).
			int? timeout = relay != null ? RelayPollTimeoutMs : null;
			// log.LogDebug("ReceiveCommand: waiting for panel message (timeout={Timeout}ms, relay={HasRelay})", timeout, relay != null);

			try
			{
				var header = await itv2Session.readMessage<ITv2Header>(default, timeout);
				lastPanelActivity = DateTime.UtcNow;

				if (header.Command == ITv2Command.Connection_Encapsulated_Command_for_Multiple_Packets
					|| header.Command == ITv2Command.Connection_Encapsulated_Command_for_Long_Packets)
				{
					log.LogDebug("  Encapsulated packet (0x{CommandHex:X4})", header.Command.HasValue ? (ushort)header.Command.Value : 0);
					log.LogDebug("  SenderSeq:   {SenderSequence}", header.SenderSequence);
					log.LogDebug("  ReceiverSeq: {ReceiverSequence}", header.ReceiverSequence);
					log.LogDebug("  AppSequence: {AppSequence}", header.AppSequence);

					if (header.CommandData != null && header.CommandData.Length > 0)
					{
						log.LogDebug("  Raw data ({Length} bytes): {Data}", header.CommandData.Length, BitConverter.ToString(header.CommandData));
						ParseEncapsulatedSubMessages(header.CommandData);
						EmitSubMessageRelayEvents(header.CommandData);
					}
				}
				else
				{
					LogCommand(header);
					EmitRelayEvent(header);
				}

				await itv2Session.SendSimpleAck();
			}
			catch (OperationCanceledException)
			{
				// Read timed out — send a keepalive poll if idle too long
				if (DateTime.UtcNow - lastPanelActivity > TimeSpan.FromSeconds(HeartbeatIntervalSeconds))
				{
					await SendHeartbeat();
				}
			}

			// Process any pending commands from relay clients
			await ProcessPendingCommands();
		}

		async Task SendHeartbeat()
		{
			try
			{
				log.LogDebug("Sending keepalive poll to panel");
				await itv2Session.sendMessage(ITv2Command.Connection_Poll);
				var response = await itv2Session.readMessage<ITv2Header>();
				await itv2Session.SendSimpleAck();
				lastPanelActivity = DateTime.UtcNow;
				log.LogDebug("Keepalive poll acknowledged by panel");
			}
			catch (Exception ex)
			{
				log.LogWarning(ex, "Keepalive poll failed");
				throw;
			}
		}

		async Task ProcessPendingCommands()
		{
			if (relay == null) return;

			IITV2ServerAPI api = this;
			while (relay.PendingCommands.TryDequeue(out var command))
			{
				try
				{
					byte partition = (byte)command.Partition;
					if (string.IsNullOrEmpty(command.Code))
					{
						log.LogWarning("Command {Type} requires an access code", command.Type);
						break;
					}
					switch (command.Type.ToLowerInvariant())
					{
						case "arm_away":
							await api.ArmAway(partition, command.Code);
							break;
						case "arm_home":
						case "arm_stay":
							await api.ArmStay(partition, command.Code);
							break;
						case "arm_night":
							await api.ArmNight(partition, command.Code);
							break;
						case "disarm":
							await api.Disarm(partition, command.Code);
							break;
						default:
							log.LogWarning("Unknown relay command type: {Type}", command.Type);
							break;
					}
				}
				catch (Exception ex)
				{
					log.LogError(ex, "Error processing relay command: {Type}", command.Type);
				}
			}
		}

		void LogCommand(ITv2Header header)
		{
			log.LogInformation("  Command:     {Command} (0x{CommandHex:X4})", header.Command, header.Command.HasValue ? (ushort)header.Command.Value : 0);

			log.LogDebug("  SenderSeq:   {SenderSequence}", header.SenderSequence);
			log.LogDebug("  ReceiverSeq: {ReceiverSequence}", header.ReceiverSequence);
			log.LogDebug("  AppSequence: {AppSequence}", header.AppSequence);

			if (header.CommandData != null && header.CommandData.Length > 0)
			{
				log.LogDebug("  Data ({Length} bytes): {Data}", header.CommandData.Length, BitConverter.ToString(header.CommandData));
			}
		}

		void ParseEncapsulatedSubMessages(byte[] data)
		{
			// Encapsulated packets contain multiple length-prefixed sub-messages.
			// Format: <length><cmd_hi><cmd_lo><payload...> repeated
			// The first sub-message may not have a length prefix — scan for the
			// pattern where a byte equals the count of remaining bytes in the
			// current sub-message to find boundaries.
			var subMessages = ExtractSubMessages(data);
			foreach (var (command, payload) in subMessages)
			{
				string commandName = Enum.IsDefined(typeof(ITv2Command), command)
					? ((ITv2Command)command).ToString()
					: "Unknown";
				log.LogInformation("    Sub-Command: {CommandName} (0x{Command:X4})", commandName, command);
				if (payload.Length > 0)
				{
					log.LogInformation("    Sub-Data:    {Data}", BitConverter.ToString(payload));
				}
			}
		}

		static List<(ushort command, byte[] payload)> ExtractSubMessages(byte[] data)
		{
			var result = new List<(ushort command, byte[] payload)>();
			int offset = 0;

			while (offset < data.Length)
			{
				// Try to find a length-prefixed boundary.
				// Look ahead: if a byte at position i equals (data.Length - i - 1)
				// or the remaining bytes from i+1, it's a length prefix for the next sub-message.
				int subMsgEnd = data.Length; // default: rest of data is one sub-message
				for (int i = offset + 2; i < data.Length - 2; i++) // need at least 2 bytes after length for a command
				{
					int remainingAfter = data.Length - i - 1;
					if (data[i] == remainingAfter && remainingAfter >= 2)
					{
						subMsgEnd = i;
						break;
					}
				}

				// Parse sub-message from offset to subMsgEnd
				if (subMsgEnd - offset >= 2)
				{
					ushort cmd = (ushort)((data[offset] << 8) | data[offset + 1]);
					byte[] payload = new byte[subMsgEnd - offset - 2];
					Array.Copy(data, offset + 2, payload, 0, payload.Length);
					result.Add((cmd, payload));
				}

				// Skip the length byte prefix of the next sub-message
				offset = subMsgEnd + 1;
			}

			return result;
		}

		void EmitRelayEvent(ITv2Header header)
		{
			if (relay == null || !header.Command.HasValue) return;
			foreach (var evt in ConvertToRelayEvents((ITv2Command)header.Command.Value, header.CommandData, isSubMessage: false))
				relay.Broadcast(evt);
		}

		void EmitSubMessageRelayEvents(byte[] data)
		{
			if (relay == null) return;
			var subMessages = ExtractSubMessages(data);
			foreach (var (command, payload) in subMessages)
			{
				if (!Enum.IsDefined(typeof(ITv2Command), command)) continue;
				foreach (var evt in ConvertToRelayEvents((ITv2Command)command, payload, isSubMessage: true))
					relay.Broadcast(evt);
			}
		}

		static IEnumerable<object> ConvertToRelayEvents(ITv2Command command, byte[]? data, bool isSubMessage)
		{
			// Sub-messages have an extra leading AppSequence byte — strip it
			byte[]? payload = isSubMessage && data?.Length > 1 ? data[1..] : data;

			switch (command)
			{
				case ITv2Command.Notification_Life_Style_Zone_Status when payload?.Length >= 2:
					yield return new { type = "zone_status", zone = (int)payload[0], open = payload[1] != 0 };
					break;

				case ITv2Command.Notification_Partition_Ready_Status when payload?.Length >= 2:
					yield return new { type = "partition_ready", partition = (int)payload[0], ready = payload[1] == 1 };
					break;

				case ITv2Command.Notification_Arming_Disarming when payload?.Length >= 2:
					string state = payload[1] switch
					{
						0x00 => "disarmed",
						0x01 => "armed_home",
						0x02 => "armed_away",
						0x03 => "armed_night",
						_ => $"unknown_0x{payload[1]:X2}"
					};
					yield return new { type = "arming", partition = (int)payload[0], state };
					break;

				case ITv2Command.Notification_Exit_Delay when payload?.Length >= 4:
					yield return new { type = "exit_delay", partition = (int)payload[0], seconds = (int)payload[3] };
					break;

				case ITv2Command.Notification_Entry_Delay when payload?.Length >= 4:
					yield return new { type = "entry_delay", partition = (int)payload[0], seconds = (int)payload[3] };
					break;

				case ITv2Command.Connection_Poll:
					yield return new { type = "heartbeat" };
					break;
			}
		}

		async Task<bool> ITlinkServerConnection.TryInitializeConnection(TLinkClient tlinkClient)
		{
			log.LogDebug("TryInitializeConnection: starting handshake (IntegrationId={IntegrationId})", IntegrationId);
			byte[] id = Encoding.UTF8.GetBytes(IntegrationId);
			tlinkClient.DefaultHeader = id;

			itv2Session = new ITv2Session(tlinkClient, loggerFactory.CreateLogger<ITv2Session>());

			log.LogDebug("Handshake step 1/10: reading OpenSessionMessage from panel");
			var openSession = await itv2Session.readMessage<OpenSessionMessage>();
			log.LogDebug("Handshake step 2/10: sending Command_Response");
			await itv2Session.sendMessage(ITv2Command.Command_Response, new CommandResponse());
			log.LogDebug("Handshake step 3/10: reading response");
			var one = await itv2Session.readMessage<ITv2Header>();

			log.LogDebug("Handshake step 4/10: sending Connection_Open_Session");
			await itv2Session.sendMessage(ITv2Command.Connection_Open_Session, openSession);
			log.LogDebug("Handshake step 5/10: reading response");
			var two = await itv2Session.readMessage<ITv2Header>();
			log.LogDebug("Handshake step 6/10: sending SimpleAck");
			await itv2Session.SendSimpleAck();

			log.LogDebug("Handshake step 7/10: reading RequestAccess (panel's AES initializer)");
			var three = await itv2Session.readMessage<RequestAccess>();

			byte[] transmitKey = ITv2AES.Type2InitializerTransform(EncryptionKey, three.Payload);
			log.LogDebug("Handshake: derived transmit AES key from panel initializer");

			itv2Session.EnableSendAES(transmitKey);

			log.LogDebug("Handshake step 8/10: sending Command_Response (encrypted from here)");
			await itv2Session.sendMessage(ITv2Command.Command_Response, new CommandResponse());
			log.LogDebug("Handshake step 9/10: reading response");
			var four = await itv2Session.readMessage<ITv2Header>();

			byte[] initializer = ITv2AES.GetRandomKey();
			byte[] receivingKey = ITv2AES.Type2InitializerTransform(EncryptionKey, initializer);
			var requestAccess = new RequestAccess()
			{
				Payload = initializer
			};

			log.LogDebug("Handshake step 10/10: sending Connection_Request_Access (our AES initializer)");
			await itv2Session.sendMessage(ITv2Command.Connection_Request_Access, requestAccess);

			itv2Session.EnableReceiveAES(receivingKey);
			log.LogDebug("Handshake: receive AES enabled, reading final confirmation");

			var five = await itv2Session.readMessage<ITv2Header>();
			await itv2Session.SendSimpleAck();

			log.LogInformation("Handshake complete — bidirectional AES encryption active");
			return true;
		}
	}
}
