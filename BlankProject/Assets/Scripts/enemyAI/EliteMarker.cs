using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteMarker : MonoBehaviour
{
    private void OnTriggerEnter(Collider eliteRobot)
    {

        if (eliteRobot.gameObject.GetComponentInParent<enemyAI>().GetEnemyType() == enemyAI.enemyType.Elite
            && eliteRobot.GetComponent<enemyAI>().GetDefaultPost() == this)
        {
            Debug.Log("Test");

            eliteRobot.gameObject.transform.rotation = gameObject.transform.rotation;
        }
    }
}
