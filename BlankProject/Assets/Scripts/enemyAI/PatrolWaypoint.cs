using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolWaypoint : MonoBehaviour
{
    [SerializeField] GameObject nextWaypoint;
    [SerializeField] int MaxRobotsOnThisRoute;
    [SerializeField] List<GameObject> robotsAssignedToRoute;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.tag == "Patrol Route Start")
        {
            EnemyManager.instance.patrolRoutes_List.Add(gameObject);
        }
    }

// Update is called once per frame
void Update()
    {

    }

    public int GetNumberRobotsOnThisRoute()
    {
        return robotsAssignedToRoute.Count;
    }

    public int GetMaxRobotsOnThisRoute()
    {
        return MaxRobotsOnThisRoute;
    }

    public void AddRobotToRoute(GameObject newPatrolBot)
    {
        robotsAssignedToRoute.Add(newPatrolBot);
    }

    public void RemoveRobotFromRoute(GameObject deadPatrolBot)
    {
        robotsAssignedToRoute.Remove(deadPatrolBot);
    }




    private void OnTriggerEnter(Collider patrolRobot)
    {

        if (patrolRobot.CompareTag("Enemy")
            && patrolRobot.GetComponent<SharedEnemyAI>().GetCurrentDestination() == gameObject
            && patrolRobot.GetComponent<enemyAI>().CheckIfOnDuty() == true)
        {

            patrolRobot.GetComponent<enemyAI>().SetCurrentDestination(nextWaypoint);
            SendToNextWaypoint(patrolRobot.gameObject);
        }
    }

    private void SendToNextWaypoint(GameObject patrolRobot)
    {
        patrolRobot.GetComponent<patrolAI>().OnPatrol();
    }
}
