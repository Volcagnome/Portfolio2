using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//Special ammo for the boss's rocket turret.

public class Rocket : damage
{
    //Components
    [SerializeField] ParticleSystem explosion;
    [SerializeField] AudioSource audioPlayer;
    [SerializeField] AudioClip flightLoop;

 
    [SerializeField] float areaOfEffect;
    [SerializeField] float aoeDamage;
    [SerializeField] float offsetRange;
    Vector3 offset;

    GameObject directHit;

    // Start is called before the first frame update
    //On instantiation will add random offset within the offset range so rockets have a spread around the player's position.
    //Plays the rocket's flight loop audio clip on loop until it explodes.
    void Start()
    {
        offset = new Vector3(Random.Range(-offsetRange, offsetRange), 0f, Random.Range(-offsetRange, offsetRange));

        rb.velocity = (GameManager.instance.player.transform.position - transform.position + offset).normalized * speed;
        audioPlayer.loop = true;
        audioPlayer.PlayOneShot(flightLoop);
    }
    
    //Overrides the OnTriggerEnter function in order to apply AOE damage. Checks to make sure object hit is not the 
    //boss itself so they don't explode when instantiated at the tip of the turret. If the rocket itself hits the player
    //or another enemy it is considered a direct hit and will apply critical damage. On impact will instantiate its explosion
    //particle effect and be destroyed.
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


    //Checks for damageable objects within its areaOfEffect radius. Makes sure targets hit were not direct hits (an already had
    //critical damage applied) and that target is not the boss before applying AOE damage.
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
                    Debug.Log(target.gameObject.name);


                    dmg.takeDamage(aoeDamage);
                }
            }
        }
    }
}
