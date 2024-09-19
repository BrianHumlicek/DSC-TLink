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

using DSC.TLink.ITv2.Enumerations;
using DSC.TLink.Messages;
using DSC.TLink.Messages.Extensions;

namespace DSC.TLink.ITv2.Messages
{
	internal record ITv2CommandHeader2 : NetworkByteMessage
	{
		protected override IAddRemoveFraming? framing => new ITv2MessageFraming();

		//This value is incremented by one with every message from this server when sending
		//When receiving, this value sets the DeviceSequenceNumber
		//ITV2Transport calls this devSeq and appSeq
		public byte HostSequence { get; set; }

		//This is the DeviceSequenceNumber when sending
		//When receiving, this needs to match ResponseSequenceNumber which is the value that was sent in the HostSequence property in the prior message
		//ITV2Transport calls this appSeq and devSeq
		public byte RemoteSequence { get;set; }
		public ITv2Command? Command { get; set; }
		public byte AppSequence { get; set; }
		public byte[]? CommandData { get; set; }
		IFormattedArray commandDataFormatter = new FixedArrayFormatter();
		protected override List<byte> buildByteList()
		{
			List<byte> result = new List<byte>();
			result.Add(HostSequence);
			result.Add(RemoteSequence);

			if (!result.TryAdd((ushort?)Command)) return result;

			result.Add(AppSequence);
			result.AddRange(CommandData);
			return result;
		}
		protected override ReadOnlySpan<byte> initialize(ReadOnlySpan<byte> workingBuffer)
		{
			workingBuffer.PopAndSetValue((value) => HostSequence = value);
			workingBuffer.PopAndSetValue((value) => RemoteSequence = value);

			if (!workingBuffer.TryPopAndSetValue((ushort value) => Command = (ITv2Command)value)) return workingBuffer;

			workingBuffer.PopAndSetValue((value) => AppSequence = value);
			workingBuffer.PopAndSetValue((value) => CommandData = value);

			return workingBuffer;
		}
	}
}