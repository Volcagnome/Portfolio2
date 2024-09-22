using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class DecoyPingScript : MonoBehaviour
{
    grenadeStats thisDecoy;

    [SerializeField] Light pointLight;
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
                pointLight.enabled = true;
                yield return new WaitForSeconds(0.3f);
                pointLight.enabled = false;
                // Wait for next ping, minus the delay for the light:
                yield return new WaitForSeconds(pingDelay-0.3f);
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
