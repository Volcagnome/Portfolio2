using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DreadnaughtTrample : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.instance.player.GetComponent<IDamage>().takeDamage(GetComponentInParent<bossAI>().GetTrampleDamage());

        }
    }
}
