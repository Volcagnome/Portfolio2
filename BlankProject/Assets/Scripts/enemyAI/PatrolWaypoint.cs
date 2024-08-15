using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolWaypoint : MonoBehaviour
{
    [SerializeField] GameObject nextWaypoint;
    [SerializeField] int MaxRobotsOnThisRoute;
    private int robotsAssignedToRoute;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.tag == "Patrol Route Start")
        {
            robotsAssignedToRoute = 0;
            EnemyManager.instance.patrolRoutes_List.Add(gameObject);
        }
    }

// Update is called once per frame
void Update()
    {

    }


    public int GetNumberRobotsOnThisRoute()
    {
        return robotsAssignedToRoute;
    }

    public int GetMaxRobotsOnThisRoute()
    {
        return MaxRobotsOnThisRoute;
    }

    public void AddRobotToRoute()
    {
        robotsAssignedToRoute++;
    }

    public void RemoveRobotFromRoute()
    {
        robotsAssignedToRoute--;
    }




    private void OnTriggerEnter(Collider patrolRobot)
    {

        if (patrolRobot.CompareTag("Enemy")
            && patrolRobot.GetComponent<enemyAI>().GetCurrentDestination() == gameObject
            && patrolRobot.GetComponent<enemyAI>().CheckIfOnDuty() == true)
        {

            patrolRobot.GetComponent<enemyAI>().SetCurrentDestination(nextWaypoint);
            SendToNextWaypoint(patrolRobot.gameObject);
        }
    }

    private void SendToNextWaypoint(GameObject patrolRobot)
    {
        patrolRobot.GetComponent<enemyAI>().OnPatrol();
    }
}
