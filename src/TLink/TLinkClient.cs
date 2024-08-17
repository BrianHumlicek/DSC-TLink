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
//	along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DSC.TLink.Extensions;

namespace DSC.TLink
{
	public class TLinkClient : IDisposable
	{
        protected DeviceHeader? deviceHeader;
		TcpClient tcpClient;
		Aes AES;
		byte[] sendHeader;

		public TLinkClient()
		{
			tcpClient = new TcpClient();
			tcpClient.ReceiveTimeout = 1000;

			AES = Aes.Create();
			AES.Mode = CipherMode.ECB;
			AES.Padding = PaddingMode.None;

			sendHeader = new byte[] { 1, 202, 254 };	//TODO: Figure out what this is supposed to be
		}

		public void Connect(IPAddress address)
		{
			Connect(new IPEndPoint(address, 3062));
		}
		public void Connect(IPEndPoint endPoint)
		{
			tcpClient.Connect(endPoint);

			var message = ReadMessage();

			deviceHeader = DLSProNetHeader.ParseInitialHeader(message);

			if (useEncryption)
			{
				AES.Key = GSEncryptionKeyGenerator.FromDeviceHeader(deviceHeader);
			}
		}

		public List<byte> ReadMessage()
		{
			byte[] packet = readPacket();

			if (useEncryption)
			{
				packet = decrypt(packet);
			}

			(List<byte> header, List<byte> payload) = parsePacket(packet);

			return payload;
		}

		public void SendMessage(byte[] message)
		{
			var packet = encodePacket(sendHeader, message);

			if (useEncryption)
			{
				packet = encrypt(packet);
			}

			ushort lengthWord = (ushort)packet.Length;

			byte[] lengthBytes = new byte[] { lengthWord.HighByte(), lengthWord.LowByte() };

			packet = lengthBytes.Concat(packet).ToArray();

			tcpClient.GetStream().Write(packet, 0, packet.Length);
		}

		public string ReadMessageBCD() => Array2HexString(ReadMessage());
		public void SendMessageBCD(string bcdMessage) => SendMessage(HexString2Array(bcdMessage));
		public static byte[] HexString2Array(string hexString)
		{
			return hexString.Split('-').Select(s => byte.Parse(s, NumberStyles.HexNumber)).ToArray();
		}
		public static string Array2HexString(IEnumerable<byte> bytes)
		{
			return String.Join('-', bytes.Select(b => $"{b:X2}"));
		}

		bool useEncryption => deviceHeader?.Encrypted ?? false;

		protected virtual byte[] readPacket()
		{
			NetworkStream tcpStream = tcpClient.GetStream();

			int length = IListExtensions.Bytes2Word(readByte(), readByte());
			byte[] result = new byte[length];
			for (int i = 0; i < length; i++)
			{
				result[i] = readByte();
			}
			return result;

			byte readByte()
			{
				int result = tcpStream.ReadByte();
				if (result == -1) throw new TLinkPacketException("Unexpected end of TCP stream");
				return (byte)result;
			}
		}

		(List<byte>, List<byte>) parsePacket(IEnumerable<byte> packetBytes)
		{
			List<byte> header = new List<byte>();	//This is called ConsoleHeader on the connect packet and is always 5 bytes long in the connect packet
			List<byte> payload = new List<byte>();
			List<byte> workingList = header;

			using (var enumerator = packetBytes.GetEnumerator())
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
					case 0x7D:	//Stuffed byte
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
						if (workingList == header) throw new TLinkPacketException("End of frame encountered before start of frame");
						return (header, payload);
					default:
						workingList.Add(enumerator.Current);
						break;
				}
			}
			UnexpectedEndOfPacket:
			throw new TLinkPacketException("No end of frame delimiter");
		}

		byte[] encodePacket(IEnumerable<byte> header, IEnumerable<byte> payload)
		{
			return stuffBytes(header).Concat(0x7E).Concat(stuffBytes(payload)).Concat(0x7F).ToArray();

			IEnumerable<byte> stuffBytes(IEnumerable<byte> inputBytes)
			{
				foreach(byte b in inputBytes)
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
		byte[] decrypt(byte[] cipherText)
		{
			cipherText = cipherText.Pad16().ToArray();

			byte[] plainText = new byte[cipherText.Length];

			using (ICryptoTransform decryptor = AES.CreateDecryptor())
			{
				decryptor.TransformBlock(cipherText, 0, cipherText.Length, plainText, 0);
			}

			return plainText;
		}
		byte[] encrypt(byte[] plainText)
		{
			plainText = plainText.Pad16().ToArray();

			byte[] cipherText = new byte[plainText.Length];

			using (ICryptoTransform encryptor = AES.CreateEncryptor())
			{
				encryptor.TransformBlock(plainText, 0, plainText.Length, cipherText, 0);
			}

			return cipherText;
		}
		public void Dispose()
		{
			tcpClient?.Dispose();
			AES?.Dispose();
		}
	}
}
