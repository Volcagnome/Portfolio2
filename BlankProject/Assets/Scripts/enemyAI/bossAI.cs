using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor.DeviceSimulation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static enemyAI;



public class bossAI : SharedEnemyAI, IDamage
{

    //Body Objects
    [SerializeField] GameObject body;
    [SerializeField] GameObject lowerBody;
    [SerializeField] GameObject bigCannon_L;
    [SerializeField] GameObject bigCannon_R;
    [SerializeField] GameObject bigCannon_L2;
    [SerializeField] GameObject bigCannon_R2;


    //Basic Stats
    [SerializeField] float speed;


    //Combat
    [SerializeField] Transform bigCannonShootPos_L;
    [SerializeField] Transform bigCannonShootPos_R;
    [SerializeField] GameObject mainCannonAmmo;
    [SerializeField] int trampleDamage;
    [SerializeField] int minTimeBetweenTrampleAttempts;
    Vector3 trampleDestination;
    float distanceToPlayer;
    bool isCharging;
    bool readyToCharge;

    //Player detection
    [SerializeField] float maxTiltAngle;
    private Quaternion bodyTiltToPlayer;

    // Start is called before the first frame update
    void Start()
    {
        isCharging = false;
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


    private void Attack()
    {
        distanceToPlayer = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);

        if (playerInView && distanceToPlayer >= 10f)
        {
            agent.stoppingDistance = combatStoppingDistance;
            AimCannons(bigCannon_L);
            AimCannons(bigCannon_R);
            AimCannons(bigCannon_L2);
            AimCannons(bigCannon_R2);

            if (!isShooting)
                StartCoroutine(FireMainCannons1());
        }
        //else if (distanceToPlayer < 10f && !isCharging)
        //    Charge();
    }

    protected void FoundPlayer()
    {
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

        if (Vector3.Distance(transform.position, trampleDestination) <= 1f)
        {
            RotateToPlayer();

            agent.stoppingDistance = combatStoppingDistance;
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

    private void Death()
    {
        Destroy(gameObject);
    }

    public int GetTrampleDamage() { return trampleDamage; }

}


