using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

//Handles all behavior unique to the boss, everything else handled by SharedEnemyAI.


public class bossAI : SharedEnemyAI, IDamage
{

    //Components
    [SerializeField] GameObject FlameThrower_L;
    [SerializeField] GameObject FlameThrower_R;
    [SerializeField] GameObject MainTurret_L;
    [SerializeField] GameObject MainTurret_R;
    [SerializeField] GameObject RocketTurret_L;
    [SerializeField] GameObject RocketTurret_R;
    [SerializeField] GameObject TurretCycler_L;
    [SerializeField] GameObject TurretCycler_R;
    [SerializeField] GameObject weapon_L;

    [SerializeField] Transform mainTurretShootPosL;
    [SerializeField] Transform mainTurretShootPosR;

    //[SerializeField] GameObject ShieldIndicator;


   
    //Ammo for all turrets
    [SerializeField] GameObject FlamethrowerAmmo;
    [SerializeField] GameObject MainTurretAmmo;
    [SerializeField] GameObject RocketAmmo;


    //Audio Clip Libraries
    [SerializeField] List<AudioClip> flameSounds = new List<AudioClip>();
    [SerializeField] List<AudioClip> mainTurretSounds = new List<AudioClip>();
    [SerializeField] AudioClip rocketTurretLaunch;
    [SerializeField] AudioClip death;
    [SerializeField] AudioClip turretCyclingSound;
    Coroutine playCyclingTurrets;
    bool cycledTurrets;


    //Particle Effects
    [SerializeField] ParticleSystem deathExplosion;
    [SerializeField] Transform deathSparks2;
    [SerializeField] Transform deathSparks3;


    //Basic Stats
    [SerializeField] float speed;

    //Turret rotation when each turret is "selected" (on top and able to fire).
    Quaternion[] turretRotations = {Quaternion.Euler(364.432f, 0.519f, 89.696f), Quaternion.Euler(433.059f, 178.225f, 267.958f), Quaternion.Euler(310.793f, 179.208f, 270.255f) };
   
    //Enum to assign an index number to each turret which corresponds to its index in the turretRotations array.
    public enum turretIndex { Flamethrower = 0, MainTurret = 1, RocketTurret = 2};
    turretIndex currentRotationIndex;


    // Start is called before the first frame update
    void Start()
    {
        if (loadedFromState == false)
            HP = HPOrig;

        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;

        readyToSpeak = true;
        playerSpotted = false;
        currentIdleSoundCooldown = Random.Range(5, maxIdleSoundCooldown);
    }

