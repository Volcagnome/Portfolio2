using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public playerDamage damageScript;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    [SerializeField] public GameObject crouchWindow;
    [SerializeField] public GameObject proneWindow;

    [SerializeField] float totalTime = 600;
    [SerializeField] TMP_Text timer;
    [SerializeField] GameObject selfDestructTimer;
    float minutes;
    float seconds;
    string timeLeft;

    [SerializeField] TMP_Text leverCountText;
    public Image staminaBar;
    public Image healthbar;
    public GameObject redFlash;

    public GameObject playerSpawn;
    public GameObject player;
    public playerMovement playerScript;
    public playerCrouch crouchScript;
    private int activeLevers;
    public bool youWin;
    private bool playerEscaped;
    private bool selfDestructActivated;

    public bool isPaused;

    int commandCodesCollected;
    int commandCodesEntered;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerMovement>();
        crouchScript = player.GetComponent<playerCrouch>();
        playerSpawn = GameObject.FindWithTag("Player Spawn");
        damageScript = GameManager.instance.GetComponent<playerDamage>();
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

    public void UpdateWinCondition(int lever)
    { 
        if (playerEscaped)
        {
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
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

    public int GetCommandCodesEntered() { return commandCodesEntered; }

    public int GetCommandCodesCollected() { return commandCodesCollected; }

    public void ActivateSelfDestruct() { selfDestructActivated = true; }

    public bool GetSelfDestructActivated() { return selfDestructActivated; }

    public string GetTimeLeft() { return timeLeft; }
}
