using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Spawns reinforcements if there are not enough guards currently in the scene to meet the configured number of guards
//for the current Security Breach level.

public class TitanSpawner : MonoBehaviour
{

    //If there is no Intruder Alert in progress, and guards have returned to the reinforcement spawner, despawns them.
    private void OnTriggerEnter(Collider other)
    {
        if (!IntruderAlertManager.instance.GetIntruderAlert() && !EnemyManager.instance.GetIsBossFight())
            Destroy(other.gameObject);
    }

}
