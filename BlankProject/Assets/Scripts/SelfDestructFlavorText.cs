using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SelfDestructFlavorText : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if(GameManager.instance.GetCommandCodesEntered() ==6)
        {
            BossFight.instance.ChangeToInvasionScreen();
            GameManager.instance.ShowSelfDestructFlavorText();
        }
    }
}
