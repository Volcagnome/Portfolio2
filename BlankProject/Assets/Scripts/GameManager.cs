using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    // UI menus:
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    // Crouch windows:
    [SerializeField] public GameObject crouchWindow;
    [SerializeField] public GameObject proneWindow;

    [SerializeField] float totalTime = 600;
    [SerializeField] TMP_Text timer;
    [SerializeField] GameObject selfDestructTimer;
    [SerializeField] int commandCodesInLevel;
    float minutes;
    float seconds;
    string timeLeft;

    [SerializeField] TMP_Text level_1_passwordDisplay;
    [SerializeField] TMP_Text level_2_passwordDisplay;
    [SerializeField] TMP_Text commandCodesText;
    [SerializeField] GameObject PickupMessageWindow;
    [SerializeField] TMP_Text PickupMessage;

    [SerializeField] TMP_Text leverCountText;
    // UI elements:
    public Image staminaBar;
    public Image healthbar;
    public GameObject redFlash;
    public GameObject webbedOverlay;

    public GameObject playerSpawn;
    int currentLevel;

    // Player Scripts:
    public GameObject player;
    public playerMovement playerScript;
    public playerCrouch crouchScript;
    public playerDamage damageScript;
    bool isRespawning;

    private int activeLevers;
    public bool youWin;
    private bool playerEscaped;
    private bool selfDestructActivated;

    public bool isPaused;

    int commandCodesCollected;
    int commandCodesEntered;

    int securityPasswordLevel_1;
    int securityPasswordLevel_2;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerSpawn = GameObject.FindWithTag("Player Spawn");

        playerScript = player.GetComponent<playerMovement>();
        crouchScript = player.GetComponent<playerCrouch>();
        damageScript = player.GetComponent<playerDamage>();
        GameManager.instance.DisplayCommandCodes();
        
        securityPasswordLevel_1 = 0;
        currentLevel = 0;
    }

    //Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(isPaused);
            }
            else if (menuActive == menuPause)
            {
                stateUnpaused();
            }
        }

        if(selfDestructActivated)
        {
            BeginCountdown();
        }

        if(GameManager.instance.GetCommandCodesCollected() == 2)
        {
            GameManager.instance.UpdateWinCondition();
        }
    }

    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void stateUnpaused()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(isPaused);
        menuActive = null;
    }

    public void UpdateWinCondition()
    {
        
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(true);

    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void PickedUpCommandCode()
    {
        commandCodesCollected++;
        commandCodesText.text = commandCodesCollected.ToString("F0");
        commandCodesInLevel--;
    }

    public void PlugInCode()
    {
        if (commandCodesCollected > 0)
        {
            commandCodesEntered++;
            commandCodesCollected--;
        }
        
    }
    private void BeginCountdown()
    {
        if (totalTime > 0)
        {
            totalTime -= Time.deltaTime;

            minutes = Mathf.FloorToInt(totalTime / 60);

            seconds = Mathf.FloorToInt(totalTime % 60);

            selfDestructTimer.SetActive(true);

            timeLeft = string.Format("{0:00}:{1:00}", minutes, seconds);
            timer.text = timeLeft;  
        }
        else
        {

        }
    }
   public void DisplayCommandCodes()
    {
        
    }

    public void DisplayPasswords()
    {
        if (securityPasswordLevel_1 != 0)
        {
            level_1_passwordDisplay.text = securityPasswordLevel_1.ToString();
        }

    }
    // Getters / setters:
    public int GetCommandCodesEntered() { return commandCodesEntered; }

    public int GetCommandCodesCollected() { return commandCodesCollected; }

    public void ActivateSelfDestruct() { selfDestructActivated = true; }

    public bool GetSelfDestructActivated() { return selfDestructActivated; }

    public string GetTimeLeft() { return timeLeft; }

    public int GetPasswordLevel1() { return securityPasswordLevel_1; }

    public int GetPasswordLevel2() { return securityPasswordLevel_2; }

    public void SetSecurtyPasswordLevel1(int password) { securityPasswordLevel_1 = password; }

    public void SetSecurtyPasswordLevel2(int password) { securityPasswordLevel_2 = password; }

    public int GetCurrentLevel() { return currentLevel; }

    public void SetCurrentLevel(int level) { currentLevel = level; }
    public void SetPlayerSpawn(GameObject spawner) { playerSpawn = spawner;}

    public bool GetIsRespawning() { return isRespawning; }  
    

    public IEnumerator RespawnBuffer()
    {
        isRespawning = true;

        yield return new WaitForSeconds(1f);

        isRespawning = false;
    }
}
