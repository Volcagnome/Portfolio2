using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pressurePlate : MonoBehaviour, ISendState
{
    [SerializeField] List<GameObject> enemyList = new List<GameObject>();
    bool itemState = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!itemState) itemState = true;

            enemyList.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyList.Remove(other.gameObject);

            if (enemyList.Count == 0 ) itemState = false;
        }
    }

    public bool getState()
    {
    return itemState; 
    }
}
