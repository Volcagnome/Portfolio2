using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Credits : MonoBehaviour
{

    [SerializeField] GameObject partyLight1;
    [SerializeField] GameObject partyLight2;
    [SerializeField] GameObject partyLight3;
    [SerializeField] GameObject credits;
    Vector3 creditsCurrentPosition;
    Vector3 creditsFinalPosition;

    bool lightOn;
    bool creditsDone;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FlashLights());
        creditsDone = false;
        creditsFinalPosition = credits.transform.position + new Vector3(0f, 4475f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        RollCredits();
    }

    IEnumerator FlashLights()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.5f);

            partyLight1.SetActive(!lightOn);
            partyLight2.SetActive(lightOn);
            partyLight3.SetActive(!lightOn);

            lightOn = !lightOn;

        }
    }

    private void RollCredits()
    {
        creditsCurrentPosition = credits.transform.position;
        

        if (Vector3.Distance(creditsCurrentPosition, creditsFinalPosition) <= 0.05f)
        {
            creditsDone = true;
            return;
        }
        else
            credits.transform.position = Vector3.MoveTowards(creditsCurrentPosition, creditsFinalPosition, Time.deltaTime * 75f);

    }


    public void Quit()
    {

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif

    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

}
