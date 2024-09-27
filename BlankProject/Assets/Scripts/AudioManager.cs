using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    [SerializeField] AudioSource BGMusic;

    public enum musicTrack { defaultMusic, intruderAlertMusic, bossFightMusic, escapeMusic }
    [SerializeField] List<AudioClip> music;

    bool readyToChangeTrack;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        BGMusic.volume = StaticData.musicVolume_Static;

        if(SceneManager.GetActiveScene().name == "Main Scene Final Level")
        {
            if (!StaticData.bossIsDead_Static && BossFight.instance.GetBossFightStage() > 0)
            {
                BGMusic.clip = music[(int)musicTrack.escapeMusic];
                BGMusic.Play();
            }
        }

        if (StaticData.selfDestructActivated_Static && SceneManager.GetActiveScene().name != "Credits")
        {
            BGMusic.clip = music[(int)musicTrack.escapeMusic];
            BGMusic.Play();
        }

    }

    private void Update()
    {
        BGMusic.volume = StaticData.musicVolume_Static;
    }


    IEnumerator FadeOut(AudioClip track)
    { 
        readyToChangeTrack = false;

        while (BGMusic.volume > 0f)
        { 
            BGMusic.volume = Mathf.Lerp(BGMusic.volume, 0f, Time.deltaTime * 2f);


            if(BGMusic.volume < 0.02f)
            {
                BGMusic.volume = 0f;
                BGMusic.Stop();
                break;
            }

            yield return null;

        }
        StartCoroutine(FadeIn(track));
        yield break;
    }


    IEnumerator FadeIn(AudioClip track)
    {
        BGMusic.clip = track;
        BGMusic.Play();

        while (BGMusic.volume < StaticData.musicVolume_Static)
        {
            BGMusic.volume = Mathf.Lerp(BGMusic.volume, 1f, Time.deltaTime * 2f);

            if (BGMusic.volume > StaticData.musicVolume_Static - 0.1f)
                BGMusic.volume = StaticData.musicVolume_Static;
            yield return null;
        }
        yield break;
    
    }

    public void ChangeTrack(musicTrack track)
    {
        StartCoroutine(FadeOut(music[(int)track]));
    }

}
