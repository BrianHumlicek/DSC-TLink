namespace DSC.TLink.Messages
{
    internal abstract partial class BinaryMessage
    {
        public abstract class FieldMetadata<T> : IFieldMetadata<T>
        {
            int? offset;
            protected T? initializationBuffer;

            //Explicit implementations of IFieldMetadata<T>
            T IFieldMetadata<T>.GetPropertyValue(byte[] messageBytes) => GetPropertyValue(messageBytes);
            void IFieldMetadata<T>.SetPropertyValue(T value) => initializationBuffer = value;

            //Explicit implementations of IFieldMetadata
            IEnumerable<byte> IFieldMetadata.GetFieldBytes() => GetFieldBytes();
            void IFieldMetadata.EnsureLengthSet(byte[] messageBytes) => EnsureLengthSet(messageBytes);
            int? IFieldMetadata.Offset { get => offset; set => offset = value; }


            public abstract int Length { get; }
            protected int Offset => offset ?? throw new Exception("Offset is being accessed before being initialized!");
            protected abstract IEnumerable<byte> GetFieldBytes();
            protected abstract T GetPropertyValue(byte[] messageBytes);
            protected virtual void EnsureLengthSet(byte[] messageBytes) { /*Empty Implementation, override only if needed*/ }
        }
    }
}
