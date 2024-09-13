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

    ////////////////////////////////////////
    ///          LEVEL DATA             ///
    ///////////////////////////////////////


    //Will inform the GameManager to set the currentSpawn variable to the Player Spawn Entry in the scene.
    public static bool isGameStart = true;

    //Tracks which spawn point the player should be spawned at after walking through a Loading Zone.
    public static bool previousLevel = false;
    public static bool nextLevel = true;

    //List that holds enemy states for each level
    public static List<enemyState> enemyStatesScene1 = new List<enemyState>();
    public static List<enemyState> enemyStatesScene2 = new List<enemyState>();
    public static List<enemyState> enemyStatesScene3 = new List<enemyState>();

    //Dictionary that returns a level's list of enemy states when passed the scene name.
    public static Dictionary<string, List<enemyState>> sceneEnemies = new Dictionary<string, List<enemyState>> 
    { {"Test1", enemyStatesScene1 }, { "Test2", enemyStatesScene2 }};

    //Dictionary that returns if this is the first time the player has entered a given scene.
    public static Dictionary<string, bool> firstTimeInScene = new Dictionary<string, bool>() 
    { { "Test1", true }, { "Test2",true } };


}
