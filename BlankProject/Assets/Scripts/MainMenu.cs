using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{



    private void Start()
    {
        SetActiveScene(SceneManager.GetSceneByBuildIndex(2));
    }

    public static void SetActiveScene(UnityEngine.SceneManagement.Scene scene)
    {
       
    }
    public void Play()
    {
        SceneManager.LoadScene("Main Scene");
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Player Has Quit The Game");
    }
}
