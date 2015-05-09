using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bskpreview
{
    class MainController
    {
        private CipherClass cipherClass;
        private FileController fileController;

        //  File paths
        public string EncryptionSourceFilePath{ get; set; }
        public string EncryptionDestinationFilePath { get; set; }
        public string DecriptionSourceFilePath { get; set; }
        public string DecryptionDestinationFileTextBox { get; set; }

        //  Encryption settings
        public string AlgorithmName { get; set; }
        public string CipherMode { get; set; }
        public int KeySize { get; set; }
        public int BlockSize { get; set; }
        public List<Receiver> Receivers { get; set; }

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
                BlockSize = this.BlockSize
            };

            this.cipherClass.EncryptFile();

            var encryptedFileHeader = new EncryptedFileHeader
            {
                AlgorithmName = "Rijndael",
                CipherMode = this.CipherMode,
                KeySize = this.KeySize,
                BlockSize = this.BlockSize,
                IV = this.cipherClass.IV,
                Receivers = new List<Receiver>()
            };

            //TODO: Cipher session key using public key of receiver in foreach
            encryptedFileHeader.Receivers.Add(new Receiver(){Name = "Maciek", SessionKey = this.cipherClass.Key});

            this.fileController.SaveCryptogram(this.EncryptionDestinationFilePath, encryptedFileHeader, this.cipherClass.EncryptedFile);
        }

        public void DecryptFile() { }
    }
}
