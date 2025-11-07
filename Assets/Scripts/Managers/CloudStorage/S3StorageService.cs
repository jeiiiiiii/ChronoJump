using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Runtime;

/// <summary>
/// Handles S3 operations for storing and retrieving story assets (images and voices)
/// </summary>
public class S3StorageService : MonoBehaviour
{
    private static S3StorageService _instance;
    public static S3StorageService Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("S3StorageService");
                _instance = go.AddComponent<S3StorageService>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private AmazonS3Client _s3Client;
    private bool _isInitialized = false;
    
    public bool IsReady => _isInitialized && _s3Client != null;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeS3Client();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize S3 client with secure credentials
    /// </summary>
    private void InitializeS3Client()
    {
        try
        {
            string accessKey = AWSConfig.Instance.GetAccessKey();
            string secretKey = AWSConfig.Instance.GetSecretKey();
            string region = AWSConfig.Instance.GetRegion();

            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            {
                Debug.LogError("❌ AWS credentials not available");
                return;
            }

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var regionEndpoint = RegionEndpoint.GetBySystemName(region);
            
            _s3Client = new AmazonS3Client(credentials, regionEndpoint);
            _isInitialized = true;
            
            Debug.Log($"✅ S3 client initialized for region: {region}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to initialize S3 client: {ex.Message}");
            _isInitialized = false;
        }
    }

    /// <summary>
    /// Upload background image to S3
    /// </summary>
    public async Task<string> UploadBackgroundImage(Texture2D texture, string teacherId, int storyIndex)
    {
        return await UploadImageToS3(texture, teacherId, storyIndex, "background");
    }

    /// <summary>
    /// Upload character 1 image to S3
    /// </summary>
    public async Task<string> UploadCharacter1Image(Texture2D texture, string teacherId, int storyIndex)
    {
        return await UploadImageToS3(texture, teacherId, storyIndex, "char1");
    }

    /// <summary>
    /// Upload character 2 image to S3
    /// </summary>
    public async Task<string> UploadCharacter2Image(Texture2D texture, string teacherId, int storyIndex)
    {
        return await UploadImageToS3(texture, teacherId, storyIndex, "char2");
    }

    /// <summary>
    /// Upload voice audio file to S3
    /// </summary>
    public async Task<string> UploadVoiceAudio(byte[] audioData, string teacherId, int storyIndex, string dialogueId)
    {
        string key = $"voices/{teacherId}/story_{storyIndex}/{dialogueId}.wav";
        return await UploadFileToS3(audioData, key, "audio/wav");
    }

