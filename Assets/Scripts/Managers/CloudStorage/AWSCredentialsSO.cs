using UnityEngine;
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

[CreateAssetMenu(fileName = "AWSCredentials", menuName = "Config/AWS Credentials (AES-256)", order = 1)]
public class AWSCredentialsSO : ScriptableObject
{
    [SerializeField] private string encryptionKey = "";
    [SerializeField] private string encryptedAccessKey = ""; 
    [SerializeField] private string encryptedSecretKey = "";
    [SerializeField] private string region = "ap-southeast-1";
    [SerializeField] private string bucketName = "";

    // Cached decrypted values
    private string decryptedAccessKey;
    private string decryptedSecretKey;
    private bool hasDecrypted = false;

    // Public properties
    public string AccessKey
    {
        get
        {
            if (!hasDecrypted) DecryptCredentials();
            return decryptedAccessKey;
        }
    }

    public string SecretKey
    {
        get
        {
            if (!hasDecrypted) DecryptCredentials();
            return decryptedSecretKey;
        }
    }

    public string Region => region;
    public string BucketName => bucketName;

    /// <summary>
    /// Decrypt the stored credentials using AES-256
    /// </summary>
    private void DecryptCredentials()
    {
        try
        {
            if (string.IsNullOrEmpty(encryptionKey))
            {
                Debug.LogError("❌ Encryption key is empty! Cannot decrypt credentials.");
                return;
            }

            if (encryptionKey.Length != 32)
            {
                Debug.LogError($"❌ Encryption key must be EXACTLY 32 characters for AES-256! Current: {encryptionKey.Length}");
                return;
            }

            if (!string.IsNullOrEmpty(encryptedAccessKey))
            {
                decryptedAccessKey = DecryptString(encryptedAccessKey, encryptionKey);
            }

            if (!string.IsNullOrEmpty(encryptedSecretKey))
            {
                decryptedSecretKey = DecryptString(encryptedSecretKey, encryptionKey);
            }

            hasDecrypted = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to decrypt AWS credentials: {e.Message}");
            Debug.LogError("💡 Make sure you're using the same encryption key that was used to encrypt!");
        }
    }

    /// <summary>
    /// Check if credentials are valid
    /// </summary>
    public bool HasValidCredentials()
    {
        if (!hasDecrypted) DecryptCredentials();
        return !string.IsNullOrEmpty(decryptedAccessKey) && 
               !string.IsNullOrEmpty(decryptedSecretKey) && 
               !string.IsNullOrEmpty(region) && 
               !string.IsNullOrEmpty(bucketName);
    }

