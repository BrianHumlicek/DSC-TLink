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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace DSC.TLink.ITv2
{
	public class ITv2AES
	{

		byte[] LocalInitKey = [0x00, 0x03, 0x4F, 0x08,
					   0x00, 0x03, 0x4F, 0x08,
					   0x00, 0x03, 0x4F, 0x08,
					   0x00, 0x03, 0x4F, 0x08 ];

		//public byte[] GenerateKey(byte[] initializer)
		//{
		//	byte[] result = zipperMerge(key1, initializer).ToArray();

		//}
		IEnumerable<byte> evenIndexes(IEnumerable<byte> bytes) => bytes.Where((element, index) => index % 2 == 0);
		IEnumerable<byte> oddIndexes(IEnumerable<byte> bytes) => bytes.Where((element, index) => index % 2 == 1);
		IEnumerable<byte> evenOddZip(IEnumerable<byte> evenIndexes, IEnumerable<byte> oddIndexes) => evenIndexes.Zip(oddIndexes, (evenElement, oddElement) => new byte[] { evenElement, oddElement }).SelectMany(e => e);
		public byte[] GenerateType1Initializer(string integrationAccessCode, byte[] localKey)
		{
			byte[] zipped = evenOddZip(LocalInitKey, localKey).ToArray();

			byte[] cipherText;
			using (Aes aes = Aes.Create())
			{
				aes.Key = transformKeyString(integrationAccessCode);
				cipherText = aes.EncryptEcb(zipped, PaddingMode.Zeros);
			}

			//return Enumerable.Repeat((byte)0x01, 16).Concat(cipherText).ToArray();
			return LocalInitKey.Concat(cipherText).ToArray();
		}
		/// <summary>
		/// Calculate the remote AES key for Type 1 encryption
		/// </summary>
		/// <param name="integrationIdentificationNumber">12 digit Integration Identification Number [851][422]</param>
		/// <param name="remoteInitializer">The command payload sent by the panel with command 0x060E Connection_Request_Access</param>
		/// <returns>The AES key used to decrypt messages from the panel</returns>
		/// <exception cref="Exception"></exception>
		public byte[] ParseType1Initializer(string integrationIdentificationNumber, byte[] remoteInitializer)
		{
			var cipherText = remoteInitializer.Skip(16).Take(32).ToArray();

			byte[] plainText;
			using (var aes = Aes.Create())
			{
				aes.Key = transformKeyString(integrationIdentificationNumber);
				plainText = aes.DecryptEcb(cipherText, PaddingMode.Zeros);

			}

			if (!evenIndexes(plainText).SequenceEqual(remoteInitializer.Take(16))) throw new Exception("");

			return oddIndexes(plainText).ToArray();
		}

		public byte[] transformKeyString(string keyString)
		{
			string first8 = keyString.Substring(0, 8);
			//This makes a 32 digit base 10 string.  When read as base 16, it makes a 16 byte array.
			return Convert.FromHexString($"{first8}{first8}{first8}{first8}");
		}
	}
}
