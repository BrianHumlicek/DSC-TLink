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
        public byte[] RemoteKey { set {  remoteAlgorithm.Key = value; } }
        public byte[] EncryptLocal(byte[] plainText) => encrypt(plainText, localAlgorithm);
        public byte[] DecryptLocal(byte[] cipherText) => decrypt(cipherText, localAlgorithm);
        public byte[] EncryptRemote(byte[] plainText) => encrypt(plainText, remoteAlgorithm);
        public byte[] DecryptRemote(byte[] cipherText) => decrypt(cipherText, remoteAlgorithm);
        byte[] decrypt(byte[] cipherText, SymmetricAlgorithm algorithm)
        {
            //cipherText = cipherText.Pad16().ToArray();

            byte[] plainText = new byte[cipherText.Length];

            using (ICryptoTransform decryptor = algorithm.CreateDecryptor())
            {
                decryptor.TransformBlock(cipherText, 0, cipherText.Length, plainText, 0);
            }

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
