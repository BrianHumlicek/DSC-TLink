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

using DSC.TLink.Extensions;
using DSC.TLink.ITv2.Enumerations;
using DSC.TLink.Messages;
using DSC.TLink.Messages.Extensions;

namespace DSC.TLink.ITv2.Messages
{
	internal record ITv2Header : NetworkByteMessage
	{
		protected override IAddRemoveFraming? framing => new ITv2MessageFraming();
		public byte SenderSequence { get; set; }
		public byte ReceiverSequence { get;set; }
		public ITv2Command? Command { get; set; }
		public byte? AppSequence { get; set; }
		public byte[]? CommandData { get => commandData.Get(); set => commandData.Set(value); }
		INullableArrayProperty commandData = new UnboundedArray();
		protected override List<byte> buildByteList()
		{
			List<byte> result =
			[
				SenderSequence,
				ReceiverSequence
			];

			if (!Command.HasValue) return result;

			result.AddRange(((ushort)Command.Value).ToArray());

			if (!AppSequence.HasValue) return result;

			result.Add(AppSequence.Value);

			if (CommandData == null) return result;

			result.AddRange(CommandData);

			return result;
		}
		protected override ReadOnlySpan<byte> initialize(ReadOnlySpan<byte> workingBuffer)
		{
			workingBuffer.PopAndSetValue((value) => SenderSequence = value);
			workingBuffer.PopAndSetValue((value) => ReceiverSequence = value);

			if (!workingBuffer.TryPopAndSetValue((ushort value) => Command = (ITv2Command)value)) return workingBuffer;

			if (!workingBuffer.TryPopAndSetValue((byte value) => AppSequence = value)) return workingBuffer;

			workingBuffer.TryPopAndSetValue(commandData);

			return workingBuffer;
		}
	}
}