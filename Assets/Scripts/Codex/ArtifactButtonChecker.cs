using UnityEngine;
using UnityEngine.UI;

public class ArtifactButtonChecker : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.GetInt("UsePowerArtifactUsed", 0) == 1)
        {
            gameObject.SetActive(false);
        }
    }
}