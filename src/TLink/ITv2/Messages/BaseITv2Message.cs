using DSC.TLink.Messages;

namespace DSC.TLink.ITv2.Messages
{
    internal abstract class BaseITv2Message : BinaryMessage
    {
        protected BaseITv2Message(byte[]? messageBytes) : base(messageBytes) { }
        protected override void OnInitializing()
        {
            DefineField(new U8(),  nameof(Length));
            DefineField(new U16(), nameof(Type));
            DefineField(new U16(), nameof(Command));
            DefineField(new U8(),  nameof(Sequence));
        }
        public new byte Length
        {
            get => GetProperty<byte>();
            set => SetProperty(value);
        }
        public ushort @Type
        {
            get => GetProperty<ushort>();
            set => SetProperty(value);
        }
        public ITv2Command Command
        {
            get => (ITv2Command)GetProperty<ushort>();
            set => SetProperty(value);
        }
        public byte Sequence
        {
            get => GetProperty<byte>();
            set => SetProperty(value);
        }
    }
}
