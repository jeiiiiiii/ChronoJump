using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionManager : MonoBehaviour
{
    public void BacktoMainMenu()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}


