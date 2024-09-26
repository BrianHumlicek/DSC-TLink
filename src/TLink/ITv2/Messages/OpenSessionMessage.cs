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
	internal record OpenSessionMessage : NetworkByteMessage
	{
		public OpenSessionMessage() { }
		public OpenSessionMessage(byte[] bytes)
		{
			Parse(bytes);
		}

		public Itv2PanelDeviceType DeviceType { get; set; }
		public byte[] DeviceID { get => deviceID.Get(); set => deviceID.Set(value); }
		readonly IArrayProperty deviceID = new FixedArrayProperty(length: 2);
		public byte[] FirmwareVersion { get => firmwareVersion.Get(); set => firmwareVersion.Set(value); }
		readonly IArrayProperty firmwareVersion = new FixedArrayProperty(length: 2);
		public byte[] ProtocolVersion { get => protocolVersion.Get(); set => protocolVersion.Set(value); }
		readonly IArrayProperty protocolVersion = new FixedArrayProperty(length: 2);
		public ushort TxBufferSize { get; set; }
		public ushort RxBufferSize { get; set; }
		public byte[] Unknown { get => unknown.Get(); set => unknown.Set(value); }   //No clue what this is but setting it to 0x00, 0x01 seems to be the thing to do when sending a message,.
		readonly IArrayProperty unknown = new FixedArrayProperty(length: 2);
		public EncryptionType EncryptionType { get; set; }

		protected override List<byte> buildByteList() =>
			[(byte)DeviceType,
			.. DeviceID,
			.. FirmwareVersion,
			.. ProtocolVersion,
			.. TxBufferSize.ToArray(),
			.. RxBufferSize.ToArray(),
			.. Unknown,
			(byte)EncryptionType];
		protected override ReadOnlySpan<byte> initialize(ReadOnlySpan<byte> bytes)
		{
			bytes.PopAndSetValue((byte value) => DeviceType = (Itv2PanelDeviceType)value);
			bytes.PopAndSetValue(deviceID);
			bytes.PopAndSetValue(firmwareVersion);
			bytes.PopAndSetValue(protocolVersion);
			bytes.PopAndSetValue((ushort value) => TxBufferSize = value);
			bytes.PopAndSetValue((ushort value) => RxBufferSize = value);
			bytes.PopAndSetValue(unknown);
			bytes.PopAndSetValue((byte value) => EncryptionType = (EncryptionType)value);
			return bytes;
		}

		//Calculated properties
		public int FirmwareVersionNumber => FirmwareVersion[0] << 4 | FirmwareVersion[1] >> 4;
		public int FirmwareRevisionNumber => FirmwareVersion[1] & 0x0F;
	}
}