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
	public class ITv2API : IDisposable
	{
		ILogger log;
		TLinkClient tlinkClient;
		public ITv2API(ILogger logger)
		{
			log = logger;
			tlinkClient = new TLinkClient(lengthEncodedPackets: false, logger);
		}
		public void Open(int port = 3072)
		{
			var m = new ITv2CommandHeader();
			m.CommandData = null;// new byte[5];

			Console.WriteLine($"{TLinkClient.Array2HexString(m.CommandData)}");
			return;

			string integrationId = "200328900112";
			byte[] id = Encoding.UTF8.GetBytes(integrationId);

			var header1 = tlinkClient.Listen<ITv2CommandHeader>(port);

			var data1 = new OpenSessionMessage(header1.CommandData);
			byte[] one = data1.FirmwareVersion;

			var response1 = new ITv2CommandHeader()
			{
				HostSequence = 1,
				RemoteSequence = 0,
				Command = ITv2Command.Command_Response
			};

			tlinkClient.SendMessage(id, response1.MessageBytes);

			var header2 = tlinkClient.ReadMessage<CommandResponse>();

			var responseheader2 = new ITv2CommandHeader()
			{
				HostSequence = 0x02,
				RemoteSequence = 0x00,
				Command = ITv2Command.Connection_Open_Session,
				CommandData = new OpenSessionMessage().MessageBytes
			};

			tlinkClient.SendMessage(id, responseheader2.MessageBytes);

			var header3 = tlinkClient.ReadMessage<ITv2CommandHeader>();
		}

		public void Dispose()
		{
			tlinkClient.Dispose();
		}
	}
}