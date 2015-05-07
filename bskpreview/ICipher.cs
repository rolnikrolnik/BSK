using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace bskpreview
{
    interface ICipher
    {
        byte[] EncryptFile(string filePathToEncrypt, int keySize, int blockSize, string cipherMode);
        byte[] DecryptFile(string filePathToDecrypt, string cipherMode);
    }
}
