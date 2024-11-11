using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public class EncryptionHelper
{
    private readonly string _encryptionKey;

    public EncryptionHelper(string encryptionKey)
    {
        _encryptionKey = encryptionKey;
    }

    public string Encrypt(object data)
    {
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32)); // Ensure 256-bit key size
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV();
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(jsonData);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public T Decrypt<T>(string encryptedData)
    {
        byte[] key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
        byte[] data = Convert.FromBase64String(encryptedData);
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            byte[] iv = new byte[aes.BlockSize / 8];
            Array.Copy(data, iv, iv.Length);
            aes.IV = iv;
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(data, iv.Length, data.Length - iv.Length))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var reader = new StreamReader(cs))
            {
                string jsonData = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(jsonData);
            }
        }
    }
}
