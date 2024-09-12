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

namespace DSC.TLink.Messages
{
	internal class Bit : BinaryMessage.BitmapMember<bool>
	{
		int bitNumber;
		int byteOffset;
		int bitOffset;
		public Bit(int bitNumber)
		{
			this.bitNumber = bitNumber;
			byteOffset = bitNumber / 8;
			bitOffset = bitNumber % 8;
		}

		protected override bool GetPropertyFromBytes(byte[] fieldBytes) => ((fieldBytes[byteOffset] >> bitOffset) & 0x01) == 0x01;
		protected override void SetPropertyInBytes(bool setBit, byte[] fieldBytes)
		{
			byte workingByte = fieldBytes[byteOffset];
			if (setBit)
			{
				workingByte = (byte)(workingByte & (1 << bitOffset));
			}
			else
			{
				workingByte = (byte)(workingByte | ~(1 << bitOffset));
			}
			fieldBytes[byteOffset] = workingByte;
		}
	}
}