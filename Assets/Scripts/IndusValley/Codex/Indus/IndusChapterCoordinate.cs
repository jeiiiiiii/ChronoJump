using UnityEngine;
using UnityEngine.SceneManagement;


public class IndusChapterCoordinate : MonoBehaviour
{
    public void Harappa()
    {
        SceneManager.LoadScene("HarappaCharacterScene");
    }
    public void Sining()
    {
        SceneManager.LoadScene("SiningCharacterScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("IndusCodexScene");
    }
    public void BackCharacterCoordinate()
    {
        SceneManager.LoadScene("IndusCharacterCoordinate");
    }
}
