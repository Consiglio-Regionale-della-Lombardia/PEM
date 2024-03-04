using System;
using System.Security.Cryptography;
using System.Text;

namespace PortaleRegione.Crypto
{
    public static class CryptoHelper
    {
        public static string DecryptString(string EncryptedString, string Key)
        {
            try
            {
                byte[] Results;
                var UTF8 = new UTF8Encoding();

                var HashProvider = new MD5CryptoServiceProvider();
                var TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Key));

                var TDESAlgorithm = new TripleDESCryptoServiceProvider
                {
                    Key = TDESKey,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                var DataToDecrypt = Convert.FromBase64String(EncryptedString);

                try
                {
                    var Decryptor = TDESAlgorithm.CreateDecryptor();
                    Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
                }
                finally
                {
                    TDESAlgorithm.Clear();
                    HashProvider.Clear();
                }

                return UTF8.GetString(Results);
            }
            catch (Exception)
            {
                return "<font style='color:red'>Valore Corrotto</font>";
            }
        }

        public static string EncryptString(string InString, string Key)
        {
            byte[] Results;
            var UTF8 = new UTF8Encoding();

            var HashProvider = new MD5CryptoServiceProvider();
            var TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Key));

            var TDESAlgorithm = new TripleDESCryptoServiceProvider
            {
                Key = TDESKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var DataToEncrypt = UTF8.GetBytes(InString);

            try
            {
                var Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            return Convert.ToBase64String(Results);
        }
    }
}