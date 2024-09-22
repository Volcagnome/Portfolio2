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
    [SerializeField] GameObject uI;
    public GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject almostWinMenu;
    [SerializeField] GameObject SelfDestructFlavorText;
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
    public List<GameObject> weaponIcons;
    public List<Image> iconFills;
    [SerializeField] List<GameObject> iconOptions;
    [SerializeField] List<GameObject> fillOptions;

    // Player Scripts:
    public GameObject player;
    public playerMovement playerScript;
    public playerCrouch crouchScript;
    public playerDamage damageScript;
    public grenadeThrow grenadeScript;
    
    bool isRespawning;

    //Current Game State
    public bool youWin;
    private bool playerEscaped;
    private bool selfDestructActivated;

    private bool wasDisabled;

    bool firstTimeInScene = false;


    [Header("----- Pause Menu Components) -----")]

    //Pause Menu Player Stats
    [SerializeField] GameObject playerHealth;
    [SerializeField] GameObject playerSpeed;
    [SerializeField] GameObject playerStamina;
    [SerializeField] GameObject playerCurrentWeapon;
    [SerializeField] GameObject playerWeaponDamage;
    [SerializeField] GameObject playerWeaponDamageMult;
    [SerializeField] GameObject playerWeaponShootRate;
    [SerializeField] GameObject playerWeaponCoolRate;
    [SerializeField] GameObject playerWeaponCooldownTime;
    [SerializeField] GameObject playerWeaponMaxHeat;
    [SerializeField] GameObject playerXrayAbility;

    [SerializeField] GameObject GameObjectiveDisplay;


    //Pause Menu Level Pickup Info
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

    [SerializeField] GameObject sceneUpgradePickupsCollectedDisplay;
    [SerializeField] GameObject sceneUpgradePickupsTotalDisplay;
    [SerializeField] int sceneUpgradePickupsTotal;

    [SerializeField] GameObject sceneWeaponPickupsCollectedDisplay;
    [SerializeField] GameObject sceneWeaponPickupsTotalDisplay;
    [SerializeField] int sceneWeaponPickupsTotal;

    [SerializeField] GameObject securityPasswordDisplay;
    [SerializeField] int sceneSecurityPassword;

    int sceneCommandCodesCollected;
    int commandCodesEntered;
    int commandCodesCollectedTotal;

    int sceneUpgradePickupsCollected;
    int sceneWeaponPickupsCollected;

    [SerializeField] int totalGameCommandCodes;
 
    
    [SerializeField] GameObject messageWindow;
    public GameObject hintWindow;
    public bool hintOff = false;
 


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerSpawnEntry = GameObject.FindWithTag("Player Spawn Entry");
        playerSpawnExit = GameObject.FindWithTag("Player Spawn Exit");

        // Player Scripts assigned:
        playerScript = player.GetComponent<playerMovement>();
        crouchScript = player.GetComponent<playerCrouch>();
        damageScript = player.GetComponent<playerDamage>();
        grenadeScript = player.GetComponent<grenadeThrow>();
        

        selfDestructActivated = StaticData.selfDestructActivated_Static;
        totalTime = StaticData.totalTime_Static;

        //Checks if the player has been to this scene before
        firstTimeInScene = StaticData.firstTimeInScene[SceneManager.GetActiveScene().buildIndex];

        //If the game was just started, sets the current spawn point to the spawn point at the beginning of the level the game was started on.
        if (StaticData.isGameStart == true)
        {
            StaticData.gameObjective_Static = "Search the area for command codes.\r\n\r\nFind the password to bypass the security checkpoint.\r\n\r\nAdvance deeper into the facility until you reach the mainframe.";

            currentSpawn = playerSpawnEntry = GameObject.FindWithTag("Player Spawn Entry");
            StaticData.isGameStart = false;
        }
      
            if (StaticData.previousLevel == true)
            {
                currentSpawn = playerSpawnExit;
            }
            else if (StaticData.nextLevel == true)
                currentSpawn = playerSpawnEntry;
    
        if (SceneManager.GetActiveScene().name == "Main Scene Final Level" && !selfDestructActivated)
            StaticData.gameObjective_Static = "Defeat the Juggernaut.\r\n\r\nPlug in the command codes.\r\n\r\nActivate the self destruct protocol.";

            GameObjectiveDisplay.GetComponent<TMP_Text>().text = StaticData.gameObjective_Static;

            //Loads the saved key item data (command codes/security passwords), loads the previous states for all pickup platform objects
            //and loads previous states for all enemies.
            if (!firstTimeInScene || StaticData.selfDestructActivated_Static)
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

                if(SceneManager.GetActiveScene().name == "Main Scene Final Level" && !StaticData.bossIsDead_Static)
                    BossFight.instance.SetBoss(FindBoss());

            }
            else if (firstTimeInScene)
            {
                firstTimeInScene = false;
            }
      
        sceneCommandCodesCollectedDisplay.GetComponent<TMP_Text>().text = sceneCommandCodesCollected.ToString();
        sceneCommandCodesTotalDisplay.GetComponent<TMP_Text>().text = sceneCommandCodesTotal.ToString();
        commandCodesCollectedTotal = StaticData.commandCodesCollectedTotal_Static;
        commandCodesCollectedTotalDisplay.GetComponent<TMP_Text>().text = commandCodesCollectedTotal.ToString();

        sceneUpgradePickupsCollectedDisplay.GetComponent<TMP_Text>().text = sceneUpgradePickupsCollected.ToString();
        sceneUpgradePickupsTotalDisplay.GetComponent<TMP_Text>().text = sceneUpgradePickupsTotal.ToString();
        sceneWeaponPickupsCollectedDisplay.GetComponent<TMP_Text>().text = sceneWeaponPickupsCollected.ToString();
        sceneWeaponPickupsTotalDisplay.GetComponent<TMP_Text>().text = sceneWeaponPickupsTotal.ToString();


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

        if(selfDestructActivated && totalTime == 0f)
        {

            statePause();
            menuActive = almostWinMenu;

            menuActive.SetActive(true);
        }

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

        if(selfDestructActivated)
            menuActive = almostWinMenu;
        else
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
            sceneUpgradePickupsCollected_Static = sceneUpgradePickupsCollected,
            sceneUpgradePickupsTotal_Static = sceneUpgradePickupsTotal,
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
        sceneUpgradePickupsCollected = state.sceneUpgradePickupsCollected_Static;
        sceneUpgradePickupsTotal = state.sceneUpgradePickupsTotal_Static;
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
        else if (type == itemPickup.platformType.xray)
            pickupPrefab = securityPasswordPickup;

        return pickupPrefab;
    }


    public void SecurityPasswordFound() 
    { 
        securityPasswordDisplay.GetComponent<TMP_Text>().text = sceneSecurityPassword.ToString();
    }

    public void IncrementSceneUpgradePickupCounter() 
    { 
        sceneUpgradePickupsCollected++;
        sceneUpgradePickupsCollectedDisplay.GetComponent<TMP_Text>().text = sceneUpgradePickupsCollected.ToString();


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

    public void UpdatePlayerStatsUI()
    {
        playerHealth.GetComponent<TMP_Text>().text = instance.player.GetComponent<playerDamage>().getMaxHP().ToString();
        playerSpeed.GetComponent<TMP_Text>().text = instance.player.GetComponent<playerMovement>().getPlayerSpeedOG().ToString();
        playerStamina.GetComponent<TMP_Text>().text = instance.player.GetComponent<playerMovement>().getMaxStamina().ToString();

        if (instance.player.GetComponent<playerCrouch>().GetXrayAbilityUnlocked())
            playerXrayAbility.SetActive(true);
    }

    public void UpdateCurrentWeaponUI(int selectedGun, List<pickupStats> weapons)
    {

        playerCurrentWeapon.GetComponent<TMP_Text>().text = weapons[selectedGun].name;
        playerWeaponDamage.GetComponent<TMP_Text>().text = weapons[selectedGun].shootDamage.ToString();
        playerWeaponDamageMult.GetComponent<TMP_Text>().text = weapons[selectedGun].dmgMultiplier.ToString();
        playerWeaponShootRate.GetComponent<TMP_Text>().text = weapons[selectedGun].shootRate.ToString();
        playerWeaponCoolRate.GetComponent<TMP_Text>().text = weapons[selectedGun].coolRate.ToString();
        playerWeaponCooldownTime.GetComponent<TMP_Text>().text = weapons[selectedGun].coolWaitTime.ToString();
        playerWeaponMaxHeat.GetComponent<TMP_Text>().text = weapons[selectedGun].maxHeat.ToString();
    }


    public void UpdateObjectiveUI(string objective)
    {
        GameObjectiveDisplay.GetComponent<TMP_Text>().text = objective;
    }

    public void UpdatePickupsUI()
    {

        sceneCommandCodesCollectedDisplay.GetComponent<TMP_Text>().text = sceneCommandCodesCollected.ToString();
        sceneCommandCodesTotalDisplay.GetComponent<TMP_Text>().text = sceneCommandCodesTotal.ToString();
        commandCodesCollectedTotal = StaticData.commandCodesCollectedTotal_Static;
        commandCodesCollectedTotalDisplay.GetComponent<TMP_Text>().text = commandCodesCollectedTotal.ToString();

        sceneUpgradePickupsCollectedDisplay.GetComponent<TMP_Text>().text = sceneUpgradePickupsCollected.ToString();
        sceneUpgradePickupsTotalDisplay.GetComponent<TMP_Text>().text = sceneUpgradePickupsTotal.ToString();
        sceneWeaponPickupsCollectedDisplay.GetComponent<TMP_Text>().text = sceneWeaponPickupsCollected.ToString();
        sceneWeaponPickupsTotalDisplay.GetComponent<TMP_Text>().text = sceneWeaponPickupsTotal.ToString();
    }

    public void AddWeaponIcon(pickupStats.weaponType weapon)
    {
        Vector3 position = Vector3.zero; position.x = 159; position.y = 59 + (75 * weaponIcons.Count);

        switch (weapon)
        {
            default:
                {
                    weaponIcons.Add(Instantiate(iconOptions[0], position, Quaternion.identity, uI.transform));
                    iconFills.Add(Instantiate(fillOptions[0], position, Quaternion.identity, uI.transform).GetComponent<Image>());
                    break;
                }
            case (pickupStats.weaponType.rifle):
                {
                    weaponIcons.Add(Instantiate(iconOptions[1], position, Quaternion.identity, uI.transform));
                    iconFills.Add(Instantiate(fillOptions[1], position, Quaternion.identity, uI.transform).GetComponent<Image>());
                    break;
                }
            case (pickupStats.weaponType.shotgun):
                {
                    weaponIcons.Add(Instantiate(iconOptions[2], position, Quaternion.identity, uI.transform));
                    iconFills.Add(Instantiate(fillOptions[2], position, Quaternion.identity, uI.transform).GetComponent<Image>());
                    break;
                }
            case (pickupStats.weaponType.sniper):
                {
                    weaponIcons.Add(Instantiate(iconOptions[3], position, Quaternion.identity, uI.transform));
                    iconFills.Add(Instantiate(fillOptions[3], position, Quaternion.identity, uI.transform).GetComponent<Image>());
                    break;
                }
        }

    }

    // Getters / setters:
    public int GetCommandCodesEntered() { return commandCodesEntered; }

    public int GetCommandCodesCollected() { return commandCodesCollectedTotal; }

    public bool GetFirstTimeInScene() { return firstTimeInScene; }  

    public bool GetIsRespawning() { return isRespawning; }


    public bool GetSelfDestructActivated() { return selfDestructActivated; }

    public int GetTotalGameCommandCodes() { return totalGameCommandCodes; }

    public float GetTotalTimeLeft() { return totalTime; }


    public void GetWinMenu()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        menuActive = menuWin;
        menuActive.SetActive(true);
    }

    public GameObject FindBoss()
    {
        GameObject bossRobot = null;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length > 0)
        {
            foreach (GameObject enemy in enemies)
            {
                if (enemy.GetComponent<SharedEnemyAI>().GetEnemyType() == SharedEnemyAI.enemyType.Boss)
                {
                    bossRobot = enemy;
                }
            }
        }

        return bossRobot;
    }

    public void ShowSelfDestructFlavorText()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        menuActive = SelfDestructFlavorText;
        SelfDestructFlavorText.SetActive(true);
    }

    public IEnumerator DisplayMessage(string message)
    {
        messageWindow.GetComponentInChildren<TMP_Text>().text = message;
        messageWindow.SetActive(true);

        yield return new WaitForSeconds(5f);

        messageWindow.SetActive(false);
    }

    public void DisplayHint(string hint)
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        hintOff = false;
        hintWindow.SetActive(true);
        hintWindow.GetComponentInChildren<TMP_Text>().text = hint;
     
     
    }


    public IEnumerator RespawnBuffer()
    {
        isRespawning = true;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies)
        {
            if(!EnemyManager.instance.GetIsBossFight())
                enemy.GetComponent<SharedEnemyAI>().CalmEnemy();
        }

        yield return new WaitForSeconds(2f);

        isRespawning = false;
    }

}
