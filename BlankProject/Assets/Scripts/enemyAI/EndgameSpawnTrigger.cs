using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EndgameSpawnTrigger : MonoBehaviour
{

    [SerializeField] List<GameObject> enemySpawnPoint_List;
    [SerializeField] int secondsBetweenEnemyRespawn;

    bool readyToSpawn;

    private void Start()
    {
        readyToSpawn = false;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player") && GameManager.instance.GetSelfDestructActivated())
        {

            if (!readyToSpawn)
            {

                enemySpawnPoint_List.ForEach(spawnPoint =>
                {

                    StartCoroutine(spawnPoint.GetComponent<EndgameEnemySpawnPoint>().SpawnEnemies());
                });

                readyToSpawn = true;

            }else
            {
                enemySpawnPoint_List.ForEach(spawnPoint =>
                {
                    spawnPoint.GetComponent<EndgameEnemySpawnPoint>().CallEnemies();
                });
            }
        }
    }


   
}
