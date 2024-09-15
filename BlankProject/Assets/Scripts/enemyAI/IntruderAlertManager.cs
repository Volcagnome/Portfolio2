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


//Handles the Intruder Alert event

public class IntruderAlertManager: MonoBehaviour
{

    public static IntruderAlertManager instance;
    
    //Objects in scene
    [SerializeField] GameObject reinforcementSpawner;
    public List<GameObject> intruderAlertButtons;
    public List<GameObject> responseTeam;
    public List<GameObject> spiderSpawners;
    GameObject whistleBlower;

    //Prefabs for guard and titan reinforcements
    [SerializeField] GameObject guardReinforcement;
    [SerializeField] GameObject titanReinforcement;


    //Configurable number of guards to respond to each Security Breach Level
    [SerializeField] int breachLevel_1_numGuards;
    [SerializeField] int breachLevel_2_numGuards;
    [SerializeField] int breachLevel_3_numGuards;
    [SerializeField] int breachLevel_4_numGuards;
    [SerializeField] int breachLevel_5_numGuards;

    //Configurable number of titans to respond to each Security Breach Level.
    [SerializeField] int breachLevel_1_numTitans;
    [SerializeField] int breachLevel_2_numTitans;
    [SerializeField] int breachLevel_3_numTitans;
    [SerializeField] int breachLevel_4_numTitans;
    [SerializeField] int breachLevel_5_numTitans;

    //Stats for current Intruder Alert
    int numGuardResponders;
    int numTitanResponders;
    int securityBreachLevel;
    Vector3 intruderLocation;

    //Configurable settings for IntruderAlerts
    [SerializeField] int maximumSecurityBreachLevel;
    [SerializeField] int minTimeBeforeReduceBreachLevel;
    [SerializeField] int minTimeBeforeIncreaseBreachLevel;
    [SerializeField] int secondsBetweenAlarmAttempts;
    [SerializeField] int secondsBetweenSpiderSpawns;

    //Configurable settings for endgame sequence
    [SerializeField] int endgameSpiderSpawnCooldown;

    //Configurable settings when searching an area for the intruder
    [SerializeField] float searchTimer;
    [SerializeField] float searchRadius;
    [SerializeField] int maxSearchAttempts;
    [SerializeField] float webSpeedDebuff;

    //Current Status
    bool readyToIncrease;
    bool intruderAlert;
    bool isRaisingAlarm;
    bool intruderFound;

   

    Coroutine breachIncreaseTimer;

    // Start is called before the first frame update

    void Awake()
    {
        instance = this;
        isRaisingAlarm = false;
        securityBreachLevel = 0;
        readyToIncrease = true;
        StartCoroutine(BreachLevelReduceCoolDown());

        
    }

    // Update is called once per frame
    void Update()
    {
        if (intruderAlert && !GameManager.instance.GetSelfDestructActivated())
        {
            CheckIfAlerted();
        }
    }

    ////////////////////////////////////////
    ///    BEGIN INTRUDER ALERT    ///
    ///////////////////////////////////////



    //Iterates through the passed list and find the element closest to the passed starting point. Used to get the nearest
    //Intruder Alert Button to the WhistleBlower or the nearest guard to the intruder's location.
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


    //Initiates the Intruder Alert. Increases the Security Breach level by 1 if not already at the max, starts the cooldown
    //before the Security Breach Level can be increased again, activates the spider spawners, assembles a response team of
    //guards and titans based on current Security Breach Level and relieves the WhistleBlower so they return to their normal
    //behavior.
    public void IntruderAlert(Vector3 location)
    {
        intruderAlert = true;
        isRaisingAlarm = false;

        intruderLocation = location;

        if (spiderSpawners.Count > 0)
            ToggleSpiderSpawners(true);

        if (!GameManager.instance.GetSelfDestructActivated())
        {

            if (securityBreachLevel < maximumSecurityBreachLevel && readyToIncrease)
                securityBreachLevel++;

            if (breachIncreaseTimer == null)
                breachIncreaseTimer = StartCoroutine(BreachLevelIncreaseCoolDown());

            GetNumResponders();

            AddToResponseTeam(numGuardResponders, EnemyManager.instance.GetCurrentNumberGuards());
            StartCoroutine(CallReinforcements(numTitanResponders, titanReinforcement));

            whistleBlower.GetComponent<patrolAI>().SetIsWhistleBlower(false);
        }

    }


