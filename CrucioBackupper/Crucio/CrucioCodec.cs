using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.IO.Compression;

namespace CrucioBackupper.Crucio
{
    public static class CrucioCodec
    {
        public static readonly byte[] Key = new byte[] { 0x60, 0x3D, 0xEB, 0x10, 0x15, 0xCA, 0x71, 0x35, 0xBE, 0x2B, 0x73, 0xAE, 0xF0, 0x85, 0x7D, 0x77, 0x07, 0x3B, 0x61, 0x08, 0xD7, 0x2D, 0x98, 0x10, 0x14, 0xDF, 0xF4, 0x2C, 0x81, 0x1F, 0xA3, 0x09 };
        public static readonly byte[] IV = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x0f, 0x07, 0x0b, 0x0c, 0x0d, 0x0e, 0x08, 0x09, 0x0a };

        private static readonly SymmetricAlgorithm cryptoProvider = new AesCryptoServiceProvider
        {
            BlockSize = 128,
            KeySize = 256,
            Key = Key,
            IV = IV,
            Padding = PaddingMode.PKCS7,
            Mode = CipherMode.CBC
        };

        public static Stream Decode(Stream raw)
        {
            return new GZipStream(DecodeWithoutGZip(raw), CompressionMode.Decompress);
        }

        public static Stream DecodeWithoutGZip(Stream raw)
        {
            return new CryptoStream(raw, cryptoProvider.CreateDecryptor(), CryptoStreamMode.Read);
        }
    }
}
