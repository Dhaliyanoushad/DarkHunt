
using UnityEngine;

public class MenuScreenSwitcher : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject howToPlayScreen;

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
