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

namespace DSC.TLink.DLSProNet
{
	internal enum DLSProNetCommand : ushort
	{
		// Assembly: Tyco.TSP.Communications.TLink, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8f1b6268456057e5
		// Type: Tyco.TSP.Communications.TLink.TLLegacyInstructionBuilder
		// Method BuildNOPInstruction
		NOP = 0xE006,
		ACK = 0x0004
	}
}