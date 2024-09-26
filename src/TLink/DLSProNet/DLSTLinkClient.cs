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
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.IO.Pipelines;
using System.Security.Cryptography;

namespace DSC.TLink.DLSProNet
{
	internal class DLSTLinkClient : TLinkClient, IDisposable
	{
		Aes AES = Aes.Create();
		bool aesActive = false;
		public DLSTLinkClient(IDuplexPipe pipe, ILogger<TLinkClient> log) : base(pipe, log)
		{
		}

		public void EnableAES(byte[] key)
		{
			AES.Key = key;
			aesActive = true;
		}
		public void DisableAES() => aesActive = false;
		protected override async Task sendPacket(byte[] packet)
		{
			if (aesActive)
			{
				packet = AES.EncryptEcb(packet, PaddingMode.Zeros);
			}
			ushort lengthWord = (ushort)packet.Length;

			byte[] lengthBytes = [ lengthWord.HighByte(), lengthWord.LowByte() ];

			packet = lengthBytes.Concat(packet).ToArray();

			await base.sendPacket(packet);
		}

		protected override (byte[] header, byte[] payload) parseTLinkFrame(ReadOnlySequence<byte> packetSequence)
		{
			if (aesActive)
			{
				ReadOnlySpan<byte> cipherText = packetSequence.IsSingleSegment ? packetSequence.FirstSpan : packetSequence.ToArray();
				byte[] plainText = AES.DecryptEcb(cipherText, PaddingMode.Zeros);
				packetSequence = new ReadOnlySequence<byte>(plainText);
				//log?.LogTrace(() => $"Unencrypted raw message '{Array2HexString(packet)}'");
			}
			return base.parseTLinkFrame(packetSequence);
		}
		//DLS packets are wrapped in a length encoded array
		protected override bool tryGetPacketSlice(ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> packetSlice)
		{
			SequenceReader<byte> sequenceReader = new SequenceReader<byte>(buffer);
			short encodedLength;    //This is probably technically a ushort, but there isnt a bcl extension that uses that type so unless I make one...
			if (!sequenceReader.TryReadBigEndian(out encodedLength) || sequenceReader.Length < encodedLength + 2)
			{
				packetSlice = default;
				return false;
			}

			packetSlice = sequenceReader.Sequence.Slice(2, encodedLength);

			return base.tryGetPacketSlice(packetSlice, out packetSlice);
		}

		public void Dispose()
		{
			AES?.Dispose();
		}
	}
}
