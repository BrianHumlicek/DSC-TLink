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
	internal enum CommandResponseCode : byte
	{
		Success = 0,
		CanNotExitConfiguration = 1,
		IgnoreFirmware = 1,
		InvalidSectionRange = 1,
		CommandOutputNotDefined = 1,
		InvalidTestModeSpecified = 1,
		InvalidProgrammingType = 2,
		InvalidIndex = 2,
		InvalidData = 2,
		InvalidOutputNumber = 2,
		UnsupportedModule = 3,
		NoNewInformationInTheSpecifiedBuffer = 3,
		IncorrectProgrammingMode = 16,
		InvalidAccessCode = 17,
		AccessCodeRequired = 18,
		SystemPartitionBusy = 19,
		InvalidPartition = 20,
		FunctionNotAvailable = 23,
		InternalError = 24,
		EnabledLateToOpen = 1,
		InvalidType = 1,
		InvalidUnsupportedTestType = 2,
		DisabledLateToOpen = 2,
		WalkTestActive = 3,
		InvalidSignalType = 4,
		SomeOrAllPartitionFailedToArm = 4,
		NoTroublesPresentForRequestedType = 26,
		NoRequestedAlarmsFound = 27,
		InvalidDeviceModule = 28,
		InvalidTroubleType = 29,
		CommandTimeOut = 25
	}
}
