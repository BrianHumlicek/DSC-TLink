﻿// DSC TLink - a communications library for DSC Powerseries NEO alarm panels
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
	internal class FixedArray : BinaryMessage.DiscreteFieldMetadata<byte[]>
	{
		readonly int length;
		public FixedArray(int length)
		{
			this.length = length;
		}
		protected override byte[] DefaultPropertyInitializer() => new byte[length];
		protected override IEnumerable<byte> Property2FieldBytes(byte[] property) => property;
		protected override byte[] MessageBytes2Property(int offset, byte[] messageBytes) => messageBytes.Skip(offset).Take(length).ToArray();
		protected override int GetValidFieldLength(byte[] property)
		{
			if (property.Length != length) throw new ArgumentException($"FixedArray is defined to be length {length} but was set with an array of length {property.Length}");
			return property.Length;
		}
	}
}