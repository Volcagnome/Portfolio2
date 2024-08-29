using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enviromentHazard : MonoBehaviour
{
    [SerializeField] enum damageType { stationary, trapSet3, trapSet2}
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject itemOn1State;
    [SerializeField] GameObject itemOn2State;
    [SerializeField] GameObject itemOn3State;
    [SerializeField] GameObject itemOff1State;
    [SerializeField] GameObject itemOff2State;
    [SerializeField] GameObject itemOff3State;

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
        else if (dmg != null && other.gameObject.tag != "Enemy" && type == damageType.trapSet3)
        {
            itemOn1State.SetActive(true);
            itemOn2State.SetActive(true);
            itemOn3State.SetActive(true);
            itemOff1State.SetActive(false);
            itemOff2State.SetActive(false);
            itemOff3State.SetActive(false);
            dmg.takeDamage(damageAmount);
        }
       

    }

    


}
