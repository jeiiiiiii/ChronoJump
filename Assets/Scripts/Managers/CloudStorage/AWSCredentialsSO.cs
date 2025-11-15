using UnityEngine;

/// <summary>
/// ScriptableObject to store AWS credentials
/// This file will be stored in Assets/Resources/Private/ which is gitignored
/// </summary>
[CreateAssetMenu(fileName = "AWSCredentials", menuName = "Config/AWS Credentials", order = 1)]
public class AWSCredentialsSO : ScriptableObject
{
    [Header("🔐 AWS Credentials - This file should be in .gitignore!")]
    [Tooltip("Your AWS Access Key ID")]
    [SerializeField] private string accessKey = "";
    
    [Tooltip("Your AWS Secret Access Key")]
    [SerializeField] private string secretKey = "";
    
    [Tooltip("AWS Region (e.g., ap-southeast-1)")]
    [SerializeField] private string region = "ap-southeast-1";
    
    [Tooltip("Your S3 Bucket Name")]
    [SerializeField] private string bucketName = "";

    // Public properties (read-only)
    public string AccessKey => accessKey;
    public string SecretKey => secretKey;
    public string Region => region;
    public string BucketName => bucketName;

    /// <summary>
    /// Validate that all required credentials are set
    /// </summary>
    public bool HasValidCredentials()
    {
        return !string.IsNullOrEmpty(accessKey) && 
               !string.IsNullOrEmpty(secretKey) && 
               !string.IsNullOrEmpty(region) && 
               !string.IsNullOrEmpty(bucketName);
    }

    private void OnValidate()
    {
        // Warn in editor if credentials are missing
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            Debug.LogWarning("⚠️ AWS credentials are not fully configured in this ScriptableObject!");
        }
    }
}