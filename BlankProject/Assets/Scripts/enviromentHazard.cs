using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class enviromentHazard : MonoBehaviour
{
    [SerializeField] enum damageType { stationary, trapSet3, trapSet2}
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject spikeOne;
    [SerializeField] GameObject spikeTwo;
    [SerializeField] GameObject spikeThree;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip;


    [SerializeField] float resetTime;
    [SerializeField] int damageAmount;

    public bool itemState;

   


    private void Start()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        

        if (other.isTrigger)
        {
            return;
        }

        IDamage dmg = other.GetComponent<IDamage>();

        itemState = true;
        if (dmg != null && other.gameObject.tag != "Enemy" && type == damageType.stationary)
        {
            dmg.takeDamage(damageAmount);

        }
        else if (dmg != null && other.gameObject.tag != "Enemy" && type == damageType.trapSet3)
        {
            spikeTripped();
            if(!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(audioClip);
            }
            dmg.takeDamage(damageAmount);
        }
       

    }

     private void spikeTripped()
    {

        this.GetComponent<Animation>().Play();


    }

   


}
