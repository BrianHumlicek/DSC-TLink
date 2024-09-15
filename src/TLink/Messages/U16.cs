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
	internal class U16 : BinaryMessage.DiscreteFieldMetadata<ushort>
	{
		protected override int GetValidFieldLength(ushort property) => 2;
		protected override IEnumerable<byte> Property2FieldBytes(ushort property) => BitConverter.GetBytes(property);
		protected override ushort MessageBytes2Property(int offset, byte[] messageBytes) => BitConverter.ToUInt16(messageBytes, offset);
	}
}