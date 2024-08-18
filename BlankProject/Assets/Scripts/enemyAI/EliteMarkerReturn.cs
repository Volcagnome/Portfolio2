using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteMarkerReturn : MonoBehaviour
{
    private void OnTriggerExit(Collider eliteRobot)
    {

        if (eliteRobot.gameObject.GetComponent<enemyAI>().GetEnemyType() == enemyAI.enemyType.elite
            && eliteRobot.GetComponent<enemyAI>().GetDefaultPost() == gameObject.GetComponentInParent<EliteMarker>().gameObject)
        {
            eliteRobot.gameObject.GetComponent<enemyAI>().CalmEnemy();
        }
    }
}
