using System.Collections;
using System.Collections.Generic;
using System.Security;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.UIElements;
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
    [SerializeField] GameObject bullet;
   
    //Basic stats
    [SerializeField] int HP;
    [SerializeField] float shootRate;

    //EnemyBehavior
    [SerializeField] public enum behaviorType { guard, patrol};
    [SerializeField] behaviorType enemyBehavior;
    [SerializeField] float combatStoppingDistance;
    [SerializeField] float passiveStoppingDistance;
    private GameObject defaultPost;

    
    //Player detection
    [SerializeField] public float FOV_Angle;
    [SerializeField] LayerMask targetMask;

    private Vector3 lastKnownPlayerLocation;
    private Vector3 playerDirection;

    //CurrentStatus
    public bool isAlerted;
    private bool isShooting;
    public bool playerInView;
    private bool playerInRange;
    private bool atPost;

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
        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;
        passiveStoppingDistance = agent.stoppingDistance;
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
            agent.stoppingDistance = passiveStoppingDistance;

        if (isAlerted)
        {
            atPost = false;

            if (!playerInView && !playerInRange)
            {
                StartCoroutine(PursuePlayer());
            }
            else if (playerInRange)
            {
                //Rotates the enemy towards the player's current position
                Vector3 playerDirection = GameManager.instance.player.transform.position - transform.position;
                playerDirection.y = 0;
                transform.rotation = Quaternion.LookRotation(playerDirection);

                if (!playerInView)
                    agent.SetDestination(GameManager.instance.player.transform.position);
            }
        }
        else if (!atPost)
        {
            ReturnToPost();
            atPost = true;
        }
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
        StartCoroutine(flashRed());


        if (HP <= 0)
        {
            Death();
        }
    }

    //Enemy model flashes red when hit
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
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
    }

    //Toggles isAlerted to false.
    public void CalmEnemy()
    {
        isAlerted = false;
        ReturnToPost();
    }

    private void Death()
    {
        if (enemyBehavior == behaviorType.guard)
        {
            EnemyManager.instance.RemoveFromGuardUnits(gameObject);
            defaultPost.GetComponent<GuardPost>().SetIsOccupied(false);
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
        lastKnownPlayerLocation = GameManager.instance.player.transform.position;

        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            CalmEnemy();
        }
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
        playerDirection = (GameManager.instance.player.transform.position - transform.position);

        if (playerInRange)
        {
            if (Vector3.Angle(transform.forward, playerDirection) < FOV_Angle / 2)
            {
                if (Physics.Raycast(transform.position, playerDirection, gameObject.GetComponent<SphereCollider>().radius, targetMask))
                    playerInView = true;
                else
                    playerInView = false;
            }
        }
        else
            playerInView = false;
    }

    

    private void ReturnToPost()
    {
        if (enemyBehavior == behaviorType.guard)
        {
            agent.SetDestination(defaultPost.transform.position);
        }
    }

    public void SetDefaultPost(GameObject guardPost)
    {
        defaultPost = guardPost;
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
}



////Changes emission texture to red on Assault Droid
//if (gameObject.name == "Assault Droid")
//    gameObject.GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_EmissionMap", emissionAlerted);

//if (gameObject.name == "Assault Droid")
//    gameObject.GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_EmissionMap", emissionOrig);