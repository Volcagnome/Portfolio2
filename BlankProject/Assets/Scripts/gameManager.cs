using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject player;
    public playerControl playerScript;
    public List<GameObject> levers_List;
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

    public void AddLeverToList(GameObject lever)
    {
        levers_List.Add(lever);
        activeLevers++;
    }

    public void UpdateWinCondition(int leverPulled)
    {
        activeLevers = activeLevers - leverPulled;

        if (activeLevers == 0)
            youWin = true;
    }



  

}
