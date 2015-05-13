using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows;

namespace bskpreview
{
    class MainController
    {
        private CipherClass cipherClass;
        private readonly FileController fileController;

        //  File paths
        public string EncryptionSourceFilePath{ get; set; }
        public string EncryptionDestinationFilePath { get; set; }
        public string DecryptionSourceFilePath { get; set; }
        public string DecryptionDestinationFilePath{ get; set; }

        //  Encryption settings
        public string AlgorithmName { get; set; }
        public string CipherMode { get; set; }
        public int KeySize { get; set; }
        public int BlockSize { get; set; }
        public int FeedbackSize { get; set; }
        public List<RSAPublicKey> ReceiversRsaPublicKeys { get; set; }
        public string Password { get; set; }
        public string Identity { get; set; }


        public MainController()
        {
            this.cipherClass = new CipherClass();
            this.fileController = new FileController();
        }

        public void EncryptFile()
        {

            this.cipherClass = new CipherClass()
            {
                EncryptionSourceFilePath = this.EncryptionSourceFilePath,
                CipherMode = this.CipherMode,
                KeySize = this.KeySize,
                BlockSize = this.BlockSize,
                FeedbackSize = this.FeedbackSize
            };

            this.cipherClass.EncryptFile();

            var encryptedFileHeader = new EncryptedFileHeader
            {
                AlgorithmName = "Rijndael",
                CipherMode = this.CipherMode,
                KeySize = this.KeySize,
                BlockSize = this.BlockSize,
                FeedbackSize = this.FeedbackSize,
                IV = this.cipherClass.IV,
                Receivers = new List<Receiver>()
            };

            foreach (var key in ReceiversRsaPublicKeys)
            {
                var receiver = new Receiver()
                {
                    Name = key.Username,
                    SessionKey = this.cipherClass.EncryptSeesionKey(File.ReadAllText(key.PathToKey))
                };
                encryptedFileHeader.Receivers.Add(receiver);
            }

            this.fileController.SaveCryptogram(this.EncryptionDestinationFilePath, encryptedFileHeader, this.cipherClass.EncryptedFile);
        }

        public void DecryptFile()
        {
            
            var encryptedFileHeader = this.fileController.GetFileHeader(this.DecryptionSourceFilePath);
            var encryptedFile = this.fileController.GetEncryptedFile(this.DecryptionSourceFilePath);
            
            var identity = encryptedFileHeader.Receivers.Find(x => x.Name == this.Identity);
            byte[] privateKey;
            try
            {
                privateKey =
                    this.fileController.ReadFile(@"C:\Users\Rolnik\Desktop\STUDIA\s06\BSK\projekt\Tożsamości\" +
                                                 this.Identity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Brak klucza publicznego dla danego odbiorcy!!");
            }

            this.cipherClass = new CipherClass()
            {
                DecriptionSourceFilePath = this.DecryptionSourceFilePath,
                CipherMode = encryptedFileHeader.CipherMode,
                KeySize = encryptedFileHeader.KeySize,
                BlockSize = encryptedFileHeader.BlockSize,
                FeedbackSize = encryptedFileHeader.FeedbackSize,
                IV = encryptedFileHeader.IV,
                Key = identity.SessionKey
            };

            byte[] decryptedFile;
            try
            {
                this.cipherClass.DecryptSessionKey(privateKey, this.Password);
                decryptedFile = this.cipherClass.DecryptFile(encryptedFile);
                this.fileController.SaveFile(this.DecryptionDestinationFilePath, decryptedFile);
            }
            catch (CryptographicException cryptographicException)
            {
                decryptedFile = new byte[encryptedFile.Length];
                var random = new Random();
                random.NextBytes(decryptedFile);
                this.fileController.SaveFile(this.DecryptionDestinationFilePath, decryptedFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ups! Coś poszło nie tak...");
            }

        }

        public List<Receiver> GetPossibleIdentities()
        {
            return this.fileController.GetFileHeader(this.DecryptionSourceFilePath).Receivers;
        }
    }
}
