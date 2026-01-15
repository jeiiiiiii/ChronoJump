using UnityEngine;
using UnityEngine.SceneManagement;


public class EgyptChapterCoordinate : MonoBehaviour
{
    public void Kingdom()
    {
        SceneManager.LoadScene("KingdomCharacterScene");
    }
    public void Nile()
    {
        SceneManager.LoadScene("NileCharacterScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("EgyptCodexScene");
    }
    public void BackCharacterCoordinate()
    {
        SceneManager.LoadScene("EgyptCharacterCoordinate");
    }
}
