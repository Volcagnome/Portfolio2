using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static StaticData;
using static UnityEngine.EventSystems.EventTrigger;





public class StaticData : MonoBehaviour
{

    ////////////////////////////////////////
    ///       GLOBAL GAME DATA           ///
    ///////////////////////////////////////

    //Will inform the GameManager to set the currentSpawn variable to the Player Spawn Entry in the scene.
    public static bool isGameStart = true;

    //Tracks how many total command codes the player has collected so far
    public static int commandCodesCollectedTotal_Static;




    ////////////////////////////////////////
    ///          PLAYER DATA             ///
    ///////////////////////////////////////

    //Stores the player's current stats. Defaults are updated when the player walks through a loading zone to reflect any changes from stat pickups.
    //The new stats are applied to the player object of the loaded scene.

    public static float playerHealth = 75;
    public static float playerMaxHealth = 75;
    public static float playerSpeedOG = 5;
    public static float playerMaxStamina = 100;
    public static List<pickupStats> playerWeaponsList = new List<pickupStats>();
    public static int playerSelectedGun;




    ////////////////////////////////////////
    ///          ENEMY DATA             ///
    ///////////////////////////////////////


    public struct enemyState
    {
        public GameObject enemyType;
        public Vector3 position;
        public Quaternion rotation;
        public float health;
        public float maxHealth;
        public bool isAlerted;
        public Vector3 defaultPost;
        public Vector3 currentDestination;
        public Vector3 agentDestination;
        public Vector3 lastKnownPlayerLocation;
        public bool isRespondingToAlert;
        public bool isEndGameEnemy;
        public bool loadedFromState;
    }


    //Dictionary of collected scene command codes and security password
    public static Dictionary<int, playerPickupState> levelData = new Dictionary<int, playerPickupState>()
    { { 0, playerPickupsStateScene1 }, { 1, playerPickupsStateScene2 }, { 2, playerPickupsStateScene2 }};

    public struct playerPickupState
    {
        public int sceneCommandCodesCollected_Static;
        public int sceneCommandCodesTotal_Static;
        public int sceneStatPickupsCollected_Static;
        public int sceneStatPickupsTotal_Static;
        public int sceneWeaponPickupsCollected_Static;
        public int sceneWeaponPickupsTotal_Static;
        public int sceneSecurityPassword_Static;
    }



    ////////////////////////////////////////
    ///          LEVEL DATA             ///
    ///////////////////////////////////////


    //Tracks which spawn point the player should be spawned at after walking through a Loading Zone.
    public static bool previousLevel = false;
    public static bool nextLevel = true;

    //List that holds enemy states for each level
    public static List<enemyState> enemyStatesScene1 = new List<enemyState>();
    public static List<enemyState> enemyStatesScene2 = new List<enemyState>();
    public static List<enemyState> enemyStatesScene3 = new List<enemyState>();

    //List that holds all pickups states for each level;
    public static List<pickupState> pickupStatesScene1 = new List<pickupState>();
    public static List<pickupState> pickupStatesScene2 = new List<pickupState>();
    public static List<pickupState> pickupStatesScene3 = new List<pickupState>();

    //States for each level that holds data specific to each level
    public static playerPickupState playerPickupsStateScene1 = new playerPickupState();
    public static playerPickupState playerPickupsStateScene2 = new playerPickupState();
    public static playerPickupState playerPickupsStateScene3 = new playerPickupState();


    //Dictionary of scene enemy states;
    public static Dictionary<int, List<enemyState>> sceneEnemies = new Dictionary<int, List<enemyState>> 
    { {0, enemyStatesScene1 }, { 1, enemyStatesScene2 }, { 2, enemyStatesScene3 }};

    //Struct to hold state for each pickup platform in scene
    public struct pickupState
    {
        public Vector3 pickupLocation_Static;
        public Quaternion pickupRotation_Static;
        public bool itemPickedUp_Static;
        public pickupStats item_Static;
        public GameObject pickupPrefab_Static;
    }

    //Dictionary of scene pickup states;
    public static Dictionary<int, List<pickupState>> scenePickups = new Dictionary<int, List<pickupState>>
    { {0, pickupStatesScene1 }, { 1, pickupStatesScene2 }, { 2, pickupStatesScene3 }};

    //Dictionary of if this is the player's first time in the scene
    public static Dictionary<int, bool> firstTimeInScene = new Dictionary<int, bool>
    { {0, true }, { 1, true },{ 2, true }};

}
