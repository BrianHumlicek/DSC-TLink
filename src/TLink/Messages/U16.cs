using DSC.TLink.Extensions;

namespace DSC.TLink.Messages
{
    internal class U16 : BinaryMessage.FieldMetadata<ushort>
    {
        public override int Length => 2;
        protected override IEnumerable<byte> GetFieldBytes() => initializationBuffer.ToBigEndianEnumerable();
        protected override ushort GetPropertyValue(byte[] messageBytes) => (ushort)(messageBytes[Offset] << 8 | messageBytes[Offset+1]);
    }
}
