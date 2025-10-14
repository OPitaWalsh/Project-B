using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //variables
    public static GameManager instance { get; private set; }
    public int maxHP { get; private set; }
    public int currHP { get; private set; }
    public Food item { get; private set; }
    public Weapon weapon1 { get; private set; }
    public Weapon weapon2 { get; private set; }

    public float deadzone { get; private set; }

    //UI components
    [SerializeField]private Image healthbar;
    [SerializeField]private GameObject itemBox;
    [SerializeField]private GameObject weaponBox1;
    [SerializeField]private GameObject weaponBox2;
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
        item = null;
        weapon1 = null;  ////
        weapon2 = null;
        SyncHUD();

        deadzone = 0.5f;

        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        buttonSet.SetActive(false);
    }



    //move enemies after player
    public void MoveEnemies() {
        //tell each enemy to move
    }



    //variable display
    public void SyncHUD() {
        healthbar.fillAmount = currHP / (float)maxHP;
        if (item != null)
        {
            itemBox.GetComponentInChildren<Image>().sprite = item.GetComponent<SpriteRenderer>().sprite;
        }
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

    public void SetItem(Food f) {
        item = f;
        SyncHUD();
    }

    public void SetWeapon1(Weapon w) {
        if (weapon1 != null) { SetWeapon2(weapon1); } //move first item to second place if applicable
        weapon1 = w;
        SyncHUD();
    }

    public void SetWeapon2(Weapon w) {
        weapon2 = w;
        SyncHUD();
    }

    public void SwitchWeapon() {
        Weapon a = weapon1;
        Weapon b = weapon2;
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
