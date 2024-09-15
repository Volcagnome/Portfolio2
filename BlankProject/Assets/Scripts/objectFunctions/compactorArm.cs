using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class compactorArm : MonoBehaviour
{
    [SerializeField] GameObject compactor;
    [SerializeField] Rigidbody otherDamageZone;


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
        if (other == transform.parent.root) return;
        if (!compactor.GetComponent<trashCompactor>().getTurningAround())
        {
            if (other.gameObject.GetComponent<Rigidbody>() == otherDamageZone)
            {
                StartCoroutine(compactor.GetComponent<trashCompactor>().turnAround());
            }
            else if (!other.isTrigger)
            {
                if (other.gameObject.GetComponent<playerDamage>() != null)
                {
                    StartCoroutine(compactor.GetComponent<trashCompactor>().turnAround());
                    other.GetComponent<playerDamage>().takeDamage(compactor.GetComponent<trashCompactor>().getPlayerDamage());
                    other.GetComponent<playerDamage>().bleed();
                }
                else if (other.gameObject.GetComponent<SharedEnemyAI>() != null)
                {
                    StartCoroutine(compactor.GetComponent<trashCompactor>().turnAround());
                    other.GetComponent<SharedEnemyAI>().takeDamage(other.GetComponent<SharedEnemyAI>().GetHP());
                }
            }
        }
    }
}
