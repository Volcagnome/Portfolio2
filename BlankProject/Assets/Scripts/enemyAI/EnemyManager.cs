using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Lumin;
using static enemyAI;


public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [SerializeField] float fabricatorSpawnInterval;
    [SerializeField] float minimumTimeBetweenSpawnAttempts;


    private int NumCurrentGuardRobots;
    private int NumCurrentPatrolRobots;
    private int NumCurrentTitanRobots;
    private int NumCurrentTotalRobots;

    public List<GameObject> robotFabricators_List;
    public List<GameObject> guardPosts_List;
    public List<GameObject> titanPosts_List;
    public List<GameObject> patrolRoutes_List;

    public List<GameObject> guardRoster;
    public List<GameObject> titanRoster;

    private int maxAllowedRobots;
    private bool readyToCallFabricator;

    bossAI boss;

    //max guards + max robots for each patrol = max robots

    void Awake()
    {
        instance = this;
        readyToCallFabricator = true;
        StartCoroutine(CalculateMaxAllowedRobots());
    }

    // Update is called once per frame
    void Update()
    {
        if (NumCurrentTotalRobots < maxAllowedRobots && readyToCallFabricator)
        {
            FillEmptyPost();
        }
    }

    private void FillEmptyPost()
    { 

        SharedEnemyAI.enemyType entityToSpawn = WhichEnemyTypeToSpawn();

        for (int index = 0; index < robotFabricators_List.Count; index++)
        {
            GameObject currentFabricator = robotFabricators_List[index];

            if (currentFabricator.GetComponent<RobotFabricator>().GetIsFunctional()
                && currentFabricator.GetComponent<RobotFabricator>().GetIsReadyToSpawn())
            {
                StartCoroutine(currentFabricator.GetComponent<RobotFabricator>().SpawnRobot(entityToSpawn));
                readyToCallFabricator = false;
                StartCoroutine(MinimumSpawnTimer());
                break;
            }
           
        }
        
    }

    IEnumerator MinimumSpawnTimer()
    {
        yield return new WaitForSeconds(minimumTimeBetweenSpawnAttempts);

        readyToCallFabricator = true;
    }


    private SharedEnemyAI.enemyType WhichEnemyTypeToSpawn()
    {
        int guardPostVacancies = guardPosts_List.Count - NumCurrentGuardRobots;
        int patrolRouteVacancies = CalculateMaxAllowedPatrolRobots() - NumCurrentPatrolRobots;

        SharedEnemyAI.enemyType entityToSpawn = SharedEnemyAI.enemyType.none;

        if (patrolRouteVacancies >= guardPostVacancies)
            entityToSpawn = SharedEnemyAI.enemyType.Patrol;

        else if (guardPostVacancies > patrolRouteVacancies)
            entityToSpawn = SharedEnemyAI.enemyType.Guard;

        return entityToSpawn;

    }

    //public void AssignRole(GameObject newRobot)
    //{
    //    if (newRobot.GetComponent<enemyAI>().GetBehavior() == SharedEnemyAI.behaviorType.guard)
    //    {
    //        AddRobotToGuardCount();
    //        guardRoster.Add(newRobot);
    //    }
    //    else if (newRobot.GetComponent<enemyAI>().GetBehavior() == SharedEnemyAI.behaviorType.patrol)
    //        AddRobotToPatrolCount();

    //    else if (guardPosts_List.Count - NumCurrentGuardRobots < CalculateMaxAllowedPatrolRobots() - NumCurrentPatrolRobots)
    //    {
    //        AssignPatrolPost(newRobot);
    //    }

    //    else
    //        AssignGuardPost(newRobot);

    //    newRobot.GetComponent<enemyAI>().SetEnemyStats();
    //}

    public void AssignGuardPost(GameObject newRobot)
    {
        AddRobotToGuardCount();
        AddGuardToRoster(newRobot);

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

    public void AssignTitanPost(GameObject newRobot)
    {
        AddRobotToTitanCount();
        AddTitanToRoster(newRobot);

        for (int index = 0; index < titanPosts_List.Count; index++)
        {
            GameObject currentTitanPost = titanPosts_List[index];


            if (!currentTitanPost.GetComponent<TitanPost>().CheckIfOccupied())
            {
                newRobot.GetComponent<enemyAI>().SetDefaultPost(currentTitanPost);
                currentTitanPost.GetComponent<TitanPost>().AssignTitan(newRobot);
                currentTitanPost.GetComponent<TitanPost>().SetIsOccupied(true);
                break;
            }

        }
    }

    public void AssignPatrolPost(GameObject newRobot)
    {
        Debug.Log(patrolRoutes_List.Count);

            AddRobotToPatrolCount();

        if( patrolRoutes_List.Count>0)
        {
            for (int index = 0; index < patrolRoutes_List.Count; index++)
            {
                GameObject currentRoute = patrolRoutes_List[index];


                int currentRobotsAssigned = currentRoute.GetComponent<PatrolWaypoint>().GetNumberRobotsOnThisRoute();

        
                int maxAllowedRobotsAssigned = currentRoute.GetComponent<PatrolWaypoint>().GetMaxRobotsOnThisRoute();



                if (currentRobotsAssigned < maxAllowedRobotsAssigned)
                {

                    currentRoute.GetComponent<PatrolWaypoint>().AddRobotToRoute(newRobot);
                    newRobot.GetComponent<SharedEnemyAI>().SetDefaultPost(currentRoute);
                    newRobot.GetComponent<patrolAI>().SetCurrentDestination(currentRoute);

                    break;
                }
            }
        }
    }

    public void RemoveDeadGuard(GameObject deadGuard)
    {
        NumCurrentTotalRobots--;
        NumCurrentGuardRobots--;
    }

    public void RemoveDeadPatrol(GameObject deadPatrol)
    {
        NumCurrentPatrolRobots--;
        NumCurrentTotalRobots--;
    }

    public void RemoveDeadTitan(GameObject deadTitan)
    {
        NumCurrentTitanRobots--;
        NumCurrentTotalRobots--;
    }

    IEnumerator CalculateMaxAllowedRobots()
    {
        yield return new WaitForSeconds(2f);

        
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
        return NumCurrentTotalRobots;
    }

    public int GetCurrentNumberGuards() { return NumCurrentGuardRobots; }

    public int GetCurrentNumberTitans() { return NumCurrentTitanRobots; }


    public void AddRobotToGuardCount()
    {
        NumCurrentGuardRobots++;
        NumCurrentTotalRobots++;
    }

    public void AddRobotToPatrolCount()
    {
        NumCurrentPatrolRobots++;
        NumCurrentTotalRobots++;
    }

    public void AddRobotToTitanCount()
    {
        NumCurrentTitanRobots++;
        NumCurrentTotalRobots++;
    }

    public void AddNewPatrolRoute(GameObject routeStart) { patrolRoutes_List.Add(routeStart); }

    public void AddNewGuardPost(GameObject guardPost) {  guardPosts_List.Add(guardPost);}

    public int GetMaxAllowedRobots() { return maxAllowedRobots; }

    public void RemoveGuardFromRoster(GameObject guard){ guardRoster.Remove(guard);}

    public void RemoveTitanFromRoster(GameObject titan) { titanRoster.Remove(titan); }

    public void AddGuardToRoster(GameObject guard) { guardRoster.Add(guard);}

    public void AddTitanToRoster(GameObject titan) { titanRoster.Add(titan); }

    public float GetEnemySpawnInterval() { return fabricatorSpawnInterval; }
    public bossAI GetBoss() { return boss; }

}
