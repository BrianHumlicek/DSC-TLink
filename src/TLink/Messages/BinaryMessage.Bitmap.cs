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
	internal abstract partial class BinaryMessage
	{
		public sealed class Bitmap : IBitmapFieldMetadata
		{
			int length;
			int? offset;
			Dictionary<string, IBitmapMember> propertyMappings = new Dictionary<string, IBitmapMember>();

			public Bitmap(int length)
			{
				this.length = length;
			}
			public Bitmap DefineMember<T>(BitmapMember<T> member, string propertyName)
			{
				propertyMappings.Add(propertyName, member);
				return this;
			}

			//Explicit implementation of IFieldMetadata
			int IFieldMetadata.Length => length;
			int IFieldMetadata.Offset => offset ?? throw new Exception();
			IEnumerable<byte> IFieldMetadata.GetFieldBytes()
			{
				byte[] result = new byte[length];
				foreach (var member in propertyMappings.Values)
				{
					member.SetFieldBytes(result);
				}
				return result;
			}

			//Explicit implementation of IBitmappedFieldMetadata
			IGetSetProperty<T> IBitmapFieldMetadata.GetPropertyAccessor<T>(string propertyName) => propertyMappings[propertyName] switch
			{
				IGetSetProperty<T> propertyAccessor => propertyAccessor,
				_ => throw new Exception()
			};
			IEnumerable<string> IBitmapFieldMetadata.GetPropertyNames() => propertyMappings.Keys;
			void IFieldMetadata.setializeFieldProperty(int offset, byte[] messageBytes)
			{
				this.offset = offset;
				byte[] fieldBytes = messageBytes.Skip(offset).Take(length).ToArray();
				foreach (var member in propertyMappings.Values)
				{
					member.setialize(fieldBytes);
				}
			}
		}
	}
}