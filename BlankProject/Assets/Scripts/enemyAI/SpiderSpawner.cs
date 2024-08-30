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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!LevelManager.instance.GetIntruderAlert() && other.gameObject.GetComponent<SharedEnemyAI>().GetDefaultPost() == gameObject)
        {
            Destroy(other.gameObject);
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
        if (status == true)
        {
            isActive = true;
            GetComponent<SphereCollider>().enabled = false;
        }
        else
        {
            isActive = false;
            GetComponent<SphereCollider>().enabled = true;
        }
    }


}
