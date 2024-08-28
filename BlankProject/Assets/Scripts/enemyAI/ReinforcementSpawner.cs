using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReinforcementSpawner : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!LevelManager.instance.GetIntruderAlert() && !LevelManager.instance.GetIsBossFight())
            Destroy(other.gameObject);
    }

}
