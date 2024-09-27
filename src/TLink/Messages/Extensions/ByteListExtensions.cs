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
using System.Runtime.Intrinsics.X86;

namespace DSC.TLink.Messages.Extensions
{
	internal static class ByteListExtensions
	{
		//u16
		public static void Add(this List<byte> list, short value) => Add(list, (ushort)value);
		public static void Add(this List<byte> list, ushort value)
		{
			list.Add(value.HighByte());
			list.Add(value.LowByte());
		}

		//u32
		public static void Add(this List<byte> list, int value) => Add(list, (uint)value);
		public static void Add(this List<byte> list, uint value)
		{
			(byte b3, byte b2, byte b1, byte b0) = BigEndianExtensions.U32(value);
			list.Add(b3);
			list.Add(b2);
			list.Add(b1);
			list.Add(b0);
		}

		//u64
		public static void Add(this List<byte> list, long value) => Add(list, (ulong)value);
		public static void Add(this List<byte> list, ulong value)
		{
			list.Add((byte)(value >> 56));
			list.Add((byte)(value >> 48));
			list.Add((byte)(value >> 40));
			list.Add((byte)(value >> 32));
			list.Add((byte)(value >> 24));
			list.Add((byte)(value >> 16));
			list.Add((byte)(value >> 8));
			list.Add((byte)value);
		}

		public static bool TryAdd(this List<byte> list, byte? nullableByte)
		{
			if (nullableByte.HasValue)
			{
				list.Add(nullableByte.Value);
				return true;
			}
			return false;

		}

		public static bool TryAdd(this List<byte> list, ushort? nullableUShort)
		{
			if (nullableUShort.HasValue)
			{
				Add(list, nullableUShort.Value);
				return true;
			}
			return false;
		}
	}
}
