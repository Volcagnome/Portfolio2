using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Functions similar to guard posts but titan will return to it if they get a certiain distance away.

public class TitanPost : MonoBehaviour
{
    //Titan assigned to post
    [SerializeField] GameObject titan;
    [SerializeField] GameObject front;

    private bool isOccupied;
    bool titanAtPost;

    private void Update()
    {
        if (titan != null)
        {
            Vector3 frontDirection = front.transform.position - titan.transform.position;
            Quaternion rotationToFront = Quaternion.LookRotation(frontDirection);


            if (titanAtPost && Quaternion.Angle(titan.transform.rotation, rotationToFront) > 2f)
                titan.transform.rotation = Quaternion.Lerp(titan.transform.rotation, rotationToFront, Time.deltaTime * titan.GetComponent<SharedEnemyAI>().GetRotationSpeed());
        }
    }

    private void OnTriggerEnter(Collider titanRobot)
    {

        if (titanRobot.gameObject == titan && titanRobot.GetComponent<SharedEnemyAI>().CheckIfOnDuty() == true)
            titanAtPost = true;
    }

    private void OnTriggerExit(Collider titanRobot)
    {
        if (titanRobot.gameObject == titan && titanRobot.GetComponent<SharedEnemyAI>().CheckIfOnDuty() == false)
            titanAtPost = false;
    }

    public bool CheckIfOccupied() { return isOccupied; }

    public void SetIsOccupied(bool status) { isOccupied = status; }

    public void AssignTitan(GameObject newTitan) { titan = newTitan; }

    public GameObject GetAssignedTitan() { return titan; }  
}
