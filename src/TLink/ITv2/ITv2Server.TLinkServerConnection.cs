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

		async Task ITlinkServerConnection.ReceiveCommand()
		{
			var r = await itv2Session.readMessage<ITv2Header>();
		}

		async Task<bool> ITlinkServerConnection.TryInitializeConnection(TLinkClient tlinkClient)
		{
			string integrationId = "200328900112";
			byte[] id = Encoding.UTF8.GetBytes(integrationId);
			tlinkClient.DefaultHeader = id;

			itv2Session = new ITv2Session(tlinkClient, loggerFactory.CreateLogger<ITv2Session>());

			var openSession = await itv2Session.readMessage<OpenSessionMessage>();
			await itv2Session.sendMessage(ITv2Command.Command_Response);
			var one = await itv2Session.readMessage<ITv2Header>();

			await itv2Session.sendMessage(ITv2Command.Connection_Open_Session, openSession);
			var two = await itv2Session.readMessage<ITv2Header>();
			await itv2Session.SendSimpleAck();

			var three = await itv2Session.readMessage<RequestAccess>();

			byte[] transmitKey = ITv2AES.ParseType1Initializer("200328900112", three.Payload);

			itv2Session.EnableSendAES(transmitKey);

			await itv2Session.sendMessage(ITv2Command.Command_Response);
			var four = await itv2Session.readMessage<ITv2Header>();



			(byte[] initializer, byte[] receivingKey) = ITv2AES.GenerateKeyAndType1Initializer("12345678");
			var requestAccess = new RequestAccess()
			{
				Payload = initializer
			};
			
			await itv2Session.sendMessage(ITv2Command.Connection_Request_Access, requestAccess);

			itv2Session.EnableReceiveAES(receivingKey);

			var five = await itv2Session.readMessage<ITv2Header>();
			await itv2Session.SendSimpleAck();

			return true;
		}
	}
}
