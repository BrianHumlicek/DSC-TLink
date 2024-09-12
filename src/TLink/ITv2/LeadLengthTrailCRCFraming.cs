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
using DSC.TLink.Messages;

namespace DSC.TLink.ITv2
{
	internal class LeadLengthTrailCRCFraming : BinaryMessage.IProcessFraming
	{
		public byte[] AddFraming(byte[] message)
		{
			List<byte> result = new List<byte>(message);
			byte length = (byte)(message.Length + 2);   //Length includes the CRC bytes that will be added later
			result.Insert(0, length);
			ushort crc = (ushort)calculateCRC(result);
			return result.Concat(crc.ToBigEndianEnumerable()).ToArray();
		}

		public byte[] RemoveFraming(byte[] message)
		{
			List<byte> result = new List<byte>(message);
			int encodedCRC = result.PopTrailingWord();
			//if (encodedCRC != calculateCRC(result)) throw new Exception("Framing CRC error");
			int encodedLength = result.PopLeadingByte();
			if (encodedLength != result.Count + 2) throw new Exception("Framing length mismatch");
			return result.ToArray();
		}

		int calculateCRC(IEnumerable<byte> message)
		{
			throw new NotImplementedException();
		}
	}
}
