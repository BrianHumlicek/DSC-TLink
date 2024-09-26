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
using DSC.TLink.Messages.Extensions;

namespace DSC.TLink.ITv2.Messages
{
	internal record RequestAccess : NetworkByteMessage
	{
		public byte[] Payload { get => payload.Get(); set => payload.Set(value); }
		readonly IArrayProperty payload = new LeadingLengthArray();
		protected override List<byte> buildByteList()
		{
			return Payload.ToList();
		}

		protected override ReadOnlySpan<byte> initialize(ReadOnlySpan<byte> bytes)
		{
			bytes.PopAndSetValue(payload);
			return bytes;
		}
	}
}
