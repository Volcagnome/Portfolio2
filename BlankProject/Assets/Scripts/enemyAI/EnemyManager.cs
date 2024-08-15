using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Lumin;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [SerializeField] public int EnemySpawnInterval;

    private int NumCurrentGuardRobots;
    private int NumCurrentPatrolRobots;
    public List<GameObject> guardPosts_List;
    public List<GameObject> patrolRoutes_List;
    private int maxAllowedRobots;

    //max guards + max robots for each patrol = max robots

    void Awake()
    {
        instance = this;
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AssignRole(GameObject newRobot)
    {
       // Debug.Log(("Guards:" + NumCurrentGuardRobots + "/" + guardPosts_List.Count));
        //Debug.Log(("Patrol:" + NumCurrentPatrolRobots + "/" + GetMaxAllowedPatrolRobots()));

        if (guardPosts_List.Count - NumCurrentGuardRobots < GetMaxAllowedPatrolRobots() - NumCurrentPatrolRobots)
        {
            AssignAsPatrol(newRobot);
        }
        else
            AssignAsGuard(newRobot);
    }

    public void AssignAsGuard(GameObject newRobot)
    {
        NumCurrentGuardRobots++;
        newRobot.GetComponent<enemyAI>().SetBehavior(enemyAI.behaviorType.guard);

        for (int index = 0; index < guardPosts_List.Count; index++)
        {

            GameObject currentGuardPost = guardPosts_List[index];

            if (!currentGuardPost.GetComponent<GuardPost>().CheckIfOccupied())
            {
                newRobot.GetComponent<enemyAI>().SetDefaultPost(currentGuardPost);
                currentGuardPost.GetComponent<GuardPost>().AssignGuard(newRobot);
                currentGuardPost.GetComponent<GuardPost>().SetIsOccupied(true);
                break;
            }
        }

    }

    public void AssignAsPatrol(GameObject newRobot)
    {
        NumCurrentPatrolRobots++;
        newRobot.GetComponent<enemyAI>().SetBehavior(enemyAI.behaviorType.patrol);


        for (int index = 0; index < patrolRoutes_List.Count; index++)
        {
            GameObject currentRoute = patrolRoutes_List[index];

            int currentRobotsAssigned = currentRoute.GetComponent<PatrolWaypoint>().GetNumberRobotsOnThisRoute();
            int maxAllowedRobotsAssigned = currentRoute.GetComponent<PatrolWaypoint>().GetMaxRobotsOnThisRoute();

            if (currentRobotsAssigned < maxAllowedRobotsAssigned)
            {
                currentRoute.GetComponent<PatrolWaypoint>().AddRobotToRoute();
                newRobot.GetComponent<enemyAI>().SetDefaultPost(currentRoute);
                newRobot.GetComponent<enemyAI>().SetCurrentDestination(currentRoute);
                break;
            }
        }
    }

    public void RemoveFromGuardRobotsCount()
    {
        NumCurrentGuardRobots--;
    }

    public void RemoveFromPatrolRobotsCount()
    {
        NumCurrentPatrolRobots--;
    }

    public int GetMaxAllowedRobots()
    {
        return maxAllowedRobots = GetMaxAllowedPatrolRobots() + guardPosts_List.Count;
    }

    public int GetMaxAllowedPatrolRobots()
    {
        int tempCount = 0;

        patrolRoutes_List.ForEach(route =>
        {
            tempCount = tempCount + route.GetComponent<PatrolWaypoint>().GetMaxRobotsOnThisRoute();
        });

        return tempCount;
    }

    public int GetCurrentNumberRobots()
    {
        return NumCurrentGuardRobots + NumCurrentPatrolRobots;
    }

}
