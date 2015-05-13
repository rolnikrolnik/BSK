using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace bskpreview
{
    public class RSAKeyManager
    {
        public RSACryptoServiceProvider rsa;

        public RSAKeyManager()
        {
            rsa = new RSACryptoServiceProvider();
        }

        public void SaveKeyPair(string pub, string priv, string password)
        {
            FileStream pubOut = new FileStream(pub, FileMode.OpenOrCreate, FileAccess.Write);
            FileStream privOut = new FileStream(priv, FileMode.OpenOrCreate, FileAccess.Write);

            RSAKeyManager.EncodePublicKey(rsa, new StreamWriter(pubOut));

            RSAKeyManager.EncodePrivateKey(rsa, password, new StreamWriter(privOut));

            pubOut.Close();
            privOut.Close();
        }

        public void LoadPrivateKey(string priv, string password)
        {
            rsa = null;
            int i = 0;
            int max = 100;

            while (rsa == null && i < max)
            {
                FileStream privOut = new FileStream(priv, FileMode.Open, FileAccess.Read);
                rsa = RSAKeyManager.DecodePrivateKey(new StreamReader(privOut), password);
                i++;
            }
            Console.WriteLine("{0} prob.", i);
        }

        public void LoadPublicKey(string pub)
        {
            FileStream pubOut = new FileStream(pub, FileMode.Open, FileAccess.Read);

            rsa = RSAKeyManager.DecodePublicKey(new StreamReader(pubOut));
        }

        public static void EncodePublicKey(RSACryptoServiceProvider csp, TextWriter outputStream)
        {
            var parameters = csp.ExportParameters(true);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);

                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                // Musimy sie upewnic, ze wielkosc wiadomosci jest podzielna przez 8 (wielkosc bloku)
                while (stream.Length % 8 != 0)
                {
                    stream.Write(new byte[] { 0x00 }, 0, 1);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();

                outputStream.WriteLine("-----BEGIN RSA PUBLIC KEY-----");
                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                }
                outputStream.WriteLine("-----END RSA PUBLIC KEY-----");
            }

            outputStream.Close();
        }
        
        public static void EncodePrivateKey(RSACryptoServiceProvider csp, string password, TextWriter outputStream)
        {
            if (csp.PublicOnly) throw new ArgumentException("CSP does not contain a private key", "csp");
            var parameters = csp.ExportParameters(true);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
                    EncodeIntegerBigEndian(innerWriter, parameters.D);
                    EncodeIntegerBigEndian(innerWriter, parameters.P);
                    EncodeIntegerBigEndian(innerWriter, parameters.Q);
                    EncodeIntegerBigEndian(innerWriter, parameters.DP);
                    EncodeIntegerBigEndian(innerWriter, parameters.DQ);
                    EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                Console.WriteLine("Key size before adding zeros: {0}", stream.Length);

                // Musimy sie upewnic, ze wielkosc wiadomosci jest podzielna przez 8 (wielkosc bloku)
                List<byte> streamBytes = new List<byte>(stream.GetBuffer());

                while (streamBytes.Count % 8 != 0)
                {
                    streamBytes.Insert(0, 0x00);
                }

                byte[] lol = streamBytes.ToArray();

                Console.WriteLine("Key size before encoding it: {0}", lol.Length);

                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] shortcut = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                byte[] key = new byte[24];
                byte[] iv = new byte[1];

                Array.Copy(shortcut, key, shortcut.Length);

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Mode = CipherMode.ECB;
                tdes.Key = key;
                tdes.Padding = PaddingMode.None;

                MemoryStream passEncoded = new MemoryStream();
                var x = tdes.CreateEncryptor();
                CryptoStream encStream = new CryptoStream(passEncoded, x, CryptoStreamMode.Write);
                encStream.Write(streamBytes.ToArray(), 0, streamBytes.Count);

                Console.WriteLine("Key size after encoding it: {0}", passEncoded.Length);

                var base64 = Convert.ToBase64String(passEncoded.GetBuffer(), 0, (int)passEncoded.Length).ToCharArray();

                //encStream.Close();

                outputStream.WriteLine("-----BEGIN RSA PRIVATE KEY-----");
                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                }
                outputStream.WriteLine("-----END RSA PRIVATE KEY-----");

            }
            outputStream.Close();
        }
        
        public static RSACryptoServiceProvider DecodePrivateKey(TextReader inputFile, string password)
        {
            string line = inputFile.ReadLine();
            if (line != "-----BEGIN RSA PRIVATE KEY-----")
                throw new ArgumentException("Wrong input file (" + line + ")", "inputFile");

            string text = inputFile.ReadToEnd();
            text = text.Substring(0, text.IndexOf("-----END RSA PRIVATE KEY-----"));
            text.Replace(System.Environment.NewLine, "");
            text = text.Replace("\n", "");
            text = text.Replace("\r", "");

            byte[] encodedArray = Convert.FromBase64String(text);

            Console.WriteLine("Key size before decoding it: {0}", encodedArray.Length);

            //Decrypt content with given password
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] shortcut = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            byte[] key = new byte[24];
            byte[] iv = new byte[64];

            Array.Copy(shortcut, key, shortcut.Length);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Mode = CipherMode.ECB;
            tdes.Key = key;
            tdes.Padding = PaddingMode.None;

            MemoryStream passDecoded = new MemoryStream();
            var x = tdes.CreateDecryptor();
            CryptoStream encStream = new CryptoStream(passDecoded, x, CryptoStreamMode.Write);

            encStream.Write(encodedArray, 0, encodedArray.Length);

            Console.WriteLine("Key size after decoding it: {0}", passDecoded.Length);

            List<byte> rawBytes = new List<byte>(passDecoded.GetBuffer());

            while (rawBytes[0] == 0x00)
            {
                rawBytes.RemoveAt(0);
            }

            Console.WriteLine("Key size after removin 0 it: {0}", rawBytes.Count);

            // Decode from ASN.1 format
            return DecodeRSAPrivateKey(rawBytes.ToArray());
        }

        public static RSACryptoServiceProvider DecodePublicKey(TextReader inputFile)
        {
            if (inputFile.ReadLine() != "-----BEGIN RSA PUBLIC KEY-----")
                throw new ArgumentException("Wrong input file", "inputFile");

            string text = inputFile.ReadToEnd();
            text = text.Substring(0, text.IndexOf("-----END RSA PUBLIC KEY-----"));
            text.Replace(System.Environment.NewLine, "");
            text = text.Replace("\n", "");
            text = text.Replace("\r", "");

            byte[] decodedArray = Convert.FromBase64String(text);

            // Decode from ASN.1 format
            return DecodeRSAPublicKey(decodedArray);
        }
        //------- Parses binary ans.1 RSA private key; returns RSACryptoServiceProvider  ---
        private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {

            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;

            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte();        //advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16();       //advance 2 bytes
            else
                return null;

            twobytes = binr.ReadUInt16();
            if (twobytes != 0x0102) //version number
                return null;
            bt = binr.ReadByte();
            if (bt != 0x00)
                return null;


            //------  all private key components are Integer sequences ----
            elems = GetIntegerSize(binr);
            MODULUS = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            E = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            D = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            P = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            Q = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            DP = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            DQ = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            IQ = binr.ReadBytes(elems);

            binr.Close();
            mem.Close();
            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSAParameters RSAparams = new RSAParameters();
            RSAparams.Modulus = MODULUS;
            RSAparams.Exponent = E;
            RSAparams.D = D;
            RSAparams.P = P;
            RSAparams.Q = Q;
            RSAparams.DP = DP;
            RSAparams.DQ = DQ;
            RSAparams.InverseQ = IQ;

            try
            {
                RSA.ImportParameters(RSAparams);
            }
            catch
            {
                return null;
            }

            return RSA;
        }

        private static RSACryptoServiceProvider DecodeRSAPublicKey(byte[] pubkey)
        {

            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
            MemoryStream mem = new MemoryStream(pubkey);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;

            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte();        //advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16();       //advance 2 bytes
            else
                return null;

            twobytes = binr.ReadUInt16();
            if (twobytes != 0x0102) //version number
                return null;
            bt = binr.ReadByte();
            if (bt != 0x00)
                return null;


            //------  all private key components are Integer sequences ----
            elems = GetIntegerSize(binr);
            MODULUS = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            E = binr.ReadBytes(elems);

            binr.Close();
            mem.Close();
            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSAParameters RSAparams = new RSAParameters();
            RSAparams.Modulus = MODULUS;
            RSAparams.Exponent = E;

            RSA.ImportParameters(RSAparams);

            return RSA;
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)     //expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();    // data size in next byte
            else
                if (bt == 0x82)
                {
                    highbyte = binr.ReadByte(); // data size in next 2 bytes
                    lowbyte = binr.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else
                {
                    count = bt;     // we already have the data size
                }

            while (binr.ReadByte() == 0x00)
            {   //remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);       //last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }

        private static void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }

        private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }
    }
}
