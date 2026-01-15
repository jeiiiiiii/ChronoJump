using UnityEngine;

/// <summary>
/// Manages AWS credentials securely from a ScriptableObject
/// NEVER stores credentials in scene files - they're in a separate asset that's gitignored
/// </summary>
public class AWSConfig : MonoBehaviour
{
    private static AWSConfig _instance;
    public static AWSConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<AWSConfig>();
                
                if (_instance == null)
                {
                    Debug.LogError("❌ AWSConfig not found in scene! Please add it to your scene.");
                }
            }
            return _instance;
        }
    }

    [Header("⚠️ DO NOT put credentials here! Use the ScriptableObject instead")]
    [SerializeField] private AWSCredentialsSO credentialsAsset;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            ValidateCredentials();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void ValidateCredentials()
    {
        if (credentialsAsset == null)
        {
            Debug.LogError("❌ AWSCredentialsSO asset not assigned! Please assign it in the Inspector.");
            return;
        }

        if (!credentialsAsset.HasValidCredentials())
        {
            Debug.LogError("❌ AWS credentials not configured in the ScriptableObject!");
        }
        else
        {
            Debug.Log("✅ AWS credentials loaded successfully");
        }
    }

    public string GetAccessKey() => credentialsAsset?.AccessKey ?? "";
    public string GetSecretKey() => credentialsAsset?.SecretKey ?? "";
    public string GetRegion() => credentialsAsset?.Region ?? "ap-southeast-1";
    public string GetBucketName() => credentialsAsset?.BucketName ?? "";

    public bool HasValidCredentials()
    {
        return credentialsAsset != null && credentialsAsset.HasValidCredentials();
    }
}
