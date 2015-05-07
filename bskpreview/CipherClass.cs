using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace bskpreview
{
    class CipherClass : ICipher
    {
        private byte[] tmpKey;
        private byte[] tmpIV;

        #region Public methods

        public byte[] EncryptFile(string filePath, int keySize, int blockSize, string cipherMode)
        {
            byte[] encryptedBytes;

            try
            {
                using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
                {

                    rijndaelManaged.Mode = this.parseToCipherMode(cipherMode);
                    rijndaelManaged.KeySize = keySize;
                    rijndaelManaged.BlockSize = blockSize;

                    rijndaelManaged.GenerateIV();
                    rijndaelManaged.GenerateKey();

                    ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);

                    //TO CHECK 
                    this.tmpKey = rijndaelManaged.Key;
                    this.tmpIV = rijndaelManaged.IV;
                    //CHECK

                   // XmlParser.CreateXml(filePath, cipherMode, blockSize, 1, keySize, );

                    var fileBytes = File.ReadAllBytes(filePath);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(fileBytes, 0, fileBytes.Length);
                            csEncrypt.FlushFinalBlock();
                        }
                        encryptedBytes = msEncrypt.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                encryptedBytes = null;
            }

            return encryptedBytes;
        }

        public byte[] DecryptFile(string filePath, string cipherMode)
        {
            byte[] decrypteBytes;

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {

                rijAlg.Mode = this.parseToCipherMode(cipherMode);
                rijAlg.Key = this.tmpKey;
                rijAlg.IV = this.tmpIV;

                var fileBytes = File.ReadAllBytes(filePath);
                decrypteBytes = new byte[fileBytes.Length];

                // Create the streams used for decryption.
                using(var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV))
                using (MemoryStream msDecrypt = new MemoryStream(fileBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        csDecrypt.Read(decrypteBytes, 0, decrypteBytes.Length);
                    }
                }
            }

            return decrypteBytes;
        }

        #endregion

        #region Private Methods

        private CipherMode parseToCipherMode(string input)
        {
            CipherMode cipherMode;
            switch (input)
            {
                case "ECB":
                    cipherMode = CipherMode.ECB;
                    break;
                case "CBC":
                    cipherMode = CipherMode.CBC;
                    break;
                case "CFB":
                    cipherMode = CipherMode.CFB;
                    break;
                case "OFB":
                    cipherMode = CipherMode.OFB;
                    break;
                default:
                    cipherMode = new CipherMode();
                    break;
            }
            return cipherMode;
        }

        #endregion
    }
}
