using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanSpawner : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!LevelManager.instance.GetIntruderAlert() && !LevelManager.instance.GetIsBossFight())
            Destroy(other.gameObject);
    }

}
