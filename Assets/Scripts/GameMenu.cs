using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public void OnRetryButton()
    {
        PlayerPrefs.SetInt("Seed", GameManager.CurrentSeed);
        SceneManager.LoadScene("Game");
    }
    
    public void OnStartButton()
    {
        SceneManager.LoadScene("Game");
    }

    public void OnMenuButton()
    {
        SceneManager.LoadScene("Menu");
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
