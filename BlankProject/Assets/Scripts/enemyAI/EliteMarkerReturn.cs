using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteMarkerReturn : MonoBehaviour
{
    private void OnTriggerExit(Collider eliteRobot)
    {

        if (eliteRobot.gameObject.CompareTag("Elite")
            && eliteRobot.GetComponent<enemyAI>().GetDefaultPost() == gameObject.GetComponentInParent<EliteMarker>().gameObject)
        {
            eliteRobot.gameObject.GetComponent<enemyAI>().CalmEnemy();
            //Quaternion rotationToDirection = Quaternion.LookRotation(gameObject.transform.position - guardRobot.gameObject.transform.position);
            //guardRobot.gameObject.transform.rotation = Quaternion.Lerp(transform.rotation, rotationToDirection, Time.deltaTime * guardRobot.gameObject.GetComponent<enemyAI>().rotationSpeed);

        }
    }
}
