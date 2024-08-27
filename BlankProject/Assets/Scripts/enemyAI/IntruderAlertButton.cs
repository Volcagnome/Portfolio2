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
        Debug.Log("tag: " + other.tag);
        Debug.Log("type: " + other.GetComponent<SharedEnemyAI>().GetEnemyType());
        Debug.Log("is whistle blower: " + other.GetComponent<patrolAI>().GetIsWhistleBlower());

        if (other.gameObject.CompareTag("Enemy") 
            && other.GetComponent<SharedEnemyAI>().GetEnemyType() == SharedEnemyAI.enemyType.Patrol 
            && other.GetComponent<patrolAI>().GetIsWhistleBlower())
        {

            Debug.Log("intruder alert button pressed");
           LevelManager.instance.IntruderAlert(other.GetComponent<enemyAI>().GetLastKnownPlayerLocation());
        }
        else
            return;
    }

}
