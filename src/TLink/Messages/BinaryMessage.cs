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
		byte[]? messageBytesBuffer;
		IProcessFraming? messageFraming;
		bool framingActive => messageFraming != null;
		public BinaryMessage()
		{
			DefineFields();
			if (fieldDefinitions.Count == 0) throw new Exception();
		}
		protected BinaryMessage(byte[] messageBytes) : this()
		{
			MessageBytes = messageBytes;
		}
		protected abstract void DefineFields();
		public int DefinedLength
		{
			get
			{   //Ensure the MessageBytes get'er is triggered in both cases (framingActive true/false) to ensure definedMessageLength is initialized.
				byte[] localMessageBytes = MessageBytes;
				return (int)(framingActive ? localMessageBytes.Length
										   : definedMessageLength);
			}
		}
		public byte[] MessageBytes
		{
			get
			{
				if (messageBytesBuffer == null)
				{
					byte[] unframedMessage = fieldDefinitions.SelectMany(definition => definition.GetFieldBytes()).ToArray();
					initializeFieldMetadata(unframedMessage, initializeFieldProperties: false);
					messageBytesBuffer = framingActive ? messageFraming!.AddFraming(unframedMessage)
													   : unframedMessage;
				}
				return messageBytesBuffer!;
			}
			set
			{
				messageBytesBuffer = value ?? throw new ArgumentNullException(nameof(MessageBytes));
				byte[] unframedMessage = framingActive ? messageFraming!.RemoveFraming(messageBytesBuffer)
													   : messageBytesBuffer;

				initializeFieldMetadata(unframedMessage, initializeFieldProperties: true);
			}
		}
		void initializeFieldMetadata(byte[] unframedMessage, bool initializeFieldProperties)
		{
			initializeField(fieldDefinitions[0], 0);

			IFieldMetadata lastFieldDefinition = fieldDefinitions.Aggregate((priorField, nextField) =>
			{
				int nextFieldOffset = priorField.Offset + priorField.Length;
				initializeField(nextField, nextFieldOffset);
				return nextField;
			});

			definedMessageLength = lastFieldDefinition.Offset + lastFieldDefinition.Length;
			
			//Unframed messages could potentially run longer than the defined length.
			//For example, an unframed nested message in the middle of another message
			//would be initialized with too many bytes because we don't know how many
			//bytes the nested message needs until after it is initialized.
			//Framed messages on the other hand are complete and should be exactly as
			//defined.  Any overrun in a framed message is a problem.
			if (framingActive && unframedMessage.Length > definedMessageLength) throw new Exception("Too many bytes to parse message!");

			void initializeField(IFieldMetadata fieldMetadata, int offset)
			{
				if (offset >= unframedMessage.Length) throw new Exception($"Too few bytes to parse message!");
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
		protected void SetFraming(IProcessFraming framing) => messageFraming = framing;
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
			getPropertyAccessor<T>(propertyName).Property = value ?? throw new ArgumentException("Cannot set Binary message properties to null!");
			messageBytesBuffer = null;	//This will cause a full re-evaluation of field properties and re-populate the messageBytesBuffer on the net 'Get' of MessageBytes
		}
		IGetSetProperty<T> getPropertyAccessor<T>(string propertyName) => fieldDefinitions[propertyMappings[propertyName]] switch
		{
			IGetSetProperty<T> propertyAccessor => propertyAccessor,
			IBitmapFieldMetadata bitmappedFieldMetadata => bitmappedFieldMetadata.GetPropertyAccessor<T>(propertyName),
			_ => throw new Exception("Unknown property metadata.  Did you forget to cast an enum to it's value type on SetProperty?")
		};
	}
}