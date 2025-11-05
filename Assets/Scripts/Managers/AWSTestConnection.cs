using UnityEngine;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

public class AwsTest : MonoBehaviour
{
    private async void Start()
    {
        try
        {
            var s3Client = new AmazonS3Client(
                "AKIA4YE2TN35QXF2RJN5",
                "V7rMbHy9ruoCiOT4P1xJhmUo+MqCTEDrkkt/N26V",
                RegionEndpoint.APSoutheast1   // change if needed
            );

            var response = await s3Client.ListBucketsAsync();
            foreach (var bucket in response.Buckets)
            {
                Debug.Log("✅ Bucket found: " + bucket.BucketName);
            }
            Debug.Log("✅ AWS SDK working!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ AWS test failed: " + ex);
        }
    }
}
