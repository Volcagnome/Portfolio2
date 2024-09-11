using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSoundEffect : MonoBehaviour
{
    
    public enum soundType { none, bulletHit, bulletCriticalHit, obstacleHit };
    [SerializeField] soundType effectSoundType;
    [SerializeField] AudioSource audioPlayer;

    // Start is called before the first frame update
    void Start()
    {
        if (effectSoundType == soundType.none)
            audioPlayer.PlayOneShot(audioPlayer.clip);
        else
        {
            List<AudioClip> trackList = new List<AudioClip>();

            if (effectSoundType == soundType.bulletHit)
                trackList = EnemyManager.instance.robotHitSounds;
            else if (effectSoundType == soundType.bulletCriticalHit)
                trackList = EnemyManager.instance.robotCriticalHitSounds;

            int playTrack = Random.Range(0, trackList.Count);

            audioPlayer.PlayOneShot(trackList[playTrack]);
        }

         
    }
}
