using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] private GameObject entityToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnEnemies()); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnEnemies()
    {

        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (EnemyManager.instance.guardUnits.Count < EnemyManager.instance.guardUnitsMax)
            {
                GameObject newEnemy = Instantiate(entityToSpawn, gameObject.transform.position, gameObject.transform.rotation);
                EnemyManager.instance.NewGuard(newEnemy);
            }

            yield return new WaitForSeconds(EnemyManager.instance.EnemySpawnInterval);
        }
    }

}
