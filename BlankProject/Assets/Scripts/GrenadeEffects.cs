using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grenadeEffects : MonoBehaviour
{
    // For storing the grenade being used:
    grenadeStats ThrowThisGrenade;

    Vector3 decoyPos;

    // Start is called before the first frame update
    void Start()
    {
        ThrowThisGrenade = GameManager.instance.grenadeScript.currentGrenade;

        // EMP effects:
        if (ThrowThisGrenade.isEMP)
        {
            StartCoroutine(ExplodeEMP());
        }

        // Distraction throwable:
        else if (ThrowThisGrenade.isDecoy)
        {
            StartCoroutine(ExplodeDecoy());
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Updates to current decoy position:
        if (ThrowThisGrenade.isDecoy)
        {
            // Start attracting enemies:
            StartCoroutine(attractEnemies());
        }

        if (ThrowThisGrenade.isEMP)
        {
            
        }
    }

    // Grenade functionality:
    IEnumerator ExplodeEMP()
    {
        // Play explosion sound:
        GameManager.instance.playAud(ThrowThisGrenade.effectSound, ThrowThisGrenade.effectVol);

        // Wait for fuse time:
        yield return new WaitForSeconds(ThrowThisGrenade.fuseTime);

        // Play explosion particles:
        Instantiate(ThrowThisGrenade.explosionFX, transform.position, Quaternion.identity);

        // Check area of effect to deal damage:
        checkAOE();

        // Destroy game object instance when finished:
        Destroy(gameObject);
    }

    IEnumerator ExplodeDecoy()
    {
        yield return new WaitForSeconds(ThrowThisGrenade.startFuse);

        // Wait for fuse time minus time that already passed:
        yield return new WaitForSeconds(ThrowThisGrenade.fuseTime - ThrowThisGrenade.startFuse);

        // Play explosion particles:
        Instantiate(ThrowThisGrenade.explosionFX, transform.position, Quaternion.identity);

        // Destroy game object instance when finished:
        Destroy(gameObject);
    }

    private IEnumerator attractEnemies()
    {
        // Wait for a few seconds before attracting enemies to position:
        yield return new WaitForSeconds(ThrowThisGrenade.startFuse);

        // Store current grenade location:
        decoyPos = gameObject.transform.position;

        if (ThrowThisGrenade.isDecoy)
        {
            SharedEnemyAI enemy;

            // Decoy grenade effects:
            if (ThrowThisGrenade.isDecoy)
            {
                // Set array of colliders:
                Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, ThrowThisGrenade.areaOfEffect);
                // Check each collider in array:
                foreach (Collider target in colliders)
                {
                    enemy = target.GetComponent<SharedEnemyAI>();

                    // If enemy has SharedAI and isn't already alerted:
                    if (enemy != null)
                    {
                        
                        // Set enemies last known player location to the decoy's current position.

                        // Calls method that updates FindIntruder and waits for a few seconds before
                        // resuming bot behaviors.
                        enemy.DistractBots(decoyPos, ThrowThisGrenade.distractTime);
                    }
                }
            }
        }
    }

    private void checkAOE()
    {
        // Store current grenade location:
        decoyPos = gameObject.transform.position;

        // EMP grenade effects:
        if (ThrowThisGrenade.isEMP)
        {
            IDamage dmg;
            SharedEnemyAI enemy;
            // Set array of colliders:
            Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, ThrowThisGrenade.areaOfEffect);
            // Check each collider in array:
            foreach (Collider target in colliders)
            {
                // Take a collider within our array of colliders caught within the area:
                enemy = target.GetComponent<SharedEnemyAI>();
                dmg = target.GetComponent<IDamage>();

                if (enemy != null)
                {
                    if (dmg != null)
                    {
                        // Deal grenade damage stat:
                        target.GetComponent<IDamage>().takeDamage(ThrowThisGrenade.aoeDamage);
                    }
                    // Add grenade's stun time to disable enemy for that amount of time:
                    
                    StartCoroutine(enemy.WaitForStun(ThrowThisGrenade.stunTime, decoyPos));
                }
            }
        }

    }
}