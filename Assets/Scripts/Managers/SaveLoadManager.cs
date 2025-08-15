using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    public void BacktoMainMenu()
    {
        SceneManager.LoadScene("TitleSCreen");
    }
}


