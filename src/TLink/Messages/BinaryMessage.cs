using DSC.TLink.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace DSC.TLink.Messages
{
    internal abstract partial class BinaryMessage
    {
        protected BinaryMessage(byte[]? messageBytes)
        {
            OnInitializing();
            if (messageBytes != default)
            {
                this.messagebytes = messageBytes;
                calculateAndValidateLength();
            }
        }
        protected abstract void OnInitializing();
        List<IFieldMetadata> definitions = new List<IFieldMetadata>();
        Dictionary<string, int> propertyMappings = new Dictionary<string, int>();
        byte[]? messagebytes;
        public byte[] MessageBytes
        {
            get
            {
                if (messagebytes == null)
                {
                    messagebytes = definitions.SelectMany(definition => definition.GetFieldBytes()).ToArray();
                    calculateAndValidateLength();
                }
                return messagebytes;
            }
        }
        void calculateAndValidateLength()
        {
            //This runs through the sequence of definitions and incrementally sets the Length and Offset properties.
            IFieldMetadata lastFieldDefinition = definitions.Aggregate((priorField, nextField) =>
            {
                if (priorField.Offset == default) priorField.Offset = 0;
                priorField.EnsureLengthSet(MessageBytes);
                nextField.Offset = priorField.Offset + priorField.Length;
                nextField.EnsureLengthSet(MessageBytes);
                return nextField;
            });

            int totalDefinedMessageLength = (int)lastFieldDefinition.Offset! + lastFieldDefinition.Length;
            if (MessageBytes.Length < totalDefinedMessageLength) throw new Exception($"{nameof(MessageBytes)} is not long enough to parse message!");
            if (MessageBytes.Length > totalDefinedMessageLength)
            {
                //Do we care if messagebytes is longer than necesary?
            }
        }
        public int Length => MessageBytes.Length;
        protected void DefineField<T>(FieldMetadata<T> fieldMetadata, string propertyName) => propertyMappings[propertyName] = definitions.AddAndReturnIndex(fieldMetadata);
        protected void DefineField<T>(MultiPropertyFieldMetadata<T> multiPropertyFieldMetadata)
        {
            int definitionIndex = definitions.AddAndReturnIndex(multiPropertyFieldMetadata);
            foreach (var property in ((IMultiPropertyFieldMetadata<T>)multiPropertyFieldMetadata).GetProperties())
            {
                propertyMappings.Add(property, definitionIndex);
            }
        }
        IFieldMetadata getFieldMetadata(string propertyName) => definitions[propertyMappings[propertyName]];
        protected T GetProperty<T>([CallerMemberName] string propertyName = "") => getFieldMetadata(propertyName) switch
        {
            IFieldMetadata<T>              fieldMetadata              => fieldMetadata.GetPropertyValue(MessageBytes),
            IMultiPropertyFieldMetadata<T> multiPropertyFieldMetadata => multiPropertyFieldMetadata.GetPropertyValue(MessageBytes, propertyName),
            _ => throw new Exception("Unknown property metadata")
        };
        protected void SetProperty<T>(T value, [CallerMemberName] string propertyName = "")
        {
            if (messagebytes != null) throw new ArgumentException($"Unable to set property {propertyName} because {nameof(BinaryMessage)}.{nameof(MessageBytes)} has been initialized.");
            var metadata = getFieldMetadata(propertyName);
            if (metadata is IFieldMetadata<T> fieldMetadata)
            {
                fieldMetadata.SetPropertyValue(value);
            }
            else if (metadata is IMultiPropertyFieldMetadata<T> multiPropertyFieldMetadata)
            {
                multiPropertyFieldMetadata.SetPropertyValue(value, propertyName);
            }
            throw new Exception("Unknown metadata type in SetProperty");
        }
    }
}
