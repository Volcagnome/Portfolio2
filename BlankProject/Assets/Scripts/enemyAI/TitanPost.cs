using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanPost : MonoBehaviour
{

    bool isOccupied;
    [SerializeField] GameObject assignedTitan;

    private void Start()
    {
        EnemyManager.instance.titanPosts_List.Add(gameObject);
    }


    private void OnTriggerEnter(Collider eliteRobot)
    {

        if (eliteRobot.gameObject.GetComponentInParent<enemyAI>().GetEnemyType() == enemyAI.enemyType.Titan
            && eliteRobot.GetComponent<enemyAI>().GetDefaultPost() == this)
        {
            Debug.Log("Test");

            eliteRobot.gameObject.transform.rotation = gameObject.transform.rotation;
        }
    }

    public bool CheckIfOccupied() { return isOccupied; }

    public void SetIsOccupied(bool status) { isOccupied = status; }

    public void AssignTitan(GameObject titan) { assignedTitan = titan; }


}
