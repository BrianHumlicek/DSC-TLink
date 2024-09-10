namespace DSC.TLink.Messages
{
    internal abstract partial class BinaryMessage
    {
        public abstract class MultiPropertyFieldMetadata<T> :  IMultiPropertyFieldMetadata<T>
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
