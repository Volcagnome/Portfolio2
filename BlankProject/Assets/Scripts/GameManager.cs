using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static StaticData;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Audio player:
    [SerializeField] AudioSource audioPlayer;

    // UI menus:
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    public GameObject tutorialWindow;
    public bool isPaused;

    //Self Destruct Timer
    [SerializeField] float totalTime = 600;
    [SerializeField] TMP_Text timer;
    [SerializeField] GameObject selfDestructTimer;
    float minutes;
    float seconds;
    string timeLeft;


    //Player Spawn Objects
    public GameObject playerSpawnEntry;
    public GameObject playerSpawnExit;
    public GameObject currentSpawn;

    // Player UI elements:
    [SerializeField] public GameObject crouchWindow;
    [SerializeField] public GameObject proneWindow;
    public Image staminaBar;
    public Image healthbar;
    public Image overheatMeter;
    public GameObject redFlash;
    public GameObject webbedOverlay;

    // Player Scripts:
    public GameObject player;
    public playerMovement playerScript;
    public playerCrouch crouchScript;
    public playerDamage damageScript;
    bool isRespawning;


    //Current Game State
    public bool youWin;
    private bool playerEscaped;
    private bool selfDestructActivated;

    int commandCodesCollected;
    int commandCodesEntered;
    int commandCodesInLevel;


    private bool wasDisabled;

    bool firstTimeInScene;



    //Pickup Info
    [SerializeField] GameObject healthPickup;
    [SerializeField] GameObject staminaPickup;
    [SerializeField] GameObject speedPickup;
    [SerializeField] GameObject damagePickup;
    [SerializeField] GameObject weaponPickup;
    [SerializeField] GameObject commandCodePickup;
    [SerializeField] GameObject securityPasswordPickup;

    

    [SerializeField] GameObject commandCodesCollectedTotalDisplay;

    [SerializeField] GameObject sceneCommandCodesCollectedDisplay;
    [SerializeField] GameObject sceneCommandCodesTotalDisplay;
    [SerializeField] int sceneCommandCodesTotal;

    [SerializeField] GameObject sceneStatPickupsCollectedDisplay;
    [SerializeField] GameObject sceneStatPickupsTotalDisplay;
    [SerializeField] int sceneStatPickupsTotal;

    [SerializeField] GameObject sceneWeaponPickupsCollectedDisplay;
    [SerializeField] GameObject sceneWeaponPickupsTotalDisplay;
    [SerializeField] int sceneWeaponPickupsTotal;

    [SerializeField] GameObject securityPasswordDisplay;
    [SerializeField] int sceneSecurityPassword;

    int sceneCommandCodesCollected;
    int sceneStatPickupsCollected;
    int sceneWeaponPickupsCollected;

    [SerializeField] int totalGameCommandCodes;
    int commandCodesCollectedTotal;
    
    [SerializeField] TMP_Text PickupMessage;


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");


        //If the game was just started, sets the current spawn point to the spawn point at the beginning of the level the game was started on.
        if (StaticData.isGameStart == true)
        { 
            currentSpawn = playerSpawnEntry = GameObject.FindWithTag("Player Spawn Entry");
            StaticData.isGameStart = false;
        }


        //Checks if the player has been to this scene before
        firstTimeInScene = StaticData.firstTimeInScene[SceneManager.GetActiveScene().buildIndex];


        //If so, loads the saved key item data (command codes/securtiy passwords), loads the previous states for all pickup platform objects
        //and loads previous states for all enemies.
        if (!firstTimeInScene)
        {
            DestroyDefaultObjects("Enemy");
            DestroyDefaultObjects("Pickup Platform");

            LoadPlayerPickupData(levelData[SceneManager.GetActiveScene().buildIndex]);

            int pickupListLength = StaticData.scenePickups[SceneManager.GetActiveScene().buildIndex].Count;
            if (pickupListLength > 0)
                LoadScenePickupData(StaticData.scenePickups[SceneManager.GetActiveScene().buildIndex]);

            int enemyListLength = StaticData.sceneEnemies[SceneManager.GetActiveScene().buildIndex].Count;
            if (enemyListLength > 0)
                LoadEnemyStates(StaticData.sceneEnemies[SceneManager.GetActiveScene().buildIndex]);
        }
        else
            StaticData.firstTimeInScene[SceneManager.GetActiveScene().buildIndex] = false;

        sceneCommandCodesCollectedDisplay.GetComponent<TMP_Text>().text = sceneCommandCodesCollected.ToString();
        sceneCommandCodesTotalDisplay.GetComponent<TMP_Text>().text = sceneCommandCodesTotal.ToString();
        commandCodesCollectedTotal = StaticData.commandCodesCollectedTotal_Static;
        commandCodesCollectedTotalDisplay.GetComponent<TMP_Text>().text = commandCodesCollectedTotal.ToString();

        sceneStatPickupsCollectedDisplay.GetComponent<TMP_Text>().text = sceneStatPickupsCollected.ToString();
        sceneStatPickupsTotalDisplay.GetComponent<TMP_Text>().text = sceneStatPickupsTotal.ToString();
        sceneWeaponPickupsCollectedDisplay.GetComponent<TMP_Text>().text = sceneWeaponPickupsCollected.ToString();
        sceneWeaponPickupsTotalDisplay.GetComponent<TMP_Text>().text = sceneWeaponPickupsTotal.ToString();



        playerSpawnEntry = GameObject.FindWithTag("Player Spawn Entry");
        playerSpawnExit = GameObject.FindWithTag("Player Spawn Exit");

        if (StaticData.previousLevel == true)
            currentSpawn = playerSpawnExit;
        else
            currentSpawn = playerSpawnEntry;

        playerScript = player.GetComponent<playerMovement>();
        crouchScript = player.GetComponent<playerCrouch>();
        damageScript = player.GetComponent<playerDamage>();
       

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
            selfDestructTimer.SetActive(true);
            timer.text = timeLeft;
            BeginCountdown();
        }

        //if(GameManager.instance.GetCommandCodesCollected() == 2)
        //{
        //    GameManager.instance.UpdateWinCondition();
        //}
    }

    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        tutorialWindow.SetActive(false);

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

    public void playAud(AudioClip sound, float vol)
    {
        // One shot is an instantiated piece of audio,
        // it allows us to play the sound over itself.
        audioPlayer.PlayOneShot(sound, vol);
    }

    public void PickedUpCommandCode()
    {
        sceneCommandCodesCollected++;
        sceneCommandCodesCollectedDisplay.GetComponent<TMP_Text>().text = sceneCommandCodesCollected.ToString();
        commandCodesCollectedTotal++;
        commandCodesCollectedTotalDisplay.GetComponent<TMP_Text>().text = commandCodesCollectedTotal.ToString();

        
    }

    public void PlugInCode()
    {
        if (sceneCommandCodesCollected > 0)
        {
            commandCodesEntered++;
            commandCodesCollectedTotal--;
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

            if (minutes == 0 && seconds < 30)
                timer.color = Color.red;

                timeLeft = string.Format("{0:00}:{1:00}", minutes, seconds);

            timer.text = timeLeft;  
        }
        else
        {

        }
    }


    public void SavePlayerPickupData()
    {
        playerPickupState state = new playerPickupState

        {
            sceneCommandCodesCollected_Static = sceneCommandCodesCollected,
            sceneCommandCodesTotal_Static = sceneCommandCodesTotal,
            sceneStatPickupsCollected_Static = sceneStatPickupsCollected,
            sceneStatPickupsTotal_Static = sceneStatPickupsTotal,
            sceneWeaponPickupsCollected_Static = sceneWeaponPickupsCollected,
            sceneWeaponPickupsTotal_Static = sceneWeaponPickupsTotal,
            sceneSecurityPassword_Static = sceneSecurityPassword

        };

        StaticData.levelData[SceneManager.GetActiveScene().buildIndex] = state;
        StaticData.commandCodesCollectedTotal_Static = StaticData.commandCodesCollectedTotal_Static + sceneCommandCodesCollected;
    }

    public void LoadPlayerPickupData(playerPickupState state)
    {
        sceneCommandCodesCollected = state.sceneCommandCodesCollected_Static;
        sceneCommandCodesTotal = state.sceneCommandCodesTotal_Static;
        sceneStatPickupsCollected = state.sceneStatPickupsCollected_Static;
        sceneStatPickupsTotal = state.sceneStatPickupsTotal_Static;
        sceneWeaponPickupsCollected = state.sceneWeaponPickupsCollected_Static;
        sceneWeaponPickupsTotal = state.sceneWeaponPickupsTotal_Static;
        sceneSecurityPassword = state.sceneSecurityPassword_Static;
        securityPasswordDisplay.GetComponent<TMP_Text>().text = sceneSecurityPassword.ToString();
 
    }

    public void DestroyDefaultObjects(string tag)
    {

        GameObject[] defaultObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in defaultObjects)
            Destroy(obj);
    }

    public void LoadScenePickupData(List <pickupState> scenePickups)
    {
        GameObject pickup = null;

        if (scenePickups.Count > 0)
        {
            scenePickups.ForEach(state =>
            {
                pickup = Instantiate(state.pickupPrefab_Static, state.pickupLocation_Static, state.pickupRotation_Static);
                pickup.GetComponent<itemPickup>().SetIfItemCollected(state.itemPickedUp_Static);
                pickup.GetComponent<itemPickup>().SetItemPickupStats(state.item_Static);
            });
        }
    }

    public void LoadEnemyStates(List<enemyState> sceneEnemyList)
    {
        GameObject enemy = null;

        if (sceneEnemyList.Count > 0)
        {
            sceneEnemyList.ForEach(state =>
            {

                enemy = Instantiate(state.enemyType, state.position, state.rotation);
                enemy.GetComponent<SharedEnemyAI>().UpdateEnemyUI();
                enemy.GetComponent<SharedEnemyAI>().SetMaxHP(state.maxHealth);
                enemy.GetComponent<SharedEnemyAI>().SetHP(state.health);
                if (state.isAlerted)
                    enemy.GetComponent<SharedEnemyAI>().AlertEnemy();
                enemy.GetComponent<SharedEnemyAI>().SetDefaultPost(GetEnemyPostObject(state.defaultPost));
                enemy.GetComponent<SharedEnemyAI>().SetCurrentDestination(GetEnemyPostObject(state.currentDestination));
                enemy.GetComponent<NavMeshAgent>().SetDestination(state.agentDestination);
                enemy.GetComponent<SharedEnemyAI>().SetLastKnownPlayerLocation(state.lastKnownPlayerLocation);
                enemy.GetComponent<SharedEnemyAI>().SetIsRespondingToAlert(state.isRespondingToAlert);
                enemy.GetComponent<SharedEnemyAI>().SetLoadedFromState(state.loadedFromState);
                enemy.GetComponent<SharedEnemyAI>().SetIsEndGameEnemy(state.isEndGameEnemy);

            });
        }
    }

    private GameObject GetEnemyPostObject(Vector3 location)
    {
        GameObject post = null;

        Collider[] enemyPosts = Physics.OverlapSphere(location, 1f);

        if (enemyPosts.Length > 0)
        {

            foreach (Collider enemyPost in enemyPosts)
            {
                if (enemyPost.gameObject.CompareTag("Patrol Route Start") || enemyPost.gameObject.CompareTag("Guard Post")
                    || enemyPost.gameObject.CompareTag("Titan Post") || enemyPost.gameObject.CompareTag("Patrol Waypoint")
                    || enemyPost.gameObject.CompareTag("Endgame Spawner") || enemyPost.gameObject.CompareTag("Reinforcement Spawner")
                    || enemyPost.gameObject.CompareTag("Spider Spawner") || enemyPost.gameObject.CompareTag("Boss Post"))
                {
                    post = enemyPost.gameObject;
                    break;
                }
            }
        }

        return post;

    }

    public GameObject GetPickupPrefab(itemPickup.platformType type)
    {
        GameObject pickupPrefab = null;

        if (type == itemPickup.platformType.health)
            pickupPrefab = healthPickup;
        else if (type == itemPickup.platformType.speed)
            pickupPrefab = speedPickup;
        else if (type == itemPickup.platformType.stamina)
            pickupPrefab = staminaPickup;
        else if (type == itemPickup.platformType.damage)
            pickupPrefab = damagePickup;
        else if (type == itemPickup.platformType.weapon)
            pickupPrefab = weaponPickup;
        else if (type == itemPickup.platformType.commandCode)
            pickupPrefab = commandCodePickup;
        else if (type == itemPickup.platformType.securityPassword)
            pickupPrefab = securityPasswordPickup;

        return pickupPrefab;
    }


    public void SecurityPasswordFound() 
    { 
        securityPasswordDisplay.GetComponent<TMP_Text>().text = sceneSecurityPassword.ToString();
    }

    public void IncrementSceneStatPickupCounter() 
    { 
        sceneStatPickupsCollected++;
        sceneStatPickupsCollectedDisplay.GetComponent<TMP_Text>().text = sceneStatPickupsCollected.ToString();

    }

    public void IncrementSceneWeaponPickupCounter()
    {
        sceneWeaponPickupsCollected++;
        sceneWeaponPickupsCollectedDisplay.GetComponent<TMP_Text>().text = sceneWeaponPickupsCollected.ToString();

    }


    public void ActivateSelfDestruct() 
    { 
        selfDestructActivated = true;

        foreach (var enemyStateList in sceneEnemies)
        {
            enemyStateList.Value.Clear();
        }
    }

    // Getters / setters:
    public int GetCommandCodesEntered() { return commandCodesEntered; }

    public int GetCommandCodesCollected() { return commandCodesCollectedTotal; }

    public bool GetFirstTimeInScene() { return firstTimeInScene; }  

    public bool GetIsRespawning() { return isRespawning; }


    public bool GetSelfDestructActivated() { return selfDestructActivated; }

    public int GetTotalGameCommandCodes() { return totalGameCommandCodes; }



    public IEnumerator RespawnBuffer()
    {
        isRespawning = true;

        yield return new WaitForSeconds(2f);

        isRespawning = false;
    }
}
