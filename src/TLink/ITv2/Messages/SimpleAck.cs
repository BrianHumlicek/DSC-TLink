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

using DSC.TLink.Messages.Extensions;
using DSC.TLink.Messages;

namespace DSC.TLink.ITv2.Messages
{
	internal record SimpleAck : NetworkByteMessage
	{
		protected override IAddRemoveFraming? framing => new ITv2MessageFraming();
		public byte HostSequence { get; set; }	//This is always zero
		public byte RemoteSequence { get; set; }
		protected override List<byte> buildByteList()
		{
			List<byte> result =
			[
				HostSequence,
				RemoteSequence
			];

			return result;
		}
		protected override ReadOnlySpan<byte> initialize(ReadOnlySpan<byte> workingBuffer)
		{
			workingBuffer.PopAndSetValue((value) => HostSequence = value);
			workingBuffer.PopAndSetValue((value) => RemoteSequence = value);

			return workingBuffer;
		}
	}
}
