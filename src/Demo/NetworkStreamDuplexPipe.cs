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

using System.IO.Pipelines;
using System.Net.Sockets;

namespace DSC.TLink.Demo
{
	public class NetworkStreamDuplexPipe : IDuplexPipe
	{
		public NetworkStreamDuplexPipe(NetworkStream networkStream)
		{
			Input = PipeReader.Create(networkStream);
			Output = PipeWriter.Create(networkStream);
		}
		public PipeReader Input { get; }
		public PipeWriter Output { get; }
	}
}
