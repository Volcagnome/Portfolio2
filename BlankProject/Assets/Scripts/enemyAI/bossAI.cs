using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;



public class bossAI : SharedEnemyAI, IDamage
{

    //Body Objects
    [SerializeField] GameObject FlameThrower_L;
    [SerializeField] GameObject FlameThrower_R;
    [SerializeField] GameObject MainTurret_L;
    [SerializeField] GameObject MainTurret_R;
    [SerializeField] GameObject RocketTurret_L;
    [SerializeField] GameObject RocketTurret_R;
    [SerializeField] GameObject TurretCycler_L;
    [SerializeField] GameObject TurretCycler_R;
    [SerializeField] GameObject weapon_L;
    [SerializeField] GameObject ShieldIndicator;
   

    [SerializeField] GameObject FlamethrowerAmmo;
    [SerializeField] GameObject MainTurretAmmo;
    [SerializeField] GameObject RocketAmmo;
  

    [SerializeField] ParticleSystem deathExplosion;
    [SerializeField] Transform deathSparks2;
    [SerializeField] Transform deathSparks3;


    //Basic Stats
    [SerializeField] float speed;

    //Combat
    Quaternion[] turretRotations = {Quaternion.Euler(364.432f, 0.519f, 89.696f), Quaternion.Euler(433.059f, 178.225f, 267.958f), Quaternion.Euler(310.793f, 179.208f, 270.255f) };
    int currentRotationIndex;


    //Player detection
    [SerializeField] float maxTiltAngle;

    // Start is called before the first frame update
    void Start()
    {
        HPOrig = HP;
        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;
    }

    // Update is called once per frame
    void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);


        anim.SetFloat("Speed", agent.velocity.magnitude);

        if (playerInView)
        {
            lastKnownPlayerLocation = GameManager.instance.player.transform.position;
            AlertEnemy();
            FoundPlayer();
        }

        if (isAlerted)
        {
            if (distanceToPlayer > 25f)
            {
                agent.stoppingDistance = idleStoppingDistance;
                StartCoroutine(PursuePlayer());
            }
            else if (distanceToPlayer < 25f)
            {
                agent.stoppingDistance = combatStoppingDistance;

                playerDirection = GameManager.instance.player.transform.position - transform.position;

                RotateToPlayer();

                if (!playerInView)
                    agent.SetDestination(GameManager.instance.player.transform.position);

            }
        }

        if (isPlayerTarget())
            UpdateEnemyUI();

        if (isAlerted || playerInOuterRange)
            UpdateDetectionUI();
        else
        {
            playerDetectionCircle.SetActive(false);
        }

    }
      

    protected override void FoundPlayer()
    {
        if (distanceToPlayer < 20f)
        {
            SelectWeapon(FlameThrower_L, FlameThrower_R, 0);
            CycleTurrets();
            shootRate = 0.05f;
        }
        else if (distanceToPlayer > 20f && distanceToPlayer < 30f)
        {
            SelectWeapon(MainTurret_L, MainTurret_R, 1);
            CycleTurrets();
            shootRate = 0.75f;
        }

        else if (distanceToPlayer > 40f)
        {
            SelectWeapon(RocketTurret_L, RocketTurret_R, 2);
            CycleTurrets();
            shootRate = 5f;
        }

      
        if (Quaternion.Angle(TurretCycler_L.transform.localRotation,turretRotations[currentRotationIndex]) <5f)
        {
            weapon_L.transform.LookAt(GameManager.instance.player.transform.position, transform.up);
            weapon_R.transform.LookAt(GameManager.instance.player.transform.position, transform.up);

            Attack();
        }
    }


    private void SelectWeapon(GameObject weaponL, GameObject weaponR, int index)
    {
  
        weapon_L = weaponL;
        weapon_R = weaponR;
        currentRotationIndex = index;
    }

    private void CycleTurrets()
    {
            TurretCycler_L.transform.localRotation = Quaternion.Lerp(TurretCycler_L.transform.localRotation, turretRotations[currentRotationIndex], Time.deltaTime * 2f);
            TurretCycler_R.transform.localRotation = Quaternion.Lerp(TurretCycler_R.transform.localRotation, turretRotations[currentRotationIndex], Time.deltaTime * 2f);
    }

   

    private void Attack()
    {

        if (weapon_L == FlameThrower_L && !isShooting)
            StartCoroutine(FireFlamethrowers());
        else if (weapon_L == MainTurret_L && !isShooting)
            StartCoroutine(FireMainTurrets());
        else if(weapon_L == RocketTurret_L && !isShooting)
            StartCoroutine(FireRocketTurrets());

    }

    IEnumerator FireFlamethrowers()
    {
        isShooting = true;

        Instantiate(FlamethrowerAmmo, FlameThrower_L.transform.GetChild(0).transform.position, FlameThrower_L.transform.localRotation);
        Instantiate(FlamethrowerAmmo, FlameThrower_R.transform.GetChild(0).transform.position, FlameThrower_R.transform.localRotation);

        yield return new WaitForSeconds(shootRate);

        isShooting = false;
    }

    IEnumerator FireMainTurrets()
    {
        isShooting = true;

        Instantiate(MainTurretAmmo, MainTurret_L.transform.GetChild(0).transform.position, MainTurret_L.transform.localRotation);
        Instantiate(MainTurretAmmo, MainTurret_L.transform.GetChild(1).transform.position, MainTurret_L.transform.localRotation);

        Instantiate(MainTurretAmmo, MainTurret_R.transform.GetChild(0).transform.position, MainTurret_R.transform.localRotation);
        Instantiate(MainTurretAmmo, MainTurret_R.transform.GetChild(1).transform.position, MainTurret_R.transform.localRotation);

        yield return new WaitForSeconds(shootRate);

        isShooting = false;
    }

    IEnumerator FireRocketTurrets()
    {
        isShooting = true;

        Instantiate(RocketAmmo, RocketTurret_L.transform.GetChild(0).transform.position, RocketTurret_L.transform.localRotation );
        Instantiate(RocketAmmo, RocketTurret_L.transform.GetChild(1).transform.position, RocketTurret_L.transform.localRotation);

        Instantiate(RocketAmmo, RocketTurret_R.transform.GetChild(0).transform.position, RocketTurret_R.transform.localRotation);
        Instantiate(RocketAmmo, RocketTurret_R.transform.GetChild(1).transform.position, RocketTurret_R.transform.localRotation);

        yield return new WaitForSeconds(shootRate);

        isShooting = false;
    }


    protected override void Death()
    {
        DeathShared();

        agent.isStopped = true;

        anim.SetBool("isDead", true);
        playerInRange = false;
        playerInView = false;
        isAlerted = false;

        GetComponent<bossAI>().enabled = false; 

        enemyHPBar.SetActive(false);

        Instantiate(DeathVFX, DeathFXPos.position, Quaternion.identity);

        StartCoroutine(DeathSparks());

        StartCoroutine(DespawnDeadRobot(gameObject));

    }

    IEnumerator DeathSparks()
    {

        Instantiate(DeathVFX, deathSparks2.position, Quaternion.identity);
        
        yield return new WaitForSeconds(0.3f);
        
        Instantiate(DeathVFX, deathSparks3.position, Quaternion.identity);


        yield return new WaitForSeconds(2.25f);

        Instantiate(deathExplosion, DeathFXPos.position, Quaternion.identity);

        anim.SetBool("Dead",true);
    }

}


