using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using static enemyAI;
using static UnityEngine.GraphicsBuffer;
//using UnityEditor.Experimental.GraphView;
//using UnityEditor.Search;
using UnityEngine.UI;

public class SharedEnemyAI : MonoBehaviour
{
    
    //Components
    [SerializeField] protected Transform headPos;
    [SerializeField] protected Renderer model;
    [SerializeField] protected GameObject defaultPost;
    [SerializeField] protected GameObject currentDestination;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator anim;
    [SerializeField] protected ParticleSystem DeathVFX;
    [SerializeField] protected Transform DeathFXPos;


    //PlayerDetection
    [SerializeField] protected float FOV_Angle;
    [SerializeField] protected LayerMask targetMask;
    [SerializeField] protected float rotationSpeed;
    protected Vector3 playerDirection;
    protected Vector3 lastKnownPlayerLocation;
    protected bool playerInRange;
    protected bool playerInView;


    //Current State
    protected bool isAlerted;
    protected bool onDuty;
    protected bool isShooting;
    protected bool isDead;
    protected Color colorOrig;

    //Stats

    [SerializeField] public enum behaviorType { none, guard, patrol, guardReinforcement };
    [SerializeField] public enum enemyType { none, Guard, Patrol, Titan, Turret, Boss };
    [SerializeField] protected behaviorType enemyBehavior;
    [SerializeField] protected enemyType enemy_Type;
    [SerializeField] protected float HP;
    [SerializeField] protected float HPRegenRate;
    [SerializeField] protected float HPRegenWaitTime;
    [SerializeField] protected float shootRate;
    [SerializeField] protected float combatSpeed;
    [SerializeField] protected float combatStoppingDistance;
    [SerializeField] protected float idleStoppingDistance;
    [SerializeField] protected float idleSpeed;

    //Ally Detection
    [SerializeField] protected int allyRadius;
    [SerializeField] protected LayerMask allyLayer;
    protected GameObject[] alliesInRange;

    //Basic Components
    [SerializeField] protected Transform shootPos;
    [SerializeField] protected GameObject weapon;

    //[SerializeField] Transform headTopPos;
    [SerializeField] protected GameObject ammoType;
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

    public float minDistance = 15f;


    // Start is called before the first frame update
    void Start()
    {
        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;
        isAlerted = false;
    }

