using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor.DeviceSimulation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static enemyAI;



public class bossAI : MonoBehaviour,IDamage
{
    //Basic Components
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform headPos;

    //Body Objects
    [SerializeField] GameObject body;
    [SerializeField] GameObject lowerBody;
    [SerializeField] GameObject bigCannon_L;
    [SerializeField] GameObject bigCannon_R;
    [SerializeField] GameObject bigCannon_L2;
    [SerializeField] GameObject bigCannon_R2;


    //Basic Stats
    [SerializeField] int HP;
    [SerializeField] float shootRate;
    [SerializeField] float rotationSpeed;
    [SerializeField] float speed;
    Color colorOrig;


    //Combat
    [SerializeField] Transform bigCannonShootPos_L;
    [SerializeField] Transform bigCannonShootPos_R;
    [SerializeField] GameObject mainCannonAmmo;
    [SerializeField] int trampleDamage;
    [SerializeField] int minTimeBetweenTrampleAttempts;
    [SerializeField] GameObject defaultPost;
    Vector3 trampleDestination;
    float distanceToPlayer;
    bool isCharging;
    bool readyToCharge;

    //Player detection
    [SerializeField] public float FOV_Angle;
    [SerializeField] LayerMask targetMask;
    [SerializeField] float maxTiltAngle;
    [SerializeField] float shootingStoppingDistance;
    [SerializeField] float idleStoppingDistance;
    private Vector3 lastKnownPlayerLocation;
    private Vector3 playerDirection;
    public bool playerInView;
    private bool playerInRange;
    private Quaternion bodyTiltToPlayer;
    



    //CurrentStatus
    private bool isAlerted;
    private bool isShooting;
    private bool onDuty;

  
    // Start is called before the first frame update
    void Start()
    {
        isCharging = false;
        isAlerted = false;
        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;
        agent.stoppingDistance = idleStoppingDistance;
        readyToCharge = true;
    }

    // Update is called once per frame
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
            {
                agent.stoppingDistance = idleStoppingDistance;
                StartCoroutine(PursuePlayer());
            }
            else if (playerInRange)
            {
                playerDirection = GameManager.instance.player.transform.position - transform.position;

          
                    RotateToPlayer();
                    LookAtPlayer();
               

                if (!playerInView)
                    agent.SetDestination(GameManager.instance.player.transform.position);
 
            }

