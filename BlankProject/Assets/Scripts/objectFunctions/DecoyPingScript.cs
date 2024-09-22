using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class DecoyPingScript : MonoBehaviour
{
    grenadeStats thisDecoy;

    [SerializeField] int NoOfPings;
    float pingDelay;
    int pingCount;

    // Start is called before the first frame update
    void Start()
    {
        
        if (GameManager.instance.grenadeScript.currentGrenade.isDecoy)
        {
            thisDecoy = GameManager.instance.grenadeScript.currentGrenade;
            // Assign an equal delay between pings based on the number of pings entered:
            pingDelay = (GameManager.instance.grenadeScript.currentGrenade.fuseTime / NoOfPings);

            // Play pings before being destroyed:
            StartCoroutine(PlayDecoyPings());
        }
    }

    protected IEnumerator PlayDecoyPings()
    {
        // If not the last ping:
        while (pingCount != NoOfPings)
        {
            if (pingCount != NoOfPings)
            {
                // Play next ping:
                PlayPing();
                pingCount++;
                yield return new WaitForSeconds(pingDelay);
            }
            else
                break;
        }
    }

    protected void PlayPing()
    {
        GameManager.instance.playAud(thisDecoy.effectSound, thisDecoy.effectVol);
    }
}
