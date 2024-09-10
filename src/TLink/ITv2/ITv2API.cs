//  DSC TLink - a communications library for DSC Powerseries NEO alarm panels
//  Copyright (C) 2024 Brian Humlicek

//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.

//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

using DSC.TLink.ITv2.Messages;
using Microsoft.Extensions.Logging;

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
            //(List<byte> header, List<byte> messageBytes) = tlinkClient.Listen(port);
            //File.WriteAllText("OpenSessionMessage.txt", TLinkClient.Array2HexString(messageBytes));
            var messagearray = TLinkClient.HexString2Array("15-00-00-06-0A-96-02-03-29-05-41-02-23-02-00-02-00-00-01-01-28-86");

            var message = new OpenSessionMessage(messagearray);

        }

        public void Dispose()
        {
            tlinkClient.Dispose();
        }
    }
}
