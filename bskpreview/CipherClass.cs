using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace bskpreview
{
    class CipherClass
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
        public byte[] EncryptedFile { get; set; }

        public string EncryptionSourceFilePath { get; set; }
        public string DecriptionSourceFilePath { get; set; }
        public string CipherMode { get; set; }
        public int KeySize { get; set; }
        public int BlockSize { get; set; }
        public int FeedbackSize { get; set; }

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
                    rijndaelManaged.FeedbackSize = this.FeedbackSize;

                    rijndaelManaged.GenerateIV();
                    rijndaelManaged.GenerateKey();

                    var encryptor = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);

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

        public byte[] DecryptFile(byte[] encryptedBytes)
        {
            using (var rijAlg = new RijndaelManaged())
            {
                try
                {
                    rijAlg.Mode = this.parseToCipherMode(this.CipherMode);
                    rijAlg.BlockSize = this.BlockSize;
                    rijAlg.FeedbackSize = this.FeedbackSize;
                    rijAlg.Key = this.Key;
                    rijAlg.IV = this.IV;


                    using (var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV))
                    using (var msDecrypt = new MemoryStream(encryptedBytes))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        var decryptedBytes = new byte[encryptedBytes.Length];
                        csDecrypt.Read(decryptedBytes, 0, decryptedBytes.Length);
                        return decryptedBytes;
                    }
                }
                catch (CryptographicException)
                {
                    rijAlg.Mode = this.parseToCipherMode(this.CipherMode);
                    rijAlg.BlockSize = this.BlockSize;
                    rijAlg.FeedbackSize = this.FeedbackSize;
                    rijAlg.Padding = PaddingMode.None;
                    rijAlg.Key = this.Key;
                    rijAlg.IV = this.IV;

                    using (var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV))
                    using (var msDecrypt = new MemoryStream(encryptedBytes))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        var decryptedBytes = new byte[encryptedBytes.Length];
                        csDecrypt.Read(decryptedBytes, 0, decryptedBytes.Length);
                        return decryptedBytes;
                    }
                }
            }
        }

        public byte[] EncryptSeesionKey(string publicKey)
        {
            const string pattern = @"<RSAKeyValue>.*<\/RSAKeyValue>";
            publicKey = new Regex(pattern).Match(publicKey).Value;

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                return rsa.Encrypt(this.Key, false);
            }
        }

        public void DecryptSessionKey(byte[] privateKey, string password)
        {
            using (var rijndael = new RijndaelManaged())
            using (var rsa = new RSACryptoServiceProvider())
            {
                var sha = new SHA1CryptoServiceProvider();
                var shortcut = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var key = new byte[24];

                Array.Copy(shortcut, key, shortcut.Length);

                rijndael.Mode = System.Security.Cryptography.CipherMode.ECB;
                rijndael.KeySize = 192;
                rijndael.Padding = PaddingMode.None;
                rijndael.Key = key;

                byte[] decryptedPrivateKey;
                using (var decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV))
                using (var msDecrypt = new MemoryStream(privateKey))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    decryptedPrivateKey = new byte[privateKey.Length];
                    csDecrypt.Read(decryptedPrivateKey, 0, decryptedPrivateKey.Length);
                }

                var xmlStringPrivateKey = Encoding.UTF8.GetString(decryptedPrivateKey);

                try
                {
                    rsa.FromXmlString(xmlStringPrivateKey);
                    this.Key = rsa.Decrypt(this.Key, false);
                }
                catch (Exception)
                {
                    var badKey = Enumerable.Repeat((byte)0x0, KeySize / 4).ToList();
                    for (var i = 0; i < key.Length; i++)
                    {
                        badKey[i] = key[i];
                    }
                    this.Key = badKey.ToArray();
                }
            }
        }

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
                    cipherMode = System.Security.Cryptography.CipherMode.CFB;
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