    //Activates all spider spawners on the list.
    private void ToggleSpiderSpawners(bool status)
    {
        if (spiderSpawners.Count > 0)
        {
            spiderSpawners.ForEach(spawner =>
            {
                spawner.GetComponent<SpiderSpawner>().SetIsActive(status);
            });
        }
    }


    //Sets the isRaisingAlarm bool to true so no other patrols attempt to raise the alarm, tracks which robot is the WhistleBlower,
    //and returns the nearest IntruderAlert button to them. 
    public GameObject SetIsRaisingAlarm(GameObject patrolBot)
    {
        isRaisingAlarm = true;

        whistleBlower = patrolBot;

        return FindClosestObject(intruderAlertButtons, patrolBot.transform.position);
    }

    //If the WhistleBlower is killed, clears the whistleBlower variable and sets isRaisingAlarm bool to false so another patrol
    //robot who sees the player can become the WhistleBlower after the configured cooldown.
    public void WhistleBlowerKilled()
    {
        whistleBlower = null;
        isRaisingAlarm = true;
        StartCoroutine(AlarmCooldown());
    }


    //Waits the configured number of seconds before another patrol robot can attempt to raise the alarm.
    IEnumerator AlarmCooldown()
    {
        yield return new WaitForSeconds(secondsBetweenAlarmAttempts);

        isRaisingAlarm = false;
    }


    ////////////////////////////////////////
    ///    ASSEMBLING RESPONSE TEAM     ///
    ///////////////////////////////////////


    //Sets the number of guards and titans to call depending on current Security Breach Level.
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


    //Checks how many guards are on the map against how many are needed to respond to the current Intruder Alert, if there are enough
    //adds them to the response team starting with those closest to the intruder's location. If there aren't enough, adds existing
    //guards to the team and spawns the remainder at the reinforcement spawner. Titans will always be spawned at the reinforcement
    //spawner. All guards added to the response team will be temporarily removed from the guard roster. 
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


