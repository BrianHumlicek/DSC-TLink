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
		public abstract class MultiPropertyFieldMetadata<T> : IMultiPropertyFieldMetadata<T>
		{
			int? offset;
			protected Dictionary<string, T> initializationBuffer = new Dictionary<string, T>();

			//Explicit implementations of IMultiPropertyFieldMetadata<T>
			IEnumerable<string> IMultiPropertyFieldMetadata<T>.GetProperties() => GetProperties();
			T IMultiPropertyFieldMetadata<T>.GetPropertyValue(byte[] messageBytes, string propertyName) => GetPropertyValue(messageBytes, propertyName);
			void IMultiPropertyFieldMetadata<T>.SetPropertyValue(T value, string propertyName) => initializationBuffer[propertyName] = value;

			//Explicit implementations of IFieldMetadata
			IEnumerable<byte> IFieldMetadata.GetFieldBytes() => GetFieldBytes();
			void IFieldMetadata.EnsureLengthSet(byte[] messageBytes) => EnsureLengthSet(messageBytes);
			int? IFieldMetadata.Offset { get => offset; set => offset = value; }


			public abstract int Length { get; }
			protected int Offset => offset ?? throw new Exception("Offset is being accessed before being initialized!");
			protected abstract IEnumerable<byte> GetFieldBytes();
			protected abstract IEnumerable<string> GetProperties();
			protected abstract T GetPropertyValue(byte[] messageBytes, string propertyName);
			protected virtual void EnsureLengthSet(byte[] messageBytes) { /*Empty Implementation, override only if needed*/ }
		}
	}
}