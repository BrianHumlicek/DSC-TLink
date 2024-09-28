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

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace DSC.TLink
{
	public class ITv2ConnectionHandler : ConnectionHandler
	{
		ILoggerFactory loggerFactory;
		ILogger log;
		ITlinkServerConnection tLinkServer;
		public ITv2ConnectionHandler(ITlinkServerConnection tLinkServerConnection, ILoggerFactory loggerFactory)
		{
			this.loggerFactory = loggerFactory;
			this.log = loggerFactory.CreateLogger<ITv2ConnectionHandler>();
			tLinkServer = tLinkServerConnection;
		}
		public async override Task OnConnectedAsync(ConnectionContext connection)
		{
			log.LogInformation($"Connection request from {connection.RemoteEndPoint}");
			try
			{
				TLinkClient tlinkClient = new TLinkClient(connection.Transport, loggerFactory.CreateLogger<TLinkClient>());
				if (!await tLinkServer.TryInitializeConnection(tlinkClient))
				{
					log.LogWarning("Unable to serve connection request from {RemoteEndPoint}", connection.RemoteEndPoint);
					return;
				}
				log.LogInformation("TLink connected to {RemoteEndPoint}", connection.RemoteEndPoint);
				while (tLinkServer.Active)
				{
					await tLinkServer.ReceiveCommand();
				}
			}
			catch (Exception ex)
			{
				log.LogError(ex, "TLink server connection error");
			}
			finally
			{
				try
				{
					tLinkServer.EnsureServerConnectionReset();
				}
				catch (Exception ex)
				{
					log.LogCritical(ex, "Critial error resetting TLink server connection.");
				}
			}
			log.LogInformation("TLink disconnected from {RemoteEndPoint}", connection.RemoteEndPoint);
		}
	}
}
