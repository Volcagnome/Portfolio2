using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !GameManager.instance.isPaused && !GameManager.instance.GetSelfDestructActivated())
        {
            GameManager.instance.tutorialWindow.SetActive(true);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.tutorialWindow.SetActive(false);
        }
    }

}
