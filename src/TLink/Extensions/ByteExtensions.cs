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

namespace DSC.TLink.Extensions
{
	internal static class ByteExtensions
	{
		public static bool Bit0(this byte @byte) => (@byte & 0x01) != 0;
		public static bool Bit1(this byte @byte) => (@byte & 0x02) != 0;
		public static bool Bit2(this byte @byte) => (@byte & 0x04) != 0;
		public static bool Bit3(this byte @byte) => (@byte & 0x08) != 0;
		public static bool Bit4(this byte @byte) => (@byte & 0x10) != 0;
		public static bool Bit5(this byte @byte) => (@byte & 0x20) != 0;
		public static bool Bit6(this byte @byte) => (@byte & 0x40) != 0;
		public static bool Bit7(this byte @byte) => (@byte & 0x80) != 0;
		public static byte HighByte(this ushort word) => (byte)(word >> 8);
		public static byte LowByte(this ushort word) => (byte)(word & 0xFF);
        public static byte HighByte(this Enum enumeration) => Convert.ToUInt16(enumeration).HighByte();
        public static byte LowByte(this Enum enumeration) => Convert.ToUInt16(enumeration).LowByte();
        public static IEnumerable<byte> ToBigEndianEnumerable(this ushort word)
        {
            yield return word.HighByte();
            yield return word.LowByte();
        }
	}
}
