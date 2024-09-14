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

using DSC.TLink.Extensions;

namespace DSC.TLink.DLSProNet
{
	internal class AESKeyGenerator
	{
		static readonly byte[] lookupTable = new byte[128]
		{
			0x10, 0x2E, 0x46, 0x47, 0x4F, 0x11, 0x62, 0x15, 0x6D, 0x26, 0x3E, 0x4B, 0x5B, 0x06, 0x2C, 0x0A,
			0x7F, 0x4C, 0x40, 0x68, 0x54, 0x23, 0x72, 0x14, 0x16, 0x74, 0x3A, 0x7E, 0x51, 0x3B, 0x5C, 0x1E,
			0x0D, 0x12, 0x65, 0x01, 0x2F, 0x6C, 0x34, 0x71, 0x03, 0x39, 0x56, 0x31, 0x35, 0x4A, 0x3C, 0x1D,
			0x42, 0x13, 0x4E, 0x17, 0x45, 0x22, 0x1F, 0x67, 0x0E, 0x78, 0x05, 0x48, 0x49, 0x1A, 0x27, 0x69,
			0x6A, 0x18, 0x66, 0x5D, 0x7A, 0x37, 0x59, 0x0B, 0x21, 0x09, 0x58, 0x1C, 0x61, 0x04, 0x64, 0x6B,
			0x28, 0x00, 0x5A, 0x20, 0x7C, 0x25, 0x63, 0x5F, 0x7D, 0x70, 0x60, 0x44, 0x24, 0x19, 0x53, 0x79,
			0x4D, 0x2D, 0x75, 0x2B, 0x3F, 0x02, 0x57, 0x0C, 0x52, 0x08, 0x41, 0x73, 0x77, 0x50, 0x43, 0x1B,
			0x55, 0x2A, 0x07, 0x36, 0x30, 0x76, 0x7B, 0x6E, 0x6F, 0x38, 0x0F, 0x32, 0x29, 0x5E, 0x3D, 0x33
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