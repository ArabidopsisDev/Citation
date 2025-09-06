using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Citation.Utils
{
    internal class Cryptography
    {
        private readonly string _key = "hoMo1145HomO1145";
        private readonly string _iv = "oiiaiiooiiaiioai";

        public Cryptography(string? key = null, string? iv = null)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow!.Project.AesKey is null ||
                mainWindow.Project.AesIv is null)
                return;

            _key = key ?? mainWindow.Project.AesKey;
            _iv = iv ?? mainWindow.Project.AesIv;
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

        internal static string Candor(byte[]? data)
        {
            if (data is null || data.Length == 0)
                return string.Empty;

            return Convert.ToBase64String(data);
        }

        internal static byte[] Qualitative(string base64String)
        {
            return Convert.FromBase64String(base64String.Trim());
        }

        internal string Decrypt(byte[] cipher)
        {
            string? plaintext = null;
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

        public static string ComputeHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            var builder = new StringBuilder();
            foreach (var bit in bytes)
                builder.Append(bit.ToString("x2"));
            return builder.ToString();
        }

        public static string ComputeMd5(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            var builder = new StringBuilder();
            foreach (var bit in bytes)
                builder.Append(bit.ToString("x2"));
            return builder.ToString();
        }

        internal static string EncryptData(string password, string plainText)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (plainText == string.Empty||
                mainWindow!.Config.SecurityVersion != "CryptoDB") return plainText;

            try
            {
                var aesKey = password.Substring(0, 16);
                var aesIv = password.Substring(16, 16);

                return Cryptography.Candor(
                    new Cryptography(aesKey, aesIv).Encrypt(plainText));
            }
            catch
            {
                return Randomization.RandomBuddha();
            }
        }

        internal static string DecryptData(string password, string cipherText)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (cipherText == string.Empty || 
                mainWindow!.Config.SecurityVersion != "CryptoDB") return cipherText;

            try
            {
                var aesKey = password.Substring(0, 16);
                var aesIv = password.Substring(16, 16);
                return new Cryptography(aesKey, aesIv)
                    .Decrypt(Cryptography.Qualitative(cipherText));
            }
            catch
            {
                return Randomization.RandomBuddha();
            }
        }

        internal static T EncryptObject<T>(string password, T instance) where T : class, new()
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var type = typeof(T);
            var result = new T();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                var ignoreEncrypt = prop.GetCustomAttribute<IgnoreEncryptAttribute>() != null;
                if (ignoreEncrypt)
                {
                    object pre = prop.GetValue(instance)!;
                    prop.SetValue(result, pre);
                    continue;
                }

                object value = prop.GetValue(instance)!;

                if (prop.PropertyType == typeof(string) && value != null)
                    prop.SetValue(result, EncryptData(password, (string)value));
                else
                    prop.SetValue(result, value);
            }
            return result;
        }

        internal static T DecryptObject<T>(string password, T instance) where T : class, new()
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var type = typeof(T);
            var result = new T();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                var ignoreEncrypt = prop.GetCustomAttribute<IgnoreEncryptAttribute>() != null;
                if (ignoreEncrypt)
                {
                    object pre = prop.GetValue(instance)!;
                    prop.SetValue(result, pre);
                    continue;
                }

                object value = prop.GetValue(instance)!;

                if (prop.PropertyType == typeof(string) && value != null)
                    prop.SetValue(result, DecryptData(password, (string)value));
                else
                    prop.SetValue(result, value);
            }
            return result;
        }
    }
}
