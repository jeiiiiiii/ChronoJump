using UnityEngine;
using UnityEngine.SceneManagement;

public class CreatorModeManager : MonoBehaviour
{
    public void BacktoMainMenu()
    {
        SceneManager.LoadScene("TitleSCreen");
    }
}


