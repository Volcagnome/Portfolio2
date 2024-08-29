using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        GameManager.instance.player.transform.position = gameObject.transform.position;

        GameManager.instance.SetPlayerSpawn(gameObject);
    }
}
