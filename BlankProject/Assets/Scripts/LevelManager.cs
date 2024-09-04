using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

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

    int numGuardResponders;
    int numTitanResponders;

    [SerializeField] int maximumSecurityBreachLevel;
    [SerializeField] int minTimeBeforeReduceBreachLevel;
    [SerializeField] int minTimeBeforeIncreaseBreachLevel;
    [SerializeField] int secondsBetweenSpiderSpawns;
    Coroutine breachIncreaseTimer;
    bool readyToIncrease;

    [SerializeField] float searchTimer;
    [SerializeField] float searchRadius;
    [SerializeField] int maxSearchAttempts;
    [SerializeField] float webSpeedDebuff;
 


    public List<GameObject> intruderAlertButtons;
    public List<GameObject> responseTeam ;
    public List<GameObject> spiderSpawners;

    bool intruderAlert;
    bool isRaisingAlarm;
    bool intruderFound;
    bool isBossFight;

    Vector3 intruderLocation;
    GameObject whistleBlower;

    int securityBreachLevel;




    // Start is called before the first frame update
    void Awake()
    {
        isRaisingAlarm = false;
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

        ToggleSpiderSpawners(true);

        GetNumResponders();

        AddToResponseTeam(numGuardResponders, EnemyManager.instance.GetCurrentNumberGuards());
        StartCoroutine(CallReinforcements(numTitanResponders, titanReinforcement));

        whistleBlower.GetComponent<patrolAI>().SetIsWhistleBlower(false);

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
            guard.GetComponent<SharedEnemyAI>().StartOrUpdateFindIntruder(intruderLocation);
            EnemyManager.instance.RemoveGuardFromRoster(guard);
          
        }

        if (numExtraGuards > 0)
        {
            StartCoroutine(CallReinforcements(numExtraGuards, guardReinforcement));
        }
    }

    private void ToggleSpiderSpawners(bool status)
    {
        spiderSpawners.ForEach(spawner =>
        {
            spawner.GetComponent<SpiderSpawner>().ToggleActive(status);
        });
    }

    private void GetNumResponders()
    { 

        switch (securityBreachLevel)
        {
            case 1:
                numGuardResponders = breachLevel_1_numGuards;
                numTitanResponders = breachLevel_1_numTitans;
                break;
            case 2:
                numGuardResponders = breachLevel_2_numGuards;
                numTitanResponders = breachLevel_2_numTitans;
                break;
            case 3:
                numGuardResponders = breachLevel_3_numGuards;
                numTitanResponders = breachLevel_3_numTitans;
                break;
            case 4:
                numGuardResponders = breachLevel_4_numGuards;
                numTitanResponders = breachLevel_4_numTitans;
                break;
            case 5:
                numGuardResponders = breachLevel_5_numGuards;
                numTitanResponders = breachLevel_5_numTitans;

                break;
        }
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
            reinforcement.GetComponent<SharedEnemyAI>().SetDefaultPost(reinforcementSpawner);
            responseTeam.Add(reinforcement);

            reinforcement.GetComponent<SharedEnemyAI>().StartOrUpdateFindIntruder(intruderLocation);

            yield return new WaitForSeconds(0.75f);
        }
    }

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

    public void SetIntruderLocation(Vector3 location) { intruderLocation = location; }

    public void SetIntruderFound(bool status) { intruderFound = status; }

    public bool GetIntruderFound() { return intruderFound; }

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
        isRaisingAlarm = false;
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
 
        if (responseTeam.Count > 0)
            ReturnToRoster();

        if(responseTeam.Count >0)
            ReAssignExtras();

        ToggleSpiderSpawners(false);

        responseTeam.Clear();
    }

    private void ReturnToRoster()
    {
        for(int index = 0; index <responseTeam.Count; index++)
        {
            GameObject responder = responseTeam[index];

            GameObject defaultPost = responder.GetComponent<SharedEnemyAI>().GetDefaultPost();
            SharedEnemyAI.enemyType type = responder.GetComponent<SharedEnemyAI>().GetEnemyType();

            responder.GetComponent<SharedEnemyAI>().SetIsRespondingToAlert(false);

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

    public void AddSpiderSpawnerToList(GameObject spawner) { spiderSpawners.Add(spawner); }

    public void FoundTheIntruder(Vector3 location)
    {
        intruderLocation = location;

        responseTeam.ForEach(responder =>
        {
            responder.GetComponent<SharedEnemyAI>().StartOrUpdateFindIntruder(intruderLocation);
        });
    }

    public int GetMinTimeBetweenSpiderSpawn() { return secondsBetweenSpiderSpawns; }

    public float GetWebSpeedDebuff() { return webSpeedDebuff; }

    public bool GetIsBossFight() { return isBossFight; }

}
