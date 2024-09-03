using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanPostReturn : MonoBehaviour
{
    private void OnTriggerExit(Collider eliteRobot)
    {

        if (eliteRobot.gameObject.GetComponent<SharedEnemyAI>().GetEnemyType() == SharedEnemyAI.enemyType.Titan
            && eliteRobot.GetComponent<SharedEnemyAI>().GetDefaultPost() == gameObject.GetComponentInParent<TitanPost>().gameObject)
        {
            eliteRobot.gameObject.GetComponent<SharedEnemyAI>().CalmEnemy();
        }
    }
}
