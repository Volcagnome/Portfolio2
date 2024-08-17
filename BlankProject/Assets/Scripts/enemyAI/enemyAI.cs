using System.Collections;
using System.Collections.Generic;
using System.Security;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Animations;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEditor.FilePathAttribute;
using static UnityEditor.LightingExplorerTableColumn;
using static UnityEngine.GraphicsBuffer;

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
    int HPOrig;
    [SerializeField] int HP;
    [SerializeField] float shootRate;
    [SerializeField] float combatSpeed;
    [SerializeField] float combatStoppingDistance;
    [SerializeField] float idleStoppingDistance;
    [SerializeField] float idleSpeed;
    [SerializeField] public float rotationSpeed;

    //EnemyBehavior
    [SerializeField] public enum behaviorType { none, guard, patrol};
    [SerializeField] public enum enemyType { none, AssaultDroid, elite, Turret };
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
    private bool isAlerted;
    private bool isShooting;
    private bool onDuty;

    //Ally Detection
    [SerializeField] int allyRadius;
    [SerializeField] LayerMask allyLayer;
    private GameObject[] alliesInRange;

    Color colorOrig;

    // Start is called before the first frame update
    //Saves original color to variable for reference
    //Adds enemy to enemy list in GameManager
    void Start()
    {
        HPOrig = HP;

        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;
        if (enemyBehavior == behaviorType.none && tag != "Elite")
            EnemyManager.instance.AssignRole(gameObject);
        else if (enemyBehavior == behaviorType.guard)
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
        }

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
        else if(!onDuty)
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

            AlertEnemy();
            AlertAllies();
            StartCoroutine(flashYellow());

        if (HP <= 0)
        {
            Death();
        }
    }

    private bool isPlayerTarget()
    {
        if (HP < HPOrig)
            return true;
        return false;
    }

    public void UpdateEnemyUI()
    {
        enemyHPBar.SetActive(true);
        enemyHPBarFill.fillAmount = (float)HP / HPOrig;
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
        transform.rotation = Quaternion.Lerp(transform.rotation,rotationToDirection, Time.deltaTime * rotationSpeed);
    }

    private void ReturnToPost()
    {
        onDuty = true;

        if (enemy_Type == enemyType.elite)
            agent.SetDestination(defaultPost.transform.position);
        else if (enemyBehavior == behaviorType.guard)
            agent.SetDestination(defaultPost.transform.position);

        else if(enemyBehavior == behaviorType.patrol)
        {
            OnPatrol();
        }
        
    }

    public void SetDefaultPost(GameObject post)
    {
        defaultPost = post;
    }

    public GameObject GetDefaultPost()
    {
        return defaultPost;
    }

    public void SetBehavior(behaviorType behavior)
    {
        enemyBehavior = behavior;
    }
    private bool CheckIfArrived(Vector3 location)
    {
        if (Vector3.Distance(transform.position,location) <= 1.3)
        { 
            return true;
        }
        else
            return false;
    }

    public void SetCurrentDestination(GameObject destination)
    {
        currentDestination = destination;
    }

    public GameObject GetCurrentDestination()
    {
        return currentDestination;
    }

    public bool CheckIfOnDuty()
    {
        return onDuty;
    }

    public void OnPatrol()
    {
            agent.SetDestination(currentDestination.transform.position);
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
        else if (enemy_Type == enemyType.elite)
            HP = 50;
    }
}
 





