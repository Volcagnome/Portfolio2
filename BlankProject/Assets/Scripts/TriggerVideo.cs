using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

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
            GameManager.instance.DisplayHint("Beware the patrol robots, they will alert other droids to your location if you let them make it to the alert button!");
            videoPlayer.SetActive(true);
            GameManager.instance.player.SetActive(false);
            Destroy(videoPlayer, timeStop);
            GameManager.instance.player.SetActive(true);
            this.gameObject.SetActive(false);

        }
    }
}

