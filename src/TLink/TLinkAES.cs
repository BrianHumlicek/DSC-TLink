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

using System.Security.Cryptography;

namespace DSC.TLink
{
	internal class TLinkAES : IDisposable
	{
		SymmetricAlgorithm localAlgorithm;
		SymmetricAlgorithm remoteAlgorithm;

		public TLinkAES()
		{
			localAlgorithm = createSymetricAlgorithm();
			remoteAlgorithm = createSymetricAlgorithm();
		}
		SymmetricAlgorithm createSymetricAlgorithm()
		{
			SymmetricAlgorithm algorithm = Aes.Create();
			algorithm.Mode = CipherMode.ECB;
			algorithm.Padding = PaddingMode.Zeros;
			return algorithm;
		}
		public byte[] LocalKey { set { localAlgorithm.Key = value; } }
		public byte[] RemoteKey { set { remoteAlgorithm.Key = value; } }
		public byte[] EncryptLocal(byte[] plainText) => encrypt(plainText, localAlgorithm);
		public byte[] DecryptLocal(ReadOnlySpan<byte> cipherText) => decrypt(cipherText, localAlgorithm);
		public byte[] EncryptRemote(byte[] plainText) => encrypt(plainText, remoteAlgorithm);
		public byte[] DecryptRemote(byte[] cipherText) => decrypt(cipherText, remoteAlgorithm);
		byte[] decrypt(ReadOnlySpan<byte> cipherText, SymmetricAlgorithm algorithm)
		{
			byte[] plainText = algorithm.DecryptEcb(cipherText, PaddingMode.Zeros);

			return plainText;
		}
		byte[] encrypt(byte[] plainText, SymmetricAlgorithm algorithm)
		{
			//plainText = plainText.Pad16().ToArray();

			byte[] cipherText = new byte[plainText.Length];

			using (ICryptoTransform encryptor = algorithm.CreateEncryptor())
			{
				encryptor.TransformBlock(plainText, 0, plainText.Length, cipherText, 0);
			}

			return cipherText;
		}
		public void Dispose()
		{
			localAlgorithm.Dispose();
			remoteAlgorithm.Dispose();
		}
	}
}