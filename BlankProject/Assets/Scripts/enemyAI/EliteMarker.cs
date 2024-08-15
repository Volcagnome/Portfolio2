using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteMarker : MonoBehaviour
{
    private void OnTriggerEnter(Collider eliteRobot)
    {
        

        if (eliteRobot.gameObject.CompareTag("Elite")
            && eliteRobot.GetComponent<enemyAI>().GetDefaultPost() == gameObject.GetComponentInParent<EliteMarker>().gameObject)
        {
            eliteRobot.gameObject.transform.rotation = gameObject.transform.rotation;
            //Quaternion rotationToDirection = Quaternion.LookRotation(gameObject.transform.position - guardRobot.gameObject.transform.position);
            //guardRobot.gameObject.transform.rotation = Quaternion.Lerp(transform.rotation, rotationToDirection, Time.deltaTime * guardRobot.gameObject.GetComponent<enemyAI>().rotationSpeed);
        }
    }
}