    //Spawns passed number of passed reinforement entity (titan or guard). Spawns at random location within small radius around 
    //reinforcement spawner and waits a moment between each spawn so they are not too tightly clustered.
    IEnumerator CallReinforcements(int numExtras, GameObject entityToSpawn)
    {
        Vector3 randomDist;

        for (int newGuards = 0; newGuards < numExtras; newGuards++)
        {
            randomDist = UnityEngine.Random.insideUnitSphere * 3f;
            randomDist += reinforcementSpawner.transform.position;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDist, out hit, 3f, 1);

            GameObject reinforcement = Instantiate(entityToSpawn, hit.position, reinforcementSpawner.transform.rotation);
            reinforcement.GetComponent<SharedEnemyAI>().SetDefaultPost(reinforcementSpawner);
            reinforcement.GetComponent<SharedEnemyAI>().SetCurrentDestination(reinforcementSpawner);
            responseTeam.Add(reinforcement);

            reinforcement.GetComponent<SharedEnemyAI>().StartOrUpdateFindIntruder(intruderLocation);
  

            yield return new WaitForSeconds(0.75f);
        }
    }


    //Will forward the passed player location to all robots on the response team. They will then travel to the new location
    //in search of the player.
    public void FoundTheIntruder(Vector3 location)
    {
        intruderLocation = location;

        responseTeam.ForEach(responder =>
        {
            responder.GetComponent<SharedEnemyAI>().StartOrUpdateFindIntruder(intruderLocation);
        });
    }


    //Waits the configured amount of time before decrementing the Security Breach Level if it is not already at 1.
    IEnumerator BreachLevelReduceCoolDown()
    {
        while (true)
        {
            yield return new WaitForSeconds(minTimeBeforeReduceBreachLevel);

            if (securityBreachLevel > 1)
                securityBreachLevel--;
        }
    }


    //Waits for the configured amount of time before the Security Breach Level can be increased to avoid the difficulty increasing
    //too rapidly if the player gets into a tight spot.
    IEnumerator BreachLevelIncreaseCoolDown()
    {
        readyToIncrease = false;

            yield return new WaitForSeconds(minTimeBeforeIncreaseBreachLevel);

        readyToIncrease = true;

        breachIncreaseTimer = null;

    }

  

    ////////////////////////////////////////
    ///      ENDING INTRUDER ALERT      ///
    ///////////////////////////////////////


    //All enemies have an AlertStatus child object whose tag will cycle between Alerted and Idle depending on the parent enemy's
    //status. During an Intruder Alert, the LevelManager will count the number of Alerted robots. If that number is 0, cancels
    //the Intruder Alert.
    private void CheckIfAlerted()
    {

        int alertedRobots = 0;

        if (GameObject.FindWithTag("Alerted"))
            alertedRobots++;

        if (alertedRobots == 0)
            CancelIntruderAlert();

    }
    
    //Sets intruderAlert to false and returns all guards to their posts, reassigns guards that were spawned by the reinforcement
    //spawner, deactivates the spider spawners, and clears any extra response team members off the list.
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


    //Iterates through the response team, if a responder is a guard who was originally in the scene before the intruder alert,
    //their default post will be a guard post. Re-adds the guard to the guard roster. Sets isRespondingToAlert to false so they 
    //resume their normal behavior.
    private void ReturnToRoster()
    {
        for(int index = 0; index <responseTeam.Count; index++)
        {
            GameObject responder = responseTeam[index];

            GameObject defaultPost = responder.GetComponent<SharedEnemyAI>().GetDefaultPost();

            responder.GetComponent<SharedEnemyAI>().SetIsRespondingToAlert(false);

            if (defaultPost.GetComponent<GuardPost>())
                EnemyManager.instance.AddGuardToRoster(responder);
            
        };

    }


    //Iterates through the remaining response team list. If they were spawned in as reinforcements, their default post will be 
    //the reinforcement spawner. If they are a guard, they are reassigned to empty guard posts by the EnemyManager if any. All 
    //remaining guards and all titan reinforcements will return to the reinforcement spawner where they will despawn.
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

    ////////////////////////////////////////
    ///             ENDGAME             ///
    ///////////////////////////////////////

    public void DecreaseSpiderSpawnerCooldown()
    {
        secondsBetweenSpiderSpawns = endgameSpiderSpawnCooldown;
    }

    public void EndgameIntruderAlert()
    {
        intruderAlert = true;
        isRaisingAlarm = false;

        if (spiderSpawners.Count > 0)
            ToggleSpiderSpawners(true);
    }


    ////////////////////////////////////////
    ///          GETTERS/SETTERS         ///
    ///////////////////////////////////////

    public GameObject GetReinforcementSpawner() { return reinforcementSpawner; }

    public int GetSearchAttempts() { return maxSearchAttempts; }

    public float GetSearchRadius() { return searchRadius; } 

    public float GetSearchTimer() { return searchTimer; }

    public int GetMinTimeBetweenSpiderSpawn() { return secondsBetweenSpiderSpawns; }

    public float GetWebSpeedDebuff() { return webSpeedDebuff; }

    public bool GetIntruderAlert() { return intruderAlert; }

    public bool GetIsRaisingAlarm() { return isRaisingAlarm; }

    public Vector3 GetIntruderLocation() { return intruderLocation; }

    public int GetMaxSearchAttempts() { return maxSearchAttempts; }    


}
