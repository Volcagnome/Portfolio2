using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TitanPostReturn : MonoBehaviour
{
    private void OnTriggerExit(Collider eliteRobot)
    {

        if (eliteRobot.gameObject.GetComponent<enemyAI>().GetEnemyType() == enemyAI.enemyType.Titan
            && eliteRobot.GetComponent<enemyAI>().GetDefaultPost() == gameObject.GetComponentInParent<TitanPost>().gameObject)
        {
            eliteRobot.gameObject.GetComponent<SharedEnemyAI>().CalmEnemy();
        }
    }
}
