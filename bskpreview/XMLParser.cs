using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;

namespace bskpreview
{
    static class XmlParser
    {
        private static readonly string DocumentSkeleton;

        static XmlParser()
        {
            DocumentSkeleton = "<EncryptedFile><Algorithm>Rijndael</Algorithm><CipherMode></CipherMode><BlockSize></BlockSize><KeySize></KeySize><ApprovedUsers><User><Email></Email><SessionKey></SessionKey></User></ApprovedUsers><IV></IV></EncryptedFile>";
        }

        public static int CreateXml(string filePath, string cipherMode, int blockSize, int subblockSize, int keySize, byte[] iv)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(DocumentSkeleton);

            XmlNodeList xmlNodeList = xml.SelectNodes("/EncryptedFile");
            XmlElement xmlCipherMode = xml.SelectSingleNode("//CipherMode") as XmlElement;
            xmlCipherMode.InnerText = cipherMode;
            XmlElement xmlBlockSize = xml.SelectSingleNode("//BlockSize") as XmlElement;
            xmlBlockSize.InnerText = blockSize.ToString();
            XmlElement xmlKeySize = xml.SelectSingleNode("//KeySize") as XmlElement;
            xmlKeySize.InnerText = keySize.ToString();

            return 0;
        }
    }
}
