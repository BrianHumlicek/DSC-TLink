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
	public abstract class ArrayProperty : IArrayProperty, INullableArrayProperty
	{
		protected byte[]? bytes;
		protected abstract byte[] GetDefaultValue();
		protected abstract bool validateLength(byte[] value);
		
		//All interfaces
		public abstract bool TrySet(ref ReadOnlySpan<byte> span);

		//IArrayProperty
		byte[] IArrayProperty.Get() => bytes ?? GetDefaultValue();
		void IArrayProperty.Set(byte[] value, string? propertyName)
		{
			if (value == null) throw new ArgumentNullException($"");
			else if (!validateLength(value)) throw new MessageException($"");
			bytes = value;
		}

		//INullableArrayProperty
		bool INullableArrayProperty.HasValue => bytes != null;
		byte[]? INullableArrayProperty.Get() => bytes;
		void INullableArrayProperty.Set(byte[]? value, string? propertyName)
		{
			if (value != null && !validateLength(value)) throw new MessageException($"");
			bytes = value;
		}
	}
}
