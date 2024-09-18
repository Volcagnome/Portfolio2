using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;


//Handles all events within the Boss Arena

public class BossFight : MonoBehaviour
{
    //Scene objects
    [SerializeField] GameObject boss;
    [SerializeField] List<GameObject> bossReinforcements;
    [SerializeField] GameObject bossDefaultPost1;
    [SerializeField] GameObject bossDefaultPost2;
    [SerializeField] GameObject bossFightGuard;
    [SerializeField] GameObject bossFightTitan;
    [SerializeField] GameObject MainFrameDoor;
    [SerializeField] GameObject LeverCover;
    [SerializeField] GameObject LeverCoverOpen;
    [SerializeField] GameObject SelfDestructLever;
    [SerializeField] GameObject CommandCodeBossPlatform;
    [SerializeField] GameObject ComputerCore1;
    [SerializeField] GameObject ComputerCore2;
    [SerializeField] GameObject ComputerCore3;
    [SerializeField] Material ComputerCoreNonEmissive;

    //Boss health threshholds that separate different stages of fight and determines when reinforcements will be called and how many
    [SerializeField] int fightStage_2_threshhold;
    [SerializeField] int fightStage_3_threshhold;
    [SerializeField] int fightStage_2_guards;
    [SerializeField] int fightStage_3_guards;
    [SerializeField] int fightStage_3_titans;
    
    int fightStage;
    bool bossFightBegin = false;
    bool bossIsDead;
    bool playerRespawned;

    //Tracks whether or not reinforcements have been called for respective fight stage
    bool spawnedStage2Reinforcements;
    bool spawnedStage3Reinforcements;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.UpdateObjectiveUI("Defeat the Juggernaut.\r\n\r\nPlug in the command codes.\r\n\r\nActivate the self destruct protocol.");

        fightStage = 0;
        spawnedStage2Reinforcements = false;
        spawnedStage3Reinforcements = false;

        MainFrameDoor.transform.GetChild(1).gameObject.SetActive(false);
        MainFrameDoor.transform.GetChild(2).gameObject.SetActive(true);


        if (!EnemyManager.instance.GetIsBossFight())
            EnemyManager.instance.SetIsBossFight(true);
    }

    // Update is called once per frame
    //Calls configured reinforcements when boss's health reaches configured threshholds. When boss is dead,
    //enables final command code pickup, and opens the door to the mainframe. Once the player inserts the command codes,
    //lifts the cover on the self-destruct lever. When the player pulls it, displays the self destruct timer and starts 
    //the countdown.
    void Update()
    {
        
        if (StaticData.bossIsDead_Static)
        {

            CommandCodeBossPlatform.SetActive(true);

            MainFrameDoor.transform.GetChild(1).gameObject.SetActive(false);
            MainFrameDoor.transform.GetChild(2).gameObject.SetActive(true);

            if (GameManager.instance.GetCommandCodesEntered() >= 2)
            {
                ComputerCore1.GetComponent<Renderer>().material = ComputerCoreNonEmissive;
                ComputerCore1.GetComponent<Light>().enabled = false;
            }
            
            if(GameManager.instance.GetCommandCodesEntered() >= 4) 
            {
                ComputerCore2.GetComponent<Renderer>().material = ComputerCoreNonEmissive;
                ComputerCore2.GetComponent<Light>().enabled = false;
            }
           
            if (GameManager.instance.GetCommandCodesEntered() == 6)
            {
                ComputerCore3.GetComponent<Renderer>().material = ComputerCoreNonEmissive;
                ComputerCore3.GetComponent<Light>().enabled = false;

                LeverCover.SetActive(false);
                LeverCoverOpen.SetActive(true);
            }

            if (SelfDestructLever.GetComponent<togglingItem>().getState())
            {
                GameManager.instance.UpdateObjectiveUI("\r\n\r\nEscape and warn the Planetary Defence Force!!");
                GameManager.instance.ActivateSelfDestruct();
                StaticData.selfDestructActivated_Static = true;
            }

            EnemyManager.instance.SetIsBossFight(false);

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
        if (other.gameObject.CompareTag("Player") && bossFightBegin == false && fightStage == 0)
        {
            bossFightBegin = true;
            EnemyManager.instance.SetIsBossFight(true);
            BossApproach();
            fightStage = 1;


            MainFrameDoor.transform.GetChild(1).gameObject.SetActive(true);
        }

        else if(!StaticData.bossIsDead_Static)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {

                if (enemy.GetComponent<SharedEnemyAI>().GetIsSearching())   
                {
                    StopCoroutine(enemy.GetComponent<SharedEnemyAI>().SearchArea(transform.position, 99));
                    enemy.GetComponent<SharedEnemyAI>().SetIsSearching(false);
                    enemy.GetComponent<SharedEnemyAI>().SetLastKnownPlayerLocation(GameManager.instance.transform.position);
                }
            }
        }else if (bossIsDead)
        {

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
        GameObject spawner = IntruderAlertManager.instance.FindClosestObject(bossReinforcements, GameManager.instance.player.transform.position);

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
            randomDist += spawner.transform.position;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDist, out hit, 3f, 1);

            GameObject guard = Instantiate(bossFightGuard, spawner.transform.position, spawner.transform.rotation);

            guard.GetComponent<SharedEnemyAI>().SetDefaultPost(spawner);
            guard.GetComponent<SharedEnemyAI>().SetCurrentDestination(spawner);
            guard.GetComponent<SharedEnemyAI>().SetLastKnownPlayerLocation(GameManager.instance.player.transform.position);
            guard.GetComponent<NavMeshAgent>().SetDestination(guard.GetComponent<SharedEnemyAI>().GetLastKnownPlayerLocation());
        }

        for (int reinforcement = 0; reinforcement < fightStageTitans; reinforcement++)
        {
            randomDist = UnityEngine.Random.insideUnitSphere * 3f;
            randomDist += spawner.transform.position;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDist, out hit, 3f, 1);

            GameObject titan = Instantiate(bossFightTitan, spawner.transform.position, spawner.transform.rotation);
            titan.GetComponent<SharedEnemyAI>().SetDefaultPost(spawner);
            titan.GetComponent<SharedEnemyAI>().SetCurrentDestination(spawner);
            titan.GetComponent<SharedEnemyAI>().SetLastKnownPlayerLocation(GameManager.instance.player.transform.position);
            titan.GetComponent<NavMeshAgent>().SetDestination(titan.GetComponent<SharedEnemyAI>().GetLastKnownPlayerLocation());
        }
    }



}
