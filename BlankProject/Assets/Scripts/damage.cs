using System.Collections;
using UnityEngine;

public class damage : MonoBehaviour
{
    [SerializeField] enum damageType {bullet, fireball, web, stationary, shield, playerBullet}
    [SerializeField] enum effectType {none, burn, bleed, shock, stun}
    [SerializeField] damageType type;
    [SerializeField] effectType status;
    [SerializeField] protected Rigidbody rb;

    [SerializeField] protected float damageAmount;
    [SerializeField] protected int speed;
    [SerializeField] protected int destroyTime;

    private GameObject source;


    // Start is called before the first frame update
    void Start()
    {
        if (type == damageType.bullet || type == damageType.web || type == damageType.fireball)
        {
            rb.velocity = (GameManager.instance.player.transform.position - transform.position).normalized * speed;
            Destroy(gameObject, destroyTime);
        }

        else if (type == damageType.playerBullet)
        {
            rb.velocity = (transform.forward).normalized * speed;
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
            

            //If damage type has a status effect to apply, applies status effect
            if (status != effectType.none)
            {
                switch (status)
                {
                    default:
                        {
                            break;
                        }
                    case (effectType.burn):
                        {
                            Debug.Log(other.gameObject.name);
                            other.gameObject.GetComponent<playerDamage>().burn();
                            break;
                        }
                    case (effectType.bleed):
                        {
                            other.GetComponent<playerDamage>().bleed();
                            break;
                        }
                    case (effectType.stun):
                        {
                            other.GetComponent<playerDamage>().stun();
                            break;
                        }
                    case (effectType.shock):
                        {
                            if (damageType.stationary == type) other.GetComponent<playerDamage>().shock();
                            break;
                        }
                }
            }

            //if damage type is bullet, is destroyed
            if(type == damageType.bullet)
                Destroy(gameObject);

            else if(type == damageType.fireball)
            {
                dmg.takeDamage(damageAmount);
                Destroy(gameObject);
            }

            //if damage type is the Titan Shield, applies damage
            else if (type == damageType.shield)
            {
                dmg.takeDamage(damageAmount);
                gameObject.GetComponent<AudioSource>().clip = GetComponentInParent<TitanAI>().shieldHit;
                gameObject.GetComponent<AudioSource>().Play();
            }
            else if (type == damageType.stationary && status == effectType.none)
            {
                dmg.takeDamage(damageAmount);
            }

        }
        else
            Destroy(gameObject);
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && damageType.stationary == type && status == effectType.shock) other.gameObject.GetComponent<playerDamage>().unshock();
    }

    public float GetDamageAmount() { return damageAmount; } 

}
