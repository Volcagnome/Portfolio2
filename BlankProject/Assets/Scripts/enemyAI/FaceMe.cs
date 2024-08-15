using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FaceMe : MonoBehaviour
{
    private void OnTriggerEnter(Collider guardRobot)
    {

        //if (guardRobot.CompareTag("Enemy")
        //    && guardRobot.GetComponent<enemyAI>().GetDefaultPost() == gameObject.GetComponentInParent<GuardPost>().gameObject
        //    && guardRobot.GetComponent<enemyAI>().CheckIfOnDuty() == true)
        //{
        //    Debug.Log("test");
        //    Quaternion rotationToDirection = Quaternion.LookRotation(gameObject.transform.position - guardRobot.gameObject.transform.position);
        //    guardRobot.gameObject.transform.rotation = Quaternion.Lerp(transform.rotation, rotationToDirection, Time.deltaTime * guardRobot.gameObject.GetComponent<enemyAI>().rotationSpeed);
        //}
    }
}
