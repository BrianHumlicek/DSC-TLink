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

namespace DSC.TLink
{
	public class TextFileLoggerProvider : ILoggerProvider
	{
		string fileName;

		public TextFileLoggerProvider(string fileName = "demolog.txt")
		{
			this.fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new TextFileLogger(categoryName, fileName);
		}

		public void Dispose()
		{
		}
	}

	// Customized ILogger, writes logs to text files
	public class TextFileLogger : ILogger
	{
		private readonly string _categoryName;
		string fileName;

		public TextFileLogger(string categoryName, string fileName)
		{
			_categoryName = categoryName;
			this.fileName = fileName;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			// Ensure that only information level and higher logs are recorded
			return logLevel >= LogLevel.Debug;
		}

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception exception,
			Func<TState, Exception, string> formatter)
		{
			// Ensure that only information level and higher logs are recorded
			if (!IsEnabled(logLevel))
			{
				return;
			}

			// Get the formatted log message
			var message = formatter(state, exception);

			//Write log messages to text file
			File.AppendAllText(fileName, $"[{DateTime.Now}][{logLevel}] [{_categoryName}] {message}\n");
		}
	}
}
