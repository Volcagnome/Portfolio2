using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderSpawner : MonoBehaviour
{
    [SerializeField] GameObject Arachnoid;
 

    GameObject currentSpider;
    bool isActive;
    bool readyToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        readyToSpawn = true;
    }

    private void Update()
    {
        if (isActive && currentSpider == null && readyToSpawn == true)
            StartCoroutine(SpawnSpider());
        else if(!isActive)
        {
            if (currentSpider != null)
            {
                currentSpider.GetComponent<NavMeshAgent>().SetDestination(gameObject.transform.position);

                if (Vector3.Distance(gameObject.transform.position, currentSpider.transform.position) < 0.5f)
                    Destroy(currentSpider);

            }

            

        }
    }

    IEnumerator SpawnSpider()
    {
        readyToSpawn = false;

        currentSpider = Instantiate(Arachnoid, transform.position, Quaternion.identity);

        currentSpider.GetComponent<SharedEnemyAI>().SetDefaultPost(gameObject);
        currentSpider.GetComponent<NavMeshAgent>().SetDestination(GameManager.instance.player.transform.position);

        yield return new WaitForSeconds(LevelManager.instance.GetMinTimeBetweenSpiderSpawn());

        readyToSpawn = true;
    }


    public void ToggleActive(bool status)
    {
        isActive = status;
    }


}
