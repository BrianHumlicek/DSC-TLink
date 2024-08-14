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

using DSC.TLink.Extensions;

namespace DSC.TLink
{
    public class TLinkSessionState
	{
		private TLinkSessionState() { }
		public ushort DeviceType { get; private set; }
		public byte SoftwareVersion { get; private set; }
		public byte SoftwareRevision { get; private set; }
		public ushort LanguageID { get; private set; }
		public byte[] DeviceID { get; private set; }
		//Status byte 0
		public bool CallbackEnabled { get; private set; }
		public bool EventBuffer75PercentFull { get; private set; }
		public bool FirstTimeUploadCall { get; private set; }
		public bool MaintenanceCall { get; private set; }
		public bool AutoMaintenanceCall { get; private set; }
		public bool PeriodicCall { get; private set; }
		public bool OnlinePCLink { get; private set; }
		public bool Insert15FFs { get; private set; }
		//Status byte 1
		public bool EnableDevice { get; private set; }
		public bool DisableDevice { get; private set; }
		public bool DeviceProgrammedSinceLastUpload { get; private set; }
		public bool FlashUpgradeCall { get; private set; }
		public bool SirenEnabled { get; private set; }
		public bool CommercialFire { get; private set; }
		public bool DynamicPasswordRequest { get; private set; }
		public bool TwoByteCommandLength { get; private set; }
		//Status byte 2
		public bool ConnectionTest { get; private set; }
		public bool CardDataChanged { get; private set; }
		public bool ParameterDataChanged { get; private set; }
		public bool FirmwareUpdated { get; private set; }
		public bool Encrypted { get; private set; }
		public bool GSMCommunication { get; private set; }
		public bool SessionAlreadyActive { get; private set; }
		//bit7 undefined
		//Status byte 3
		public bool GSMPluginPresent { get; private set; }
		public bool IPPluginPresent { get; private set; }
		//bits 2-7 undefined
		//Additional info
		public byte MarketID { get; private set; }
		public byte ApprovalID { get; private set; }
		public byte CustomerID { get; private set; }
		public ushort SequenceNumber { get; private set; }
		public List<bool> ServiceRequest { get; private set; }
		public byte TestVersion { get; private set; }
		public byte TestRevision { get; private set; }
		public byte[] KeyID { get; private set; }
		public byte[] CommunicatorVersion { get; private set; }

		public static TLinkSessionState ParseConnectionPayload(IList<byte> payload)
		{
			if (payload.Count < 8) throw new Exception();

			TLinkSessionState result = new TLinkSessionState();

			result.DeviceType = payload.PopLeadingWord();
			result.SoftwareVersion = payload.PopLeadingByte();
			result.SoftwareRevision = payload.PopLeadingByte();
			result.LanguageID = payload.PopLeadingWord();

			int length = payload.PopLeadingByte();
			if (payload.Count < length) throw new Exception();
			result.DeviceID = payload.PopLeadingBytes(length).ToArray();

			if (!parseStatusBytes(payload, result))         return result;
			if (!parseVariantData(payload, result))         return result;
			if (!parseSequence(payload, result))            return result;
			if (!parseServiceRequestData(payload, result))  return result;
			if (!parseBuildNumber(payload, result))         return result;
			if (!parseKeyID(payload, result))               return result;
			if (!parseAdditionalInfo(payload, result))      return result;
			if (!parseCommunicatorVersion(payload, result)) return result;
			return result;
		}

