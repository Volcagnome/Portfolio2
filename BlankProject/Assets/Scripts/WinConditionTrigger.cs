using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinConditionTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(GameManager.instance.GetSelfDestructActivated())
        {
            GameManager.instance.GetWinMenu();
        }
    }
}
