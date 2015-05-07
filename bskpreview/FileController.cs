using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bskpreview
{
    class FileController
    {
        //TODO - encapsulate file in the XML + encrypted bytes - now it is just raw bytes
        public void SaveFile(string destinationFilePath, string xml)
        {
           
        }

        public void SaveFile(string destinationFilePath, byte[] encryptedBytes)
        {
            File.WriteAllBytes(destinationFilePath, encryptedBytes);
        }
    }
}
