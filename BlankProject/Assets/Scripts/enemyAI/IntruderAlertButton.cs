using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Object that WhistleBlower patrol robot will run to to activate the Intruder Alert.

public class IntruderAlertButton : MonoBehaviour
{
    [SerializeField] Material outlineMaterial;
    [SerializeField] Material originalMaterial;
    [SerializeField] Sprite idleScreenSprite;
    [SerializeField] Sprite alertScreenSprite;

    public enum buttonState { idle, raisingAlarm, Alert };


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

    public void UpdateButtonState(buttonState state)
    { 
        if(state == buttonState.idle)
        {
            GetComponentInChildren<MeshRenderer>().material = originalMaterial;
            GetComponentInChildren<SpriteRenderer>().sprite = idleScreenSprite;
            GetComponentInChildren<SpriteRenderer>().color = Color.green;
        }
        else if(state == buttonState.raisingAlarm)
        {
            GetComponentInChildren<MeshRenderer>().material = outlineMaterial;
        }
        else if(state == buttonState.Alert)
        {
            GetComponentInChildren<MeshRenderer>().material = originalMaterial;
            GetComponentInChildren<SpriteRenderer>().sprite = alertScreenSprite;
            GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
    }
}
