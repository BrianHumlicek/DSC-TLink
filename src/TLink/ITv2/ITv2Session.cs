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

using DSC.TLink.Extensions;
using DSC.TLink.ITv2.Enumerations;
using DSC.TLink.ITv2.Messages;
using DSC.TLink.Messages;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace DSC.TLink.ITv2
{
	internal class ITv2Session
	{
		ILogger log;
		TLinkClient tlinkClient;
		byte localSequence;	//This is the sequence number that this server is updating and the TL280 is checking
		byte tl280Sequence;	//This is the sequence number that the TL280 is updating amd this server is checking.
		byte appSequence;
		Aes sendAES = Aes.Create();
		Aes receiveAES = Aes.Create();
		bool sendAESActive;
		bool receiveAESActive;
		public void EnableSendAES(byte[] remoteKey)
		{
			log.LogDebug("Enabling send AES encryption (key length={KeyLen})", remoteKey.Length);
			sendAES.Key = remoteKey;
			sendAESActive = true;
		}
		public void EnableReceiveAES(byte[] localKey)
		{
			log.LogDebug("Enabling receive AES encryption (key length={KeyLen})", localKey.Length);
			receiveAES.Key = localKey;
			receiveAESActive = true;
		}
		public ITv2Session(TLinkClient tlinkClient, ILogger<ITv2Session> log)
		{
			this.log = log;
			this.tlinkClient = tlinkClient;
		}
		async Task<ITv2Header> readHeaderMessage(CancellationToken cancellationToken = default, int? timeoutMs = null)
		{
			// log.LogDebug("readHeaderMessage: waiting for message (receiveAES={AesActive}, timeout={Timeout})", receiveAESActive, timeoutMs);
			(_, byte[] message) = await tlinkClient.ReadMessage(cancellationToken, timeoutMs);
			log.LogDebug("readHeaderMessage: received {Length} bytes", message.Length);
			ITv2Header header = new ITv2Header();

			if (receiveAESActive)
			{
				byte[] plainText = receiveAES.DecryptEcb(message, PaddingMode.Zeros);
				log.LogDebug("Unencrypted {plainText}", plainText);
				header.Parse(plainText);
			}
			else
			{
				header.Parse(message);
			}

			tl280Sequence = header.SenderSequence;	//The TL280 sends its sequence number in the Host field when it is sending commands.
			if (localSequence != header.ReceiverSequence)
			{
				log.LogWarning("Sequence mismatch! localSequence={LocalSeq} != header.ReceiverSequence={RecvSeq}", localSequence, header.ReceiverSequence);
			}
			log.LogDebug("readHeaderMessage: SenderSeq={SenderSeq}, ReceiverSeq={ReceiverSeq}, AppSeq={AppSeq}, Command={Command}",
				header.SenderSequence, header.ReceiverSequence, header.AppSequence, header.Command);
			if (header.AppSequence.HasValue) appSequence = header.AppSequence.Value;
			return header;
		}

		public async Task<T> readMessage<T>(CancellationToken cancellationToken = default, int? timeoutMs = null) where T : NetworkByteMessage, new()
		{
			ITv2Header header = await readHeaderMessage(cancellationToken, timeoutMs);

			//validate that T is compatible with the Command
			if (header is T) return (header as T)!;

			T result = new();
			result.Parse(header.CommandData);
			return result;
		}
		public async Task SendSimpleAck()
		{
			var simpleAck = new SimpleAck()
			{
				HostSequence = 0,	//This doesn't get seem to be set in a simple ack
				RemoteSequence = tl280Sequence
			};
			if (sendAESActive)
			{
				byte[] plainText = simpleAck.ToByteArray();
				log.LogDebug("Unencrypted {plainText}", plainText);
				await tlinkClient.SendMessage(sendAES.EncryptEcb(plainText, PaddingMode.Zeros));
			}
			else
			{
				await tlinkClient.SendMessage(simpleAck.ToByteArray());
			}
		}
		public async Task sendMessage(ITv2Command? command, NetworkByteMessage? message = null)
		{
			var header = new ITv2Header()
			{
				SenderSequence = ++localSequence,
				ReceiverSequence = tl280Sequence,
				Command = command,
				AppSequence = ++appSequence	//The appsequence doesn't appear to cause problems or errors
			};

			if (message != null) header.CommandData = message.ToByteArray();

			if (sendAESActive)
			{
				byte[] plainText = header.ToByteArray();
				log.LogDebug("Unencrypted {plainText}", plainText);
				await tlinkClient.SendMessage(sendAES.EncryptEcb(plainText, PaddingMode.Zeros));
			}
			else
			{
				await tlinkClient.SendMessage(header.ToByteArray());
			}
		}
	}
}
