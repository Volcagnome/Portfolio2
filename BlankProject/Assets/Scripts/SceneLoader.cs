using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static StaticData;

public class SceneLoader : MonoBehaviour
{

    [SerializeField] string SceneToLoad;
    [SerializeField] bool isEntry;
    [SerializeField] bool isExit;

    private void OnTriggerEnter(Collider other)
    {

        //When the player enters the loading zone will save their current stats to the static variables in the StaticPlayerData script.
        StaticData.playerHealth = GameManager.instance.player.GetComponent<playerDamage>().getHP();
        StaticData.playerMaxHealth = GameManager.instance.player.GetComponent<playerDamage>().getMaxHP();
        StaticData.playerSpeedOG = GameManager.instance.player.GetComponent<playerMovement>().GetSpeedOG();
        StaticData.playerMaxStamina = GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina();
        StaticData.playerWeaponsList = GameManager.instance.player.GetComponent<playerDamage>().GetWeaponList();
        StaticData.playerSelectedGun = GameManager.instance.player.GetComponent<playerDamage>().GetSelectedGun();
        StaticData.playerXrayAbility = GameManager.instance.player.GetComponent<playerCrouch>().GetXrayAbilityUnlocked();
     


        //Checks if the Loading Zone is an entrance or an exit. If it was an entrance, the player is loading into the previous level and
        //the GameManager will know to spawn them at the spawn point near the exit of the previous level. If they walked through an exit,
        //they are advancing to the next level and the GameManager will know to spawn them at the spawn point near the entrance of the next level. 
        if (isEntry)
        {
            StaticData.previousLevel = true;
            StaticData.nextLevel = false;
        }
        else if (isExit)
        {
            StaticData.previousLevel = false;
            StaticData.nextLevel = true;
        }

        GameManager.instance.SavePlayerPickupData();
        SaveEnemyStates(sceneEnemies[SceneManager.GetActiveScene().buildIndex]);
        SavePickupStates(scenePickups[SceneManager.GetActiveScene().buildIndex]);

        if (IntruderAlertManager.instance.GetIntruderAlert())
            IntruderAlertManager.instance.CancelIntruderAlert();

        //Loads the scene entered in the Scene To Load field.
        SceneManager.LoadScene(SceneToLoad);
    }



    public void SaveEnemyStates(List <enemyState> sceneEnemyList)
    {

        sceneEnemyList.Clear();

        GameObject[] sceneEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in sceneEnemies)
        {

            if (enemy.GetComponent<SharedEnemyAI>())
            {
                if (!enemy.GetComponent<SharedEnemyAI>().GetIsDead())
                {

                    if (enemy.GetComponent<SharedEnemyAI>().GetIsRespondingToAlert())
                        enemy.GetComponent<SharedEnemyAI>().SetIsRespondingToAlert(false);
                    if(enemy.GetComponent<SharedEnemyAI>().GetIsSearching())
                        enemy.GetComponent<SharedEnemyAI>().SetIsSearching(false);

                    enemyState state = new enemyState
                    {
                        enemyType = EnemyManager.instance.GetEnemyPrefab(enemy.GetComponent<SharedEnemyAI>().GetEnemyType()),
                        position = enemy.transform.position,
                        rotation = enemy.transform.rotation,
                        health = enemy.GetComponent<SharedEnemyAI>().GetHP(),
                        maxHealth = enemy.GetComponent<SharedEnemyAI>().GetMaxHP(),
                        isAlerted = enemy.GetComponent<SharedEnemyAI>().GetIsAlerted(),
                        defaultPost = enemy.GetComponent<SharedEnemyAI>().GetDefaultPost().transform.position,
                        currentDestination = enemy.GetComponent<SharedEnemyAI>().GetCurrentDestination().transform.position,
                        agentDestination = enemy.GetComponent<NavMeshAgent>().destination,
                        lastKnownPlayerLocation = enemy.GetComponent<SharedEnemyAI>().GetLastKnownPlayerLocation(),
                        isEndGameEnemy = enemy.GetComponent<SharedEnemyAI>().GetIsEndGameEnemy(),
                        loadedFromState = true
                    };

                    sceneEnemyList.Add(state);
                }
            }
        }
    }

    public void SavePickupStates(List<pickupState> scenePickupsList)
    {
        scenePickupsList.Clear();

        GameObject[] scenePickups = GameObject.FindGameObjectsWithTag("Pickup Platform");
        foreach (GameObject pickup in scenePickups)
        {
            pickupState state = new pickupState
            {
                pickupLocation_Static = pickup.transform.position,
                pickupRotation_Static = pickup.transform.rotation,
                itemPickedUp_Static = pickup.GetComponent<itemPickup>().GetIfItemCollected(),
                item_Static = pickup.GetComponent<itemPickup>().GetItemPickupStats(),
                pickupPrefab_Static = GameManager.instance.GetPickupPrefab(pickup.GetComponent<itemPickup>().GetPlatformType())

            };

            scenePickupsList.Add(state);
        }
    }

}

