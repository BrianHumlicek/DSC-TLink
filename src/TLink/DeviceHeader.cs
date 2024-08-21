//  Copyright (C) 2024 Brian Humlicek

//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.

//  You should have received a copy of the GNU General Public License
//	along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace DSC.TLink
{
	public class DeviceHeader
	{
		protected DeviceHeader() { }
		public ushort DeviceType { get; protected set; }
		public byte SoftwareVersion { get; protected set; }
		public byte SoftwareRevision { get; protected set; }
		public ushort LanguageID { get; protected set; }
		public byte[] DeviceID { get; protected set; }
		//Status byte 0
		public bool CallbackEnabled { get; protected set; }
		public bool EventBuffer75PercentFull { get; protected set; }
		public bool FirstTimeUploadCall { get; protected set; }
		public bool MaintenanceCall { get; protected set; }
		public bool AutoMaintenanceCall { get; protected set; }
		public bool PeriodicCall { get; protected set; }
		public bool OnlinePCLink { get; protected set; }
		public bool Insert15FFs { get; protected set; }
		//Status byte 1
		public bool EnableDevice { get; protected set; }
		public bool DisableDevice { get; protected set; }
		public bool DeviceProgrammedSinceLastUpload { get; protected set; }
		public bool FlashUpgradeCall { get; protected set; }
		public bool SirenEnabled { get; protected set; }
		public bool CommercialFire { get; protected set; }
		public bool DynamicPasswordRequest { get; protected set; }
		public bool TwoByteCommandLength { get; protected set; }
		//Status byte 2
		public bool ConnectionTest { get; protected set; }
		public bool CardDataChanged { get; protected set; }
		public bool ParameterDataChanged { get; protected set; }
		public bool FirmwareUpdated { get; protected set; }
		public bool Encrypted { get; protected set; }
		public bool GSMCommunication { get; protected set; }
		public bool SessionAlreadyActive { get; protected set; }
		//bit7 undefined
		//Status byte 3
		public bool GSMPluginPresent { get; protected set; }
		public bool IPPluginPresent { get; protected set; }
		//bits 2-7 undefined
		//Additional info
		public byte MarketID { get; protected set; }
		public byte ApprovalID { get; protected set; }
		public byte CustomerID { get; protected set; }
		public ushort SequenceNumber { get; protected set; }
		public List<bool> ServiceRequest { get; protected set; }
		public byte TestVersion { get; protected set; }
		public byte TestRevision { get; protected set; }
		public byte[] KeyID { get; protected set; }
		public byte[] CommunicatorVersion { get; protected set; }
	}
}