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

using System.Runtime.InteropServices;

namespace DSC.TLink.Messages
{
	internal class U16Enum<T> : BinaryMessage.DiscreteFieldMetadata<T> where T : struct, Enum
	{
		public U16Enum()
		{
			int length = Marshal.SizeOf(typeof(T));
			if (length != 2) throw new InvalidOperationException($"Unable to create {nameof(U8Enum<T>)} because the underlying type of generic enum argument {typeof(T).Name} is of length {length}");
		}
		protected override int GetValidFieldLength(T property) => 2;
		//If anyone knows a better way than boxing/unboxing, let me know...
		protected override T MessageBytes2Property(int offset, byte[] messageBytes) => (T)(object)BitConverter.ToUInt16(messageBytes, offset);
		protected override IEnumerable<byte> Property2FieldBytes(T property) => BitConverter.GetBytes((ushort)(object)property);
	}
}
