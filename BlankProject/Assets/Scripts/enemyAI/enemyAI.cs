using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class enemyAI : SharedEnemyAI, IDamage
{

    //Basic Components
    [SerializeField] Transform shootPos;

    //[SerializeField] Transform headTopPos;
    [SerializeField] GameObject ammoType;
    [SerializeField] Texture emissionAlerted;
    [SerializeField] Texture emissionIdle;

    //CurrentStatus
    protected bool isRespondingToAlert;

    //Health Regen
    protected bool isTakingDamage;
    protected Coroutine regenCoroutine;
    protected float HPOrig;
    public GameObject enemyHPBar;
    public Image enemyHPBarFill;
    [SerializeField] Vector3 HPBarPos;

    Coroutine FindIntruderCoroutine;

    // Start is called before the first frame update

    void Start()
    {
        if(enemy_Type == SharedEnemyAI.enemyType.Titan)
        {
            if (defaultPost == null)
            {
                if (Vector3.Distance(transform.position, LevelManager.instance.GetReinforcementSpawner().transform.position) < 0.5f)
                    defaultPost = LevelManager.instance.GetReinforcementSpawner();
                else
                    EnemyManager.instance.AssignTitanPost(gameObject);
            }
            else if (defaultPost.GetComponent<TitanPost>())
            {
                EnemyManager.instance.AddRobotToTitanCount();
                EnemyManager.instance.AddTitanToRoster(gameObject);
                defaultPost.GetComponent<TitanPost>().SetIsOccupied(true);
                defaultPost.GetComponent<TitanPost>().AssignTitan(gameObject);
            }
        }


            
    }

    // Update is called once per frame
   
    void Update()
    {
        if (playerInView)
        {
            AlertEnemy();
            AlertAllies();
            FoundPlayer();
            agent.stoppingDistance = combatStoppingDistance;
        }
        else
            agent.stoppingDistance = idleStoppingDistance;

        if (isAlerted)
        {
            if (!playerInView && !playerInRange && !isRespondingToAlert)
                StartCoroutine(PursuePlayer());

            else if (playerInRange && !playerInView)
                agent.SetDestination(GameManager.instance.player.transform.position);

            else if (playerInRange && agent.velocity.magnitude > 0.1f)
            {
                RotateToPlayer();

                //Vector3 playerDirection = GameManager.instance.player.transform.position - transform.position;
                //playerDirection.y = 0;
                //transform.rotation = Quaternion.LookRotation(playerDirection);
            }
        }
        else if (!onDuty)
            ReturnToPost();


        if (isPlayerTarget())
        {
            UpdateEnemyUI();

            if (!isTakingDamage)
                RegenerateHealth();
        }
        else
            enemyHPBar.SetActive(false);
    }


    ////////////////////////////////////////
    ///           COMBAT                 ///
    ////////////////////////////////////////


    protected void FoundPlayer()
    {
        agent.SetDestination(GameManager.instance.player.transform.position);
        agent.stoppingDistance = combatStoppingDistance;

        if (!isShooting)
            StartCoroutine(shoot());

        if (LevelManager.instance.GetIntruderAlert())
            LevelManager.instance.FoundTheIntruder(lastKnownPlayerLocation);
    }

    IEnumerator shoot()
    {
        isShooting = true;
        Instantiate(ammoType, shootPos.position, transform.rotation);

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    //When enemy is damaged will lose health, become alerted, alert allies within its configured ally radius,and flash red
    //If HP falls to or below zero enemy calls Death function
    public void takeDamage(int amount)
    {
        HP -= amount;
        isTakingDamage = true;

        AlertEnemy();
        AlertAllies();
        StartCoroutine(flashYellow());

        if (HP <= 0)
        {
            Death();
        }

        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
        regenCoroutine = StartCoroutine(EnableHealthRegen());
    }

    public void criticalHit(int amount)
    {
        takeDamage(amount);
        StartCoroutine(flashRed());
    }


    protected virtual void Death()
    {
        if (enemy_Type == SharedEnemyAI.enemyType.Titan)
        {
            if (defaultPost.GetComponent<TitanPost>())
            {
                EnemyManager.instance.RemoveDeadTitan(gameObject);
                defaultPost.GetComponent<TitanPost>().SetIsOccupied(false);
            }

            if (LevelManager.instance.responseTeam.Contains(gameObject))
                LevelManager.instance.responseTeam.Remove(gameObject);
        }

        Destroy(gameObject);
    }


    ////////////////////////////////////////
    ///          HEALTH REGEN            ///
    ///////////////////////////////////////


    protected void RegenerateHealth()
    {
        //Debug.Log("Regenerating enemy health: " + HPRegenRate * Time.deltaTime);
        HP += HPRegenRate * Time.deltaTime;

        if (HP > HPOrig)
        {
            HP = HPOrig;
        }
    }

    protected IEnumerator EnableHealthRegen()
    {
        yield return new WaitForSeconds(HPRegenWaitTime);
        isTakingDamage = false;
        regenCoroutine = null;
    }

    protected bool isPlayerTarget()
    {
        if (HP < HPOrig)
            return true;
        return false;
    }

    public void UpdateEnemyUI()
    {
        enemyHPBar.SetActive(true);
        enemyHPBarFill.fillAmount = HP / HPOrig;
        //EnemyManager.instance.enemyHPBar.transform.position = headTopPos.position + HPBarPos;
        enemyHPBar.transform.parent.rotation = Camera.main.transform.rotation;
    }


    ////////////////////////////////////////
    ///          ENEMY BEHAVIOR         ///
    ///////////////////////////////////////


    public void StartOrUpdateFindIntruder(Vector3 location)
    {
        if (FindIntruderCoroutine != null)
            StopCoroutine(FindIntruder(location));

        FindIntruderCoroutine = StartCoroutine(FindIntruder(location));
    }

    public IEnumerator FindIntruder(Vector3 intruderLocation)
    {
        isRespondingToAlert = true;
        AlertEnemy();
        lastKnownPlayerLocation = intruderLocation;

        agent.SetDestination(lastKnownPlayerLocation);

        while (true)
        { 

            yield return new WaitForSeconds(0.05f);

            if(playerInView)
            {
                isRespondingToAlert = false;
                break;
            }

           if (agent.remainingDistance <= 0.5f)
            {
                isRespondingToAlert = false;
                StartCoroutine(SearchArea());
                break;
            }
        }
    }

        public IEnumerator SearchArea()
    {
     
        int maxSearchAttempts = LevelManager.instance.GetSearchAttempts();
        float searchRadius = LevelManager.instance.GetSearchRadius();
        float searchTimer = LevelManager.instance.GetSearchTimer();
        bool playerFound = false;



        for (int attempts = 0; attempts < maxSearchAttempts; attempts++)
        {
            Vector3 randomDist = Random.insideUnitSphere * searchRadius;
            randomDist += LevelManager.instance.GetIntruderLocation();

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDist, out hit, searchRadius, 1);
            agent.SetDestination(hit.position);

            yield return new WaitForSeconds(searchTimer);

            if (playerInView)
            {
                playerFound = true;

                yield break;
            }
        }

        if (!playerFound)
        {
            Debug.Log("Must have been the wind.");
            CalmEnemy();
        }
    }

    ////////////////////////////////////////
    ///          GETTERS/SETTERS         ///
    ///////////////////////////////////////


    public void SetIsRespondingToAlert(bool status) { isRespondingToAlert = status; }

   

}
