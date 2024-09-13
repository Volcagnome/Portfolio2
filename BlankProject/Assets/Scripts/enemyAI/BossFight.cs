using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;


//Handles all events within the Boss Arena

public class BossFight : MonoBehaviour
{
    //Scene objects
    [SerializeField] bossAI boss;
    [SerializeField] GameObject reinforcementSpawner;
    [SerializeField] GameObject bossDefaultPost2;
    [SerializeField] GameObject bossFightGuard;
    [SerializeField] GameObject bossFightTitan;
    [SerializeField] GameObject MainFrameDoor;
    [SerializeField] GameObject LeverCover;
    [SerializeField] GameObject LeverCoverOpen;
    [SerializeField] GameObject SelfDestructLever;
    [SerializeField] GameObject SelfDestructTimer;
    [SerializeField] TMP_Text timeLeft;
    [SerializeField] GameObject CommandCodeBossPlatform;


    //Boss health threshholds that separate different stages of fight and determines when reinforcements will be called and how many
    [SerializeField] int fightStage_2_threshhold;
    [SerializeField] int fightStage_3_threshhold;
    [SerializeField] int fightStage_2_guards;
    [SerializeField] int fightStage_3_guards;
    [SerializeField] int fightStage_3_titans;
    
    int fightStage;
    bool bossFightBegin = false;

    //Tracks whether or not reinforcements have been called for respective fight stage
    bool spawnedStage2Reinforcements;
    bool spawnedStage3Reinforcements;
 
    // Start is called before the first frame update
    void Start()
    {
        fightStage = 0;
        spawnedStage2Reinforcements = false;
        spawnedStage3Reinforcements= false;

        MainFrameDoor.transform.GetChild(1).gameObject.SetActive(false);
        MainFrameDoor.transform.GetChild(2).gameObject.SetActive(true);
    }

    // Update is called once per frame
    //Calls configured reinforcements when boss's health reaches configured threshholds. When boss is dead,
    //enables final command code pickup, and opens the door to the mainframe. Once the player inserts the command codes,
    //lifts the cover on the self-destruct lever. When the player pulls it, displays the self destruct timer and starts 
    //the countdown.
    void Update()
    {


        if (boss.GetComponent<bossAI>().GetIsDead())
        {
            CommandCodeBossPlatform.SetActive(true);

            MainFrameDoor.transform.GetChild(1).gameObject.SetActive(false);
            MainFrameDoor.transform.GetChild(2).gameObject.SetActive(true);

            if (GameManager.instance.GetCommandCodesEntered() == 3)
            {
                LeverCover.SetActive(false);
                LeverCoverOpen.SetActive(true);
            }

            if (SelfDestructLever.GetComponent<togglingItem>().getState())
            {
                GameManager.instance.ActivateSelfDestruct();
            }
        }
        else if (boss.GetComponent<SharedEnemyAI>().GetHP() < fightStage_2_threshhold && !spawnedStage2Reinforcements)
        {
            fightStage = 2;
            SpawnReinforcements(fightStage);
            spawnedStage2Reinforcements = true;
        }
        else if (boss.GetComponent<SharedEnemyAI>().GetHP() < fightStage_3_threshhold && !spawnedStage3Reinforcements)
        {
            fightStage = 3;
            SpawnReinforcements(fightStage);
            spawnedStage3Reinforcements = true;
        }
       
    }

    //When player exits arena entry area, the doors to the mainframe will close, the boss will emerge and the fight will begin.
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && bossFightBegin == false)
        {
            bossFightBegin = true;
            BossApproach();
            fightStage = 1;
            boss.GetEnemyHealthBar().SetActive(true);

            MainFrameDoor.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    //Makes boss approach the arena entry area.
    private void BossApproach()
    {
        boss.GetComponent<SharedEnemyAI>().SetDefaultPost(bossDefaultPost2);
        boss.GetComponent<NavMeshAgent>().SetDestination(bossDefaultPost2.transform.position);

    }

    //Spawns reinforcements depending on current stage of the fight. Spawns them at random locations in a short radius
    //around the spawner so they will be slightly dispersed.
    private void SpawnReinforcements(int stage)
    {
        int fightStageGuards = 0;
        int fightStageTitans = 0;

        if (stage == 2)
        {
            fightStageGuards = fightStage_2_guards;
            fightStageTitans = 0;
        }
        else if(stage == 3) 
        {
            fightStageGuards = fightStage_3_guards;
            fightStageTitans = fightStage_3_titans;
        }
        

        Vector3 randomDist;

        for (int reinforcement = 0; reinforcement < fightStageGuards; reinforcement++)
        {
            randomDist = UnityEngine.Random.insideUnitSphere * 3f;
            randomDist += reinforcementSpawner.transform.position;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDist, out hit, 3f, 1);

            GameObject guard = Instantiate(bossFightGuard, hit.position, reinforcementSpawner.transform.rotation);

            guard.GetComponent<SharedEnemyAI>().SetDefaultPost(reinforcementSpawner);
        }

        for (int reinforcement = 0; reinforcement < fightStageTitans; reinforcement++)
        {
            randomDist = UnityEngine.Random.insideUnitSphere * 3f;
            randomDist += reinforcementSpawner.transform.position;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDist, out hit, 3f, 1);

            GameObject titan = Instantiate(bossFightTitan, hit.position, reinforcementSpawner.transform.rotation);
            titan.GetComponent<SharedEnemyAI>().SetDefaultPost(reinforcementSpawner);
        }
    }

}
