
namespace DSC.TLink.Messages
{
    internal class BoolBit : BinaryMessage.MultiPropertyFieldMetadata<bool>
    {
        int bitNumber;
        public BoolBit(int size, params KeyValuePair<string, int>[] bits)
        {
            this.bitNumber = bitNumber;
        }
        public override int Length => throw new NotImplementedException();

        protected override IEnumerable<byte> GetFieldBytes()
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> GetProperties()
        {
            throw new NotImplementedException();
        }

        protected override bool GetPropertyValue(byte[] messageBytes, string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}
