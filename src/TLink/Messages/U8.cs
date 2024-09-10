namespace DSC.TLink.Messages
{
    internal class U8 : BinaryMessage.FieldMetadata<byte>
    {
        public override int Length => 1;
        protected override byte GetPropertyValue(byte[] bytes) => bytes[Offset];
        protected override IEnumerable<byte> GetFieldBytes()
        {
            yield return initializationBuffer;
        }
    }
}
