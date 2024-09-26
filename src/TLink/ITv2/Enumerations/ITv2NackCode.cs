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

namespace DSC.TLink.ITv2.Enumerations
{
	internal enum ITv2NackCode : byte	//ITv2InstructionHelper CheckForNack
	{
		UnknownError,
		InvalidCommandLength = 1,
		InvalidCommand = 2,
		InvalidSequence = 3,
		PanelNotResponding = 4,
		InvalidPassthruCommand = 5,
		InvalidDestination = 6,
		InvalidSession = 7,
		InsufficientBufferSize = 8,
		LockOut = 9,
		UnsupportedSubCommand = 10,
		PowerupShunt = 11,
		InvalidProgrammingMode = 16,
		InvalidAccessCode = 17,
		AccessCodeRequired = 18,
		SystemBusy = 19,
		InvalidPartition = 20,
		FunctionNotAvailable = 23,
		InternalError = 24,
		CommandTimeout = 25,
		InvalidDeviceModule = 28,
		InvalidTroubleType = 29,
		ProgTagOrAccessCodeRequired = 30,
		ProgTagRequired = 31
	}
}
