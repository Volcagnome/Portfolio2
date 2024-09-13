using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//Post that guard robots will report to when not currently engaging with the player or responding to an Intruder Alert.


public class GuardPost : MonoBehaviour
{
    //Guard assigned to post
    [SerializeField] GameObject guard;
    [SerializeField] GameObject front;

    private bool isOccupied;
    bool guardAtPost;


    //When guard is at post, will rotate to face the front of the post (the smaller cube on the guard post model)
    private void Update()
    {
        if (guard != null && guardAtPost && guard.GetComponent<SharedEnemyAI>().CheckIfOnDuty())
        {
            Vector3 frontDirection = front.transform.position - guard.transform.position;
            Quaternion rotationToFront = Quaternion.LookRotation(frontDirection);

            if (guardAtPost && Quaternion.Angle(guard.transform.rotation, rotationToFront) > 3f)
                guard.transform.rotation = Quaternion.Lerp(guard.transform.rotation, rotationToFront, Time.deltaTime * guard.GetComponent<SharedEnemyAI>().GetRotationSpeed());
        }
    }


    //Trigger collider monitors if guard is at post.
    private void OnTriggerEnter(Collider guardRobot)
    {

        if (guardRobot.gameObject == guard && guardRobot.GetComponent<SharedEnemyAI>().CheckIfOnDuty() == true)
                guardAtPost = true;
    }

    private void OnTriggerExit(Collider guardRobot)
    {
        if (guardRobot.gameObject == guard && guardRobot.GetComponent<SharedEnemyAI>().CheckIfOnDuty() == false)
            guardAtPost = false;
    }

    public void SetIsOccupied(bool status) { isOccupied = status;}

    public bool CheckIfOccupied(){ return isOccupied; }

    public void AssignGuard(GameObject newGuard) { guard = newGuard; }

}
