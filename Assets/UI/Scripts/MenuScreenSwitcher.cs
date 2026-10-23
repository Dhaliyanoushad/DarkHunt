
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScreenSwitcher : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject howToPlayScreen;

    private bool loading;

    public void Play()
    {
        if (loading) return;
        loading = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(LevelProgress.SelectionScene);
    }

    public void OpenHowToPlay()
    {
        mainMenu.SetActive(false);
        howToPlayScreen.SetActive(true);
    }

    public void BackToMenu()
    {
        howToPlayScreen.SetActive(false);
        mainMenu.SetActive(true);
    }
}
