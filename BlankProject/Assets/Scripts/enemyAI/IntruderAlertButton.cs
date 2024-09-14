using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Object that WhistleBlower patrol robot will run to to activate the Intruder Alert.

public class IntruderAlertButton : MonoBehaviour
{

    //If the current WhistleBlower enters the trigger box, activates the intruder alert and passes the location where
    //the WhistleBlower last saw the player. 
    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<patrolAI>() && other.GetComponent<patrolAI>().GetIsWhistleBlower())
        {
            IntruderAlertManager.instance.IntruderAlert(other.GetComponent<SharedEnemyAI>().GetLastKnownPlayerLocation());
            
        }
        else
            return;
    }
}
