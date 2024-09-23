using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeTutorialTrigger : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !GameManager.instance.isPaused && !GameManager.instance.GetSelfDestructActivated())
        {
            GameManager.instance.grenadeTutWindow.SetActive(true);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.grenadeTutWindow.SetActive(false);
        }
    }

}