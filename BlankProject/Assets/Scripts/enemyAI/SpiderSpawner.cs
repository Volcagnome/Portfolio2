using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//Spawns spider robots during an Intruder Alert.

public class SpiderSpawner : MonoBehaviour
{
    //Reference to the spider prefab
    [SerializeField] GameObject spider;

    //Current spider spawned from this spider spawner
    GameObject currentSpider;

    bool isActive;
    bool readyToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        readyToSpawn = true;
    }
    

    //When Intruder Alert is initiated, spider spawners will be activated and a spider will be spawned in each spider spawner. 
    //When the Intruder Alert ends, they are deactivated. They calm their spider and call it back to the spawner. When they
    //get back, despawns the spider. 
    private void Update()
    {

        if (isActive && currentSpider == null && readyToSpawn == true)
            StartCoroutine(SpawnSpider());
        else if (!isActive)
        {
            if (currentSpider != null)
            {
                currentSpider.GetComponent<NavMeshAgent>().SetDestination(gameObject.transform.position);
                currentSpider.GetComponent<SharedEnemyAI>().CalmEnemy();
                if (Vector3.Distance(gameObject.transform.position, currentSpider.transform.position) < 0.5f)
                    Destroy(currentSpider);

            }
        }
    }


    //Spawns a new spider and sets their default post to the instantiating spider spawner. It then waits the configured, 
    //number of seconds before becoming available to spawn another spider if its current one is killed.
    IEnumerator SpawnSpider()
    {
        readyToSpawn = false;

        currentSpider = Instantiate(spider, transform.position, Quaternion.identity);

        currentSpider.GetComponent<SharedEnemyAI>().SetDefaultPost(gameObject);
        currentSpider.GetComponent<SharedEnemyAI>().SetCurrentDestination(gameObject);

        yield return new WaitForSeconds(IntruderAlertManager.instance.GetMinTimeBetweenSpiderSpawn());

        readyToSpawn = true;
    }


    public void SetIsActive(bool status) { isActive = status; }

    public void ResetCurrentSpider() { currentSpider = null; }
}
