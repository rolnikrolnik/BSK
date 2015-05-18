using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Windows.Documents;
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
                //fileStream.Write(new byte[] { 0 }, 0, 1);
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
                var allBytes = new byte[fileStream.Length];
                fileStream.Read(allBytes, 0, allBytes.Length);

                const string patternString = "</EncryptedFileHeader>";
                var patternBytes = System.Text.Encoding.ASCII.GetBytes(patternString);
                var patternIndex = ByteSearch(allBytes, patternBytes);

                bytesToSkip = patternIndex + patternBytes.Length;

                var bytesToDeserialize = allBytes.Take(bytesToSkip).ToArray();
                fileStream.Read(bytesToDeserialize, 0, bytesToSkip);
                using (var ms = new MemoryStream(bytesToDeserialize))
                {
                    var serializer = new XmlSerializer(typeof(EncryptedFileHeader));
                    return (EncryptedFileHeader)serializer.Deserialize(ms);
                }
            }
        }

        public byte[] GetEncryptedFile(string sourcePath)
        {
            using (var fileStream = new FileStream(sourcePath, FileMode.Open))
            {
                var encryptedFile = new byte[fileStream.Length - this.bytesToSkip];
                fileStream.Seek(this.bytesToSkip, SeekOrigin.Current);
                fileStream.Read(encryptedFile, 0, encryptedFile.Length);
                return encryptedFile;
            }
        }

        private int ByteSearch(byte[] sourceBytes, byte[] patternBytes, int startByte = 0)
        {
            var found = -1;
            if (sourceBytes.Length <= 0 || patternBytes.Length <= 0 ||
                startByte > (sourceBytes.Length - patternBytes.Length) || sourceBytes.Length < patternBytes.Length)
                return found;
            for (var i = startByte; i <= sourceBytes.Length - patternBytes.Length; i++)
            {
                if (sourceBytes[i] != patternBytes[0]) continue;
                if (sourceBytes.Length > 1)
                {
                    var matched = true;
                    for (var y = 1; y <= patternBytes.Length - 1; y++)
                    {
                        if (sourceBytes[i + y] == patternBytes[y]) continue;
                        matched = false;
                        break;
                    }
                    if (!matched) continue;
                    found = i;
                    break;
                }
                found = i;
                break;
            }
            return found;
        }
    }
}
