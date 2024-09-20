using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Lumin;
using UnityEngine.SceneManagement;
using static StaticData;
//using static UnityEditor.FilePathAttribute;

//Handles all guard post and patrol route assingments and keeps track of how many respawnable robots are currently in the scene.


public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    //Reference to enemy prefabs
    [SerializeField] GameObject guard;
    [SerializeField] GameObject patrol;
    [SerializeField] GameObject titan;
    [SerializeField] GameObject spider;
    [SerializeField] GameObject boss;

    //Minimum time between fabricator spawn attempts
    [SerializeField] float fabricatorSpawnInterval;
    //[SerializeField] float minimumTimeBetweenSpawnAttempts;

    //Current count of each robot type and current total count
    private int NumCurrentGuardRobots;
    private int NumCurrentPatrolRobots;
    private int NumCurrentTitanRobots;
    private int NumCurrentTotalRobots;

    //Lists of fabricators and robot posts.
    public List<GameObject> robotFabricators_List;
    public List<GameObject> guardPosts_List;
    public List<GameObject> titanPosts_List;
    public List<GameObject> patrolRoutes_List;

    //Lists of currently spawned guards and titans
    public List<GameObject> guardRoster;
    public List<GameObject> titanRoster;

    //List of endgame spawners that will be activated when the player is backtracking through the level
    public List<GameObject> endGameSpawners_List;

    //Max robots allowed in scene determined by number of posts and patrol route slots
    private int maxAllowedRobots;
    private bool readyToCallFabricator;

    [SerializeField] GameObject bossRobot;
    bool bossIsDead;
    bool isBossFight;
    public List<AudioClip> robotHitSounds;
    public List<AudioClip> robotCriticalHitSounds;

    void Awake()
    {
        instance = this;

        if (GameManager.instance.GetSelfDestructActivated() )
        {
            IntruderAlertManager.instance.DecreaseSpiderSpawnerCooldown();
            ClearDefaultEnemyPostLists();
        }
        else
        {
            readyToCallFabricator = true;
            StartCoroutine(CalculateMaxAllowedRobots());
        }   
    }


    // Update is called once per frame
    //If current number of robots dips below the max allowed number and a robot isn't currently being spawned, 
    //calls the FillEmptyPost function.
    void Update()
    {
        if (NumCurrentTotalRobots < maxAllowedRobots && readyToCallFabricator)
            FillEmptyPost();
    }


    ////////////////////////////////////////
    ///     ROBOT POPULATION CONTROL    ///
    ///////////////////////////////////////


    //Iterates through the list of Patrol Route Starts and adds up their total number of slots on each route to get 
    //the total number of patrol robots allowed on the map.
    public int CalculateMaxAllowedPatrolRobots()
    {
        int tempCount = 0;


        if (patrolRoutes_List.Count != 0)
        {
            for (int index = 0; index < patrolRoutes_List.Count; index++)

                tempCount = tempCount + patrolRoutes_List[index].GetComponent<PatrolWaypoint>().GetMaxRobotsOnThisRoute();

        }

        return tempCount;
    }

    //Adds up total number of allowed patrol robots and the number of guard posts to get the total number of respawnable 
    //robots in the scene.
    IEnumerator CalculateMaxAllowedRobots()
    {
        yield return new WaitForSeconds(2f);

        maxAllowedRobots = CalculateMaxAllowedPatrolRobots() + guardPosts_List.Count;
    }

    
    //Adds the current number of guards and patrol robots to get the current number of respawnable robots.
    public int GetCurrentNumberRobots()
    {
        NumCurrentTotalRobots = NumCurrentGuardRobots + NumCurrentPatrolRobots;
        return NumCurrentTotalRobots;
    }

    //Checks how many guard vacancies and patrol vacancies there are. If there is a greater need for guards, returns
    //the guard enemy type, if there is a greater need for patrols, returns the patrol enemy type.
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


    //After checking which enemy type to spawn, iterates through the list of robot fabricators to find one off cooldown
    //starts its SpawnRobot coroutine and passes it the enemy type to spawn. Then starts the respawn timer. 
    private void FillEmptyPost()
    {

        SharedEnemyAI.enemyType entityToSpawn = WhichEnemyTypeToSpawn();

        
        for (int index = 0; index < robotFabricators_List.Count; index++)
        {
            GameObject currentFabricator = robotFabricators_List[index];

            if (currentFabricator.GetComponent<RobotFabricator>().GetIsReadyToSpawn())
            {
                StartCoroutine(currentFabricator.GetComponent<RobotFabricator>().SpawnRobot(entityToSpawn));
                readyToCallFabricator = false;
                StartCoroutine(MinimumSpawnTimer());
                break;
            }
           
        }
        
    }

    //Timer between spawn attempts if there are multiple robot vacancies.
    IEnumerator MinimumSpawnTimer()
    {
        yield return new WaitForSeconds(fabricatorSpawnInterval);

        readyToCallFabricator = true;
    }



    ////////////////////////////////////////
    ///      ROBOT ROLE ASSIGNMENT      ///
    ///////////////////////////////////////


    //First adds the new robot to the guard robot count and to the roster of guard robots. Then iterates through the list 
    //of guard posts until it finds an empty one and assigns the robot to it. 
    public void AssignGuardPost(GameObject newRobot)
    {
        AddRobotToCount(newRobot);
        AddGuardToRoster(newRobot);

        for (int index = 0; index < guardPosts_List.Count; index++)
        {
            GameObject currentGuardPost = guardPosts_List[index];
            

            if (!currentGuardPost.GetComponent<GuardPost>().CheckIfOccupied())
            { 
                newRobot.GetComponent<SharedEnemyAI>().SetDefaultPost(currentGuardPost);
                newRobot.GetComponent<SharedEnemyAI>().SetCurrentDestination(currentGuardPost);
                currentGuardPost.GetComponent<GuardPost>().AssignGuard(newRobot);
                currentGuardPost.GetComponent<GuardPost>().SetIsOccupied(true);
                break;
            }
        }
    }


    //Adds the new robot to the titan robot count and to the titan roster. Then iterates through the list of titan posts
    //until it finds an empty one and assigns the titan to it.
    public void AssignTitanPost(GameObject newRobot)
    {
        AddRobotToCount(newRobot);
        AddTitanToRoster(newRobot);

        for (int index = 0; index < titanPosts_List.Count; index++)
        {
            GameObject currentTitanPost = titanPosts_List[index];


            if (!currentTitanPost.GetComponent<TitanPost>().CheckIfOccupied())
            {
                newRobot.GetComponent<SharedEnemyAI>().SetDefaultPost(currentTitanPost);
                currentTitanPost.GetComponent<TitanPost>().AssignTitan(newRobot);
                currentTitanPost.GetComponent<TitanPost>().SetIsOccupied(true);
                break;
            }
        }
    }

    //Iterates through the list of Patrol Waypoint Starts until it finds the one with the vacancy and assigns the new robot
    //to it. Sends the new patrol to their patrol start and adds them to the patrol robot count.
    public void AssignPatrolPost(GameObject newRobot)
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
                    newRobot.GetComponent<NavMeshAgent>().destination = currentRoute.transform.position;
                    AddRobotToCount(newRobot);
                    break;
                }
            }
    }



    ////////////////////////////////////////
    ///      ROBOT LIST MANAGEMENT      ///
    ///////////////////////////////////////


    //Decrements the appropriate robot count depending on the robot enemy type and refreshes the current robot count. 
    public void RemoveDeadRobot(GameObject deadRobot)
    {

        SharedEnemyAI.enemyType type = deadRobot.GetComponent<SharedEnemyAI>().GetEnemyType();

        if (type == SharedEnemyAI.enemyType.Guard)
            NumCurrentGuardRobots--;
   
        else if (type == SharedEnemyAI.enemyType.Patrol)
            NumCurrentPatrolRobots--;

        else if (type == SharedEnemyAI.enemyType.Titan)
            NumCurrentTitanRobots--;

        NumCurrentTotalRobots = GetCurrentNumberRobots();
    }

    //Increments the appropriate robot count depending on enemy type and refreshes the current robot count.
    public void AddRobotToCount(GameObject newRobot)
    {

        SharedEnemyAI.enemyType type = newRobot.GetComponent<SharedEnemyAI>().GetEnemyType();

        if (type == SharedEnemyAI.enemyType.Guard)
            NumCurrentGuardRobots++;

        else if (type == SharedEnemyAI.enemyType.Patrol)
            NumCurrentPatrolRobots++;

        else if (type == SharedEnemyAI.enemyType.Titan)
            NumCurrentTitanRobots++;

        NumCurrentTotalRobots = GetCurrentNumberRobots();
    }

    ////////////////////////////////////////
    ///            ENDGAME               ///
    ///////////////////////////////////////


   



    private void ClearDefaultEnemyPostLists()
    {
        guardRoster.Clear();

        titanRoster.Clear();

        guardPosts_List.Clear();

        patrolRoutes_List.Clear();

        titanPosts_List.Clear();
    }


    public GameObject GetEnemyPrefab(SharedEnemyAI.enemyType type)
    {
        GameObject enemyPrefab = null;

        if (type == SharedEnemyAI.enemyType.Guard)
            enemyPrefab = guard;
        else if (type == SharedEnemyAI.enemyType.Patrol)
            enemyPrefab = patrol;
        else if (type == SharedEnemyAI.enemyType.Titan)
            enemyPrefab = titan;
        else if (type == SharedEnemyAI.enemyType.Arachnoid)
            enemyPrefab = spider;
        else if (type == SharedEnemyAI.enemyType.Boss)
            enemyPrefab = boss;

        return enemyPrefab;
    }




    public int GetCurrentNumberGuards() { return NumCurrentGuardRobots; }

    public int GetCurrentNumberTitans() { return NumCurrentTitanRobots; }

    public int GetMaxAllowedRobots() { return maxAllowedRobots; }

    public void RemoveGuardFromRoster(GameObject guard){ guardRoster.Remove(guard);}

    public void RemoveTitanFromRoster(GameObject titan) { titanRoster.Remove(titan); }

    public void AddGuardToRoster(GameObject guard) { guardRoster.Add(guard);}

    public void AddTitanToRoster(GameObject titan) { titanRoster.Add(titan); }

    public float GetEnemySpawnInterval() { return fabricatorSpawnInterval; }

    public bool GetIsBossFight() { return isBossFight; }

    public void SetIsBossFight(bool status) { isBossFight = status; }

    public void SetBossIsDead(bool status) { bossIsDead = status; }

    public bool GetBossIsDead() { return bossIsDead; }
 
    public GameObject GetBoss() { return bossRobot; }



}
