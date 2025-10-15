using System.Collections.Generic;
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
    public PlayerControl player { get; private set; }
    public List<Enemy> enemies;

    //UI components
    [Header("Stats")]
    [SerializeField]private Image healthbar;
    [SerializeField]private GameObject itemBoxImageChild;
    [SerializeField]private GameObject weaponBox1ImageChild;
    [SerializeField]private GameObject weaponBox2ImageChild;
    
    [Header("Menus")]
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
            //DontDestroyOnLoad(gameObject);
            // this game functionally only has one level, and no use to persist data to menus, so may as well destroy it
        }
    }

    private void Start()
    {
        maxHP = currHP = 5;
        item = null;
        weapon1 = null;
        weapon2 = null;
        SyncHUD();

        deadzone = 0.5f;

        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        buttonSet.SetActive(false);
    }



    public void SetPlayer(PlayerControl p) {
        player = p;
        foreach (Enemy e in enemies) {
            e.player = p;
        }
    }



    //move enemies after player
    public void MoveEnemies() {
        foreach (Enemy e in enemies) {
            e.Tick();
        }
    }



    //variable display
    public void SyncHUD() {
        healthbar.fillAmount = currHP / (float)maxHP;
        
        if (item != null)
        {
            itemBoxImageChild.GetComponent<Image>().sprite = item.GetComponent<SpriteRenderer>().sprite;
        }
        else
        {
            itemBoxImageChild.GetComponent<Image>().sprite = null;
        }

        if (weapon1 != null)
        {
            weaponBox1ImageChild.GetComponent<Image>().sprite = weapon1.GetComponent<SpriteRenderer>().sprite;
        }
        else
        {
            weaponBox1ImageChild.GetComponent<Image>().sprite = null;
        }
        
        if (weapon2 != null)
        {
            weaponBox2ImageChild.GetComponent<Image>().sprite = weapon2.GetComponent<SpriteRenderer>().sprite;
        }
        else
        {
            weaponBox2ImageChild.GetComponent<Image>().sprite = null;
        }
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

    public void UseItem() {
        if (item != null) {
            HPUp(item.health);
            SetItem(null);
        }
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
