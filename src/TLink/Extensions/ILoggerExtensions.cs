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

using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Globalization;

namespace DSC.TLink.Extensions
{
	public static class ILoggerExtensions
	{
		//These are intended to be a temporary solution to logging the data that is passed as arrays and sequences.
		//Ideally, I think there should be some kind of logger that can handle these structures or allow a custom
		//output format.
		public static void LogDebug(this ILogger log, string message, ReadOnlySequence<byte> sequence)
		{
			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug(message, sequence.ToArray());
			}
		}
		public static void LogDebug(this ILogger log, string message, IEnumerable<byte> bytes)
		{
			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug(message, Enumerable2HexString(bytes));
			}
		}
		public static void LogTrace(this ILogger log, Func<string> message)
		{
			if (log.IsEnabled(LogLevel.Trace))
			{
				log.LogTrace(message());
			}
		}
		public static byte[] HexString2Array(string hexString) => hexString.Split('-').Select(s => byte.Parse(s, NumberStyles.HexNumber)).ToArray();
		public static string Enumerable2HexString(IEnumerable<byte> bytes) => String.Join('-', bytes.Select(b => $"{b:X2}"));
	}
}