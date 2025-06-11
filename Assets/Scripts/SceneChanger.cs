using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
public void LoadMainMenu()
{
    SceneManager.LoadScene("MainMenu");
}

public void LoadMainGame()
{
    SceneManager.LoadScene("MainGame");
}


    public void QuitGame()
    {
        Application.Quit();
    }
}
