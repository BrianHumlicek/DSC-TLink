// DSC TLink - a communications library for DSC Powerseries NEO alarm panels
// Copyright (C) 2024 Brian Humlicek
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using DSC.TLink.Messages;

namespace DSC.TLink.ITv2.Messages
{
	internal abstract class BaseITv2Message : BinaryMessage
	{
		protected BaseITv2Message(byte[]? messageBytes) : base(messageBytes) { }
		protected override void OnInitializing()
		{
			SetFraming(new LeadLengthTrailCRCFraming());
			DefineField(new U8(), nameof(HostSequence));
			DefineField(new U8(), nameof(RemoteSequence));
			DefineField(new U16(), nameof(Command));
			DefineField(new U8(), nameof(AppSequence));
		}
		public byte HostSequence
		{
			get => GetProperty<byte>();
			set => SetProperty(value);
		}
		public byte RemoteSequence
		{
			get => GetProperty<byte>();
			set => SetProperty(value);
		}
		public ITv2Command Command
		{
			get => (ITv2Command)GetProperty<ushort>();
			set => SetProperty((ushort)value);
		}
		public byte AppSequence
		{
			get => GetProperty<byte>();
			set => SetProperty(value);
		}
	}
}