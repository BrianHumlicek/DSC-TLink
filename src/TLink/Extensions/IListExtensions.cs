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
	internal static class IListExtensions
	{
		public static ushort Bytes2Word(byte byte1, byte byte2) => (ushort)(byte1 << 8 | byte2);
		public static ushort PeekWord(this IList<byte> byteList, int startIndex) => Bytes2Word(byteList[startIndex], byteList[startIndex + 1]);
		public static ushort PeekLeadingWord(this IList<byte> byteList) => byteList.PeekWord(0);
		public static ushort PeekTrailingWord(this IList<byte> byteList) => byteList.PeekWord(byteList.Count - 2);
		//public static ushort GetTrailingWord(this IList<byte> byteList, int lengthIncludingWord) => GetWord(byteList, lengthIncludingWord - 2);
		public static ushort PopLeadingWord(this IList<byte> byteList)
		{
			ushort word = byteList.PeekWord(0);
			byteList.RemoveAt(0);
			byteList.RemoveAt(0);
			return word;
		}
		public static int PopTrailingWord(this IList<byte> byteList)
		{
			int word = byteList.PeekTrailingWord();
			byteList.RemoveAt(byteList.Count - 1);
			byteList.RemoveAt(byteList.Count - 1);
			return word;
		}
		public static byte PopLeadingByte(this IList<byte> byteList)
		{
			byte result = byteList[0];
			byteList.RemoveAt(0);
			return result;
		}
		public static IList<byte> PopLeadingBytes(this List<byte> byteList, int length)
		{
			IList<byte> result = byteList.Take(length).ToList();
			byteList.RemoveRange(0, length);
			return result;
		}
		public static IList<byte> PopLeadingBytes(this IList<byte> byteList, int length)
		{
			if (byteList is List<byte>)
			{
				return ((List<byte>)byteList).PopLeadingBytes(length);
			}
			IList<byte> result = byteList.Take(length).ToList();
			for (int i = 0; i < length; i++)
			{
				byteList.RemoveAt(0);
			}
			return result;
		}
		public static IEnumerable<byte> Pad16(this IEnumerable<byte> byteEnumerable)
		{
			int padLength = 16 - (byteEnumerable.Count() % 16);
			return padLength < 16 ? byteEnumerable.Concat(Enumerable.Repeat((byte)0, padLength)) : byteEnumerable;
		}
        public static int AddAndReturnIndex<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return list.Count -1;
        }
	}
}
