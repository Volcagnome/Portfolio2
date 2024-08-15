using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enviromentHazard : MonoBehaviour
{
    [SerializeField] enum damageType { stationary }
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && other.gameObject.tag != "Enemy")
        {
            dmg.takeDamage(damageAmount);

        }


    }



}
