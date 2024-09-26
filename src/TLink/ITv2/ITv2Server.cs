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
using DSC.TLink.Messages;
using Microsoft.Extensions.Logging;
using System.IO.Pipelines;
using System.Text;

namespace DSC.TLink.ITv2
{
	public partial class ITv2Server
	{
		ILoggerFactory loggerFactory;

		ILogger log;
		TLinkClient tlinkClient;
		CancellationToken shutdownToken;
		public ITv2Server(ILoggerFactory loggerFactory, CancellationToken shutdownToken)
		{
			this.loggerFactory = loggerFactory;
			this.shutdownToken = shutdownToken;
		}
		async Task<T> readMessage<T>() where T : NetworkByteMessage, new()
		{
			(_, byte[] message) = await tlinkClient.ReadMessage();
			ITv2Header header = new ITv2Header();
			header.Parse(message);
			//validate that T is compatible with the Command
			if (header is T) return (header as T)!;

			T result = new();
			result.Parse(header.CommandData);
			return result;
		}
		public async Task OpenSession()
		{
			string integrationId = "200328900112";
			byte[] id = Encoding.UTF8.GetBytes(integrationId);

			var openSessionMessage = await readMessage<OpenSessionMessage>();

			var response1 = new ITv2Header()
			{
				SenderSequence = 1,
				ReceiverSequence = 0,
				Command = ITv2Command.Command_Response,
				AppSequence = 0,
				CommandData = [0x00]   //This is some kind of response code that is always zero
			};

			await tlinkClient.SendMessage(id, response1.ToByteArray());

			//var header2 = await readMessage();

			var reply = new OpenSessionMessage()
			{
				DeviceType = Itv2PanelDeviceType.DscInterfaceCommunicatorModule,
				DeviceID = [0x03, 0x2E],	//814, this should probably be a ushort
				FirmwareVersion = [0x01, 0x40],//320
				ProtocolVersion = [0x02, 0x10],//528 My device reports 2.35, so it would appear DLS is a little behind
				TxBufferSize = 1000,
				RxBufferSize = 1000,
				Unknown = [0x00, 0x01], //DLS doesnt all this to change ref:ITV2OpenSessionInstruction.ctor
				EncryptionType = EncryptionType.Unknown
			};
			//var responseheader2 = new ITv2CommandHeader()
			//{
			//	HostSequence = 0x02,
			//	RemoteSequence = 0x00,
			//	Command = ITv2Command.Connection_Open_Session,
			//	CommandData = new OpenSessionMessage().ToByteArray()
			//};

			//tlinkClient.SendMessage(id, responseheader2.ToByteArray());

			//var header3 = tlinkClient.ReadMessage<ITv2CommandHeader>();
			//This response is a byte, and a LeadingLength Array that is the property Identifier in ITV2RequestAccessInstructionReply
			//This should get the same response as response1 on line 45
		}
		public bool Active => false;
		public async Task ReceiveCommand()
		{
			//wait for read message
			//	-If read is canceled by token, that is a heartbeat timeout, so close session and return.
			//perform action
			//receive follow up replys
			//done
		}
		public void Dispose()
		{
			//tlinkClient.Dispose();
		}
	}
}