		static bool parseStatusBytes(IList<byte> payload, TLinkSessionState sessionState)
		{
			if (payload.Count < 2) return false;

			int numberOfStatusBytes = payload.PopLeadingByte();
			if (payload.Count < numberOfStatusBytes) throw new Exception();

			byte statusByte = payload.PopLeadingByte();

			sessionState.CallbackEnabled          = statusByte.Bit0();
			sessionState.EventBuffer75PercentFull = statusByte.Bit1();
			sessionState.FirstTimeUploadCall      = statusByte.Bit2();
			sessionState.MaintenanceCall          = statusByte.Bit3();
			sessionState.AutoMaintenanceCall      = statusByte.Bit4();
			sessionState.PeriodicCall             = statusByte.Bit5();
			sessionState.OnlinePCLink             = statusByte.Bit6();
			sessionState.Insert15FFs              = statusByte.Bit7();

			if (numberOfStatusBytes < 2) return true;

			statusByte = payload.PopLeadingByte();

			sessionState.EnableDevice                    = statusByte.Bit0();
			sessionState.DisableDevice                   = statusByte.Bit1();
			sessionState.DeviceProgrammedSinceLastUpload = statusByte.Bit2();
			sessionState.FlashUpgradeCall                = statusByte.Bit3();
			sessionState.SirenEnabled                    = statusByte.Bit4();
			sessionState.CommercialFire                  = statusByte.Bit5();
			sessionState.DynamicPasswordRequest          = statusByte.Bit6();
			sessionState.TwoByteCommandLength            = statusByte.Bit7();

			if (numberOfStatusBytes < 3) return true;

			statusByte = payload.PopLeadingByte();

			sessionState.ConnectionTest       = statusByte.Bit0();
			sessionState.CardDataChanged      = statusByte.Bit1();
			sessionState.ParameterDataChanged = statusByte.Bit2();
			sessionState.FirmwareUpdated      = statusByte.Bit3();
			sessionState.Encrypted            = statusByte.Bit4();
			sessionState.GSMCommunication     = statusByte.Bit5();
			sessionState.SessionAlreadyActive = statusByte.Bit6();

			if (numberOfStatusBytes < 4) return true;

			statusByte = payload.PopLeadingByte();

			sessionState.GSMPluginPresent = statusByte.Bit0();
			sessionState.IPPluginPresent  = statusByte.Bit1();

			return true;
		}
		static bool parseVariantData(IList<byte> payload, TLinkSessionState sessionState)
		{
			if (payload.Count < 2) return false;
			int length = payload.PopLeadingByte();
			if (payload.Count < length) throw new Exception();
			IList<byte> block = payload.PopLeadingBytes(length);
			if (length >= 3)
			{
                sessionState.MarketID   = block.PopLeadingByte();
                sessionState.ApprovalID = block.PopLeadingByte();
                sessionState.CustomerID = block.PopLeadingByte();
            }
            return true;
		}
		static bool parseSequence(IList<byte> payload, TLinkSessionState sessionState)
		{
			if (payload.Count < 2) return false;
			int length = payload.PopLeadingByte();
			
			if (payload.Count < length) throw new Exception();

			if (length == 1)
			{
				sessionState.SequenceNumber = payload.PopLeadingByte();
			}
			else if (length > 1)
			{
				sessionState.SequenceNumber = payload.PopLeadingWord();
			}
			return true;
		}
		static bool parseServiceRequestData(IList<byte> payload, TLinkSessionState sessionState)
		{
			if (payload.Count < 2) return false;
			int length = payload.PopLeadingByte();

			if (payload.Count < length) throw new Exception();

			IList<byte> block = payload.PopLeadingBytes(length);
            sessionState.ServiceRequest = new List<bool>();
			foreach(byte b in block)
			{
                sessionState.ServiceRequest.Add(b.Bit7());
                sessionState.ServiceRequest.Add(b.Bit6());
                sessionState.ServiceRequest.Add(b.Bit5());
                sessionState.ServiceRequest.Add(b.Bit4());
                sessionState.ServiceRequest.Add(b.Bit3());
                sessionState.ServiceRequest.Add(b.Bit2());
                sessionState.ServiceRequest.Add(b.Bit1());
                sessionState.ServiceRequest.Add(b.Bit0());
				if (sessionState.ServiceRequest.Count >= 32) break;
            }
            return true;
		}
		static bool parseBuildNumber(IList<byte> payload, TLinkSessionState sessionState)
		{
			if (payload.Count < 2) return false;
			int length = payload.PopLeadingByte();

			if (payload.Count < length) throw new Exception();

			IList<byte> block = payload.PopLeadingBytes(length);

			if (block.Count > 0)
			{
				sessionState.TestVersion = block.PopLeadingByte();
			}
			if (block.Count > 0)
			{
				sessionState.TestRevision = block.PopLeadingByte();
			}

			return true;
		}
		static bool parseKeyID(IList<byte> payload, TLinkSessionState sessionState)
		{
			if (payload.Count < 2) return false;
			int length = payload.PopLeadingByte();

			if (payload.Count < length) throw new Exception();

			sessionState.KeyID = payload.PopLeadingBytes(length).ToArray();

			return true;
		}
		static bool parseAdditionalInfo(IList<byte> payload, TLinkSessionState sessionState)
		{
			if (payload.Count < 2) return false;
			int length = payload.PopLeadingByte();

			if (payload.Count < length) throw new Exception();

			IList<byte> block = payload.PopLeadingBytes(length);
			//Implementation
			return true;
		}
		static bool parseCommunicatorVersion(IList<byte> payload, TLinkSessionState sessionState)
		{
			if (payload.Count < 2) return false;
			int length = payload.PopLeadingByte();

			if (payload.Count < length) throw new Exception();

			sessionState.CommunicatorVersion = payload.PopLeadingBytes(length).ToArray();

			return true;
		}
	}
}