using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace bskpreview
{
    class FileController
    {
        private int bytesToSkip;

        public void SaveCryptogram(string destinationPath, EncryptedFileHeader encryptedFileHeader, byte[] encryptedFile)
        {
            using (var fileStream = new FileStream(destinationPath, FileMode.Create))
            {
                var serializer = new XmlSerializer(encryptedFileHeader.GetType());
                serializer.Serialize(fileStream, encryptedFileHeader);
                fileStream.Write(new byte[]{0}, 0, 1);
                fileStream.Write(encryptedFile, 0, encryptedFile.Length);
            }
        }

        public byte[] ReadFile(string sourceFilePath)
        {
            return File.ReadAllBytes(sourceFilePath);
        }


        public void SaveFile(string destinationFilePath, byte[] encryptedBytes)
        {
            File.WriteAllBytes(destinationFilePath, encryptedBytes);
        }

        public EncryptedFileHeader GetFileHeader(string sourcePath)
        {
            using (var fileStream = new FileStream(sourcePath, FileMode.Open))
            {
                var bytesToDeserialize = new List<byte>();
                while (true)
                {
                    var b = (byte) fileStream.ReadByte();
                    if (b == 0) break;
                    bytesToDeserialize.Add(b);
                }
                bytesToSkip = bytesToDeserialize.Count + 1;
                using (var ms = new MemoryStream(bytesToDeserialize.ToArray()))
                {
                    var serializer = new XmlSerializer(typeof (EncryptedFileHeader));
                    return (EncryptedFileHeader) serializer.Deserialize(ms);
                }
            }
        }

        public byte[] GetEncryptedFile(string sourcePath)
        {
            using (var fileStream = new FileStream(sourcePath, FileMode.Open))
            {
                var encryptedFile = new byte[fileStream.Length-this.bytesToSkip];
                fileStream.Seek(this.bytesToSkip, SeekOrigin.Current);
                fileStream.Read(encryptedFile, 0, encryptedFile.Length);
                return encryptedFile;
            }   
        }
    }
}
