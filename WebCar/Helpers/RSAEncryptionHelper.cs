using System;
using System.Security.Cryptography;
using System.Text;

namespace WebCar.Helpers
{
    public static class RSAEncryptionHelper
    {
        private static RSAParameters _publicKey;
        private static RSAParameters _privateKey;
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;

        private static void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    using (var rsa = new RSACryptoServiceProvider(2048))
                    {
                        rsa.PersistKeyInCsp = false;

                        _publicKey = rsa.ExportParameters(false);
                        _privateKey = rsa.ExportParameters(true);

                        _initialized = true;

                        System.Diagnostics.Debug.WriteLine("========================================");
                        System.Diagnostics.Debug.WriteLine("✅ RSA KEYS INITIALIZED");
                        System.Diagnostics.Debug.WriteLine($"Key Size: 2048 bits");
                        System.Diagnostics.Debug.WriteLine("========================================");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ RSA Init Error: {ex.Message}");
                    _initialized = false;
                }
            }
        }

        /// <summary>
        /// Encrypt - Returns plaintext if encryption fails
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                if (!_initialized)
                    Initialize();

                // If init failed, return plaintext
                if (!_initialized)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ RSA not initialized, returning plaintext");
                    return plainText;
                }

                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.ImportParameters(_publicKey);

                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = rsa.Encrypt(plainBytes, false);

                    string encrypted = Convert.ToBase64String(encryptedBytes);

                    System.Diagnostics.Debug.WriteLine($"[RSA ENCRYPT] ✅");
                    System.Diagnostics.Debug.WriteLine($"  Input:  {plainText.Length} chars");
                    System.Diagnostics.Debug.WriteLine($"  Output: {encrypted.Length} chars");

                    return encrypted;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RSA Encrypt Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"⚠️ Returning plaintext");

                // ✅ RETURN PLAINTEXT - DON'T THROW
                return plainText;
            }
        }

        /// <summary>
        /// Decrypt - Returns [ENCRYPTED] marker if decryption fails
        /// </summary>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                if (!_initialized)
                    Initialize();

                if (!_initialized)
                {
                    return "[ENCRYPTED - RSA NOT INITIALIZED]";
                }

                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.ImportParameters(_privateKey);

                    byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                    byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, false);

                    string decrypted = Encoding.UTF8.GetString(decryptedBytes);

                    System.Diagnostics.Debug.WriteLine($"[RSA DECRYPT] ✅");
                    System.Diagnostics.Debug.WriteLine($"  Input: {encryptedText.Length} chars");
                    System.Diagnostics.Debug.WriteLine($"  Output: {decrypted.Length} chars");

                    return decrypted;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RSA Decrypt Error: {ex.Message}");
                return "[ENCRYPTED DATA]";
            }
        }

        public static (string publicKey, string privateKey) GenerateKeyPair()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                string publicKey = rsa.ToXmlString(false);
                string privateKey = rsa.ToXmlString(true);
                return (publicKey, privateKey);
            }
        }

        public static void RegenerateKeys()
        {
            lock (_lockObject)
            {
                _initialized = false;
                Initialize();
            }
        }
    }
}