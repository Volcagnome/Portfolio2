using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damage : MonoBehaviour
{
    [SerializeField] enum damageType {bullet, web, stationary, shield}
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;


    [SerializeField] int damageAmount;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    private GameObject source;


    // Start is called before the first frame update
    void Start()
    {
        if (type == damageType.bullet || type == damageType.web)
        {
            rb.velocity = (GameManager.instance.player.transform.position - transform.position + new Vector3(0, 0.5f, 0)).normalized * speed;
            Destroy(gameObject, destroyTime);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        IDamage dmg = other.GetComponent<IDamage>();

        if (other.isTrigger || other.gameObject.tag == "Enemy")
        {
            return;
        } 
        else if (dmg != null && other.gameObject.tag == "Player")
        {
            dmg.takeDamage(damageAmount);
            Destroy(gameObject);
        }else
            Destroy(gameObject);
    }
}
