namespace DSC.TLink.Messages
{
    internal abstract partial class BinaryMessage
    {
        interface IMultiPropertyFieldMetadata<T> : IFieldMetadata
        {
            IEnumerable<string> GetProperties();
            T GetPropertyValue(byte[] messageBytes, string propertyName);
            void SetPropertyValue(T value, string propertyName);
        }
    }
}
