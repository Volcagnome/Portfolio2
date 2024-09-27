using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


//Handles all events within the Boss Arena

public class BossFight : MonoBehaviour
{

    public static BossFight instance;

    //Scene objects
    [SerializeField] GameObject boss;
    [SerializeField] List<GameObject> bossReinforcements;
    [SerializeField] GameObject bossDefaultPost1;
    [SerializeField] GameObject bossDefaultPost2;
    [SerializeField] GameObject bossFightGuard;
    [SerializeField] GameObject bossFightTitan;
    [SerializeField] GameObject MainFrameDoorControl;
    [SerializeField] GameObject LeverCover;
    [SerializeField] GameObject LeverCoverOpen;
    [SerializeField] GameObject SelfDestructLever;
    [SerializeField] GameObject ComputerCore1;
    [SerializeField] GameObject ComputerCore2;
    [SerializeField] GameObject ComputerCore3;
    [SerializeField] Material ComputerCoreNonEmissive;
    [SerializeField] AudioClip foundPlayer;
    [SerializeField] GameObject loadingZone;
    [SerializeField] GameObject computerTerminalAudioSource;
    [SerializeField] AudioClip alert;
    [SerializeField] AudioClip leverCoverOpenSound;
    [SerializeField] AudioClip selfDestructAlarm;
    [SerializeField] GameObject selfDestructScreen;
    [SerializeField] GameObject selfDestructSprite;
    [SerializeField] GameObject computerTerminalIdle1;
    [SerializeField] GameObject computerTerminalIdle2;
    [SerializeField] GameObject invasionScreen;
    [SerializeField] GameObject invasionSprite;
    [SerializeField] Material computerCoreMaterial;


    //Boss health threshholds that separate different stages of fight and determines when reinforcements will be called and how many
    [SerializeField] int fightStage_2_threshhold;
    [SerializeField] int fightStage_3_threshhold;
    [SerializeField] int fightStage_2_guards;
    [SerializeField] int fightStage_3_guards;
    [SerializeField] int fightStage_3_titans;
    
    int fightStage;
    bool bossIsDead;
    bool playerRespawned;

    //Tracks whether or not reinforcements have been called for respective fight stage
    bool spawnedStage2Reinforcements;
    bool spawnedStage3Reinforcements;

    // Start is called before the first frame update
    void Awake()
    {

        instance = this;
        bossIsDead = StaticData.bossIsDead_Static;

        computerCoreMaterial.mainTextureOffset = new Vector2(0f, 0f);

        if (!StaticData.bossIsDead_Static)
        {
            fightStage = 0;
            spawnedStage2Reinforcements = false;
            spawnedStage3Reinforcements = false;
        }

        
        if (StaticData.firstTimeInScene[SceneManager.GetActiveScene().buildIndex] || StaticData.mainFrameDoorOpen)
        {
                MainFrameDoorControl.GetComponent<togglingItem>().interact();
        }

        if(StaticData.selfDestructActivated_Static)
        {
            SelfDestructLever.GetComponent<togglingItem>().interact();
            computerTerminalIdle1.SetActive(false);
            computerTerminalIdle2.SetActive(false);
            invasionScreen.SetActive(true);
            invasionSprite.SetActive(true);
        }

        StartCoroutine(ScrollComputercoreTexture());

    }

