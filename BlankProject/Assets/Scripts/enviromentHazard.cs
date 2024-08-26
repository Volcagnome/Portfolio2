using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enviromentHazard : MonoBehaviour
{
    [SerializeField] enum damageType { stationary, trap }
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject itemOnState;
    [SerializeField] GameObject itemOffState;

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
        else if (dmg != null && other.gameObject.tag != "Enemy" && type == damageType.trap)
        {
            itemOnState.SetActive(true);
            itemOffState.SetActive(false);
            dmg.takeDamage(damageAmount);
        }

    }

    


}
