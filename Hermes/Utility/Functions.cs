using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Utility
{
    public static class Functions
    {
        public static int GetAvailablePort(int startingPort)
        {
            var portArray = new List<int>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            // Ignore active connections
            var connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            // Ignore active tcp listners
            var endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            // Ignore active UDP listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (var i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }

        public static Message GetMessage(string dataReceived)
        {
            if (!string.IsNullOrEmpty(dataReceived))
            {
                try
                {
                    Message mes = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(dataReceived);
                    return mes;
                }
                catch (Exception ex)
                {
                    Log.LogEngine.Instance.Engine.Error(ex);
                }
            }
            return null;
        }
        public static string EncryptString(string text, string keyValue)
        {
            return Encrypt(text, keyValue, "EZHUTHELLAM", "SHA1", 3, "@1B2c3D4e5F6g7H8", 256);
        }
        public static  string Encrypt(string passtext, string passPhrase, string saltV, string hashstring, int Iterations, string initVect, int keysize)
        {
            string functionReturnValue = null;
            // Convert strings into byte arrays.
            // Let us assume that strings only contain ASCII codes.
            // If strings include Unicode characters, use Unicode, UTF7, or UTF8
            // encoding.
            byte[] initVectorBytes = null;
            initVectorBytes = Encoding.ASCII.GetBytes(initVect);
            byte[] saltValueBytes = null;
            saltValueBytes = Encoding.ASCII.GetBytes(saltV);

            // Convert our plaintext into a byte array.
            // Let us assume that plaintext contains UTF8-encoded characters.
            byte[] plainTextBytes = null;
            plainTextBytes = Encoding.UTF8.GetBytes(passtext);
            // First, we must create a password, from which the key will be derived.
            // This password will be generated from the specified passphrase and
            // salt value. The password will be created using the specified hash
            // algorithm. Password creation can be done in several iterations.
            PasswordDeriveBytes password = default(PasswordDeriveBytes);
            password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashstring, Iterations);
            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = null;
            keyBytes = password.GetBytes(keysize / 8);
            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = default(RijndaelManaged);
            symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;
            // Generate encryptor from the existing key bytes and initialization
            // vector. Key size will be defined based on the number of the key
            // bytes.
            ICryptoTransform encryptor = default(ICryptoTransform);
            encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            MemoryStream memoryStream = default(MemoryStream);
            memoryStream = new MemoryStream();

            // Define cryptographic stream (always use Write mode for encryption).
            CryptoStream cryptoStream = default(CryptoStream);
            cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            // Start encrypting.
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

            // Finish encrypting.
            cryptoStream.FlushFinalBlock();
            // Convert our encrypted data from a memory stream into a byte array.
            byte[] cipherTextBytes = null;
            cipherTextBytes = memoryStream.ToArray();

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            // Convert encrypted data into a base64-encoded string.
            string cipherText = null;
            cipherText = Convert.ToBase64String(cipherTextBytes);

            functionReturnValue = cipherText;
            Log.LogEngine.Instance.Engine.Debug("Encrypt ="+ functionReturnValue);
            return RemoveSpecialChar(functionReturnValue);
        }

        public static string RemoveSpecialChar(string value)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[*'\",_&#^@]");
            value = reg.Replace(value, string.Empty);

            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex("[ ]");
            return reg.Replace(value, "-");
        }
        public static string MixString(string value1, string value2)
        {
            string mix = "";
            int idx1 = 0;
            int idx2 = 0;
            for (int i = 0; i < value1.Length + value2.Length; i++)
            {
                if (idx1 < value1.Length)
                {
                    mix += value1.Substring(idx1, 1);
                    idx1++;
                }
                if (idx2 < value2.Length)
                {
                    mix += value2.Substring(idx2, 1);
                    idx2++;
                }
            }
            return mix;
        }
    }
}