    // Update is called once per frame
    //Calls configured reinforcements when boss's health reaches configured threshholds. When boss is dead,
    //enables final command code pickup, and opens the door to the mainframe. Once the player inserts the command codes,
    //lifts the cover on the self-destruct lever. When the player pulls it, displays the self destruct timer and starts 
    //the countdown.
    void Update()
    {

        if (!playerRespawned && GameManager.instance.GetIsRespawning())
        {
            playerRespawned = true;
        }

        if (StaticData.bossIsDead_Static)
        {
            loadingZone.SetActive(true);

            if (!StaticData.mainFrameDoorOpen)
            { 
                MainFrameDoorControl.GetComponent<togglingItem>().interact();
                StaticData.mainFrameDoorOpen = true;
            }

            if (StaticData.commandCodesEntered_Static == 6)
            {
                if (!LeverCoverOpen.activeInHierarchy == true)
                {
                    LeverCover.SetActive(false);
                    LeverCoverOpen.SetActive(true);
                    computerTerminalAudioSource.GetComponent<AudioSource>().PlayOneShot(leverCoverOpenSound);
                }
                
            }

            if (SelfDestructLever.GetComponent<togglingItem>().getState())
            {
                AudioSource source = computerTerminalAudioSource.GetComponent <AudioSource>();

                StaticData.gameObjective_Static = "\r\n\r\nEscape and warn the Planetary Defense Force!!";
                GameManager.instance.UpdateObjectiveUI(StaticData.gameObjective_Static);


                if (!StaticData.selfDestructActivated_Static)
                    GameManager.instance.ActivateSelfDestruct();
                StaticData.selfDestructActivated_Static = true;
                //source.clip = selfDestructAlarm;
                //source.loop = true;
                //if(!source.isPlaying)
                //    source.Play();

                selfDestructScreen.SetActive(true);
                StartCoroutine(FlashScreen());
            }

        }

        else if (boss != null && !StaticData.bossIsDead_Static)
        {
            if (boss.GetComponent<SharedEnemyAI>().GetHP() < fightStage_2_threshhold && !spawnedStage2Reinforcements)
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
       
    }

    //When player exits arena entry area, the doors to the mainframe will close, the boss will emerge and the fight will begin.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
         
            if (!StaticData.bossIsDead_Static && fightStage == 0)
            {
                AudioManager.instance.ChangeTrack(AudioManager.musicTrack.bossFightMusic);
                boss.GetComponent<AudioSource>().PlayOneShot(foundPlayer);
                EnemyManager.instance.SetIsBossFight(true);
                BossApproach();
                fightStage = 1;
                MainFrameDoorControl.GetComponent<togglingItem>().interact();
                StaticData.mainFrameDoorOpen = false;
                loadingZone.SetActive(false);
            }
            else if (fightStage > 0)
            {
                playerRespawned = false;

                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

                if (enemies.Length > 0)
                {
                    foreach (GameObject enemy in enemies)
                    {
                        StopCoroutine(enemy.GetComponent<SharedEnemyAI>().SearchArea(bossDefaultPost1.transform.position,5));
                        enemy.GetComponent<SharedEnemyAI>().SetIsSearching(false);
                        enemy.GetComponent<NavMeshAgent>().SetDestination(GameManager.instance.player.transform.position);
                    }
                        
                }
            }
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
        GameObject spawner = null;

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
            spawner = bossReinforcements[Random.Range(0, bossReinforcements.Count)];

            randomDist = UnityEngine.Random.insideUnitSphere * 3f;
            randomDist += spawner.transform.position;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDist, out hit, 3f, 1);

            GameObject guard = Instantiate(bossFightGuard, spawner.transform.position, spawner.transform.rotation);

            guard.GetComponent<SharedEnemyAI>().SetDefaultPost(spawner);
            guard.GetComponent<SharedEnemyAI>().AlertEnemy();
            guard.GetComponent<SharedEnemyAI>().SetCurrentDestination(spawner);
            guard.GetComponent<SharedEnemyAI>().SetLastKnownPlayerLocation(GameManager.instance.player.transform.position);
            guard.GetComponent<NavMeshAgent>().SetDestination(guard.GetComponent<SharedEnemyAI>().GetLastKnownPlayerLocation());
        }

        for (int reinforcement = 0; reinforcement < fightStageTitans; reinforcement++)
        {
            spawner = bossReinforcements[Random.Range(0, bossReinforcements.Count)];

            randomDist = UnityEngine.Random.insideUnitSphere * 3f;
            randomDist += spawner.transform.position;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDist, out hit, 3f, 1);

            GameObject titan = Instantiate(bossFightTitan, spawner.transform.position, spawner.transform.rotation);
            titan.GetComponent<SharedEnemyAI>().SetDefaultPost(spawner);
            titan.GetComponent<SharedEnemyAI>().AlertEnemy();
            titan.GetComponent<SharedEnemyAI>().SetCurrentDestination(spawner);
            titan.GetComponent<SharedEnemyAI>().SetLastKnownPlayerLocation(GameManager.instance.player.transform.position);
            titan.GetComponent<NavMeshAgent>().SetDestination(titan.GetComponent<SharedEnemyAI>().GetLastKnownPlayerLocation());
        }
    }

    IEnumerator FlashScreen()
    {
        while (true)
        {
            selfDestructSprite.SetActive(true);

            yield return new WaitForSeconds(0.5f);

            selfDestructSprite.SetActive(false);
        }
    }

    IEnumerator ScrollComputercoreTexture()
    {
        while(true)
        {
            float yValue = computerCoreMaterial.mainTextureOffset.y;

            computerCoreMaterial.mainTextureOffset = new Vector2(0f,yValue + 0.025f );

            yield return new WaitForSeconds(0.01f);
        }
    }


    public bool GetPlayerRespawned()
    {
        return playerRespawned;
    }

    public void SetBoss(GameObject bossRobot) { boss = bossRobot; }

    public void ChangeToInvasionScreen()
    {
        computerTerminalAudioSource.GetComponent<AudioSource>().PlayOneShot(alert);
        computerTerminalIdle1.SetActive(false);
        computerTerminalIdle2.SetActive(false);
        invasionScreen.SetActive(true);
        invasionSprite.SetActive(true);
    }

    public int GetBossFightStage()
    {
        return fightStage;
    }

    public void ResetBossHealth()
    {
        if (fightStage == 1)
            boss.GetComponent<SharedEnemyAI>().SetHP(boss.GetComponent<SharedEnemyAI>().GetMaxHP());
        else if (fightStage == 2)
            boss.GetComponent<SharedEnemyAI>().SetHP(fightStage_2_threshhold);
        else if (fightStage == 3)
            boss.GetComponent<SharedEnemyAI>().SetHP(fightStage_3_threshhold);
    }

}
