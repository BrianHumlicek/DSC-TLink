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

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using DSC.TLink.Extensions;

namespace DSC.TLink
{
	internal class TLinkClient : IDisposable
	{
		bool lengthEncodedPackets;
		ILogger log;

		TcpClient? tcpClient;
		TLinkAES tlinkAES = new TLinkAES();

		public TLinkClient(bool lengthEncodedPackets, ILogger logger)
		{
			this.lengthEncodedPackets = lengthEncodedPackets;
			log = logger;
		}

		public TLinkAES AES => tlinkAES;
		public bool UseEncryption { get; set; }
		public byte[] DefaultHeader { get; set; }

		/// <summary>
		/// Connects to the default DLS port
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public (List<byte> header, List<byte> message) Connect(IPAddress address) => Connect(new IPEndPoint(address, 3062));
		//3060 Local Port [851][105] Ethernet receiver 1
		//3061 Remote Port [851][104] Ethernet receiver 1
		//3062 DLS Incomming port [851][012]
		//3066 DLS Outgoing port [851][013]
		//3072 [851][429] Integraion notification Integration session 1
		//3073 [851][430] Integration polling Integration session 1
		//3070 [851][432] Integration Outgoing Integration session 1
		//3071 [851][433] Integration Incoming Integration session 1
		//3092 SA Incomming port [851][095]
		//3094 SA Outgoing port.[851][096]  messages sent after an SMS request
		//3064 is the "ReceiverPort"  Not sure where this came from

