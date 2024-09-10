namespace DSC.TLink.Messages
{
    internal class FixedArray : BinaryMessage.FieldMetadata<byte[]>
    {
        int length;
        public FixedArray(int length)
        {
            this.length = length;
        }
        public override int Length => length;
        protected override IEnumerable<byte> GetFieldBytes()
        {
            for (int i = 0; i < length; i++)
            {
                if (initializationBuffer != null && i < initializationBuffer.Length)
                {
                    yield return initializationBuffer[i];
                }
                yield return 0;
            }
        }
        protected override byte[] GetPropertyValue(byte[] messageBytes)
        {
            byte[] result = new byte[Length];
            Array.Copy(messageBytes, Offset, result, 0, Length);
            return result;
        }
    }
}