    // Update is called once per frame
    void Update()
    {
        CallMovementAnimation();

        if (!isDead)
        {
            if (playerInView)
            {
                AlertEnemy();
                AlertAllies();
                FoundPlayer();
                agent.stoppingDistance = combatStoppingDistance;
            }
            else
            {
                agent.stoppingDistance = idleStoppingDistance;
                anim.SetBool("Aiming", false);
            }

            if (isAlerted)
            {
                if (!playerInView && !playerInRange && !isRespondingToAlert)
                    StartCoroutine(PursuePlayer());

                else if (playerInRange)
                {
                    RotateToPlayer();

                    //Vector3 playerDirection = GameManager.instance.player.transform.position - transform.position;
                    //playerDirection.y = 0;
                    //transform.rotation = Quaternion.LookRotation(playerDirection);
                }
                else if (playerInRange && !playerInView)
                    agent.SetDestination(GameManager.instance.player.transform.position);
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
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDead)
        {
            playerInRange = true;
            StartCoroutine(FOVRoutine());
        }
        else
            return;
    }

    //When player exits detection range notes their last known location and toggles playerInRange and isAlerted bools
    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInView = false;

            if (isAlerted)
                lastKnownPlayerLocation = GameManager.instance.player.transform.position;
        }
        else
            return;
    }


    protected IEnumerator FOVRoutine()
    {
        while (playerInRange)
        {
            yield return new WaitForSeconds(0.05f);
            FieldOfViewCheck();
        }
    }

    protected bool FieldOfViewCheck()
    {
        bool result;

        playerDirection = GameManager.instance.player.transform.position - headPos.position;
        float angleToPlayer = Vector3.Angle(playerDirection, transform.forward);
        if (playerInRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(headPos.position, playerDirection, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Player") && angleToPlayer <= FOV_Angle)
                    result = true;

                else result = false;

            }
            else result = false;
        }
        else result = false;

        playerInView = result;

        return result;
    }


    protected IEnumerator PursuePlayer()
    {
        agent.SetDestination(lastKnownPlayerLocation);

        if (agent.remainingDistance <= 0.3f || !agent.hasPath)
        {
            yield return new WaitForSeconds(1.5f);
            CalmEnemy();
        }
    }

    protected virtual void RotateToPlayer()
    {
        playerDirection = GameManager.instance.player.transform.position - transform.position;

        Quaternion rotationToPlayer = Quaternion.LookRotation(playerDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotationToPlayer, Time.deltaTime * rotationSpeed);  
    }

    
    protected IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    protected IEnumerator flashYellow()
    {
        model.material.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }


    protected virtual void ReturnToPost()
    {
        onDuty = true;
        agent.SetDestination(defaultPost.transform.position);   
    }

    public virtual void AlertEnemy()
    {
        isAlerted = true;
        lastKnownPlayerLocation = GameManager.instance.player.transform.position;
        agent.speed = combatSpeed;
        onDuty = false;
        transform.GetChild(0).tag = "Alerted";
    }

    public virtual void CalmEnemy()
    {
        isAlerted = false;
        ReturnToPost();
        agent.speed = idleSpeed;
        onDuty = true;
        transform.GetChild(0).tag = "Idle";
    }

    protected virtual void AlertAllies()
    {
        Collider[] alliesInRange = Physics.OverlapSphere(gameObject.transform.position, allyRadius, allyLayer);

        if (alliesInRange.Length > 0)
        {
            foreach (Collider ally in alliesInRange)
            {
                    ally.gameObject.GetComponent<SharedEnemyAI>().lastKnownPlayerLocation = lastKnownPlayerLocation;
                    ally.gameObject.GetComponent<SharedEnemyAI>().AlertEnemy();
            }
        }
    }

    protected IEnumerator DespawnDeadRobot(GameObject robot)
    {
        yield return new WaitForSeconds(60f);

        Destroy(robot);
    }

    protected virtual void FoundPlayer()
    {
        lastKnownPlayerLocation = GameManager.instance.player.transform.position;

        agent.SetDestination(GameManager.instance.player.transform.position);
        agent.stoppingDistance = combatStoppingDistance;

        anim.SetBool("Aiming", true);

        weapon.transform.LookAt(GameManager.instance.player.transform.position + new Vector3(0f, 1f, 0f));


        if (!isShooting && !isDead)
            StartCoroutine(shoot());

        if (LevelManager.instance.GetIntruderAlert())
            LevelManager.instance.FoundTheIntruder(lastKnownPlayerLocation);
    }

    protected virtual IEnumerator shoot()
    {
        Debug.Log("Shooting");

        anim.SetTrigger("Shoot");

        isShooting = true;

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    private void CreateBullet()
    {
        Instantiate(ammoType, shootPos.position, transform.rotation);
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
            isDead = true;

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


    protected void DeathShared()
    {
        agent.isStopped = true;

        anim.SetBool("isDead", true);
        playerInRange = false;
        playerInView = false;
        isAlerted = false;

        enemyHPBar.SetActive(false);

        Instantiate(DeathVFX, DeathFXPos.position, Quaternion.identity);
    }

    protected virtual void Death()
    {
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


    protected void CallMovementAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);
    }

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

            if (playerInView)
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





public GameObject GetDefaultPost() { return defaultPost; }

    public enemyType GetEnemyType() { return enemy_Type; }

    public void SetDefaultPost(GameObject post) { defaultPost = post; }

    public bool CheckIfOnDuty() { return onDuty; }

    public void SetIsOnDuty(bool status) { onDuty = status; }

    public Vector3 GetLastKnownPlayerLocation() { return lastKnownPlayerLocation; }
    public void SetCurrentDestination(GameObject destination) { currentDestination = destination; }
    public GameObject GetCurrentDestination() { return currentDestination; }

    public void SetLastKnownPlayerLocation(Vector3 location) { lastKnownPlayerLocation = location; }

    public float GetHP() { return HP; }
    public void SetHP(float value) { HP = value; }
    public float GetShootRate() { return shootRate; }
    public void SetShootRate(float value) { shootRate = value; }
}
