using UnityEngine;
using UnityEngine.SceneManagement;

public class CivilizationLoader : MonoBehaviour
{
    public void LoadAkkadian()
    {
        if (PlayerProgressManager.IsCivilizationUnlocked("Akkadian"))
        {
            SceneManager.LoadScene("AkkadianSceneOne");
        }
    }

    public void LoadSumerian()
    {
        SceneManager.LoadScene("SumerianSceneOne");
    }

    public void LoadBabylonian()
    {
        if (PlayerProgressManager.IsCivilizationUnlocked("Babylonian"))
        {
            SceneManager.LoadScene("BabylonianSceneOne");
        }
    }

    public void LoadAssyrian()
    {
        if (PlayerProgressManager.IsCivilizationUnlocked("Assyrian"))
        {
            SceneManager.LoadScene("AssyrianSceneOne");
        }
    }
}