    // Update is called once per frame
    void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);
        anim.SetFloat("Speed", agent.velocity.magnitude);


        //When player is in view, travels to their location, changes their alert status and engages with player.
        if (playerInView)
        {
            lastKnownPlayerLocation = GameManager.instance.player.transform.position;

            if (!isAlerted)
                AlertEnemy();
            FoundPlayer();

            if (!playerSpotted)
            {
                audioPlayer.PlayOneShot(foundPlayer, 3f);
                playerSpotted = true;
            }

        }else if (playerInRange && isAlerted && !GameManager.instance.GetIsRespawning())
            agent.SetDestination(GameManager.instance.player.transform.position);


        //If player moves outside of 25f, reduces their stopping distance and sets a path to their location. Otherwise
        //rotates towards the player. If the player is not in view (behind cover) will move to their location.
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


        //If the enemy has taken damage, makes their health bar visible and updates it to reflect any health loss. Otherwise,
        //hides it.
        if (isPlayerTarget())
            UpdateEnemyUI();
        else
            enemyHPBar.SetActive(false);



        if (isAlerted || playerInOuterRange)
            UpdateDetectionUI();
        else
        {
            playerDetectionCircle.SetActive(false);
        }

        if (readyToSpeak)
            StartCoroutine(playIdleSound());

    }
      
    //Cycles turret depending on player proximity, Flamethrower for close, Main turret for mid-range, and rocket turret for 
    //long range. Once the turret has finished cycling to the correct one (the selected turret is at its rotation saved in the
    //turret rotation array), turrets will aim (LookAt) player and the Attack function is called.
    protected override void FoundPlayer()
    {

        if (distanceToPlayer < 20f)
        {
            SelectWeapon(FlameThrower_L, FlameThrower_R, turretIndex.Flamethrower);
            CycleTurrets();
            shootRate = 0.05f;
        }
        else if (distanceToPlayer > 20f && distanceToPlayer < 30f)
        {


            SelectWeapon(MainTurret_L, MainTurret_R, turretIndex.MainTurret);
            CycleTurrets();
            shootRate = 0.2f;
        }

        else if (distanceToPlayer > 40f)
        {
            SelectWeapon(RocketTurret_L, RocketTurret_R, turretIndex.RocketTurret);
            CycleTurrets();
            shootRate = 5f;
        }

      
        if (Quaternion.Angle(TurretCycler_L.transform.localRotation,turretRotations[(int)currentRotationIndex]) <8f)
        {
            weapon_L.transform.LookAt(GameManager.instance.player.transform.position, transform.up);
            weapon_R.transform.LookAt(GameManager.instance.player.transform.position, transform.up);

            Attack();
        }
    }

    IEnumerator playCycleTurretsSound()
    { 
        TurretCycler_L.transform.GetComponentInChildren<AudioSource>().PlayOneShot(turretCyclingSound);
        yield return new WaitForSeconds(2f);
    }

    //Sets the L and R weapon variables to the passed turret types and sets the currentRotationIndex to the passed index.
    private void SelectWeapon(GameObject weaponL, GameObject weaponR, turretIndex index)
    {
  
        weapon_L = weaponL;
        weapon_R = weaponR;
        currentRotationIndex = index;
    }


    //Rotates the turret cyclers to match the rotation of the currently selected turret in the the turretRotations array.
    private void CycleTurrets()
    {

            TurretCycler_L.transform.localRotation = Quaternion.Lerp(TurretCycler_L.transform.localRotation, turretRotations[(int)currentRotationIndex], Time.deltaTime * 2f);
            TurretCycler_R.transform.localRotation = Quaternion.Lerp(TurretCycler_R.transform.localRotation, turretRotations[(int)currentRotationIndex], Time.deltaTime * 2f);
    }

   
    //Calls the appropriate shoot function depending on which turret is currently selected
    private void Attack()
    {

        if (weapon_L == FlameThrower_L && !isShooting)
            StartCoroutine(FireFlamethrowers());
        else if (weapon_L == MainTurret_L && !isShooting)
            StartCoroutine(FireMainTurrets());
        else if(weapon_L == RocketTurret_L && !isShooting)
            StartCoroutine(FireRocketTurrets());

    }


    //Plays the beginning of the flame thrower sound effect. While player is in shooting distance and flamethrower is still selected
    //loops the flamethrower_Mid audio clip and continues to shoot fire at the player. If they leave flamethrower distance or the 
    //turret is cycled, stops audio loop, and plays the flamethrower_end sound clip. 
    IEnumerator FireFlamethrowers()
    {
        isShooting = true;

        AudioSource audioPlayer = TurretCycler_L.GetComponent<AudioSource>();

        audioPlayer.PlayOneShot(flameSounds[0],2f);

        
        while (distanceToPlayer < 20f && playerInView && weapon_L == FlameThrower_L)
        {
            if (!audioPlayer.isPlaying)
            {
                audioPlayer.loop = true;
                audioPlayer.PlayOneShot(flameSounds[1], 2f);
            }

            Instantiate(FlamethrowerAmmo, FlameThrower_L.transform.GetChild(0).transform.position, FlameThrower_L.transform.localRotation);
            Instantiate(FlamethrowerAmmo, FlameThrower_R.transform.GetChild(0).transform.position, FlameThrower_R.transform.localRotation);
            yield return new WaitForSeconds(shootRate);
        }

        if (audioPlayer.isPlaying)
        {
            audioPlayer.Stop();
            audioPlayer.loop = false;
            audioPlayer.PlayOneShot(flameSounds[2],2f);
        }

        isShooting = false;
    }




    //Alternates between L and R MainTurrets to fire a projectile and play their shooting audio clip. 
    IEnumerator FireMainTurrets()
    {
        isShooting = true;

        AudioSource audioPlayer = TurretCycler_L.GetComponent<AudioSource>();



        StartCoroutine(shoot(MainTurretAmmo,mainTurretShootPosL));
        Instantiate(MainTurretAmmo, MainTurret_L.transform.GetChild(0).transform.position, MainTurret_L.transform.localRotation);
        audioPlayer.PlayOneShot(mainTurretSounds[Random.Range(0, 2)]);

        StartCoroutine(shoot(MainTurretAmmo, mainTurretShootPosR));
        Instantiate(MainTurretAmmo, MainTurret_R.transform.GetChild(0).transform.position, MainTurret_R.transform.localRotation);
        audioPlayer.PlayOneShot(mainTurretSounds[Random.Range(0, 2)]);

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(shoot(MainTurretAmmo, mainTurretShootPosL));
        Instantiate(MainTurretAmmo, MainTurret_L.transform.GetChild(1).transform.position, MainTurret_L.transform.localRotation);
        audioPlayer.PlayOneShot(mainTurretSounds[Random.Range(0, 2)]);

        StartCoroutine(shoot(MainTurretAmmo, mainTurretShootPosR));
        Instantiate(MainTurretAmmo, MainTurret_R.transform.GetChild(1).transform.position, MainTurret_R.transform.localRotation);
        audioPlayer.PlayOneShot(mainTurretSounds[Random.Range(0, 2)]);
        

        yield return new WaitForSeconds(shootRate);

        isShooting = false;
    }

    protected virtual IEnumerator shoot(GameObject ammoType, Transform shootPos)
    {
        anim.SetTrigger("Shoot");

        isShooting = true;

        playerDirection = GameManager.instance.player.transform.position - shootPos.position;

        Vector3 offset = new Vector3(Random.Range(-aimOffset, aimOffset), 0f, Random.Range(-aimOffset, aimOffset));

        RaycastHit hit;
        if (Physics.Raycast(shootPos.position, playerDirection + offset, out hit))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
                GameManager.instance.player.GetComponent<IDamage>().takeDamage(ammoType.GetComponent<damage>().GetDamageAmount());
        }

            yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }


    //Shoots two rockets from each turret with a slight delay between them and plays their missleLaunch audio clip.
    IEnumerator FireRocketTurrets()
    {
        isShooting = true;

        AudioSource audioPlayer = TurretCycler_L.GetComponent<AudioSource>();

        audioPlayer.PlayOneShot(rocketTurretLaunch);
        Instantiate(RocketAmmo, RocketTurret_L.transform.GetChild(0).transform.position, RocketTurret_L.transform.localRotation );
        yield return new WaitForSeconds(0.2f);

        audioPlayer.PlayOneShot(rocketTurretLaunch);
        Instantiate(RocketAmmo, RocketTurret_L.transform.GetChild(1).transform.position, RocketTurret_L.transform.localRotation);
        yield return new WaitForSeconds(0.25f);

        audioPlayer.PlayOneShot(rocketTurretLaunch);
        Instantiate(RocketAmmo, RocketTurret_R.transform.GetChild(0).transform.position, RocketTurret_R.transform.localRotation);
        yield return new WaitForSeconds(0.2f);

        audioPlayer.PlayOneShot(rocketTurretLaunch);
        Instantiate(RocketAmmo, RocketTurret_R.transform.GetChild(1).transform.position, RocketTurret_R.transform.localRotation);

        yield return new WaitForSeconds(shootRate);

        isShooting = false;
    }

    //Calls the DeathShared function to perform all the common death operations, plays their disables their AI,
    //plays their "death twitch" animation and their initial death particle effects before starting the timer
    //before their corpse is despawned.
    protected override void Death()
    {
        StaticData.bossIsDead_Static = true;

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

    //Instantiates various particle effects on the enemy's body before calling the animation that will make them fall. 
    IEnumerator DeathSparks()
    {

        Instantiate(DeathVFX, deathSparks2.position, Quaternion.identity);
        
        yield return new WaitForSeconds(0.3f);
        
        Instantiate(DeathVFX, deathSparks3.position, Quaternion.identity);

        audioPlayer.PlayOneShot(death,0.4f);

        yield return new WaitForSeconds(2.25f);

        Instantiate(deathExplosion, DeathFXPos.position, Quaternion.identity);

        anim.SetBool("Dead",true);
    }


    protected override void playFootstepSound()
    {
        int playTrack = Random.Range(0, footsteps.Count);

        if(!isDead)
            audioPlayer.PlayOneShot(footsteps[playTrack], 1f);
    }
}


