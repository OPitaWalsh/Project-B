using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    // Buttons
    public void PlayLevel1() {
        SceneManager.LoadScene("Level1");
    }
    public void MainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
    public void Options() {
        SceneManager.LoadScene("Credits");
    }
    public void QuitGame() {
        Application.Quit();
    }
}
