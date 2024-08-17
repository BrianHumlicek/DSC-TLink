using DSC.TLink.Extensions;
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

namespace DSC.TLink
{
	public class GSEncryptionKeyGenerator
	{
		private static byte[] lookupTable = new byte[128]
		{
			 16,	//0x10
			 46,	//0x2E
			 70,	//0x46
			 71,	//0x47
			 79,	//0x4F
			 17,	//0x11
			 98,
			 21,
			 109,
			 38,
			 62,
			 75,
			 91,
			 6,
			 44,
			 10,
			 127,
			 76,
			 64,
			 104,
			 84,
			 35,
			 114,
			 20,
			 22,
			 116,
			 58,
			 126,
			 81,
			 59,
			 92,
			 30,
			 13,
			 18,
			 101,
			 1,
			 47,
			 108,
			 52,
			 113,
			 3,
			 57,
			 86,
			 49,
			 53,
			 74,
			 60,
			 29,
			 66,
			 19,
			 78,
			 23,
			 69,
			 34,
			 31,
			 103,
			 14,
			 120,
			 5,
			 72,
			 73,
			 26,
			 39,
			 105,
			 106,
			 24,
			 102,
			 93,
			 122,
			 55,
			 89,
			 11,
			 33,
			 9,
			 88,
			 28,
			 97,
			 4,
			 100,
			 107,
			 40,
			 0,
			 90,
			 32,
			 124,
			 37,
			 99,
			 95,
			 125,
			 112,
			 96,
			 68,
			 36,
			 25,
			 83,
			 121,
			 77,
			 45,
			 117,
			 43,
			 63,
			 2,
			 87,
			 12,
			 82,
			 8,
			 65,
			 115,
			 119,
			 80,
			 67,
			 27,
			 85,
			 42,
			 7,
			 54,
			 48,
			 118,
			 123,
			 110,
			 111,
			 56,
			 15,
			 50,
			 41,
			 94,
			 61,
			 51
		};
		public static byte[] FromDeviceHeader(DeviceHeader deviceHeader)
		{
			List<byte> result = deviceHeader.KeyID.ToList();
			crc16(result, 0);
			byte version1 = (byte)(deviceHeader.SoftwareVersion >> 4);
			byte version2 = (byte)(deviceHeader.SoftwareVersion << 4 | deviceHeader.SoftwareRevision & 0x0F);
			byte testVersion = deviceHeader.TestVersion;
			byte testRevision = deviceHeader.TestRevision;
			if (deviceHeader.CommunicatorVersion != null)
			{
				if (deviceHeader.CommunicatorVersion.Length >= 2)
				{
					version1 = (byte)(deviceHeader.CommunicatorVersion[0] >> 4);
					version2 = (byte)(deviceHeader.CommunicatorVersion[0] << 4 | deviceHeader.CommunicatorVersion[1] & 15);
				}
				if (deviceHeader.CommunicatorVersion.Length >= 4)
				{
					testVersion = deviceHeader.CommunicatorVersion[2];
					testRevision = deviceHeader.CommunicatorVersion[3];
				}
			}
			result.Add(version1);
			result.Add(version2);
			crc16(result, result.Count - 2);
			result.Add(testVersion);
			result.Add(testRevision);
			crc16(result, result.Count - 2);
			return tableScramble(result);
		}
		private static void crc16(List<byte> byteList, int startIndex)
		{
			ushort crc = 0;
			for (int listIndex = startIndex; listIndex < byteList.Count; listIndex++)
			{
				byte workingByte = byteList[listIndex];
				for (byte count = 0; count < 8; count++)
				{
					bool flag = ((workingByte ^ crc) & 1) == 1;
					crc >>= 1;
					if (flag)
						crc ^= 0xA001;
					workingByte >>= 1;
				}
			}
			byteList.Add(crc.HighByte());
			byteList.Add(crc.LowByte());
		}
		private static byte[] tableScramble(List<byte> inputBytes)
		{
			byte[] result = new byte[16];
			for (byte index = 0; index < lookupTable.Length; index++)
			{
				if ((inputBytes[index >> 3] & 1 << (index & 7)) != 0)
					result[lookupTable[index] >> 3] |= (byte)(1 << (lookupTable[index] & 7));
			}
			return result;
		}
	}
}
