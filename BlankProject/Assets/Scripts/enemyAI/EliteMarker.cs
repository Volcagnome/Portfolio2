using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteMarker : MonoBehaviour
{
    private void OnTriggerEnter(Collider eliteRobot)
    {


        if (eliteRobot.gameObject.GetComponent<enemyAI>().GetEnemyType(eliteRobot.gameObject) == enemyAI.enemyType.elite
            && eliteRobot.GetComponent<enemyAI>().GetDefaultPost() == gameObject.GetComponentInParent<EliteMarker>().gameObject)
        {
            eliteRobot.gameObject.transform.rotation = gameObject.transform.rotation;
        }
    }
}
