using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Special web attack for spider robots to subdue the player until the guards arrive.

public class WebAttack : damage
{

    //Reference to the spider who instantiated the web projectile.
    GameObject shooter;


    //If the web projectile hits the player, sets the shooter variable to the spider who shot it, and sets the Caught bools on the 
    //player and shooter to true. Otherwise destroys the projectile.
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || other.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && other.gameObject.tag == "Player")
        {
            if (shooter != null)
            {
                //shooter.GetComponent<ArachnoidAI>().SetCaughtPlayer(true);
                //GameManager.instance.playerScript.SetIsCaught(true);
            }
            else Destroy(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void SetShooter(GameObject enemy) { shooter = enemy; }

}
