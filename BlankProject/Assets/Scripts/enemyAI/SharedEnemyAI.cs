using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
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
    [SerializeField] GameObject weakspot;


    //PlayerDetection
    [SerializeField] protected float FOV_Angle;
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
    protected bool hasPost;

    //Stats

    [SerializeField] public enum enemyType { none, Guard, Patrol, Titan, Turret, Boss, Arachnoid };
    [SerializeField] protected enemyType enemy_Type;
    [SerializeField] protected float HP;
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
    [SerializeField] protected GameObject weapon_R;

    //[SerializeField] Transform headTopPos;
    [SerializeField] protected GameObject ammoType;
   

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
    Coroutine PursuePlayerCoroutine;



    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.player.GetComponent<Camera>().transform.position = GameManager.instance.player.GetComponent<Camera>().transform.position - new Vector3(0f, 1f, 0f);

        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;
        isAlerted = false;
    }

    // Update is called once per frame
    void Update()
    {
        CallMovementAnimation();

        if (!isDead)
        {

            if (LevelManager.instance.GetIsBossFight())
            {
                isAlerted = true;
                agent.speed = combatSpeed;
                agent.stoppingDistance = combatStoppingDistance;
                agent.SetDestination(GameManager.instance.player.transform.position);
            }

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
                                                                             
                if (!playerInView && !playerInRange && !isRespondingToAlert && !LevelManager.instance.GetIsBossFight())
                {
                    PursuePlayerCoroutine = StartCoroutine(PursuePlayer());
        
                }

                else if (playerInRange)
                {
                    RotateToPlayer();

                }
                else if (playerInRange && !playerInView)
                    agent.SetDestination(GameManager.instance.player.transform.position);
            }
                                
            else if (!onDuty && defaultPost !=null && !LevelManager.instance.GetIsBossFight())
            {
                ReturnToPost();

            }


            if (isPlayerTarget())
                UpdateEnemyUI();

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

        if(defaultPost != null)
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

        weapon_R.transform.LookAt(GameManager.instance.player.transform.position + new Vector3(0f, 1f, 0f));


        if (!isShooting && !isDead)
            StartCoroutine(shoot());

        if (LevelManager.instance.GetIntruderAlert())
            LevelManager.instance.FoundTheIntruder(lastKnownPlayerLocation);
    }

    protected virtual IEnumerator shoot()
    {

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
    public void takeDamage(float amount)
    {
        HP -= amount;
        isTakingDamage = true;

        AlertEnemy();
        AlertAllies();
        StartCoroutine(flashYellow());

        if (HP <= 0 && !isDead)
        {
            isDead = true;
            
            Death();
        }

    }

    public void criticalHit(float amount)
    {
        takeDamage(amount);
        StartCoroutine(flashRed());
    }


    protected void DeathShared()
    {
        agent.isStopped = true;

        Debug.Log("he dead");
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



public GameObject GetEnemyHealthBar() { return enemyHPBar; }

public GameObject GetDefaultPost() { return defaultPost; }

    public enemyType GetEnemyType() { return enemy_Type; }

    public void SetDefaultPost(GameObject post) { defaultPost = post; }

    public bool CheckIfOnDuty() { return onDuty; }

    public void SetIsOnDuty(bool status) { onDuty = status; }

    public Vector3 GetLastKnownPlayerLocation() { return lastKnownPlayerLocation; }
    public void SetCurrentDestination(GameObject destination) { currentDestination = destination; }
    public GameObject GetCurrentDestination() { return currentDestination; }

    public void SetLastKnownPlayerLocation(Vector3 location) { lastKnownPlayerLocation = location; }

    public float GetHealth() { return HP; }
    public float GetHP() { return HP; }
    public void SetHP(float value) { HP = value; }
    public float GetShootRate() { return shootRate; }
    public void SetShootRate(float value) { shootRate = value; }
}
