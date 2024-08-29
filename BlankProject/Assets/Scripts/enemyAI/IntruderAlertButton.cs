using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntruderAlertButton : MonoBehaviour
{

    GameObject button;

    // Start is called before the first frame update
    void Start()
    {
        LevelManager.instance.intruderAlertButtons.Add(gameObject);
    }

    protected void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Enemy")
            && other.GetComponent<SharedEnemyAI>().GetEnemyType() == SharedEnemyAI.enemyType.Patrol
            && other.GetComponent<patrolAI>().GetIsWhistleBlower())
        {

            LevelManager.instance.IntruderAlert(other.GetComponent<SharedEnemyAI>().GetLastKnownPlayerLocation());
            button.transform.GetChild(0).gameObject.SetActive(false);
            button.transform.GetChild(1).gameObject.SetActive(true) ;
        }
        else
            return;
    }

}
