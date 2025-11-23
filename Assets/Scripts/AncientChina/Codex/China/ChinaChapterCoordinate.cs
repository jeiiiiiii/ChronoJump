using UnityEngine;
using UnityEngine.SceneManagement;


public class ChinaChapterCoordinate : MonoBehaviour
{
    public void HuangHe()
    {
        SceneManager.LoadScene("HuangHeCharacterScene");
    }
    public void Shang()
    {
        SceneManager.LoadScene("ShangCharacterScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("ChinaCodexScene");
    }
    public void BackCharacterCoordinate()
    {
        SceneManager.LoadScene("ChinaCharacterCoordinate");
    }
}
