using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //variables
    public static GameManager instance { get; private set; }
    public int maxHP { get; private set; }
    public int currHP { get; private set; }
    public int item { get; private set; }
    public int weapon1 { get; private set; }
    public int weapon2 { get; private set; }

    public float deadzone { get; private set; }

    //UI components
    [SerializeField]private Image healthbar;
    [SerializeField]private GameObject winScreen;
    [SerializeField]private GameObject loseScreen;
    [SerializeField]private GameObject buttonSet;



    private void Awake()
    {
        if (instance != null && instance != this) {
            Destroy(gameObject);
        }
        else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        maxHP = currHP = 5;
        item = 0;
        weapon1 = 1;
        weapon2 = 0;
        SyncHUD();

        deadzone = 0.5f;

        /*
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        buttonSet.SetActive(false);
        */
    }



    //move enemies after player
    public void MoveEnemies() {
        //tell each enemy to move
    }



    //variable display
    public void SyncHUD() {
        //healthbar.fillAmount = currHP / (float)maxHP;
        //show item
        //show weapon1
        //show weapon2
    }


    //variable control
    public void HPDown(int amount) {
        currHP -= amount;
        SyncHUD();
        if (currHP < 1)
            LoseLevel();
    }

    public void HPUp(int amount) {
        currHP += amount;
        if (currHP > maxHP)
            currHP = maxHP;
        SyncHUD();
    }

    public void SetItem(int i) {
        item = i;
        SyncHUD();
    }

    public void SetWeapon1(int i) {
        weapon1 = i;
        SyncHUD();
    }

    public void SetWeapon2(int i) {
        weapon2 = i;
        SyncHUD();
    }

    public void SwitchWeapon() {
        int a = weapon1;
        int b = weapon2;
        weapon1 = b;
        weapon2 = a;
        SyncHUD();
    }



    //win/lose
    public void WinLevel() {
        winScreen.SetActive(true);
        buttonSet.SetActive(true);
    }

    public void LoseLevel() {
        loseScreen.SetActive(true);
        buttonSet.SetActive(true);
    }


    //menu buttons
    public void RestartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Start();
    }

    public void QuitToMainMenu() {
        SceneManager.LoadScene("MainMenu");
        Destroy(gameObject);
    }
}
