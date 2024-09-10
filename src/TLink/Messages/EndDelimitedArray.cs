namespace DSC.TLink.Messages
{
    internal class EndDelimitedArray : BinaryMessage.FieldMetadata<byte[]>
    {
        public override int Length => throw new NotImplementedException();

        protected override IEnumerable<byte> GetFieldBytes()
        {
            throw new NotImplementedException();
        }

        protected override byte[] GetPropertyValue(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
