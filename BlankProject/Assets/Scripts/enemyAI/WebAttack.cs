using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebAttack : damage
{
    // Start is called before the first frame update


    // Update is called once per frame

    GameObject shooter;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && other.gameObject.tag == "Player")
        {
            shooter.GetComponent<ArachnoidAI>().SetCaughtPlayer(true);
            //GameManager.instance.playerScript.SetIsCaught(true);
        }
        else
            Destroy(gameObject);
    }

    public void SetShooter(GameObject enemy) { shooter = enemy; }

}
