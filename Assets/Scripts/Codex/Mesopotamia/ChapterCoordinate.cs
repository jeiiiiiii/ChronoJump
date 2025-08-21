using UnityEngine;
using UnityEngine.SceneManagement;


public class ChapterCoordinate : MonoBehaviour
{
    public void Sumerian()
    {
        SceneManager.LoadScene("SumerianCharacterScene");
    }
    public void Akkadian()
    {
        SceneManager.LoadScene("AkkadianCharacterScene");
    }
    public void Babylonian()
    {
        SceneManager.LoadScene("BabylonianCharacterScene");
    }
    public void Assyrian()
    {
        SceneManager.LoadScene("AssyrianCharacterScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("CodexScene");
    }
    public void BackCharacterCoordinate()
    {
        SceneManager.LoadScene("CharacterCoordinate");
    }
}
