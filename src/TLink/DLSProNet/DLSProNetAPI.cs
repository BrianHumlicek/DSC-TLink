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

using System.Net;
using System.Timers;
using Microsoft.Extensions.Logging;
using DSC.TLink.Extensions;

namespace DSC.TLink.DLSProNet
{
	public class DLSProNetAPI : IDisposable
	{
		ILogger log;
		TLinkClient tlinkClient;
		System.Timers.Timer heartbeat;
		public DLSProNetAPI(ILogger logger)
		{
			log = logger;

			tlinkClient = new TLinkClient(lengthEncodedPackets: true, logger);

			heartbeat = new System.Timers.Timer(TimeSpan.FromSeconds(1));
			heartbeat.Elapsed += sendHeartbeat;
			heartbeat.AutoReset = true;
		}
		public void Open(IPAddress address, ushort installerCode = 0xCAFE)
		{
			log?.LogDebug($"{nameof(DLSProNetAPI)} opening '{address}' with installer code '{installerCode}'");

			(List<byte> consoleHeader, List<byte> message) = tlinkClient.Connect(address);

			DeviceHeader deviceHeader = DLSProNetHeader.ParseInitialHeader(message);

			if (deviceHeader.KeyID != null)
			{
				var aesKey = AESKeyGenerator.FromDeviceHeader(deviceHeader);
				//DLSProNet uses the same key for all messages, sent or received, so set both local and remote keys the same.
				tlinkClient.AES.LocalKey = aesKey;
				tlinkClient.AES.RemoteKey = aesKey;
				tlinkClient.UseEncryption = true;
			}

			byte dataMode = 0x01;
			tlinkClient.DefaultHeader = new byte[] { dataMode, installerCode.HighByte(), installerCode.LowByte() };

			heartbeat.Start();
			log?.LogInformation($"{nameof(DLSProNetAPI)} open");
		}
		void sendHeartbeat(object? sender, ElapsedEventArgs e)
		{
			try
			{
				var nopMessage = encodeCommand(DLSProNetCommand.NOP);

				tlinkClient.SendMessage(nopMessage);

				var response = tlinkClient.ReadMessage().message;

				validateMessage(response);

				var responseCommand = parseCommand(response);

				if (responseCommand != DLSProNetCommand.ACK)
				{
					//Figure out what to do with error
				}
			}
			catch
			{
				//Figure out what to do with exception
			}
			Console.WriteLine("ping");
		}

		void validateMessage(List<byte> message)
		{
			if (message.Count < 6) throw new Exception();

			var checkSum = message.PopTrailingWord();

			if (checkSum != calculateSimpleChecksum(message)) throw new Exception();

			var length = message.PopLeadingWord();

			if (length != message.Count + 2) throw new Exception();
		}

		DLSProNetCommand parseCommand(List<byte> message)
		{
			ushort result = message.PopLeadingWord();
			return (DLSProNetCommand)result;
		}

		byte[] encodeCommand(DLSProNetCommand command)
		{
			ushort length = 4;
			List<byte> result = new List<byte>(6)
			{
				length.HighByte(),
				length.LowByte(),
				command.HighByte(),
				command.LowByte()
			};
			var csum = calculateSimpleChecksum(result);
			result.Add(csum.HighByte());
			result.Add(csum.LowByte());
			return result.ToArray();
		}
		ushort calculateSimpleChecksum(IEnumerable<byte> bytes) => (ushort)bytes.Sum(b => b);
		public void Dispose()
		{
			heartbeat.Elapsed -= sendHeartbeat;
			heartbeat.Dispose();
			tlinkClient.Dispose();
		}
	}
}