using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
                robotsAssignedToRoute.Remove(robotsAssignedToRoute[0]) ;
 
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
        if (patrolRobot.gameObject.GetComponent<SharedEnemyAI>().GetCurrentDestination() == gameObject
            && patrolRobot.gameObject.GetComponent<SharedEnemyAI>().CheckIfOnDuty() == true)
        {

            patrolRobot.gameObject.GetComponent<SharedEnemyAI>().SetCurrentDestination(nextWaypoint);
            patrolRobot.gameObject.GetComponent<NavMeshAgent>().SetDestination(nextWaypoint.transform.position);
        }
    }

  
}
