using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Lumin;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [SerializeField] public int EnemySpawnInterval;

    public int NumCurrentGuardRobots;
    public int NumCurrentPatrolRobots;
    public int NumCurrentTotalRobots;
    public List<GameObject> guardPosts_List;
    public List<GameObject> patrolRoutes_List;
    public int maxAllowedRobots;

    //max guards + max robots for each patrol = max robots

    void Awake()
    {
        instance = this;
        StartCoroutine(CalculateMaxAllowedRobots());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AssignRole(GameObject newRobot)
    {

        if (guardPosts_List.Count - NumCurrentGuardRobots < CalculateMaxAllowedPatrolRobots() - NumCurrentPatrolRobots)
        {
            AssignPatrolPost(newRobot);
        }
        else
            AssignGuardPost(newRobot);

        newRobot.GetComponent<enemyAI>().SetEnemyStats();
    }

    public void AssignGuardPost(GameObject newRobot)
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
            newRobot.GetComponent<enemyAI>().SetEnemyStats();
        }
    }

    public void AssignPatrolPost(GameObject newRobot)
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

        newRobot.GetComponent<enemyAI>().SetEnemyStats();
    }

    public void RemoveFromGuardRobotsCount()
    {
        NumCurrentGuardRobots--;
    }

    public void RemoveFromPatrolRobotsCount()
    {
        NumCurrentPatrolRobots--;
    }

    IEnumerator CalculateMaxAllowedRobots()
    {
        yield return new WaitForSeconds(0.6f);

        maxAllowedRobots = CalculateMaxAllowedPatrolRobots() + guardPosts_List.Count;
    }

    public int CalculateMaxAllowedPatrolRobots()
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
        NumCurrentTotalRobots = NumCurrentGuardRobots + NumCurrentPatrolRobots;
        return NumCurrentTotalRobots;
    }

    public void AddRobotToGuardCount()
    {
        NumCurrentGuardRobots++;
    }

    public void AddRobotToPatrolCount()
    {
        NumCurrentPatrolRobots++;
    }

}
