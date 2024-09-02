using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Rocket : damage
{
    Vector3 offset;
    Vector3 destination;
    [SerializeField] ParticleSystem explosion;
    [SerializeField] float areaOfEffect;
    [SerializeField] float aoeDamage;

    GameObject directHit;

    // Start is called before the first frame update
    void Start()
    {
        offset = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));

        rb.velocity = (GameManager.instance.player.transform.position - transform.position + offset).normalized * speed;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        IDamage dmg = other.GetComponent<IDamage>();

        if (other.isTrigger)
        {
            return;
        }
        else if (dmg != null && other.gameObject != EnemyManager.instance.GetBoss())
        {
            directHit = other.gameObject;

            dmg.criticalHit(damageAmount);

            Destroy(gameObject);
        }

        ApplyAOEDamage();
            
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
    }

    private void ApplyAOEDamage()
    {
        IDamage dmg;

        Collider[] targetsHit = Physics.OverlapSphere(gameObject.transform.position, areaOfEffect);

        if (targetsHit.Length > 0)
        {

            foreach (Collider target in targetsHit)
            {
                dmg = target.GetComponent<IDamage>();

                if (dmg != null && target.gameObject != directHit && target.gameObject != EnemyManager.instance.GetBoss())
                {
                    dmg.takeDamage(aoeDamage);
                }
            }
        }
    }


}
