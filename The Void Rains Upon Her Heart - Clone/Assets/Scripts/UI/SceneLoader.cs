using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader: MonoBehaviour
{
  

    public void LoadNewScene(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
