using UnityEngine;
using UnityEngine.SceneManagement;


public class ChinaStoryCoordinate : MonoBehaviour
{
    public void HuangHe()
    {
        SceneManager.LoadScene("HuangHeStoryScene");
    }
    public void Shang()
    {
        SceneManager.LoadScene("ShangStoryScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("ChinaCodexScene");
    }
    public void BackStoryCoordinate()
    {
        SceneManager.LoadScene("ChinaStoryCoordinate");
    }
}
