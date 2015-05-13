using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace bskpreview
{
    [Serializable]
    [XmlRoot("EncryptedFileHeader")]
    public class EncryptedFileHeader
    {
        public string AlgorithmName { get; set; }
        public string CipherMode { get; set; }
        public int KeySize { get; set; }
        public int BlockSize { get; set; }
        public int FeedbackSize { get; set; }
        public byte[] IV { get; set; }
        public List<Receiver> Receivers { get; set; }

        public EncryptedFileHeader()
        {        
        }

    }

    [Serializable]
    public class Receiver
    {
        public string Name { get; set; }
        public byte[] SessionKey { get; set; }

        public Receiver()
        {
        }
    }
}
