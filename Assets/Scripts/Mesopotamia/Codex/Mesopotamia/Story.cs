using UnityEngine;
using UnityEngine.SceneManagement;


public class Story : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene("StoryCoordinate");
    }
}