            if (isCharging)
                FinishCharging();

        }
        else if (!onDuty)
            ReturnToPost();
    }

    private void ReturnToPost()
    {
        onDuty = true;
            agent.SetDestination(defaultPost.transform.position);
    }

    IEnumerator PursuePlayer()
    {
       
            agent.SetDestination(lastKnownPlayerLocation);

            if (CheckIfArrived(lastKnownPlayerLocation, 1.3f) == true && !playerInRange || !agent.hasPath)
            {
                yield return new WaitForSeconds(1.5f);
                CalmEnemy();
            }
       
    }

    private bool CheckIfArrived(Vector3 location, float distance)
    {
       
        if (Vector3.Distance(transform.position, location) <= distance)
        {
            return true;
        }
        else
            return false;
    }

    private void Attack()
    {
        distanceToPlayer = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);

        if (playerInView && distanceToPlayer >= 10f)
        {
            agent.stoppingDistance = shootingStoppingDistance;
            AimCannons(bigCannon_L);
            AimCannons(bigCannon_R);
            AimCannons(bigCannon_L2);
            AimCannons(bigCannon_R2);

            if(!isShooting)
                StartCoroutine(FireMainCannons1());
        }
        //else if (distanceToPlayer < 10f && !isCharging)
        //    Charge();
    }

    private void RotateToPlayer()
    {
        //Vector3 playerDirection = (GameManager.instance.player.transform.position - transform.position);

        Quaternion rotationToDirection = Quaternion.LookRotation(playerDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotationToDirection, Time.deltaTime * rotationSpeed);

    }

    private void FoundPlayer()
    {
            agent.SetDestination(GameManager.instance.player.transform.position);

            Attack();
    }

    public void LookAtPlayer()
    {

        if (Vector3.Distance(transform.position, GameManager.instance.player.transform.position) >= 5f)
        {
            Vector3 playerLookDirection = GameManager.instance.player.transform.position - body.transform.position;

            bodyTiltToPlayer = Quaternion.LookRotation(playerLookDirection, body.transform.up);

            Vector3 eulerRotation = bodyTiltToPlayer.eulerAngles;
            float pitch = eulerRotation.x;
            float clampedPitch = Mathf.Clamp(pitch, -maxTiltAngle, maxTiltAngle);
            Quaternion clampedRotation = Quaternion.Euler(clampedPitch, eulerRotation.y, eulerRotation.z);

            body.transform.rotation = Quaternion.Lerp(body.transform.rotation, clampedRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void AimCannons(GameObject cannon)
    {
        Vector3 lookDirectionCannon = GameManager.instance.player.transform.position - cannon.transform.position;
        Quaternion bigCannonRotation = Quaternion.LookRotation(lookDirectionCannon, cannon.transform.up);

        Vector3 eulerRotation = bigCannonRotation.eulerAngles;
        float pitch = eulerRotation.x;
        float clampedPitch = Mathf.Clamp(pitch, -maxTiltAngle, maxTiltAngle);
        Quaternion clampedRotation = Quaternion.Euler(clampedPitch, eulerRotation.y, eulerRotation.z);

        cannon.transform.rotation = Quaternion.Lerp(cannon.transform.rotation, bigCannonRotation, Time.deltaTime * rotationSpeed);

    }

    IEnumerator FireMainCannons1()
    {
        Debug.Log("shoot");

        isShooting = true;
        Instantiate(mainCannonAmmo, bigCannonShootPos_L.position, bigCannon_L.transform.rotation);
        Instantiate(mainCannonAmmo, bigCannonShootPos_R.position, bigCannon_R.transform.rotation);

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    private void Charge()
    {
        isCharging = true;
        readyToCharge = false;

        float stoppingDistOrig = agent.stoppingDistance;

        agent.speed = 20;
        agent.stoppingDistance = 0.2f;

        trampleDestination = GameManager.instance.player.transform.position - transform.forward;

        agent.SetDestination(trampleDestination * 50);
        
    }

    private void FinishCharging()
    {
        Debug.Log(Vector3.Distance(transform.position, trampleDestination));

        if(Vector3.Distance(transform.position,trampleDestination) <= 1f)
        {
            RotateToPlayer();

            agent.stoppingDistance = shootingStoppingDistance;
            agent.speed = speed;
            isCharging = false;
            Debug.Log("finished charging");

            StartCoroutine(ChargeTimer());
        }
    }

    IEnumerator ChargeTimer()
    {
        yield return new WaitForSeconds(minTimeBetweenTrampleAttempts);
        readyToCharge = true;
    }

    public void takeDamage(int amount)
    {
        lastKnownPlayerLocation = GameManager.instance.player.transform.position;

        HP -= amount;

        AlertEnemy();
        StartCoroutine(flashYellow());

        if (HP <= 0)
        {
            Death();
        }
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

    private void Death()
    {
        Destroy(gameObject);
    }

    public int GetTrampleDamage()  { return trampleDamage;}

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
        float angleToPlayer = Vector3.Angle(playerDirection, transform.forward);

        if (playerInRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(headPos.position, playerDirection, out hit))
            {
                if (hit.collider.CompareTag("Player") && angleToPlayer <= FOV_Angle)
                    playerInView = true;
                else
                    playerInView = false;
            }
        }
        else
            playerInView = false;
    }


    public void AlertEnemy()
    {
        lastKnownPlayerLocation = GameManager.instance.player.transform.position;
        isAlerted = true;
        onDuty = false;

        RotateToPlayer();
        LookAtPlayer();
    }

    //Toggles isAlerted to false.
    public void CalmEnemy()
    {
        isAlerted = false;
    }

    }