    /// <summary>
    /// Generic image upload method
    /// </summary>
    private async Task<string> UploadImageToS3(Texture2D texture, string teacherId, int storyIndex, string imageType)
    {
        try
        {
            if (!IsReady)
            {
                Debug.LogError("❌ S3 client not ready");
                return null;
            }

            if (texture == null)
            {
                Debug.LogError("❌ Texture is null");
                return null;
            }

            // Convert texture to bytes
            byte[] imageBytes = texture.EncodeToPNG();
            
            // Generate S3 key (path)
            string key = $"images/{teacherId}/story_{storyIndex}/{imageType}.png";
            
            // Upload to S3
            string url = await UploadFileToS3(imageBytes, key, "image/png");
            
            if (!string.IsNullOrEmpty(url))
            {
                Debug.Log($"✅ Image uploaded to S3: {url}");
            }
            
            return url;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to upload image to S3: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Generic file upload to S3
    /// </summary>
    private async Task<string> UploadFileToS3(byte[] fileData, string key, string contentType)
    {
        try
        {
            if (!IsReady)
            {
                Debug.LogError("❌ S3 client not ready");
                return null;
            }

            string bucketName = AWSConfig.Instance.GetBucketName();

            using (var stream = new MemoryStream(fileData))
            {
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = contentType,
                };

                var response = await _s3Client.PutObjectAsync(request);
                
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Generate public URL
                    string url = $"https://{bucketName}.s3.{AWSConfig.Instance.GetRegion()}.amazonaws.com/{key}";
                    return url;
                }
                else
                {
                    Debug.LogError($"❌ S3 upload failed with status: {response.HttpStatusCode}");
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ S3 upload error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Download image from S3 URL
    /// </summary>
    public async Task<Texture2D> DownloadImage(string s3Url)
    {
        try
        {
            if (!IsReady)
            {
                Debug.LogError("❌ S3 client not ready");
                return null;
            }

            byte[] fileData = await DownloadFileFromS3(s3Url);
            if (fileData == null) return null;

            // Create texture from downloaded data
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                Debug.Log($"✅ Image downloaded successfully: {texture.width}x{texture.height}");
                return texture;
            }
            else
            {
                Debug.LogError("❌ Failed to load image from downloaded data");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to download image: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Download voice audio from S3 URL
    /// </summary>
    public async Task<byte[]> DownloadVoiceAudio(string s3Url)
    {
        return await DownloadFileFromS3(s3Url);
    }

    /// <summary>
    /// Generic file download from S3
    /// </summary>
    private async Task<byte[]> DownloadFileFromS3(string s3Url)
    {
        try
        {
            if (!IsReady)
            {
                Debug.LogError("❌ S3 client not ready");
                return null;
            }

            // Extract bucket and key from URL
            if (!TryParseS3Url(s3Url, out string bucketName, out string key))
            {
                Debug.LogError($"❌ Invalid S3 URL: {s3Url}");
                return null;
            }

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using (var response = await _s3Client.GetObjectAsync(request))
            using (var stream = new MemoryStream())
            {
                await response.ResponseStream.CopyToAsync(stream);
                Debug.Log($"✅ File downloaded successfully: {key} ({stream.Length} bytes)");
                return stream.ToArray();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to download file from S3: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Delete file from S3
    /// </summary>
    public async Task<bool> DeleteImage(string s3Url)
    {
        try
        {
            if (!IsReady)
            {
                Debug.LogError("❌ S3 client not ready");
                return false;
            }

            if (!TryParseS3Url(s3Url, out string bucketName, out string key))
            {
                Debug.LogError($"❌ Invalid S3 URL: {s3Url}");
                return false;
            }

            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.DeleteObjectAsync(request);
            
            if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
            {
                Debug.Log($"✅ File deleted from S3: {key}");
                return true;
            }
            else
            {
                Debug.LogError($"❌ S3 delete failed with status: {response.HttpStatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to delete file from S3: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Parse S3 URL to extract bucket and key
    /// </summary>
    private bool TryParseS3Url(string url, out string bucketName, out string key)
    {
        bucketName = null;
        key = null;

        try
        {
            if (string.IsNullOrEmpty(url)) return false;

            Uri uri = new Uri(url);
            
            // Handle different S3 URL formats
            if (uri.Host.EndsWith(".amazonaws.com"))
            {
                // Format: https://bucket-name.s3.region.amazonaws.com/key
                bucketName = uri.Host.Split('.')[0];
                key = uri.AbsolutePath.TrimStart('/');
                return true;
            }
            else if (uri.Host == "s3.amazonaws.com")
            {
                // Format: https://s3.amazonaws.com/bucket-name/key
                string[] segments = uri.AbsolutePath.Split('/');
                if (segments.Length >= 3)
                {
                    bucketName = segments[1];
                    key = string.Join("/", segments, 2, segments.Length - 2);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to parse S3 URL: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Test S3 connection by listing buckets
    /// </summary>
    public async Task TestS3Connection()
    {
        try
        {
            if (!IsReady)
            {
                Debug.LogError("❌ S3 client not ready");
                return;
            }

            var response = await _s3Client.ListBucketsAsync();
            Debug.Log($"✅ S3 connection successful! Found {response.Buckets.Count} buckets");
            
            foreach (var bucket in response.Buckets)
            {
                Debug.Log($"   📂 {bucket.BucketName} (Created: {bucket.CreationDate})");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ S3 connection test failed: {ex.Message}");
            throw;
        }
    }

    private void OnDestroy()
    {
        _s3Client?.Dispose();
    }
}