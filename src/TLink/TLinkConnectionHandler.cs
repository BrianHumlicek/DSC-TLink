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
			log.LogInformation("Connection request from {RemoteEndPoint}", connection.RemoteEndPoint);
			string phase = "pre-init";
			int commandCount = 0;
			try
			{
				log.LogDebug("Creating TLinkClient for {RemoteEndPoint}", connection.RemoteEndPoint);
				TLinkClient tlinkClient = new TLinkClient(connection.Transport, loggerFactory.CreateLogger<TLinkClient>());
				phase = "handshake";
				log.LogDebug("Starting TryInitializeConnection for {RemoteEndPoint}", connection.RemoteEndPoint);
				if (!await tLinkServer.TryInitializeConnection(tlinkClient))
				{
					log.LogWarning("Unable to serve connection request from {RemoteEndPoint} — initialization handshake failed", connection.RemoteEndPoint);
					return;
				}
				phase = "command-loop";
				log.LogInformation("TLink connected to {RemoteEndPoint} — entering command receive loop", connection.RemoteEndPoint);
				while (tLinkServer.Active)
				{
					await tLinkServer.ReceiveCommand();
					commandCount++;
					if (commandCount % 100 == 0)
						log.LogDebug("Processed {CommandCount} commands from {RemoteEndPoint}", commandCount, connection.RemoteEndPoint);
				}
				log.LogDebug("Command loop ended: Active={Active}, commands processed={CommandCount}", tLinkServer.Active, commandCount);
			}
			catch (OperationCanceledException)
			{
				log.LogDebug("Connection to {RemoteEndPoint} cancelled during {Phase} (shutdown)", connection.RemoteEndPoint, phase);
			}
			catch (TLinkPacketException ex)
			{
				log.LogWarning("Panel disconnected from {RemoteEndPoint} during {Phase} (commands processed={CommandCount}): {Message}",
					connection.RemoteEndPoint, phase, commandCount, ex.Message);
			}
			catch (Exception ex)
			{
				log.LogError(ex, "TLink server connection error with {RemoteEndPoint} during {Phase}: {Message}", connection.RemoteEndPoint, phase, ex.Message);
			}
			finally
			{
				log.LogDebug("Resetting server connection state for {RemoteEndPoint}", connection.RemoteEndPoint);
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
