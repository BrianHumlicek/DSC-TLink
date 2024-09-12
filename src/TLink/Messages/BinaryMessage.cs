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
using System.Runtime.CompilerServices;

namespace DSC.TLink.Messages
{
	internal abstract partial class BinaryMessage
	{
		protected BinaryMessage(byte[]? messageBytes)
		{
			OnInitializing();
			if (fieldDefinitions.Count == 0) throw new Exception();
			if (messageBytes != default)
			{
				MessageBytes = messageBytes;
			}
		}
		protected abstract void OnInitializing();
		List<IFieldMetadata> fieldDefinitions = new List<IFieldMetadata>();
		Dictionary<string, int> propertyMappings = new Dictionary<string, int>();
		byte[]? messageBuffer;
		public byte[] MessageBytes
		{
			get
			{
				if (messageBuffer == null)
				{
					MessageBytes = fieldDefinitions.SelectMany(definition => definition.GetFieldBytes()).ToArray();
				}
				return messageBuffer!;
			}
			private set
			{
				messageBuffer = value;
				initialize();
			}
		}
		void initialize()
		{
			fieldDefinitions[0].SetOffsetAndInitialize(offset: 0, MessageBytes);
			IFieldMetadata lastFieldDefinition = fieldDefinitions.Aggregate((priorField, nextField) =>
			{
				int nextFieldOffset = priorField.Offset + priorField.Length;
				nextField.SetOffsetAndInitialize(nextFieldOffset, MessageBytes);
				return nextField;
			});

			int totalDefinedMessageLength = lastFieldDefinition.Offset + lastFieldDefinition.Length;
			if (MessageBytes.Length < totalDefinedMessageLength) throw new Exception($"{nameof(MessageBytes)} is not long enough to parse message!");
			if (MessageBytes.Length > totalDefinedMessageLength)
			{
				//Do we care if messagebytes is longer than necesary?
			}
		}
		protected void DefineField<T>(DiscreteFieldMetadata<T> discreteFieldMetadata, string propertyName) => propertyMappings[propertyName] = fieldDefinitions.AddAndReturnIndex(discreteFieldMetadata);
		protected void DefineField(Bitmap compoundFieldMetadata)
		{
			int definitionIndex = fieldDefinitions.AddAndReturnIndex(compoundFieldMetadata);
			foreach (var propertyName in ((IBitmapFieldMetadata)compoundFieldMetadata).GetPropertyNames())
			{
				propertyMappings.Add(propertyName, definitionIndex);
			}
		}
		IGetSetProperty<T> getPropertyAccessor<T>(string propertyName) => fieldDefinitions[propertyMappings[propertyName]] switch
		{
			IGetSetProperty<T> propertyAccessor => propertyAccessor ,
			IBitmapFieldMetadata bitmappedFieldMetadata => bitmappedFieldMetadata.GetPropertyAccessor<T>(propertyName),
			_ => throw new Exception("Unknown property metadata")
		};
		protected T GetProperty<T>([CallerMemberName] string propertyName = "") => getPropertyAccessor<T>(propertyName).Property;
		protected void SetProperty<T>(T value, [CallerMemberName] string propertyName = "")
		{
			if (messageBuffer != null) throw new ArgumentException($"Unable to set property {propertyName} because {nameof(BinaryMessage)}.{nameof(MessageBytes)} has been initialized.");

			getPropertyAccessor<T>(propertyName).Property = value ?? throw new ArgumentException("Cannot set Binary message properties to null!");
		}
	}
}