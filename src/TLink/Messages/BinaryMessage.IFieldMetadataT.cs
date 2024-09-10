namespace DSC.TLink.Messages
{
    internal abstract partial class BinaryMessage
    {
        interface IFieldMetadata<T> : IFieldMetadata
        {
            T GetPropertyValue(byte[] messageBytes);
            void SetPropertyValue(T value);
        }
    }
}
