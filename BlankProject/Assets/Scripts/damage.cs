using System.Collections;
using UnityEngine;

public class damage : MonoBehaviour
{
    [SerializeField] enum damageType {bullet, web, stationary, shield}
    [SerializeField] damageType type;
    [SerializeField] protected Rigidbody rb;


    [SerializeField] protected float damageAmount;
    [SerializeField] protected int speed;
    [SerializeField] protected int destroyTime;
    private GameObject source;


    // Start is called before the first frame update
    void Start()
    {
        if (type == damageType.bullet || type == damageType.web)
        {
            rb.velocity = (GameManager.instance.player.transform.position - transform.position).normalized * speed;
            Destroy(gameObject, destroyTime);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        IDamage dmg = other.GetComponent<IDamage>();

        if (other.isTrigger || other.gameObject == gameObject.transform.root.gameObject
            || other.gameObject.CompareTag("Enemy"))
            return;

        else if(dmg != null && other.gameObject.tag == "Player")
        {
            dmg.takeDamage(damageAmount);

            if(type == damageType.bullet)
                Destroy(gameObject);
        }
    }
}
