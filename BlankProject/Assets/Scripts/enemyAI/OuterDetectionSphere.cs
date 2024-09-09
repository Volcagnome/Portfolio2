using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterDetectionSphere : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
 
        if (other.CompareTag("Player"))
        {
            gameObject.GetComponentInParent<SharedEnemyAI>().SetInOuterRange(true);
        }
        else return;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameObject.GetComponentInParent<SharedEnemyAI>().SetInOuterRange(false);
        }
    }
}
