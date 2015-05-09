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
    class CipherClass
    {
        public byte[] Key { get; private set; }
        public byte[] IV { get; private set; }
        public byte[] EncryptedFile { get; private set; }

        public string EncryptionSourceFilePath { get; set; }
        public string DecriptionSourceFilePath { get; set; }
        public string CipherMode { get; set; }
        public int KeySize { get; set; }
        public int BlockSize { get; set; }

        #region Public methods

        public void EncryptFile()
        {
            byte[] encryptedBytes;
            try
            {
                using (var rijndaelManaged = new RijndaelManaged())
                {
                    rijndaelManaged.Mode = this.parseToCipherMode(this.CipherMode);
                    rijndaelManaged.KeySize = this.KeySize;
                    rijndaelManaged.BlockSize = this.BlockSize;

                    rijndaelManaged.GenerateIV();
                    rijndaelManaged.GenerateKey();

                    ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);

                    this.Key = rijndaelManaged.Key;
                    this.IV = rijndaelManaged.IV;

                    var fileBytes = File.ReadAllBytes(this.EncryptionSourceFilePath);

                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
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
            this.EncryptedFile = encryptedBytes;
        }

        //public byte[] DecryptFile(string filePath, string cipherMode)
        //{
        //    byte[] decrypteBytes;

        //    using (RijndaelManaged rijAlg = new RijndaelManaged())
        //    {

        //        rijAlg.Mode = this.parseToCipherMode(cipherMode);
        //        rijAlg.Key = this.tmpKey;
        //        rijAlg.IV = this.tmpIV;

        //        var fileBytes = File.ReadAllBytes(filePath);
        //        decrypteBytes = new byte[fileBytes.Length];

        //        // Create the streams used for decryption.
        //        using(var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV))
        //        using (MemoryStream msDecrypt = new MemoryStream(fileBytes))
        //        {
        //            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        //            {
        //                csDecrypt.Read(decrypteBytes, 0, decrypteBytes.Length);
        //            }
        //        }
        //    }

        //    return decrypteBytes;
        //}

        #endregion

        #region Private Methods

        private System.Security.Cryptography.CipherMode parseToCipherMode(string input)
        {
            System.Security.Cryptography.CipherMode cipherMode;
            switch (input)
            {
                case "ECB":
                    cipherMode = System.Security.Cryptography.CipherMode.ECB;
                    break;
                case "CBC":
                    cipherMode = System.Security.Cryptography.CipherMode.CBC;
                    break;
                case "CFB":
                    cipherMode = System.Security.Cryptography.CipherMode.CFB;
                    break;
                case "OFB":
                    cipherMode = System.Security.Cryptography.CipherMode.OFB;
                    break;
                default:
                    cipherMode = new System.Security.Cryptography.CipherMode();
                    break;
            }
            return cipherMode;
        }

        #endregion
    }
}
