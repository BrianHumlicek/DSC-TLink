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

namespace DSC.TLink.Extensions
{
	public static class BigEndianExtensions
	{
		//U8
		public static bool Bit0(this byte @byte) => (@byte & 0x01) != 0;
		public static bool Bit1(this byte @byte) => (@byte & 0x02) != 0;
		public static bool Bit2(this byte @byte) => (@byte & 0x04) != 0;
		public static bool Bit3(this byte @byte) => (@byte & 0x08) != 0;
		public static bool Bit4(this byte @byte) => (@byte & 0x10) != 0;
		public static bool Bit5(this byte @byte) => (@byte & 0x20) != 0;
		public static bool Bit6(this byte @byte) => (@byte & 0x40) != 0;
		public static bool Bit7(this byte @byte) => (@byte & 0x80) != 0;


		//U16

		public static ushort U16(ReadOnlySpan<byte> span, int offset = 0) => U16(span[offset], span[offset + 1]);
		public static ushort U16(byte highByte, byte lowByte) => (ushort)(highByte << 8 | lowByte);

		//u16 extensions
		public static byte HighByte(this ushort u16) => (byte)(u16 >> 8);
		public static byte LowByte(this ushort u16) => (byte)u16;
		public static byte U16HighByte(this Enum enumeration) => Convert.ToUInt16(enumeration).HighByte();
		public static byte U16LowByte(this Enum enumeration) => Convert.ToUInt16(enumeration).LowByte();
		public static byte[] ToArray(this ushort u16) => [u16.HighByte(), u16.LowByte()];

		//U32
		public static (byte HighByte, byte Byte2, byte Byte1, byte LowByte) U32(uint u32) => ((byte)(u32 >> 24), (byte)(u32 >> 16), (byte)(u32 >> 8), (byte)u32);
	}
}
