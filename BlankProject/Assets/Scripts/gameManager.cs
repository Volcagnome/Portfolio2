using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject player;
    public playerControl playerScript;
    private int activeLevers;
    public bool youWin;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerControl>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateWinCondition(int lever)
    {
        activeLevers += lever;

        if (activeLevers == 0) youWin = true;
    }



  

}
