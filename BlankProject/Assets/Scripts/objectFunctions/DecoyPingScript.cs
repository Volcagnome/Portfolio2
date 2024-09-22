using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoyPingScript : MonoBehaviour
{
    [SerializeField] grenadeStats thisDecoy;
    float pingTimer;

    // Start is called before the first frame update
    void Start()
    {
        pingTimer = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        // Play pings before being destroyed:
        PlayDecoyPings();
    }
    protected void PlayDecoyPings()
    {
        if (pingTimer > thisDecoy.fuseTime)
        {
            GameManager.instance.playAud(thisDecoy.effectSound, thisDecoy.effectVol);
        }

        else if (pingTimer == (thisDecoy.fuseTime / 2))
        {
            GameManager.instance.playAud(thisDecoy.effectSound, thisDecoy.effectVol);
        }

        else if (pingTimer == (thisDecoy.fuseTime))
        {
            GameManager.instance.playAud(thisDecoy.effectSound, thisDecoy.effectVol);
            pingTimer = 0;
        }
    }
}
