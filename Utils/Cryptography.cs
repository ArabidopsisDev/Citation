using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Citation.Utils
{
    internal class Cryptography
    {
        private readonly string _key = "hoMo1145HomO1145";
        private readonly string _iv = "oiiaiiooiiaiioai";

        public Cryptography()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow!.Project.AesKey is null ||
                mainWindow.Project.AesIv is null)
                return;

            _key = mainWindow.Project.AesKey;
            _iv = mainWindow.Project.AesIv;
        }

        internal byte[] Encrypt(string plainText)
        {
            byte[] encrypted;

            using var aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(_key);
            aesAlg.IV = Encoding.UTF8.GetBytes(_iv);
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new System.IO.MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
                swEncrypt.Write(plainText);

            encrypted = msEncrypt.ToArray();
            return encrypted;
        }

        internal string Decrypt(byte[] cipher)
        {
            string plaintext = null;
            using var aesAlg = Aes.Create();

            aesAlg.Key = Encoding.UTF8.GetBytes(_key);
            aesAlg.IV = Encoding.UTF8.GetBytes(_iv);
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using var msDecrypt = new System.IO.MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new System.IO.StreamReader(csDecrypt);
            plaintext = srDecrypt.ReadToEnd();

            return plaintext;
        }
    }
}
