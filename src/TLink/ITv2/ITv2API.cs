using DSC.TLink.ITv2.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