    /// <summary>
    /// Encrypt a string using AES-256
    /// </summary>
    private static string EncryptString(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText)) return "";
        if (string.IsNullOrEmpty(key) || key.Length != 32)
        {
            Debug.LogError("❌ Encryption key must be exactly 32 characters!");
            return "";
        }

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV(); // Generate random IV for each encryption

            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            using (MemoryStream ms = new MemoryStream())
            {
                // Write IV at the beginning
                ms.Write(aes.IV, 0, aes.IV.Length);

                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    /// <summary>
    /// Decrypt a string using AES-256
    /// </summary>
    private static string DecryptString(string cipherText, string key)
    {
        if (string.IsNullOrEmpty(cipherText)) return "";
        if (string.IsNullOrEmpty(key) || key.Length != 32)
        {
            Debug.LogError("❌ Encryption key must be exactly 32 characters!");
            return "";
        }

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] buffer = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV from the beginning
            byte[] iv = new byte[aes.IV.Length];
            Array.Copy(buffer, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            using (MemoryStream ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length))
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

#if UNITY_EDITOR
    [Header("📝 Editor Tools - Enter Credentials Here")]
    [Space(10)]
    [TextArea(2, 3)]
    [SerializeField] private string plainAccessKey = "";
    
    [TextArea(2, 3)]
    [SerializeField] private string plainSecretKey = "";

    [ContextMenu("🔐 Encrypt Credentials")]
    private void EncryptCredentials()
    {
        if (string.IsNullOrEmpty(encryptionKey))
        {
            Debug.LogError("❌ Please set your Encryption Key first!");
            return;
        }

        if (encryptionKey.Length != 32)
        {
            Debug.LogError($"❌ Encryption key must be EXACTLY 32 characters! Current: {encryptionKey.Length}");
            Debug.LogError("💡 Try something like: 'MyCapstone2024SecretKey123456'");
            return;
        }

        if (string.IsNullOrEmpty(plainAccessKey) || string.IsNullOrEmpty(plainSecretKey))
        {
            Debug.LogWarning("⚠️ Please enter both Access Key and Secret Key in the plain text fields first!");
            return;
        }

        encryptedAccessKey = EncryptString(plainAccessKey, encryptionKey);
        encryptedSecretKey = EncryptString(plainSecretKey, encryptionKey);

        // Clear plain text fields for security
        plainAccessKey = "";
        plainSecretKey = "";

        Debug.Log("✅ Credentials encrypted with AES-256 successfully!");
        Debug.Log("🔒 Your keys are now ENCRYPTED (not just obfuscated)");
        Debug.Log($"🔑 Remember your encryption key: '{encryptionKey}' - you'll need it to decrypt!");
        
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }

    [ContextMenu("👁️ Decrypt and Display (Debug Only!)")]
    private void DecryptAndDisplay()
    {
        if (string.IsNullOrEmpty(encryptionKey))
        {
            Debug.LogError("❌ Encryption key is empty! Cannot decrypt.");
            return;
        }

        hasDecrypted = false; // Force re-decrypt
        DecryptCredentials();
        
        if (!string.IsNullOrEmpty(decryptedAccessKey))
        {
            Debug.Log($"Access Key: {decryptedAccessKey}");
            Debug.Log($"Secret Key: {decryptedSecretKey}");
            Debug.Log("⚠️ Remember: This is visible in your Console! Don't screenshot.");
        }
        else
        {
            Debug.LogError("❌ Decryption failed! Make sure you're using the correct encryption key.");
        }
    }

    [ContextMenu("🔑 Generate Random 32-Character Key")]
    private void GenerateRandomKey()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        System.Random random = new System.Random();
        char[] key = new char[32];
        
        for (int i = 0; i < 32; i++)
        {
            key[i] = chars[random.Next(chars.Length)];
        }
        
        encryptionKey = new string(key);
        Debug.Log($"✅ Generated random 32-character encryption key: {encryptionKey}");
        Debug.Log("⚠️ SAVE THIS KEY! You won't be able to decrypt without it!");
        
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("🧹 Clear All Credentials")]
    private void ClearCredentials()
    {
        encryptedAccessKey = "";
        encryptedSecretKey = "";
        plainAccessKey = "";
        plainSecretKey = "";
        decryptedAccessKey = "";
        decryptedSecretKey = "";
        encryptionKey = "";
        hasDecrypted = false;

        Debug.Log("🧹 All credentials cleared from this asset");
        
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }

    [ContextMenu("🔄 Test Encrypt/Decrypt")]
    private void TestEncryptDecrypt()
    {
        if (string.IsNullOrEmpty(encryptionKey))
        {
            Debug.LogError("❌ Set an encryption key first!");
            return;
        }

        if (encryptionKey.Length != 32)
        {
            Debug.LogError($"❌ Key must be 32 characters! Current: {encryptionKey.Length}");
            return;
        }

        string testString = "TestKey123!@#";
        string encrypted = EncryptString(testString, encryptionKey);
        string decrypted = DecryptString(encrypted, encryptionKey);

        Debug.Log($"Original: {testString}");
        Debug.Log($"Encrypted: {encrypted}");
        Debug.Log($"Decrypted: {decrypted}");
        Debug.Log(testString == decrypted ? "✅ AES-256 Encryption working correctly!" : "❌ Encryption FAILED!");
    }

    private void OnValidate()
    {
        // Reset decrypted cache when asset is modified
        hasDecrypted = false;
        
        // Warn if key length is wrong
        if (!string.IsNullOrEmpty(encryptionKey) && encryptionKey.Length != 32)
        {
            Debug.LogWarning($"⚠️ Encryption key must be 32 characters! Current: {encryptionKey.Length}");
        }
    }
#endif
}
