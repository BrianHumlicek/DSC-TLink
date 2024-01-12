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
		TLinkSessionState session;
		TcpClient? tcpClient;
		NetworkStream? stream;

		Aes AES;
		public TLinkClient()
		{
			AES = Aes.Create();
			AES.Mode = CipherMode.ECB;
			AES.Padding = PaddingMode.None;
		}
		public void Connect(IPEndPoint target)
		{
			initializeTcpClient(target);
			List<byte> packet = readPacket();
			var payload = Parse(packet);

			session = DLSProNet.ParseConnectPacket(payload);

			if (UseEncryption)
			{
				AES.Key = GSEncryptionKeyGenerator.FromSessionState(session);
			}
		}

		bool UseEncryption => session?.Encrypted ?? false;
		void initializeTcpClient(IPEndPoint target)
		{
			tcpClient?.Dispose();
			tcpClient = new TcpClient();
			tcpClient.ReceiveTimeout = 1000;
			tcpClient.Connect(target);
			stream = tcpClient.GetStream();
		}
		List<byte> readPacket()
		{
			int length = IListExtensions.Bytes2Word(readByte(), readByte());
			List<byte> result = new List<byte>(length);
			for (int i = 0; i < length; i++)
			{
				result.Add(readByte());
			}
			return result;
		}
		byte readByte()
		{
			int result = stream.ReadByte();
			if (result == -1) throw new TLinkPacketException("Unexpected end of TCP stream");
			return (byte)result;
		}
		public List<byte> Parse(IList<byte> packet)
		{
			IEnumerable<byte> packetEnumerable = UseEncryption ? decrypt(packet) : packet;

			(List<byte> header, List<byte> payload) = parseFraming(packetEnumerable);

			return payload;
		}

		static (List<byte>, List<byte>) parseFraming(IEnumerable<byte> packetBytes)
		{
			List<byte> header = new List<byte>();
			List<byte> payload = new List<byte>();
			List<byte> workingList = header;

			using (var enumerator = packetBytes.GetEnumerator())
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
					case 0x7D:
						if (!enumerator.MoveNext()) throw new TLinkPacketException("No end of frame delimiter");
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
			throw new TLinkPacketException("No end of frame delimiter");
		}
		byte[] decrypt(IEnumerable<byte> cipherText)
		{
			byte[] cipherTextArray = cipherText.ToArray();

			byte[] plainText = new byte[cipherTextArray.Length];

			using (ICryptoTransform transform = AES.CreateDecryptor())
			{
				transform.TransformBlock(cipherTextArray, 0, cipherTextArray.Length, plainText, 0);
			}

			return plainText;
		}
		byte[] encrypt(byte[] plainText)
		{
			byte[] cipherText = new byte[plainText.Length];

			using (ICryptoTransform transform = AES.CreateEncryptor())
			{
				transform.TransformBlock(plainText, 0, plainText.Length, cipherText, 0);
			}

			return cipherText;
		}

		public void Dispose()
		{
			tcpClient?.Dispose();
		}
	}
}
