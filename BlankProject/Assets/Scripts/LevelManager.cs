using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.FilePathAttribute;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;

    [SerializeField] int secondsBetweenAlarmAttempts;
    [SerializeField] GameObject reinforcementSpawner;
    [SerializeField] GameObject guardReinforcement;
    [SerializeField] GameObject titanReinforcement;

    [SerializeField] int breachLevel_1_numGuards;
    [SerializeField] int breachLevel_2_numGuards;
    [SerializeField] int breachLevel_3_numGuards;
    [SerializeField] int breachLevel_4_numGuards;
    [SerializeField] int breachLevel_5_numGuards;

    [SerializeField] int breachLevel_1_numTitans;
    [SerializeField] int breachLevel_2_numTitans;
    [SerializeField] int breachLevel_3_numTitans;
    [SerializeField] int breachLevel_4_numTitans;
    [SerializeField] int breachLevel_5_numTitans;


    [SerializeField] int maximumSecurityBreachLevel;
    [SerializeField] int minTimeBeforeReduceBreachLevel;
    [SerializeField] int minTimeBeforeIncreaseBreachLevel;
    Coroutine breachIncreaseTimer;
    bool readyToIncrease;

    [SerializeField] float searchTimer;
    [SerializeField] float searchRadius;
    [SerializeField] int maxSearchAttempts;


    public List<GameObject> intruderAlertButtons;
    public List<GameObject> responseTeam;
    bool intruderAlert;
    bool isRaisingAlarm;
    bool intruderFound;

    Vector3 intruderLocation;
    GameObject whistleBlower;

    int securityBreachLevel;




    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        securityBreachLevel = 0;
        readyToIncrease = true;
        StartCoroutine(BreachLevelReduceCoolDown());
    }

    // Update is called once per frame
    void Update()
    {
        if (intruderAlert)
        {
            CheckIfAlerted();
        }
    }

    IEnumerator BreachLevelReduceCoolDown()
    {
        while (true)
        {
            yield return new WaitForSeconds(minTimeBeforeReduceBreachLevel);

            if (securityBreachLevel > 1)
                securityBreachLevel--;
        }
    }

    IEnumerator BreachLevelIncreaseCoolDown()
    {
        readyToIncrease = false;

            yield return new WaitForSeconds(minTimeBeforeIncreaseBreachLevel);

        readyToIncrease = true;
    }

    private void CheckIfAlerted()
    {
        int alertedRobots = 0;

        if(GameObject.FindWithTag("Alerted"))
            alertedRobots++;

        if (alertedRobots == 0)
            CancelIntruderAlert();
    }
    public void IntruderAlert(Vector3 location)
    {
        intruderAlert = true;
        isRaisingAlarm = false;

        intruderLocation = location;

        if(securityBreachLevel < maximumSecurityBreachLevel && readyToIncrease)
            securityBreachLevel++;

        if (breachIncreaseTimer == null)
            StartCoroutine(BreachLevelIncreaseCoolDown());

        int numGuardResponders = GetNumGuardResponders();
        int numTitanResponders = GetNumTitanResponders();

        AddToResponseTeam(numGuardResponders, EnemyManager.instance.GetCurrentNumberGuards());
        StartCoroutine(CallReinforcements(numTitanResponders, titanReinforcement));

    }

    private void AddToResponseTeam(int numGuardResponders, int currentNumGuardsOnMap)
    {
        int numExtraGuards = 0;
        int existingGuardsToSend = 0;
        GameObject guard;
        

        if (numGuardResponders <= currentNumGuardsOnMap)
            existingGuardsToSend = numGuardResponders;

        else if (numGuardResponders > currentNumGuardsOnMap)
        {
            numExtraGuards = numGuardResponders - currentNumGuardsOnMap;
            existingGuardsToSend = currentNumGuardsOnMap;
        }

        for (int responders = 0; responders < existingGuardsToSend; responders++)
        {
            guard = FindClosestObject(EnemyManager.instance.guardRoster, intruderLocation);

            responseTeam.Add(guard);
            guard.GetComponent<enemyAI>().StartOrUpdateFindIntruder(intruderLocation);
            EnemyManager.instance.RemoveGuardFromRoster(guard);
          
        }

        if (numExtraGuards > 0)
        {
            StartCoroutine(CallReinforcements(numExtraGuards, guardReinforcement));
        }
    }

    private int GetNumGuardResponders()
    {
        int numGuardResponders = 0;

        switch (securityBreachLevel)
        {
            case 1:
                numGuardResponders = breachLevel_1_numGuards;
                break;
            case 2:
                numGuardResponders = breachLevel_2_numGuards;
                break;
            case 3:
                numGuardResponders = breachLevel_3_numGuards;
                break;
            case 4:
                numGuardResponders = breachLevel_4_numGuards;
                break;
            case 5:
                numGuardResponders = breachLevel_5_numGuards;
                break;
        }

        return numGuardResponders;
    }

    private int GetNumTitanResponders()
    {
        int numTitanResponders = 0;

        switch (securityBreachLevel)
        {
            case 1:
                numTitanResponders = breachLevel_2_numTitans;
                break;
            case 2:
                numTitanResponders = breachLevel_2_numTitans;
                break;
            case 3:
                numTitanResponders = breachLevel_3_numTitans;
                break;
            case 4:
                numTitanResponders = breachLevel_4_numTitans;
                break;
            case 5:
                numTitanResponders = breachLevel_5_numTitans;
                break;
        }

        return numTitanResponders;  
    }

    private 

    IEnumerator CallReinforcements(int numExtras, GameObject entityToSpawn)
    {
        Vector3 randomDist;

        for (int newGuards = 0; newGuards < numExtras; newGuards++)
        {
            randomDist = UnityEngine.Random.insideUnitSphere * 3f;
            randomDist += LevelManager.instance.reinforcementSpawner.transform.position;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDist, out hit, 3f, 1);

            GameObject reinforcement = Instantiate(entityToSpawn, hit.position, reinforcementSpawner.transform.rotation);
            reinforcement.GetComponent<enemyAI>().SetDefaultPost(reinforcementSpawner);
            responseTeam.Add(reinforcement);

            reinforcement.GetComponent<enemyAI>().StartOrUpdateFindIntruder(intruderLocation);

            yield return new WaitForSeconds(0.75f);
        }
    }

        //--------------If spotted by a patrol bot:
        //--------------any guards within his immediate ally radius are alerted of the player's position and will move to engage
        //--------------patrol bot runs for nearest intruder alert button
        //(button and patrol bot will be highlighted by silouhette)
        //--------------if running patrol bot is killed, the task may pass to any other alerted patrol bot after a cooldown
        //--------------if spotted by guard will alert patrol bots within his ally radius

        //if intruder alert successfully activated:
        //sirens, lights, all deactivated security forcefields re-activated, cooldown for repair on disabled fabricators is reduced depending on breach level
        //arachnoid bot(s) are deployed to subdue player until guards arrive
        //variable number of guards are dispatched to player's last known location depending on breach level
        //if not found at that location, guards will split up into "search parties" and travel to any search nodes within a configurable radius of player's LKL
        //(one node placed in each room with box collider sized to room)
        //will perform a configurable number of "search attempts" (random roams) witihin that room before giving up and returning to idle behavior
        //if player found will alert other search parties
        //guards will return to their posts, extra guards will exit the door they came in from
        //intruder alert cancelled once number of alerted bots = 0 (killed or stop searching)
        //each time red alert triggered, increases security breach level up to maximum. For each level, more guard bots are deployed. 
        /////   level 1: nearest 2 guards are dispatched
        /////   level 2: nearest 4 guards are dispatched
        /////   level 3: nearest 6 guards are dispatched
        /////   level 4: nearest 6 guards and 1 titan dispatched
        /////   level 5: nearest 6 guards and 2 titans dispatched
        // if less than prescribed number of troops currently on map, will be called in from unopenable door. Once they return to idle, will be assigned
        //to available posts or if none exist will exit through unopenable door and despaw
        //breach level gradually decreases over time
        //max breach level can be limited for earlier levels


    public void SetIntruderLocation(Vector3 location) { intruderLocation = location; }

    public void SetIntruderFound(bool status) { intruderFound = status; }

    public bool GetIntruderFound() { return intruderFound; }

    public GameObject FindClosestObject(List<GameObject> listOfObjects, Vector3 startingPoint)
    {
        GameObject closestObject = listOfObjects[0];
        float distanceToFirst = Vector3.Distance(startingPoint, closestObject.transform.position);

        GameObject compareObject;
        float distanceToCompareObject;

        if (listOfObjects.Count > 1)
        {
            for (int objectIndex = 1; objectIndex < listOfObjects.Count; objectIndex++)
            {
                compareObject = listOfObjects[objectIndex];
                distanceToCompareObject = Vector3.Distance(startingPoint, compareObject.transform.position);

                if (distanceToCompareObject < distanceToFirst)
                    closestObject = compareObject;
            }
        }

        return closestObject;
    }

    public void SetIntruderAlert(bool status) { intruderAlert = status; }

    public bool GetIntruderAlert() { return intruderAlert; }

    public bool GetIsRaisingAlarm() { return isRaisingAlarm; }

    public void AddIntruderAlertButton(GameObject button) { intruderAlertButtons.Add(button); }

    public Vector3 GetIntruderLocation() { return intruderLocation; }

    public GameObject SetIsRaisingAlarm(GameObject patrolBot)
    {
        isRaisingAlarm = true;

        whistleBlower = patrolBot;

        return FindClosestObject(intruderAlertButtons,patrolBot.transform.position);
    }

    public void WhistleBlowerKilled()
    {
        whistleBlower = null;
        StartCoroutine(AlarmCooldown());
    }

    IEnumerator AlarmCooldown()
    {
        yield return new WaitForSeconds(secondsBetweenAlarmAttempts);

        isRaisingAlarm = false;
    }



    public void CancelIntruderAlert()
    {
        intruderAlert = false;
        Debug.Log("Response Team Size: " + responseTeam.Count);


        if (responseTeam.Count > 0)
            ReturnToRoster();

        if(responseTeam.Count >0)
            ReAssignExtras();

        responseTeam.Clear();
    }

    private void ReturnToRoster()
    {
        for(int index = 0; index <responseTeam.Count; index++)
        {
            GameObject responder = responseTeam[index];

            GameObject defaultPost = responder.GetComponent<SharedEnemyAI>().GetDefaultPost();
            SharedEnemyAI.enemyType type = responder.GetComponent<SharedEnemyAI>().GetEnemyType();

            responder.GetComponent<enemyAI>().SetIsRespondingToAlert(false);

            if (defaultPost.GetComponent<GuardPost>())
            {
                EnemyManager.instance.AddGuardToRoster(responder);
            }
            else if (defaultPost.GetComponent<TitanPost>())
                EnemyManager.instance.AddTitanToRoster(responder);
        };

    }

    private void ReAssignExtras()
    {
        int emptyGuardPosts = EnemyManager.instance.guardPosts_List.Count - EnemyManager.instance.GetCurrentNumberGuards();

        for (int index = 0; index < responseTeam.Count; index++)
        {
            GameObject responder = responseTeam[index];

            GameObject defaultPost = responder.GetComponent<SharedEnemyAI>().GetDefaultPost();
            SharedEnemyAI.enemyType type = responder.GetComponent<SharedEnemyAI>().GetEnemyType();

            if (defaultPost == reinforcementSpawner && type == SharedEnemyAI.enemyType.Guard && emptyGuardPosts > 0)
            {
                EnemyManager.instance.AssignGuardPost(responder);
                emptyGuardPosts--;
            }
        };
    }

    public void RemoveFromResponseTeam(GameObject responder) { responseTeam.Remove(responder); }

    public GameObject GetReinforcementSpawner() { return reinforcementSpawner; }

    public int GetSearchAttempts() { return maxSearchAttempts; }

    public float GetSearchRadius() { return searchRadius; } 

    public float GetSearchTimer() { return searchTimer; }

    public void FoundTheIntruder(Vector3 location)
    {
        intruderLocation = location;

        responseTeam.ForEach(responder =>
        {
            responder.GetComponent<enemyAI>().StartOrUpdateFindIntruder(intruderLocation);
        });
    }
}
