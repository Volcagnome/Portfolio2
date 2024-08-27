using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class enemyAI : MonoBehaviour, IDamage
{

    //Basic Components
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;
    //[SerializeField] Transform headTopPos;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject guardBullet;
    [SerializeField] Texture emissionAlerted;
    [SerializeField] Texture emissionIdle;
    [SerializeField] Material guard;
    [SerializeField] Material patrol;
    public GameObject enemyHPBar;
    public Image enemyHPBarFill;
    //[SerializeField] Vector3 HPBarPos;

    //Stats
    float HPOrig;
    [SerializeField] float HP;
    [SerializeField] float HPRegenRate;
    [SerializeField] float HPRegenWaitTime;
    [SerializeField] float shootRate;
    [SerializeField] float combatSpeed;
    [SerializeField] float combatStoppingDistance;
    [SerializeField] float idleStoppingDistance;
    [SerializeField] float idleSpeed;
    [SerializeField] public float rotationSpeed;

    //EnemyBehavior
    [SerializeField] public enum behaviorType { none, guard, patrol};
    [SerializeField] public enum enemyType { none, AssaultDroid, Elite, Turret, Boss };
    [SerializeField] behaviorType enemyBehavior;
    [SerializeField] enemyType enemy_Type;
    [SerializeField] GameObject defaultPost;
    [SerializeField] GameObject currentDestination;

    //Player detection
    [SerializeField] public float FOV_Angle;
    [SerializeField] LayerMask targetMask;
    private Vector3 lastKnownPlayerLocation;
    public bool playerInView;
    private bool playerInRange;

    //CurrentStatus
    private bool isTakingDamage;
    private bool isAlerted;
    private bool isShooting;
    private bool onDuty;

    //Ally Detection
    [SerializeField] int allyRadius;
    [SerializeField] LayerMask allyLayer;
    private GameObject[] alliesInRange;

    Color colorOrig;
    Coroutine regenCoroutine;

    // Start is called before the first frame update
    //Saves original color to variable for reference
    //Adds enemy to enemy list in GameManager
    void Start()
    {
        HPOrig = HP;

        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;

        CheckIfBehaviorAssigned();
    }

    private void CheckIfBehaviorAssigned()
    {

        if (enemyBehavior == behaviorType.guard)
        {
            EnemyManager.instance.AddRobotToGuardCount();
            defaultPost.GetComponent<GuardPost>().SetIsOccupied(true);
            gameObject.GetComponentInChildren<Renderer>().materials[0] = guard;
        }
        else if (enemyBehavior == behaviorType.patrol)
        {
           EnemyManager.instance.AddRobotToPatrolCount();
           gameObject.GetComponentInChildren<Renderer>().materials[0] = patrol;
        }
        else if(enemyBehavior == behaviorType.none && enemy_Type == enemyType.AssaultDroid)
        {
            EnemyManager.instance.AssignRole(gameObject);
        }

    }

    // Update is called once per frame
    //If player is in enemy field of view will become alerted and attack
    //If enemy is alerted player is out of range will move to the player's last known location (at the time it was alerted)
    //If enemy is alerted and player is within range will rotate to face them, if they can't see them will move to their position
    void Update()
    {
        if (playerInView)
        {
            AlertEnemy();
            FoundPlayer();
            agent.stoppingDistance = combatStoppingDistance;
        }
        else
            agent.stoppingDistance = idleStoppingDistance;

        if (isPlayerTarget())
        {
            UpdateEnemyUI();

            if (!isTakingDamage)
                RegenerateHealth();
        }
        else
            enemyHPBar.SetActive(false);

        if (isAlerted)
        {
            if (!playerInView && !playerInRange)
            {
                StartCoroutine(PursuePlayer());
            }
            else if (playerInRange)
            {
                //Rotates the enemy towards the player's current position
                //RotateTo(playerDirection);

                Vector3 playerDirection = GameManager.instance.player.transform.position - transform.position;
                playerDirection.y = 0;
                transform.rotation = Quaternion.LookRotation(playerDirection);

                if (!playerInView)
                    agent.SetDestination(GameManager.instance.player.transform.position);
            }
        }
        else if (!onDuty)
            ReturnToPost();
    }


    ////////////////////////////////////////
    ///           COMBAT                 ///
    ////////////////////////////////////////


    private void FoundPlayer()
    {
        agent.SetDestination(GameManager.instance.player.transform.position);

        if (!isShooting)
            StartCoroutine(shoot());
    }

    //Shoots a bullet in the direction the enemy is facing at the configured fire rate
    IEnumerator shoot()
    {
        isShooting = true;
        Instantiate(bullet, shootPos.position, transform.rotation);

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

    void RegenerateHealth()
    {
        //Debug.Log("Regenerating enemy health: " + HPRegenRate * Time.deltaTime);
        HP += HPRegenRate * Time.deltaTime;

        if (HP > HPOrig)
        {
            HP = HPOrig;
        }
    }

    IEnumerator EnableHealthRegen()
    {
        yield return new WaitForSeconds(HPRegenWaitTime);
        isTakingDamage = false;
        regenCoroutine = null;
    }

    bool isPlayerTarget()
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

    public void criticalHit(int amount)
    {
        takeDamage(amount);
        StartCoroutine(flashRed());
    }

    //Enemy model flashes red when hit
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    IEnumerator flashYellow()
    {
        model.material.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    //Checks if any other enemies within it's configured ally radius, if so alerts them.
    private void AlertAllies()
    {
        Collider[] alliesInRange = Physics.OverlapSphere(gameObject.transform.position, allyRadius, allyLayer);

        if (alliesInRange.Length > 0)
        {
            foreach (Collider ally in alliesInRange)
            {
                ally.gameObject.GetComponent<enemyAI>().AlertEnemy();
            }
        }
    }

    IEnumerator PursuePlayer()
    {
        agent.SetDestination(lastKnownPlayerLocation);
        if (CheckIfArrived(lastKnownPlayerLocation) == true && !playerInRange)
        {
            yield return new WaitForSeconds(1.5f);
            CalmEnemy();
        }
    }

    //Notes player's current location at the time of alert and sets isAlerted to true. 
    public void AlertEnemy()
    {
        lastKnownPlayerLocation = GameManager.instance.player.transform.position;
        isAlerted = true;
        agent.speed = combatSpeed;
        onDuty = false;
        //Changes emission texture to red on Assault Droid
        if (enemy_Type == enemyType.AssaultDroid)
            gameObject.GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_EmissionMap", emissionAlerted);
    }

    //Toggles isAlerted to false.
    public void CalmEnemy()
    {
        isAlerted = false;
        ReturnToPost();
        agent.speed = idleSpeed;
        onDuty = true;
        if (enemy_Type == enemyType.AssaultDroid)
            gameObject.GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_EmissionMap", emissionIdle);
    }

    private void Death()
    {
        if (enemyBehavior == behaviorType.guard)
        {
            EnemyManager.instance.RemoveFromGuardRobotsCount();
            defaultPost.GetComponent<GuardPost>().SetIsOccupied(false);
        }
        else if (enemyBehavior == behaviorType.patrol)
        {
            EnemyManager.instance.RemoveFromPatrolRobotsCount();
            defaultPost.GetComponent<PatrolWaypoint>().RemoveRobotFromRoute();
        }
        Destroy(gameObject);
    }


    //////////////////////////////////////////
    ///       PLAYER DETECTION            ///
    ////////////////////////////////////////

    //When player enters detection range toggles playerInRange variable
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            StartCoroutine(FOVRoutine());
        }
    }

    //When player exits detection range notes their last known location and toggles playerInRange and isAlerted bools
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lastKnownPlayerLocation = GameManager.instance.player.transform.position;
            playerInRange = false;
            CalmEnemy();
        }
        else
            return;
    }

    //While player is in range, calls function to check if in line of sight
    private IEnumerator FOVRoutine()
    {
        while (playerInRange)
        {
            yield return new WaitForSeconds(0.3f);
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        //Calculates direction from enemy to player.
        Vector3 playerDirection = (GameManager.instance.player.transform.position - headPos.position);

        if (playerInRange)
        {
            if (Vector3.Angle(transform.forward, playerDirection) < FOV_Angle / 2)
            {
                if (Physics.Raycast(headPos.position, playerDirection, gameObject.GetComponent<SphereCollider>().radius, targetMask))
                    playerInView = true;
                else
                    playerInView = false;
            }
        }
        else
            playerInView = false;
    }

    public void RotateTo(Vector3 direction)
    {
        Quaternion rotationToDirection = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotationToDirection, Time.deltaTime * rotationSpeed);
    }



    ////////////////////////////////////////
    ///          ENEMY BEHAVIOR         ///
    ///////////////////////////////////////


    private void ReturnToPost()
    {
        onDuty = true;

        if (enemy_Type == enemyType.Elite)
            agent.SetDestination(defaultPost.transform.position);
        else if (enemyBehavior == behaviorType.guard)
        {
            agent.SetDestination(defaultPost.transform.position);
        }

        else if (enemyBehavior == behaviorType.patrol)
        {
            OnPatrol();
        }
    }

    public void OnPatrol()
    {
        agent.SetDestination(currentDestination.transform.position);
    }

    private bool CheckIfArrived(Vector3 location)
    {
        if (Vector3.Distance(transform.position, location) <= 1.3)
        {
            return true;
        }
        else
            return false;
    }


    ////////////////////////////////////////
    ///          GETTERS/SETTERS         ///
    ///////////////////////////////////////


    public GameObject GetDefaultPost()
    {
        return defaultPost;
    }

    public void SetDefaultPost(GameObject post)
    {
        defaultPost = post;
    }


    public behaviorType GetBehaviorType()
    {
        return enemyBehavior;
    }

    public void SetBehavior(behaviorType behavior)
    {
        enemyBehavior = behavior;
    }

    public GameObject GetCurrentDestination()
    {
        return currentDestination;
    }

    public void SetCurrentDestination(GameObject destination)
    {
        currentDestination = destination;
    }


    public bool CheckIfOnDuty()
    {
        return onDuty;
    }


    public void SetEnemyStats()
    {
        if (enemyBehavior == behaviorType.guard)
        {
            gameObject.GetComponentInChildren<Renderer>().materials[0] = guard;
            HP = 15;
            combatSpeed = 3;
            if (enemy_Type == enemyType.AssaultDroid)
                gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material = guard;
            bullet = guardBullet;

        }
        else if (enemyBehavior == behaviorType.patrol)
        {
            if (enemy_Type == enemyType.AssaultDroid)
                gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material = patrol;
            HP = 5;
            combatSpeed = 4;
        }
        else if (enemy_Type == enemyType.Elite)
            HP = 50;
    }

    public enemyType GetEnemyType()
    {
        return enemy_Type;
    }

    public float GetHP() {  return HP;  }
    public void SetHP(float value) {  HP = value; }
    public float GetShootRate() { return shootRate; }
    public void SetShootRate(float value) { shootRate = value; }
}
