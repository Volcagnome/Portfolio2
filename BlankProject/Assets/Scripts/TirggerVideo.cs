using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TirggerVideo : MonoBehaviour
{
    public GameObject videoPlayer;
    public int timeStop;


    // Start is called before the first frame update
    void Start()
    {
        videoPlayer.SetActive(false);
    }

    // Update is called once per frame

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
           
            videoPlayer.SetActive(true);
            Destroy(videoPlayer, timeStop);
            this.gameObject.SetActive(false);
        }
    }
}

