using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;


    public GameObject player;
    public playerControl playerScript;
    private int activeLevers;
    public bool youWin;

    public bool isPaused;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerControl>();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetButtonDown("Cancel"))
    //    {
    //        if (menuActive == null)
    //        {
    //            statePause();
    //            menuActive = menuPause;
    //            menuActive.SetActive(isPaused);
    //        }
    //        else if (menuActive == menuPause)
    //        {
    //            stateUnpaused();
    //        }
    //    }
    //}

    //public void statePause()
    //{
    //    isPaused = !isPaused;
    //    Time.timeScale = 0;
    //    Cursor.visible = true;
    //    Cursor.lockState = CursorLockMode.Confined;
    //}

    //public void stateUnpaused()
    //{
    //    isPaused = !isPaused;
    //    Time.timeScale = 1;
    //    Cursor.visible = false;
    //    Cursor.lockState = CursorLockMode.Locked;
    //    menuActive.SetActive(isPaused);
    //    menuActive = null;
    //}

    public void UpdateWinCondition(int lever)
    {
        activeLevers += lever;

        if (activeLevers == 0) youWin = true;
    }



  

}
