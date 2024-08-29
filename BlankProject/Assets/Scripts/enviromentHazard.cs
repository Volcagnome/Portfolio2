using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enviromentHazard : MonoBehaviour
{
    [SerializeField] enum damageType { stationary, trapSet3, trapSet2 }
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject itemOnOneState;
    [SerializeField] GameObject itemOnTwoState;
    [SerializeField] GameObject itemOnThreeState;
    [SerializeField] GameObject itemOffOneState;
    [SerializeField] GameObject itemOffTwoState;
    [SerializeField] GameObject itemOffThreeState;

    [SerializeField] int damageAmount;


    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && other.gameObject.tag != "Enemy" && type == damageType.stationary)
        {
            dmg.takeDamage(damageAmount);

        }
        //Activate all 3 spikes simutaneously(to be used with set of three spike trap)
        else if (dmg != null && other.gameObject.tag != "Enemy" && type == damageType.trapSet3)
        {
            itemOnOneState.SetActive(true);
            itemOnTwoState.SetActive(true);
            itemOnThreeState.SetActive(true);
            itemOffOneState.SetActive(false);
            itemOffTwoState.SetActive(false);
            itemOffThreeState.SetActive(false);
            dmg.takeDamage(damageAmount);
        }
        //Activate spikes indepndently(to be used with set of two, add script to spikes -not trap empty object parent and
        //set other states to an empty game object)
        else if (dmg != null && other.gameObject.tag != "Enemy" && type == damageType.trapSet2)
        {
            itemOnOneState.SetActive(true);
            itemOffOneState.SetActive(false);
            dmg.takeDamage(damageAmount);
        }

    }

    


}
