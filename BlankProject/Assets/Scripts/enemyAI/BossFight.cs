using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BossFight : MonoBehaviour
{
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

    [SerializeField] int fightStage_2_threshhold;
    [SerializeField] int fightStage_3_threshhold;
    [SerializeField] int fightStage_2_guards;
    [SerializeField] int fightStage_3_guards;
    [SerializeField] int fightStage_3_titans;
    


    int fightStage;

    bool bossFightBegin = false;
    bool spawnedStage2Reinforcements;
    bool spawnedStage3Reinforcements;
    bool bossDead;
    bool mainframeDoorClosed;


    // Start is called before the first frame update
    void Start()
    {
        fightStage = 0;
        spawnedStage2Reinforcements = false;
        spawnedStage3Reinforcements= false;

        MainFrameDoor.transform.GetChild(1).gameObject.SetActive(false);
        MainFrameDoor.transform.GetChild(2).gameObject.SetActive(true);
        mainframeDoorClosed = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (boss.GetComponent<SharedEnemyAI>().GetHealth() < fightStage_2_threshhold && !spawnedStage2Reinforcements)
        {
            fightStage = 2;
            SpawnReinforcements(fightStage);
            spawnedStage2Reinforcements = true;
        }

        else if (boss.GetComponent<SharedEnemyAI>().GetHealth() < fightStage_3_threshhold && !spawnedStage3Reinforcements)
        {
            fightStage = 3;
            SpawnReinforcements(fightStage);
            spawnedStage3Reinforcements = true;
        }
        else if (boss.GetComponent<bossAI>().GetIsDead())
        {
            bossDead = true;

            MainFrameDoor.transform.GetChild(1).gameObject.SetActive(false);
            MainFrameDoor.transform.GetChild(2).gameObject.SetActive(true);

            if (GameManager.instance.GetCommandCodesEntered() == 4)
            {
                LeverCover.SetActive(false);
                LeverCoverOpen.SetActive(true);
            }

            if (SelfDestructLever.GetComponent<togglingItem>().getState())
            {
                GameManager.instance.ActivateSelfDestruct();
                SelfDestructTimer.SetActive(true);
                timeLeft.text = GameManager.instance.GetTimeLeft();
            }
        }

        

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && bossFightBegin == false)
        {
            bossFightBegin = true;
            BossApproach();
            fightStage = 1;


            MainFrameDoor.transform.GetChild(1).gameObject.SetActive(true);
            mainframeDoorClosed = true;
        }
    }

    private void BossApproach()
    {
        boss.GetComponent<SharedEnemyAI>().SetDefaultPost(bossDefaultPost2);
        boss.GetComponent<NavMeshAgent>().SetDestination(bossDefaultPost2.transform.position);

    }


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
