using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDoorOpener : MonoBehaviour
{
    [SerializeField] GameObject door;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<SharedEnemyAI>() != null)
        {
            door.transform.GetChild(1).gameObject.SetActive(false);
            door.transform.GetChild(2).gameObject.SetActive(true);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("test");

        if (other.gameObject.GetComponent<SharedEnemyAI>() != null)
        {
            door.transform.GetChild(1).gameObject.SetActive(true);
            door.transform.GetChild(2).gameObject.SetActive(false);
        }
    }

}
