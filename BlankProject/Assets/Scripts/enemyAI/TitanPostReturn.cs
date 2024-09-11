using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//If titan exits the TitanPost's Return object's sphere collider, Calms the titan so they return to their post.

public class TitanPostReturn : MonoBehaviour
{
    private void OnTriggerExit(Collider titanRobot)
    {

        if (titanRobot.gameObject == GetComponentInParent<TitanPost>().GetAssignedTitan())
        {
            titanRobot.gameObject.GetComponent<SharedEnemyAI>().CalmEnemy();
        }
    }
}
