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
		int definedMessageLength;
		List<IFieldMetadata> fieldDefinitions = new List<IFieldMetadata>();
		Dictionary<string, int> propertyMappings = new Dictionary<string, int>();
		byte[]? messageBytesBackingField;
		IProcessFraming? framingBackingField;
		IProcessFraming framing
		{
			get => framingBackingField ??= new DefaultFraming();
			set => framingBackingField = value ?? throw new ArgumentNullException(nameof(framing));
		}
		bool framingActive => framing is not DefaultFraming;
		public BinaryMessage()
		{
			OnInitializing();
			if (fieldDefinitions.Count == 0) throw new InvalidOperationException($"No fields have been defined!  Use {nameof(DefineField)}() in {nameof(OnInitializing)}() to configure at least one field for message type '{GetType().Name}'");
		}
		protected BinaryMessage(byte[] messageBytes) : this()
		{
			MessageBytes = messageBytes ?? throw new ArgumentNullException(nameof(messageBytes));
		}
		protected abstract void OnInitializing();
		public byte[] MessageBytes
		{
			get
			{
				if (messageBytesBackingField == null)
				{
					byte[] unframedMessage = fieldDefinitions.SelectMany(definition => definition.GetFieldBytes()).ToArray();
					initializeFieldMetadata(unframedMessage, initializeFieldProperties: false);
					messageBytesBackingField = framing.AddFraming(unframedMessage);
				}
				return messageBytesBackingField!;
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(MessageBytes));
				byte[] unframedMessage = framing.RemoveFraming(value);
				int unframedMessageDefinedLength = initializeFieldMetadata(unframedMessage, initializeFieldProperties: true);

				//Unframed messages could potentially run longer than the defined length.
				//For example, an unframed nested message in the middle of another message
				//would be initialized with too many bytes because we don't know how many
				//bytes the nested message needs until after it is initialized.
				//Framed messages on the other hand are complete and should be exactly as
				//defined.  Any overrun in a framed message is a problem.
				if (framingActive && unframedMessage.Length != unframedMessageDefinedLength) throw new BinaryMessageException($"Framing error!  Expected {unframedMessageDefinedLength} bytes but got {unframedMessage.Length} bytes");

				int totalDefinedMessageLength = unframedMessageDefinedLength + framing.OverheadLength;

				messageBytesBackingField = value.Length == totalDefinedMessageLength ? value
																					 : value.Take(totalDefinedMessageLength).ToArray();
			}
		}
		int initializeFieldMetadata(byte[] unframedMessage, bool initializeFieldProperties)
		{
			initializeField(fieldDefinitions[0], 0);

			IFieldMetadata lastFieldDefinition = fieldDefinitions.Aggregate((priorField, nextField) =>
			{
				int nextFieldOffset = priorField.Offset + priorField.Length;
				initializeField(nextField, nextFieldOffset);
				return nextField;
			});

			return lastFieldDefinition.Offset + lastFieldDefinition.Length;
			
			void initializeField(IFieldMetadata fieldMetadata, int offset)
			{
				if (offset >= unframedMessage.Length) throw new BinaryMessageException($"No available bytes for initializing field {fieldDefinitions.IndexOf(fieldMetadata)}  with type '{fieldMetadata.GetType().Name}' of message type '{GetType().Name}'!");

				if (initializeFieldProperties)
				{
					fieldMetadata.InitializeFieldProperty(offset, unframedMessage);
				}
				else
				{
					fieldMetadata.Offset = offset;
				}
			}
		}

		//Field definitions
		protected void SetFraming(IProcessFraming framing) => this.framing = framing;
		protected void DefineField<T>(DiscreteFieldMetadata<T> discreteFieldMetadata, string propertyName) => propertyMappings[propertyName] = fieldDefinitions.AddAndReturnIndex(discreteFieldMetadata);
		protected void DefineField(Bitmap bitmapMetadata)
		{
			int definitionIndex = fieldDefinitions.AddAndReturnIndex(bitmapMetadata);
			foreach (var propertyName in ((IBitmapFieldMetadata)bitmapMetadata).GetPropertyNames())
			{
				propertyMappings.Add(propertyName, definitionIndex);
			}
		}

		//Property access
		protected T GetProperty<T>([CallerMemberName] string propertyName = "") => getPropertyAccessor<T>(propertyName).Property;
		protected void SetProperty<T>(T value, [CallerMemberName] string propertyName = "")
		{
			getPropertyAccessor<T>(propertyName).Property = value ?? throw new ArgumentNullException(propertyName);
			messageBytesBackingField = null;	//This will cause a full re-evaluation of field properties and re-populate the messageBytesBuffer on the net 'Get' of MessageBytes
		}
		IGetSetProperty<T> getPropertyAccessor<T>(string propertyName) => fieldDefinitions[propertyMappings[propertyName]] switch
		{
			IGetSetProperty<T> propertyAccessor => propertyAccessor,
			IBitmapFieldMetadata bitmappedFieldMetadata => bitmappedFieldMetadata.GetPropertyAccessor<T>(propertyName),
			object metadata => throw new InvalidOperationException($"Error accessing property '{propertyName}'.  Metadata describes type '{metadata.GetType().GenericTypeArguments[0].Name}' and property accessor is type '{typeof(T).Name}'.  Did you forget to explicitly cast an enum to it's value type on Get/SetProperty() or use the generic Enum<T> metadata type?")
		};
	}
}