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
			bool isPropertyInitialized;
			int? offset;
			int? length;
			protected int Offset => offset ?? throw new Exception();
			protected virtual T? DefaultPropertyInitializer() => default(T);
			protected abstract T MessageBytes2Property(byte[] messageBytes);
			protected abstract IEnumerable<byte> Property2FieldBytes(T property);
			protected abstract int GetFieldLength(T property);	//This can be used to validate set length as well as returning actual or spec'ed length.
			T propertyAccessor
			{
				get
				{
					if (!isPropertyInitialized)
					{
						propertyBuffer = DefaultPropertyInitializer();
					}
					return propertyBuffer ?? throw new Exception("");
				}
				set
				{
					propertyBuffer = value ?? throw new Exception("");
					isPropertyInitialized = true;
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
			void IFieldMetadata.SetOffsetAndInitialize(int offset, byte[] messageBytes)
			{
				this.offset = offset;
				if (!isPropertyInitialized)
				{
					propertyAccessor = MessageBytes2Property(messageBytes);
				}
				length = GetFieldLength(propertyAccessor);
			}
			int IFieldMetadata.Offset => Offset;
			int IFieldMetadata.Length => length ?? throw new Exception();
		}
	}
}