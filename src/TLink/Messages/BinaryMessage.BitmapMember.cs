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
	internal partial class BinaryMessage
	{
		public abstract class BitmapMember<T> : IGetSetProperty<T>, IBitmapMember
		{
			T? propertyBuffer;
			T GetPropertyBuffer() => (propertyBuffer ?? DefaultPropertysetializer()) ?? throw new Exception();
			protected virtual T? DefaultPropertysetializer() => default(T);
			protected abstract T GetPropertyFromBytes(byte[] fieldBytes);
			protected abstract void SetPropertyInBytes(T property, byte[] fieldBytes);

			//Explicit implementations of IBitmappedFieldMember
			void IBitmapMember.setialize(byte[] fieldBytes)
			{
				if (propertyBuffer == null)
				{
					propertyBuffer = GetPropertyFromBytes(fieldBytes);
				}
			}
			void IBitmapMember.SetFieldBytes(byte[] fieldBytes) => SetPropertyInBytes(GetPropertyBuffer(), fieldBytes);

			//Explicit implementations of IGetSetProperty<T>
			T IGetSetProperty<T>.Property
			{
				get => GetPropertyBuffer();
				set => propertyBuffer = value;
			}
		}
	}
}
