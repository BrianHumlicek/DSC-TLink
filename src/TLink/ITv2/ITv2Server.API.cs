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
using Microsoft.Extensions.Logging;

namespace DSC.TLink.ITv2
{
	public partial class ITv2Server : IITV2ServerAPI
	{
		// Arm mode bytes (from captured traffic — arm away used 0x01)
		const byte ArmModeAway = 0x01;
		const byte ArmModeStay = 0x02;
		const byte ArmModeNight = 0x03;

		async Task IITV2ServerAPI.ArmAway(byte partition, string accessCode)
		{
			log.LogInformation("Sending Arm Away for partition {Partition}", partition);
			await SendPartitionArmCommand(partition, ArmModeAway, accessCode);
		}

		async Task IITV2ServerAPI.ArmStay(byte partition, string accessCode)
		{
			log.LogInformation("Sending Arm Stay for partition {Partition}", partition);
			await SendPartitionArmCommand(partition, ArmModeStay, accessCode);
		}

		async Task IITV2ServerAPI.ArmNight(byte partition, string accessCode)
		{
			log.LogInformation("Sending Arm Night for partition {Partition}", partition);
			await SendPartitionArmCommand(partition, ArmModeNight, accessCode);
		}

		async Task IITV2ServerAPI.Disarm(byte partition, string accessCode)
		{
			log.LogInformation("Sending Disarm for partition {Partition}", partition);
			// Captured disarm payload: {partition, 0x01, ...accessCodeBCD}
			byte[] codeBytes = AccessCodeToPackedBCD(accessCode);
			byte[] payload = new byte[2 + codeBytes.Length];
			payload[0] = partition;
			payload[1] = 0x01;
			Array.Copy(codeBytes, 0, payload, 2, codeBytes.Length);
			await SendCommandWithPayload(ITv2Command.ModuleControl_Partition_Disarm_Control, payload);
		}

		private async Task SendPartitionArmCommand(byte partition, byte armMode, string accessCode)
		{
			// Captured arm payload: {partition, armMode, 0x02, ...accessCodeBCD}
			byte[] codeBytes = AccessCodeToPackedBCD(accessCode);
			byte[] payload = new byte[3 + codeBytes.Length];
			payload[0] = partition;
			payload[1] = armMode;
			payload[2] = 0x02;
			Array.Copy(codeBytes, 0, payload, 3, codeBytes.Length);
			await SendCommandWithPayload(ITv2Command.ModuleControl_Partition_Arm_Control, payload);
		}

		private async Task SendCommandWithPayload(ITv2Command command, byte[] payload)
		{
			log.LogDebug("Sending command 0x{Command:X4} payload: {Payload}",
				(ushort)command, BitConverter.ToString(payload));

			await itv2Session.sendMessage(command, new RawPayload(payload));

			// Read the panel's response to keep sequence numbers in sync.
			// Same pattern as TryInitializeConnection(): send → read response → ack.
			var response = await itv2Session.readMessage<ITv2Header>();

			if (response.Command == ITv2Command.Command_Response && response.CommandData?.Length >= 1)
			{
				var responseCode = (CommandResponseCode)response.CommandData[0];
				log.LogInformation("Panel command response: {ResponseCode}", responseCode);
			}
			else
			{
				LogCommand(response);
			}

			await itv2Session.SendSimpleAck();
		}

		/// <summary>
		/// Convert an access code string (e.g. "4910") to packed BCD byte array.
		/// Two digits per byte: "4910" → { 0x49, 0x10 }.
		/// Odd-length codes are padded with a trailing 0 nibble.
		/// </summary>
		private static byte[] AccessCodeToPackedBCD(string accessCode)
		{
			foreach (char c in accessCode)
			{
				if (!char.IsDigit(c))
					throw new ArgumentException($"Access code must be numeric, got '{c}'");
			}

			// Pad to even length
			string padded = accessCode.Length % 2 == 1 ? accessCode + "0" : accessCode;
			byte[] result = new byte[padded.Length / 2];
			for (int i = 0; i < result.Length; i++)
			{
				int hi = padded[i * 2] - '0';
				int lo = padded[i * 2 + 1] - '0';
				result[i] = (byte)((hi << 4) | lo);
			}
			return result;
		}
	}
}
