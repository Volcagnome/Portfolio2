using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanPost : MonoBehaviour
{

    bool isOccupied;
    [SerializeField] GameObject assignedTitan;

    private void Start()
    {
       
    }


    private void OnTriggerEnter(Collider eliteRobot)
    {

        if (eliteRobot.gameObject.GetComponentInParent<SharedEnemyAI>().GetEnemyType() == SharedEnemyAI.enemyType.Titan
            && eliteRobot.GetComponent<SharedEnemyAI>().GetDefaultPost() == this)
        {
            eliteRobot.gameObject.transform.rotation = gameObject.transform.rotation;
        }
    }

    public bool CheckIfOccupied() { return isOccupied; }

    public void SetIsOccupied(bool status) { isOccupied = status; }

    public void AssignTitan(GameObject titan) { assignedTitan = titan; }


}
