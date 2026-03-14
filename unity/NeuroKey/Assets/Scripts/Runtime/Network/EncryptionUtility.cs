using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NeuroKey.Network
{
    public static class EncryptionUtility
    {
        private const int TagLengthBit = 128;
        private const int TagLengthByte = TagLengthBit / 8;
        private const int IvLengthByte = 12;

        private static byte[] DeriveKey(string password)
        {
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static byte[] EncryptLong(long value, string key)
        {
            byte[] bytes = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                bytes[i] = (byte)(value & 0xFF);
                value >>= 8;
            }
            return EncryptBytes(bytes, key);
        }

        public static long DecryptLong(byte[] encryptedData, string encryptionKey)
        {
            byte[] decryptedBytes = DecryptBytes(encryptedData, encryptionKey);
            if (decryptedBytes.Length != 8)
                throw new ArgumentException("Decrypted data length mismatch.");

            long value = 0;
            for (int i = 0; i < 8; i++)
            {
                value = (value << 8) | (decryptedBytes[i] & 0xFF);
            }
            return value;
        }

        public static byte[] EncryptBytes(byte[] data, string encryptionKey)
        {
            byte[] key = DeriveKey(encryptionKey);
            byte[] iv = new byte[IvLengthByte];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }

            byte[] tag = new byte[TagLengthByte];
            byte[] cipherText = new byte[data.Length];

            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Encrypt(iv, data, cipherText, tag);
            }

            byte[] finalBuffer = new byte[IvLengthByte + cipherText.Length + TagLengthByte];
            Buffer.BlockCopy(iv, 0, finalBuffer, 0, IvLengthByte);
            Buffer.BlockCopy(cipherText, 0, finalBuffer, IvLengthByte, cipherText.Length);
            Buffer.BlockCopy(tag, 0, finalBuffer, IvLengthByte + cipherText.Length, TagLengthByte);

            return finalBuffer;
        }

        public static byte[] DecryptBytes(byte[] encryptedData, string encryptionKey)
        {
            byte[] key = DeriveKey(encryptionKey);
            byte[] iv = new byte[IvLengthByte];
            Buffer.BlockCopy(encryptedData, 0, iv, 0, IvLengthByte);

            int cipherTextLength = encryptedData.Length - IvLengthByte - TagLengthByte;
            byte[] cipherText = new byte[cipherTextLength];
            byte[] tag = new byte[TagLengthByte];

            Buffer.BlockCopy(encryptedData, IvLengthByte, cipherText, 0, cipherTextLength);
            Buffer.BlockCopy(encryptedData, IvLengthByte + cipherTextLength, tag, 0, TagLengthByte);

            byte[] decryptedData = new byte[cipherTextLength];

            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Decrypt(iv, cipherText, tag, decryptedData);
            }

            return decryptedData;
        }
    }
}
