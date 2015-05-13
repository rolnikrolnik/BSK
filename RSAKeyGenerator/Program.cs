using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RSAKeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Write("Zła ilość argumentów");
                return;
            }

            GenerateKeys(args[0], args[1]);
        }

        static void GenerateKeys(string name, string password)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                //public
                var str = "<User><Name>" + name + "</Name>" + rsa.ToXmlString(false) + "</User>";
                File.WriteAllText(name + ".xml", str);

                //private
                var privateXml = rsa.ToXmlString(true);
                byte[] encrypted;

                var sha = new SHA1CryptoServiceProvider();
                var shortcut = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var key = new byte[24];

                Array.Copy(shortcut, key, shortcut.Length);

                using (var rij = new RijndaelManaged())
                {
                    rij.Mode = CipherMode.ECB;
                    rij.KeySize = 192;
                    rij.Key = key;

                    var encryptor = rij.CreateEncryptor();
                    using (var msEncrypt = new MemoryStream())
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(privateXml);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
                File.WriteAllBytes(name, encrypted);
            }
        }
    }
}
