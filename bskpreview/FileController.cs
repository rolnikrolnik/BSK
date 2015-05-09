using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bskpreview
{
    class FileController
    {
        //TODO - encapsulate file in the XML + encrypted bytes - now it is just raw bytes
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

        public void SaveFile(string destinationFilePath, byte[] encryptedBytes)
        {
            File.WriteAllBytes(destinationFilePath, encryptedBytes);
        }
    }
}
