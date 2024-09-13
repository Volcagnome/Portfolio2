using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EndgameEnemySpawnPoint : MonoBehaviour
{
    //Prefabs for each enemy type
    [SerializeField] GameObject guard;
    [SerializeField] GameObject titan;
    [SerializeField] GameObject patrol;


    //Number of enemies for this spawner to spawn when the player enters the trigger box.
    [SerializeField] int numGuardsToSpawn;
    [SerializeField] int numTitansToSpawn;
    [SerializeField] int numPatrolsToSpawn;

    //Enemies spawned by this spawner;
     List<GameObject> enemiesSpawned_List = new List<GameObject>();


    //Checks the number entered for each enemy type and spawns them near the enemy spawn point. Sets their alert status to isAlerted and
    //gives them the player's location. 
    public IEnumerator SpawnEnemies()
    { 

        Vector3 randomDist;

        GameObject[] enemyTypes = { guard, titan, patrol};
        int[] numToSpawn = { numGuardsToSpawn, numTitansToSpawn, numPatrolsToSpawn };

        GameObject entityToSpawn;
        int numEnemiesToSpawn;

        for (int index = 0; index < 3; index++)
        {

            entityToSpawn = enemyTypes[index];
            numEnemiesToSpawn = numToSpawn[index];

            for (int enemiesSpawned = 0; enemiesSpawned < numEnemiesToSpawn; enemiesSpawned++)
            {

                randomDist = UnityEngine.Random.insideUnitSphere * 3f;
                randomDist += gameObject.transform.position;

                NavMeshHit hit;
                NavMesh.SamplePosition(randomDist, out hit, 3f, 1);

                GameObject enemy = Instantiate(entityToSpawn, hit.position, gameObject.transform.rotation);

                enemiesSpawned_List.Add(enemy);
                enemy.GetComponent<SharedEnemyAI>().SetIsEndgameEnemy(true);

                enemy.GetComponent<SharedEnemyAI>().SetDefaultPost(gameObject);
                enemy.GetComponent<SharedEnemyAI>().SetCurrentDestination(gameObject);

                enemy.GetComponent<SharedEnemyAI>().AlertEnemy();
                enemy.GetComponent<SharedEnemyAI>().SetLastKnownPlayerLocation(GameManager.instance.player.transform.position);
                enemy.GetComponent<NavMeshAgent>().SetDestination(GameManager.instance.player.transform.position);

                yield return new WaitForSeconds(0.75f);
            }
        }

        yield break;

    }


    public void CallEnemies()
    {
        if(enemiesSpawned_List.Count > 0)
        {
            enemiesSpawned_List.ForEach(enemy => 
            {
                enemy.GetComponent<SharedEnemyAI>().SetLastKnownPlayerLocation(GameManager.instance.player.transform.position);
                enemy.GetComponent<SharedEnemyAI>().AlertEnemy();
            });
        }
    }

    public void RemoveFromEnemySpawnedList(GameObject robot)
    {
        enemiesSpawned_List.Remove(robot);
    }

}
