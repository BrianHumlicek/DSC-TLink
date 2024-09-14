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
		public abstract class DiscreteFieldMetadata<T> : IGetSetProperty<T>, IFieldMetadata
		{
			T? propertyBuffer;
			int? offset;
			int? length;
			protected virtual T? DefaultPropertyInitializer() => default;
			protected abstract T MessageBytes2Property(int offset, byte[] messageBytes);
			protected abstract IEnumerable<byte> Property2FieldBytes(T property);
			protected abstract int GetValidFieldLength(T property);
			T propertyAccessor
			{
				get
				{
					if (propertyBuffer == null)
					{
						propertyAccessor = DefaultPropertyInitializer() ?? throw new Exception("");
					}
					return propertyBuffer!;
				}
				set
				{
					propertyBuffer = value ?? throw new Exception();
					length = GetValidFieldLength(propertyBuffer);
				}
			}

			//Explicit implementations of IGetSetProperty<T>
			T IGetSetProperty<T>.Property
			{
				get => propertyAccessor;
				set => propertyAccessor = value;
			}

			//Explicit implementations of IFieldMetadata
			IEnumerable<byte> IFieldMetadata.GetFieldBytes() => Property2FieldBytes(propertyAccessor);
			void IFieldMetadata.InitializeFieldProperty(int offset, byte[] messageBytes)
			{
				this.offset = offset;
				propertyAccessor = MessageBytes2Property(offset, messageBytes);
			}
			int IFieldMetadata.Offset
			{
				get => offset ?? throw new Exception();
				set => offset = value;
			}
			int IFieldMetadata.Length => length ?? throw new Exception();
		}
	}
}