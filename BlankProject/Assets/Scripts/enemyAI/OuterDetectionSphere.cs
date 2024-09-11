using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterDetectionSphere : MonoBehaviour
{

    //Larger trigger sphere to monitor when player is about to get in range to increment stealth meter. 
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
