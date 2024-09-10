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

namespace DSC.TLink.DLSProNet
{
    public class DLSProNetHeader : DeviceHeader
    {
        public static DeviceHeader ParseInitialHeader(List<byte> packetBytes)
        {
            (List<byte> header, List<byte> payload) = parseFraming(packetBytes);

            if (payload.Count < 15) throw new ArgumentException("Parsed payload length is too short");

            int encodedCRC = payload.PopTrailingWord();

            if (encodedCRC != CalculateCRC(payload)) throw new ArgumentException("Parsed header CRC is invalid");

            payload.RemoveRange(0, 5);  //The start delimiter and length bytes are part of the CRC so this is the soonest they can be removed.

            return ParseDeviceHeader(payload);
        }

        public static ushort CalculateCRC(IEnumerable<byte> data)
        {
            ushort crc = 0;
            foreach (byte @byte in data)
            {
                byte workingByte = @byte;
                for (int index2 = 0; index2 < 8; ++index2)
                {
                    bool flag = ((workingByte ^ crc) & 1) == 1;
                    crc >>= 1;
                    if (flag)
                    {
                        crc ^= 0xD003;
                    }
                    workingByte = (byte)((workingByte & 1) << 7 | workingByte >> 1);
                }
            }
            return crc;
        }

        static (List<byte>, List<byte>) parseFraming(IEnumerable<byte> packetBytes)
        {
            List<byte> header = new List<byte>();   //The header is everything before the sequence 0x05-0x05-0x05 that is followed by a byte that is NOT 0x05.
            List<byte> payload = new List<byte>();  //The payload is 0x05-0x05-0x05 followed by a length WORD, and (n) number of bytes where n equals length.
            List<byte> footer = new List<byte>();   //The footer is everything after the payload.
            List<byte> workingList = header;
            int delimiterCount = 0;
            int remainingPayloadBytes = -1;

            using (var enumerator = packetBytes.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    if (workingList == header)
                    {
                        if (enumerator.Current == 0x05)
                        {
                            if (delimiterCount < 3)
                            {
                                delimiterCount++;
                                continue;
                            }
                        }
                        else if (delimiterCount == 3)
                        {
                            //I think its odd that the payload includes the delimiter as well as the length bytes, but it does.
                            payload.AddRange(Enumerable.Repeat((byte)0x05, 3));
                            payload.Add(enumerator.Current);
                            if (!enumerator.MoveNext()) throw new Exception();
                            payload.Add(enumerator.Current);
                            remainingPayloadBytes = payload.PeekTrailingWord();
                            workingList = payload;
                            continue;
                        }
                        else if (delimiterCount > 0)
                        {
                            header.AddRange(Enumerable.Repeat((byte)0x05, delimiterCount));
                            delimiterCount = 0;
                        }
                    }
                    else if (remainingPayloadBytes == 0)
                    {
                        workingList = footer;
                    }
                    workingList.Add(enumerator.Current);
                    remainingPayloadBytes--;
                }

            if (delimiterCount != 3) throw new Exception();
            if (remainingPayloadBytes > 0) throw new Exception();

            return (header, payload);
        }
        static DLSProNetHeader ParseDeviceHeader(IList<byte> payload)
        {
            if (payload.Count < 8) throw new Exception();

            DLSProNetHeader deviceHeader = new DLSProNetHeader();

            deviceHeader.DeviceType = payload.PopLeadingWord();
            deviceHeader.SoftwareVersion = payload.PopLeadingByte();
            deviceHeader.SoftwareRevision = payload.PopLeadingByte();
            deviceHeader.LanguageID = payload.PopLeadingWord();

            int length = payload.PopLeadingByte();
            if (payload.Count < length) throw new Exception();
            deviceHeader.DeviceID = payload.PopLeadingBytes(length).ToArray();

            if (!parseStatusBytes(payload, deviceHeader)) return deviceHeader;
            if (!parseVariantData(payload, deviceHeader)) return deviceHeader;
            if (!parseSequence(payload, deviceHeader)) return deviceHeader;
            if (!parseServiceRequestData(payload, deviceHeader)) return deviceHeader;
            if (!parseBuildNumber(payload, deviceHeader)) return deviceHeader;
            if (!parseKeyID(payload, deviceHeader)) return deviceHeader;
            if (!parseAdditionalInfo(payload, deviceHeader)) return deviceHeader;
            if (!parseCommunicatorVersion(payload, deviceHeader)) return deviceHeader;
            return deviceHeader;
        }
        static bool parseStatusBytes(IList<byte> payload, DLSProNetHeader deviceHeader)
        {
            if (payload.Count < 2) return false;

            int numberOfStatusBytes = payload.PopLeadingByte();
            if (payload.Count < numberOfStatusBytes) throw new Exception();

            byte statusByte = payload.PopLeadingByte();

            deviceHeader.CallbackEnabled = statusByte.Bit0();
            deviceHeader.EventBuffer75PercentFull = statusByte.Bit1();
            deviceHeader.FirstTimeUploadCall = statusByte.Bit2();
            deviceHeader.MaintenanceCall = statusByte.Bit3();
            deviceHeader.AutoMaintenanceCall = statusByte.Bit4();
            deviceHeader.PeriodicCall = statusByte.Bit5();
            deviceHeader.OnlinePCLink = statusByte.Bit6();
            deviceHeader.Insert15FFs = statusByte.Bit7();

            if (numberOfStatusBytes < 2) return true;

            statusByte = payload.PopLeadingByte();

            deviceHeader.EnableDevice = statusByte.Bit0();
            deviceHeader.DisableDevice = statusByte.Bit1();
            deviceHeader.DeviceProgrammedSinceLastUpload = statusByte.Bit2();
            deviceHeader.FlashUpgradeCall = statusByte.Bit3();
            deviceHeader.SirenEnabled = statusByte.Bit4();
            deviceHeader.CommercialFire = statusByte.Bit5();
            deviceHeader.DynamicPasswordRequest = statusByte.Bit6();
            deviceHeader.TwoByteCommandLength = statusByte.Bit7();

            if (numberOfStatusBytes < 3) return true;

            statusByte = payload.PopLeadingByte();

            deviceHeader.ConnectionTest = statusByte.Bit0();
            deviceHeader.CardDataChanged = statusByte.Bit1();
            deviceHeader.ParameterDataChanged = statusByte.Bit2();
            deviceHeader.FirmwareUpdated = statusByte.Bit3();
            deviceHeader.Encrypted = statusByte.Bit4();
            deviceHeader.GSMCommunication = statusByte.Bit5();
            deviceHeader.SessionAlreadyActive = statusByte.Bit6();

            if (numberOfStatusBytes < 4) return true;

            statusByte = payload.PopLeadingByte();

            deviceHeader.GSMPluginPresent = statusByte.Bit0();
            deviceHeader.IPPluginPresent = statusByte.Bit1();

            return true;
        }
        static bool parseVariantData(IList<byte> payload, DLSProNetHeader deviceHeader)
        {
            if (payload.Count < 2) return false;
            int length = payload.PopLeadingByte();
            if (payload.Count < length) throw new Exception();
            IList<byte> block = payload.PopLeadingBytes(length);
            if (length >= 3)
            {
                deviceHeader.MarketID = block.PopLeadingByte();
                deviceHeader.ApprovalID = block.PopLeadingByte();
                deviceHeader.CustomerID = block.PopLeadingByte();
            }
            return true;
        }
        static bool parseSequence(IList<byte> payload, DLSProNetHeader deviceHeader)
        {
            if (payload.Count < 2) return false;
            int length = payload.PopLeadingByte();

            if (payload.Count < length) throw new Exception();

            if (length == 1)
            {
                deviceHeader.SequenceNumber = payload.PopLeadingByte();
            }
            else if (length > 1)
            {
                deviceHeader.SequenceNumber = payload.PopLeadingWord();
            }
            return true;
        }
        static bool parseServiceRequestData(IList<byte> payload, DLSProNetHeader deviceHeader)
        {
            if (payload.Count < 2) return false;
            int length = payload.PopLeadingByte();

            if (payload.Count < length) throw new Exception();

            IList<byte> block = payload.PopLeadingBytes(length);
            deviceHeader.ServiceRequest = new List<bool>();
            foreach (byte b in block)
            {
                deviceHeader.ServiceRequest.Add(b.Bit7());
                deviceHeader.ServiceRequest.Add(b.Bit6());
                deviceHeader.ServiceRequest.Add(b.Bit5());
                deviceHeader.ServiceRequest.Add(b.Bit4());
                deviceHeader.ServiceRequest.Add(b.Bit3());
                deviceHeader.ServiceRequest.Add(b.Bit2());
                deviceHeader.ServiceRequest.Add(b.Bit1());
                deviceHeader.ServiceRequest.Add(b.Bit0());
                if (deviceHeader.ServiceRequest.Count >= 32) break;
            }
            return true;
        }
        static bool parseBuildNumber(IList<byte> payload, DLSProNetHeader deviceHeader)
        {
            if (payload.Count < 2) return false;
            int length = payload.PopLeadingByte();

            if (payload.Count < length) throw new Exception();

            IList<byte> block = payload.PopLeadingBytes(length);

            if (block.Count > 0)
            {
                deviceHeader.TestVersion = block.PopLeadingByte();
            }
            if (block.Count > 0)
            {
                deviceHeader.TestRevision = block.PopLeadingByte();
            }

            return true;
        }
        static bool parseKeyID(IList<byte> payload, DLSProNetHeader deviceHeader)
        {
            if (payload.Count < 2) return false;
            int length = payload.PopLeadingByte();

            if (payload.Count < length) throw new Exception();

            deviceHeader.KeyID = payload.PopLeadingBytes(length).ToArray();

            return true;
        }
        static bool parseAdditionalInfo(IList<byte> payload, DLSProNetHeader deviceHeader)
        {
            if (payload.Count < 2) return false;
            int length = payload.PopLeadingByte();

            if (payload.Count < length) throw new Exception();

            IList<byte> block = payload.PopLeadingBytes(length);
            //Implementation
            return true;
        }
        static bool parseCommunicatorVersion(IList<byte> payload, DLSProNetHeader deviceHeader)
        {
            if (payload.Count < 2) return false;
            int length = payload.PopLeadingByte();

            if (payload.Count < length) throw new Exception();

            deviceHeader.CommunicatorVersion = payload.PopLeadingBytes(length).ToArray();

            return true;
        }
    }
}
