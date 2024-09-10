namespace DSC.TLink.Messages
{
    internal abstract partial class BinaryMessage
    {
        interface IFieldMetadata
        {
            IEnumerable<byte> GetFieldBytes();
            void EnsureLengthSet(byte[] messageBytes);
            int Length { get; }
            int? Offset { get; set; }
        }
    }
}
