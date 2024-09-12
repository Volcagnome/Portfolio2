using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 

public class StaticPlayerData : MonoBehaviour
{

    //Stores the player's current stats. Defaults are updated when the player walks through a loading zone to reflect any changes from stat pickups.
    //The new stats are applied to the player object of the loaded scene.

    public static float playerHealth = 75;
    public static float playerMaxHealth = 75;
    public static float playerSpeedOG = 5;
    public static float playerMaxStamina = 100;

    //Will inform the GameManager to set the currentSpawn variable to the Player Spawn Entry in the scene.
    public static bool isGameStart = true;
    
    //Tracks which spawn point the player should be spawned at after walking through a Loading Zone.
    public static bool previousLevel = false;
    public static bool nextLevel = true;

}
