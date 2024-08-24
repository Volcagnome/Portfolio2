using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntruderAlertButton : MonoBehaviour
{

    

    // Start is called before the first frame update
    void Start()
    {
        LevelManager.instance.intruderAlertButtons.Add(gameObject);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") 
            && other.GetComponent<SharedEnemyAI>().GetEnemyType() == SharedEnemyAI.enemyType.Patrol 
            && other.GetComponent<patrolAI>().GetIsWhistleBlower())
        {
           LevelManager.instance.IntruderAlert(other.GetComponent<enemyAI>().GetLastKnownPlayerLocation());
        }
        else
            return;
    }

}
