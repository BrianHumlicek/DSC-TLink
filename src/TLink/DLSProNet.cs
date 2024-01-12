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

using DSC.TLink.Extensions;

namespace DSC.TLink
{
    public class DLSProNet
	{
		public static TLinkSessionState ParseConnectPacket(List<byte> packetBytes)
		{
			(List<byte> header, List<byte> payload) = parseFraming(packetBytes);

			if (payload.Count < 15) throw new ArgumentException("Parsed payload length is too short");

			int encodedCRC = payload.TrimTrailingWord();

			if (encodedCRC != CalculateCRC(payload)) throw new ArgumentException("Parsed header CRC is invalid");

			payload.RemoveRange(0, 5);  //The start delimiter and length bytes are part of the CRC so this is the soonest they can be removed.

			return TLinkSessionState.ParseConnectionPayload(payload);
		}

		static ushort CalculateCRC(IEnumerable<byte> data)
		{
			ushort crc = 0;
			foreach (byte @byte in data)
			{
				byte workingByte = @byte;
				for (int index2 = 0; index2 < 8; ++index2)
				{
					bool flag = ((workingByte ^ crc) & 1) == 1;
					crc >>= 1;
					if (flag)
					{
						crc ^= 0xD003;
					}
					workingByte = (byte)((workingByte & 1) << 7 | workingByte >> 1);
				}
			}
			return crc;
		}

		static (List<byte>, List<byte>) parseFraming(IEnumerable<byte> packetBytes)
		{
			List<byte> header = new List<byte>();	//The header is everything before the sequence 0x05-0x05-0x05 that is followed by a byte that is NOT 0x05.
			List<byte> payload = new List<byte>();	//The payload is 0x05-0x05-0x05 followed by a length WORD, and (n) number of bytes where n equals length.
			List<byte> footer = new List<byte>();	//The footer is everything after the payload.
			List<byte> workingList = header;
			int delimiterCount = 0;
			int remainingPayloadBytes = -1;

			using (var enumerator = packetBytes.GetEnumerator())
			while (enumerator.MoveNext())
			{
				if (workingList == header)
				{
					if (enumerator.Current == 0x05)
					{
						if (delimiterCount < 3)
						{
							delimiterCount++;
							continue;
						}
					}
					else if (delimiterCount == 3)
					{
						//I think its odd that the payload includes the delimiter as well as the length bytes, but it does.
						payload.AddRange(Enumerable.Repeat((byte)0x05, 3));
						payload.Add(enumerator.Current);
						if (!enumerator.MoveNext()) throw new Exception();
						payload.Add(enumerator.Current);
						remainingPayloadBytes = payload.GetTrailingWord();
						workingList = payload;
						continue;
					}
					else if (delimiterCount > 0)
					{
						header.AddRange(Enumerable.Repeat((byte)0x05, delimiterCount));
						delimiterCount = 0;
					}
				}
				else if (remainingPayloadBytes == 0)
				{
					workingList = footer;
				}
				workingList.Add(enumerator.Current);
				remainingPayloadBytes--;
			}

			if (delimiterCount != 3) throw new Exception();
			if (remainingPayloadBytes > 0) throw new Exception();

			return (header, payload);
		}
	}
}
