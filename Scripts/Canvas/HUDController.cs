using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HUDController : MonoBehaviour
{
    //---------------EVENTS-----------------------------------------------------------------------------------------------------

    public delegate void StartMatchHandler();
    public event StartMatchHandler OnStartMatch;

    //---------------------------------------------------------------------------------------------------------------------------------------

    [Header("Screens")]
    [SerializeField] private GameObject inGameScreen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject serverScreen;
    [SerializeField] private GameObject clientScreen;
    [SerializeField] private GameObject spawnScreen;
    [SerializeField] private GameObject exitScreen;

    //Text Fields
    [Header("UI Elements")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text weaponNameText;
    [SerializeField] private Text weaponAmmunitionText;
    [SerializeField] private Text serverPlayersText;
    [SerializeField] private Text clientPlayersText;
    [SerializeField] private Text AlertText;
    [SerializeField] private RectTransform weaponReloadBar;
    [SerializeField] private GameObject sniperAim;

    
    private void Start()
    {
        ShowScreen("");
        sniperAim.SetActive(false);
        AlertText.gameObject.SetActive(false);
    }

    public bool SniperAimVisibility { set { sniperAim.SetActive(value); } }


    //Text Getter "Setter"
    public float Health
    {
        set
        {
            healthText.text = "Health " + Mathf.CeilToInt(value); 
        }
    }
    
    public int Players ////set the number of players on the canvas
    {
        set
        {
            serverPlayersText.text = "Players: " + value;
            clientPlayersText.text = "Players: " + value;
        }
    }



    public void UpdateWeapon(Weapon weapon)
    {
        if (weapon == null)
        {
            weaponNameText.enabled = false;
            weaponAmmunitionText.enabled = false;
            weaponReloadBar.localScale = new Vector3(0, 1, 1);
        }
        else
        {
            weaponNameText.enabled = true;
            weaponAmmunitionText.enabled = true;

            weaponNameText.text = weapon.Name;
            weaponAmmunitionText.text = weapon.ClipAmmunition + " / " + weapon.TotalAmmuition;

            if (weapon.ReloadTimer > 0)
            {
                weaponReloadBar.localScale = new Vector3(weapon.ReloadTimer / weapon.RealoadDuration, 1, 1);
            }
            else
            {
                weaponReloadBar.localScale = new Vector3(0, 1, 1);
            }
        }
    }

    public void ShowScreen(string screenName)
    {
        inGameScreen.SetActive(screenName == "inGame");
        gameOverScreen.SetActive(screenName == "gameOver");
        serverScreen.SetActive(screenName == "server");
        clientScreen.SetActive(screenName == "client");
        spawnScreen.SetActive(screenName == "spawn");
        exitScreen.SetActive(screenName == "exit");
    }

    public void OnPressedStartMatch()
    {
        if (OnStartMatch != null)
        {
            OnStartMatch();
            
        }
    }
    public void Alert()
    {
        AlertText.gameObject.SetActive(true);
        Invoke("HideAlert", 3);
    }
    public void HideAlert()
    {
        AlertText.gameObject.SetActive(false);
    }
}
