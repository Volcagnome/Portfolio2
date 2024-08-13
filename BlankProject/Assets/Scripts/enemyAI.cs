using System.Collections;
using System.Collections.Generic;
using System.Security;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
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
    

    //Player detection
    [SerializeField] public float FOV_Angle;
    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask obstructionMask;
    private Vector3 lastKnownPlayerLocation;

    //CurrentStatus
    public bool isAlerted;
    private bool isShooting;
    public bool playerInView;
    private bool playerInRange;

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

        GameManager.instance.AddToEnemyList(gameObject);

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
        }

        if (isAlerted)
        {
            if (!playerInView && !playerInRange)
                GoToLastKnownPlayerLocation();
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

    //When player enters detection range toggles playerInRange variable
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }

        StartCoroutine(FOVRoutine());
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

    //Enemy model flashes red when hit
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    //While player is in range, calls function to check if in line of sight
    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.3f);
       
        while (playerInRange)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

     
    private void FieldOfViewCheck()
    {
        //Calculates direction from enemy to player.
        Vector3 directionToPlayer = (GameManager.instance.player.transform.position - transform.position);

        //If the player is within range, is within the configures FOV angle, and the raycast from the enemy
        //to the player is not obstructed by any object on the obstructionLayer, the player is in view, otherwise they are not in view.
        if (playerInRange)
        {
            if (Vector3.Angle(transform.forward, directionToPlayer) < FOV_Angle / 2)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);

                if (!Physics.Raycast(transform.position, directionToPlayer, gameObject.GetComponent<SphereCollider>().radius, obstructionMask))
                    playerInView = true;
                else
                    playerInView = false;
            }
            else
                playerInView = false;
        }
        else if (playerInView)
            playerInView = false;
    }

    //Removes enemy from the enemyList and removes them from the scene.
    private void Death()
    {
        GameManager.instance.RemoveFromEnemyList(gameObject);
        Destroy(gameObject);
    }

    //Enemy will travel to player's last known location at the time they were alerted
    private void GoToLastKnownPlayerLocation()
    {
        
        agent.SetDestination(lastKnownPlayerLocation);
    }

    //Enemy will move towards the player's location and start shooting
    private void FoundPlayer()
    {
        
        agent.SetDestination(GameManager.instance.player.transform.position);

        if (!isShooting)
            StartCoroutine(shoot());
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

    //Notes player's current location at the time of alert and sets isAlerted to true. 
    public void AlertEnemy()
    {
        lastKnownPlayerLocation = GameManager.instance.player.transform.position;
        isAlerted = true;
    }

    //Toggels isAlerted to false.
    public void CalmEnemy()
    {
        isAlerted = false;
    }


}

////Changes emission texture to red on Assault Droid
//if (gameObject.name == "Assault Droid")
//    gameObject.GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_EmissionMap", emissionAlerted);

//if (gameObject.name == "Assault Droid")
//    gameObject.GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_EmissionMap", emissionOrig);