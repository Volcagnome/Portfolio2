using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    [SerializeField] string SceneToLoad;
    [SerializeField] bool isEntry;
    [SerializeField] bool isExit;

    private void OnTriggerEnter(Collider other)
    {

        //When the player enters the loading zone will save their current stats to the static variables in the StaticPlayerData script.
        StaticPlayerData.playerHealth = GameManager.instance.player.GetComponent<playerDamage>().getHP();
        StaticPlayerData.playerMaxHealth = GameManager.instance.player.GetComponent<playerDamage>().getMaxHP();
        StaticPlayerData.playerSpeedOG = GameManager.instance.player.GetComponent<playerMovement>().GetSpeedOG();
        StaticPlayerData.playerMaxStamina = GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina();


        //Checks if the Loading Zone is an entrance or an exit. If it was an entrance, the player is loading into the previous level and
        //the GameManager will know to spawn them at the spawn point near the exit of the previous level. If they walked through an exit,
        //they are advancing to the next level and the GameManager will know to spawn them at the spawn point near the entrance of the next level. 
        if (isEntry)
        {
            StaticPlayerData.previousLevel = true;
            StaticPlayerData.nextLevel = false;
        }
        else if (isExit)
        {
            StaticPlayerData.previousLevel = false;
            StaticPlayerData.nextLevel = true;
        }


        //Loads the scene entered in the Scene To Load field.
        SceneManager.LoadScene(SceneToLoad);

    }


}