		/// <summary>
		/// 
		/// </summary>
		/// <param name="endPoint"></param>
		public (List<byte> header, List<byte> message) Connect(IPEndPoint endPoint)
		{
			tcpClient = new TcpClient(endPoint);

			tcpClient.Connect(endPoint);

			return ReadMessage();
		}
		public (List<byte> header, List<byte> message) Listen(int port)
		{
			TcpListener listener = TcpListener.Create(port);

			listener.Start();

			tcpClient = listener.AcceptTcpClient();

			return ReadMessage();
		}
		public void SendMessageBCD(string bcdMessage) => SendMessage(HexString2Array(bcdMessage));
		public void SendMessage(byte[] message) => SendMessage(DefaultHeader, message);
		public void SendMessage(byte[] header, byte[] message)
		{
			log?.LogTrace(() => $"Sending header '{Array2HexString(header)}' with message '{Array2HexString(message)}'");

			var packet = encodeConsoleHeader(header, message);

			if (UseEncryption)
			{
				log?.LogDebug(() => $"Raw packet before encrypting '{Array2HexString(packet)}'");
				packet = tlinkAES.EncryptRemote(packet);
			}

			if (lengthEncodedPackets)
			{
				ushort lengthWord = (ushort)packet.Length;

				byte[] lengthBytes = new byte[] { lengthWord.HighByte(), lengthWord.LowByte() };

				packet = lengthBytes.Concat(packet).ToArray();
			}

			if (UseEncryption)
			{
				log?.LogTrace($"Final encrypted packet '{Array2HexString(packet)}'");
			}
			else
			{
				log?.LogDebug(() => $"Final encoded packet {Array2HexString(packet)}");
			}

			tcpClient.GetStream().Write(packet, 0, packet.Length);
		}
		public string ReadMessageBCD() => Array2HexString(ReadMessage().message);
		public (List<byte> header, List<byte> message) ReadMessage()
		{
			byte[] packet = readPacket();

			if (UseEncryption)
			{
				packet = tlinkAES.DecryptLocal(packet);
				log?.LogTrace(() => $"Unencrypted raw message '{Array2HexString(packet)}'");
			}

			(var header, var payload) = parseConsoleHeader(packet);

			log?.LogDebug(() => $"Received header '{Array2HexString(header)}' with payload '{Array2HexString(payload)}'");

			return (header, payload);
		}
		byte[] readPacket()
		{
			NetworkStream tcpStream = tcpClient.GetStream();
			Thread.Sleep(500);
			//This is done because this method would occasionally return 0 byte messages because this method was executing before the packet arrived.
			//The ReadByte method will block until a byte is available and since the whole packet is transmitted atomically, it effectivelly blocks until the whole packet is ready.
			byte firstByte = (byte)tcpStream.ReadByte();
			byte[] buffer = new byte[tcpStream.Socket.Available + 1];
			tcpStream.Read(buffer, 1, buffer.Length - 1);
			buffer[0] = firstByte;

			log?.LogTrace(() => $"Raw packet received '{Array2HexString(buffer)}'");

			if (!lengthEncodedPackets)
			{
				return buffer;
			}

			List<byte> packet = new List<byte>(buffer);
			int encodedLength = IListExtensions.Bytes2Word(packet.PopLeadingByte(), packet.PopLeadingByte());
			if (packet.Count < encodedLength) throw new TLinkPacketException($"Insufficient number of bytes in packet. '{Array2HexString(buffer)}'");
			if (packet.Count > encodedLength)
			{
				log?.LogWarning($"Packet encoded length is {encodedLength} but {packet.Count} bytes were read. '{Array2HexString(buffer)}'");
				packet.RemoveRange(encodedLength, packet.Count - encodedLength);
			}
			return packet.ToArray();
		}
		(List<byte> header, List<byte> payload) parseConsoleHeader(IEnumerable<byte> packetBytes)
		{
			//Consoleheader
			//[0] Datamode	- This seems to always be 1
			//[1] Console password HB	This is the installer code default 0xCAFE
			//[2] Console password LB
			List<byte> consoleHeader = new List<byte>();    //This is called ConsoleHeader on the connect packet and is always 5 bytes long in the connect packet
			List<byte> payload = new List<byte>();
			List<byte> workingList = consoleHeader;

			using (var enumerator = packetBytes.GetEnumerator())
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current)
					{
						case 0x7D:  //Stuffed byte
							if (!enumerator.MoveNext()) goto UnexpectedEndOfPacket;
							switch (enumerator.Current)
							{
								case 0:
									workingList.Add(0x7D);
									break;
								case 1:
									workingList.Add(0x7E);
									break;
								case 2:
									workingList.Add(0x7F);
									break;
								default:
									throw new TLinkPacketException("Invalid escape value");
							}
							break;
						case 0x7E:   //Start of frame
							if (workingList == payload) throw new TLinkPacketException("Duplicate start of frame delimiter encountered");
							workingList = payload;
							break;
						case 0x7F:   //End of frame
							if (workingList == consoleHeader) throw new TLinkPacketException("End of frame encountered before start of frame");
							return (consoleHeader, payload);
						default:
							workingList.Add(enumerator.Current);
							break;
					}
				}
			UnexpectedEndOfPacket:
			throw new TLinkPacketException("No end of frame delimiter");
		}
		byte[] encodeConsoleHeader(IEnumerable<byte> consoleHeader, IEnumerable<byte> payload)
		{
			var stuffedConsoleHeader = stuffBytes(consoleHeader);
			var stuffedPayload = stuffBytes(payload);

			return stuffedConsoleHeader.Concat(0x7E).Concat(stuffedPayload).Concat(0x7F).ToArray();

			IEnumerable<byte> stuffBytes(IEnumerable<byte> inputBytes)
			{
				foreach (byte b in inputBytes)
				{
					switch (b)
					{
						case 0x7D:
							yield return 0x7D;
							yield return 0x00;
							break;
						case 0x7E:
							yield return 0x7D;
							yield return 0x01;
							break;
						case 0x7F:
							yield return 0x7D;
							yield return 0x02;
							break;
						default:
							yield return b;
							break;
					}
				}
			}
		}
		public static byte[] HexString2Array(string hexString) => hexString.Split('-').Select(s => byte.Parse(s, NumberStyles.HexNumber)).ToArray();
		public static string Array2HexString(IEnumerable<byte> bytes) => String.Join('-', bytes.Select(b => $"{b:X2}"));
		public void Dispose()
		{
			tcpClient?.Dispose();
			tlinkAES.Dispose();
		}
	}
}