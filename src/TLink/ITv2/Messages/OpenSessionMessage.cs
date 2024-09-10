using DSC.TLink.Messages;

namespace DSC.TLink.ITv2.Messages
{
    internal class OpenSessionMessage : BaseITv2Message
    {
        public OpenSessionMessage(byte[]? messageBytes = default) : base(messageBytes) { }
        protected override void OnInitializing()
        {
            base.OnInitializing();
            DefineField(new U8(), nameof(DeviceType));
            DefineField(new FixedArray(length:2), nameof(DeviceID));
            DefineField(new FixedArray(length: 2), nameof(FirmwareVersion));
            DefineField(new FixedArray(length: 2), nameof(ProtocolVersion));
            DefineField(new U16(), nameof(TxBufferSize));
            DefineField(new U16(), nameof(RxBufferSize));
            DefineField(new FixedArray(length: 2), nameof(Unknown));
            DefineField(new U8(), nameof(EncryptionType));
            DefineField(new U16(), nameof(CRC));
        }
        public byte DeviceType
        {
            get => GetProperty<byte>();
            init => SetProperty(value);
        }
        public byte[] DeviceID
        {
            get => GetProperty<byte[]>();
            init => SetProperty(value);
        }
        public byte[] FirmwareVersion
        {
            get => GetProperty<byte[]>();
            init => SetProperty(value);
        }
        public byte[] ProtocolVersion
        {
            get => GetProperty<byte[]>();
            init => SetProperty(value);
        }
        public ushort TxBufferSize
        {
            get => GetProperty<ushort>();
            init => SetProperty(value);
        }
        public ushort RxBufferSize
        {
            get => GetProperty<ushort>();
            init => SetProperty(value);
        }
        public byte[] Unknown   //No clue what this is but setting it to 0x00, 0x01 seems to be the thing to do when sending a message.
        {
            get => GetProperty<byte[]>();
            set => SetProperty(value);
        }
        public EncryptionType EncryptionType
        {
            get => (EncryptionType)GetProperty<byte>();
            init => SetProperty(value);
        }
        public ushort CRC
        {
            get => GetProperty<ushort>();
            set => SetProperty(value);
        }
    }
}
