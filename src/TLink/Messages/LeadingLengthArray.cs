namespace DSC.TLink.Messages
{
    internal class LeadingLengthArray : BinaryMessage.FieldMetadata<byte[]>
    {
        byte? length;
        public override int Length => length ?? throw new Exception($"Length is not initialized!");
        protected override IEnumerable<byte> GetFieldBytes()
        {
            length = (byte)(initializationBuffer?.Length ?? throw new Exception($"{nameof(LeadingLengthArray)} was not initialized!"));
            foreach (var @byte in initializationBuffer)
            {
                yield return @byte;
            }
        }
        protected override byte[] GetPropertyValue(byte[] bytes)
        {
            throw new NotImplementedException();
        }
        protected override void EnsureLengthSet(byte[] messageBytes)
        {
            if (length == default)
            {
                length = messageBytes[Offset];
            }
        }
    }
}
