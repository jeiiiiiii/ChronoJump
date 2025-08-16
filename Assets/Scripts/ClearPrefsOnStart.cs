using UnityEngine;

public class ClearPrefsOnStart : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("✅ PlayerPrefs cleared.");
    }
}
