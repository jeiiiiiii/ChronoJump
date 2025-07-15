using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class CoordinateSelect : MonoBehaviour
{
    public void BacktoChapterSelect()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

        public void LoadGameScene()
    {
        SceneManager.LoadScene("SumerianSceneOne");
    }

}
