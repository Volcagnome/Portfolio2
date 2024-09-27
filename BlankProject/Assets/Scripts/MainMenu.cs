using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEditor;

public class MainMenu : MonoBehaviour
{

    [Header("Volume Settings")]
    [SerializeField] private TMP_Text volumeTextValue = null;
    [SerializeField] private Slider volumeSliderValue = null;
    [SerializeField] private float defaultVolume = 0.5f;


    [Header("GamePlay Settings")]
    [SerializeField] private TMP_Text controllerSensTextValue = null;
    [SerializeField] private Slider controllerSensSliderValue = null;
    [SerializeField] private int defaultSens = 4;
    public int mainControllerSens = 4;

    [Header("Toggle Settings")]
    [SerializeField] private Toggle invertYToggle = null;

    [Header("Graphics Settings")]
    [SerializeField] private Slider brightnessSlider = null;
    [SerializeField] private TMP_Text brightnessTextValue = null;
    [SerializeField] private float defaultBrightness = 1;


    private bool _isFullScreen;
    private float _brightnessLevel;

    [Header("Confirmation")]
    [SerializeField] private GameObject confirmationPrompt = null;


    private void Start()
    {
        SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
    }

    public static void SetActiveScene(UnityEngine.SceneManagement.Scene scene)
    {
       
    }
    public void Play()
    {
        SceneManager.LoadScene("Main Scene Tutorial Level");
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void ViewCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        volumeTextValue.text = volume.ToString("0.0");
    }

    public void VolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
        StartCoroutine(ConfirmationBox());
    }

    public void SetControllerSens(float sensitivity)
    {
        mainControllerSens = Mathf.RoundToInt(sensitivity);
        controllerSensTextValue.text = sensitivity.ToString("0");
    }

    public void GamePlayApply()
    {
        if (invertYToggle.isOn)
        {
            PlayerPrefs.SetInt("masterInvertY", 1);
        }
        else
        {
            PlayerPrefs.SetInt("masterInvertY", 0);
        }

        PlayerPrefs.SetFloat("masterSens", mainControllerSens);
        StartCoroutine(ConfirmationBox());
    }

    public void SetBrightness(float brightness)
    {
        _brightnessLevel = brightness;
        brightnessTextValue.text = brightness.ToString("0.0");
    }

    public void SetFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen; 
    }

    public void GraphicsApply()
    {
        PlayerPrefs.SetFloat("masterBrightness", _brightnessLevel);

        StartCoroutine(ConfirmationBox());
    }

    public void ResetButton(string MenuType)
    {
        if (MenuType == "Graphic")
        {
            brightnessSlider.value = defaultBrightness;
            brightnessTextValue.text = defaultBrightness.ToString("0.0");

        }

        if (MenuType == "Audio")
        {
            AudioListener.volume = defaultVolume;
            volumeSliderValue.value = defaultVolume;
            volumeTextValue.text = defaultVolume.ToString("0.0");
            VolumeApply();
        }

        if (MenuType == "GamePlay")
        {
            controllerSensTextValue.text = defaultSens.ToString("0");
            controllerSensSliderValue.value = defaultSens;
            mainControllerSens = defaultSens;
            invertYToggle.isOn = false;
            GamePlayApply();
        }
    }

    public IEnumerator ConfirmationBox()
    {
        confirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(2);
        confirmationPrompt.SetActive(false);
    }
}
