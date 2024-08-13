using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject player;
    public playerControl playerScript;

    public List<GameObject> enemyList;

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

    public void AddToEnemyList(GameObject enemy)
    {
        enemyList.Add(enemy);
    }

    public void RemoveFromEnemyList(GameObject enemy)
    {
        enemyList.Remove(enemy);
    }


}
