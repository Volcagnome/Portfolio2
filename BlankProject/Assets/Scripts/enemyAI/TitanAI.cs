using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//Handles all behavior unique to Titans, everything else handled by SharedEnemyAI.

public class TitanAI : SharedEnemyAI, IDamage
{

    [SerializeField] Collider shieldBashCollider;
    [SerializeField] int minTimeBetweenBashes;
    [SerializeField] AudioClip shieldSwing;
    [SerializeField] public AudioClip shieldHit;

    

    //[SerializeField] float shieldDamageReduction;

    bool isBashing;
    bool inBashingDistance;
    bool isRepositioning;

    // Start is called before the first frame update
    //On start if their default post is null and they are within 0.5f of a reinforcement spawner, they are responding to an
    //intruder alert and the reinforcement spawner is set as their default post. Otherwise they are sent to the
    //EnemyManager to be assigned a Titan post.
    //
    //If their default post is not null, they add themselves to the Titan count and Titan roster within the
    //EnemyManager, and they set their manually assigned default post to occupied.
    void Start()
    {
        currentAmmo = ammoCapacity;
        enemyDetectionLevel = 0;
        currentDetectTime = detectTime;

        if (loadedFromState == false)
            HP = HPOrig;

        if (defaultPost == null)
        {
            if (Vector3.Distance(transform.position, IntruderAlertManager.instance.GetReinforcementSpawner().transform.position) < 0.5f)
                defaultPost = IntruderAlertManager.instance.GetReinforcementSpawner();
            else
                EnemyManager.instance.AssignTitanPost(gameObject);
        }
        else if (defaultPost.GetComponent<TitanPost>())
        {
            EnemyManager.instance.AddRobotToCount(gameObject);
            EnemyManager.instance.AddTitanToRoster(gameObject);
            defaultPost.GetComponent<TitanPost>().SetIsOccupied(true);
            defaultPost.GetComponent<TitanPost>().AssignTitan(gameObject);
        }

        isBashing = false;


        colorOrig = model.sharedMaterial.color;

        enemyDetectionLevel = 0;
        readyToSpeak = true;
        playerSpotted = false;
        currentIdleSoundCooldown = Random.Range(5, maxIdleSoundCooldown);
    }

    void Update()
    {

        if (!isDead)
        {
            CallMovementAnimation();

            if (!isAlerted && inCrouchRadius && GameManager.instance.player.GetComponent<playerCrouch>().GetIsCrouched())
            {
                SetPlayerCrouchedDetectionRadius();
            }
            else
                RevertDetectionRadius();

            //If boss fight is currently in progress, enemies will immediately proceed to the player's location 
            //regardless of if they are in range.
            if (EnemyManager.instance.GetIsBossFight())
            {
                if (!BossFight.instance.GetPlayerRespawned())
                {
                    isAlerted = true;
                    onDuty = false;
                    agent.speed = combatSpeed;
                    agent.stoppingDistance = combatStoppingDistance;
                    agent.SetDestination(GameManager.instance.player.transform.position);
                }
                else
                    agent.SetDestination(defaultPost.transform.position);
            }


            if (playerInView)
            {
                if (!detecting)
                    lastKnownPlayerLocation = GameManager.instance.player.transform.position;
                playerInViewIndicator.SetActive(true);

            }
            else
                playerInViewIndicator.SetActive(false);

            if (!isAlerted && !playerDetected && playerInView && !detecting)
                StartCoroutine(DetectPlayerCoroutine());

            if (isAlerted || playerDetected)
                ChangeMaterial(hostileMaterial);
            else if (detecting || isSearching || isPursuing)
                ChangeMaterial(searchingMaterial);
            else if(!isXrayed)
                ChangeMaterial(originalMaterial);

            //If player is in view, notes their location, changes their alert status, alerts nearby allies and
            //begins engaging with the player. Otherwise reduces their stopping distance so they can reach their
            //destinations, stops aiming their weapon, and deactivates their playerInView indicator.
            if (playerInView && playerDetected)
            {
                lastKnownPlayerLocation = GameManager.instance.player.transform.position;


                if (!isAlerted)
                {
                    if (!audioPlayer.isPlaying)
                        audioPlayer.PlayOneShot(foundPlayer, 0.75f);
                    AlertEnemy();
                }

                AlertAllies();
                FoundPlayer();
                if(!inBashingDistance && !isRepositioning)
                    agent.stoppingDistance = combatStoppingDistance;

                enemyDetectionLevel = 100f;
                playerInViewIndicator.SetActive(true);

            }
            else
            {
                if (isAlerted)
                    agent.stoppingDistance = idleStoppingDistance;
                anim.SetBool("Aiming", false);
                playerInViewIndicator.SetActive(false);


            }


            //When alerted, will pursue the player if they are not already en route to the player's location during an Intruder Alert.
            //Otherwise if a boss fight is not in progress, they will return to their post.
            if (isAlerted)
            {

                if (!playerInView && !isRespondingToAlert && !isSearching)
                {
                    StartCoroutine(PursuePlayer());
                }

                if (playerInRange)
                    RotateToPlayer();
            }
            else
            {
                if (readyToSpeak)
                    StartCoroutine(playIdleSound());

                if (!onDuty && defaultPost != null && !EnemyManager.instance.GetIsBossFight())
                    ReturnToPost();
            }

            //If enemy has taken damage, makes its health bar visible and updates it to reflect health loss, otherwise hides it
            if (isPlayerTarget())
                UpdateEnemyUI();
            else
                enemyHPBar.SetActive(false);


            //If player is within its outer range or is alerted, makes its detection meter visible and updates it appropriately,
            //otherwise hides it.
            if (detecting || isAlerted)
                UpdateDetectionUI();
            else
            {
                playerDetectionCircle.SetActive(false);
            }
        }
    }

    //If the player is in view, they travel to their position up to their combat stopping distance and aim their weapon
    //at the player. If the player is within 5f, they will attempt to bash them with their shield. Otherwise they will 
    //shoot at them. If there is an IntruderAlert in progress, they will inform the response team of the player's location.
    protected override void FoundPlayer()
    {

        agent.SetDestination(GameManager.instance.player.transform.position);
        agent.stoppingDistance = combatStoppingDistance;

        if (currentAmmo < 0)
            weapon_R.transform.LookAt(GameManager.instance.player.transform.position + new Vector3(0, -90f, 0)) ;

        if (Vector3.Distance(transform.position, GameManager.instance.player.transform.position) > 7f &&!isShooting)
        {
            inBashingDistance = false;

            if (playerInView)
            {
                RaycastHit hit;
                if (Physics.Raycast(shootPos.position + new Vector3(-0.5f, 0f, 0f), playerDirection, out hit))
                {
                    if (!hit.collider.gameObject.CompareTag("Player") && angleToPlayer <= FOV_Angle)
                    {
                        isRepositioning = true;
                        Debug.Log("test");
                        NavMeshHit navHit;
                        NavMesh.SamplePosition(transform.position + new Vector3(0f, 0f, -2f), out navHit, 5f, 1);
                        agent.stoppingDistance = idleStoppingDistance;
                        agent.SetDestination(navHit.position);
                    }
                    else
                    {
                        isRepositioning = false;
                        agent.stoppingDistance = combatStoppingDistance;
                        if (currentAmmo > 0)
                            StartCoroutine(shoot(ammoType));
                        else if (currentAmmo == 0)
                        {
                            anim.SetTrigger("Reload");
                            anim.SetBool("isReloaded", false);
                        }
                    }
                }
            }
            else
                isRepositioning = false;
        }

        else if (Vector3.Distance(transform.position, GameManager.instance.player.transform.position) <= 7f && !isBashing)
        {
            inBashingDistance = true;
            agent.stoppingDistance = idleStoppingDistance;
            agent.SetDestination(GameManager.instance.player.transform.position);

            if (Vector3.Distance(transform.position, GameManager.instance.player.transform.position) < 3.5f)
                StartCoroutine(ShieldBash());
        }

        if (IntruderAlertManager.instance.GetIntruderAlert())
            IntruderAlertManager.instance.FoundTheIntruder(lastKnownPlayerLocation);
    }


    protected override void CreateBullet()
    {
        currentAmmo--;
        weapon_R.GetComponentInChildren<Light>().intensity += 0.1f;

        if (currentAmmo < 3)
            WeaponCoolingVFXStart();

        Instantiate(ammoType, shootPos.position, transform.rotation);
        Vector3 offset = new Vector3(Random.Range(-aimOffset, aimOffset), 0f, Random.Range(aimOffset, aimOffset));
        playerDirection = GameManager.instance.player.transform.position - shootPos.position;

        if (Vector3.Distance(transform.position, GameManager.instance.player.transform.position) < 6f)
            offset = offset * 0.5f;

        RaycastHit hit;
        if (Physics.Raycast(shootPos.position, playerDirection + offset, out hit))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
                GameManager.instance.player.GetComponent<IDamage>().takeDamage(ammoType.GetComponent<damage>().GetDamageAmount());
        }


    }
    private void WeaponCoolingVFXStart()
    { 
        weapon_R.GetComponentInChildren<ParticleSystem>().Play();   
    }

    private void PlayWeaponCooldownSound()
    {
        weapon_R.GetComponent<AudioSource>().PlayOneShot(weaponReload);
    }

    private void WeaponCoolingVFXStop()
    {
        weapon_R.GetComponentInChildren<Light>().intensity = 0f;
        weapon_R.GetComponentInChildren<ParticleSystem>().Stop();
    }

    //Turns on the shield collider when called by the shield bash animation envent.
    private void ShieldColliderOn()
    {
        shieldBashCollider.enabled = true;
    }

    //Turns offthe shield collider when called by the shield bash animation envent.
    private void ShieldColliderOff()
    {
        shieldBashCollider.enabled = false;
    }

    //Activates the Bash trigger within the shield bash animation and waits for the configured shield bash cooldown beofore
    //another shield bash can be initiated.
    IEnumerator ShieldBash()
    {

        isBashing = true;

        anim.SetTrigger("Bash");

        yield return new WaitForSeconds(minTimeBetweenBashes);

        isBashing = false;
        agent.stoppingDistance = combatStoppingDistance;
    }

    //Overrides the standard Death function in SharedEnemyAI. Calls the DeathShared function to execute all common death operations
    //If they were assigned to a Titan post, passes themselves to the EnemyManager to decrement the Titan count and remove them
    //from the Titan roster. Also sets their Titan post to unoccupied. If they were part of an Intruder Alert response team, removes
    //them from the response team list. Starts the coroutine to despawn their corpse.
    protected override void Death()
    {
        DeathShared();
        agent.isStopped = true;

        ChangeMaterial(originalMaterial);

        weapon_R.GetComponent<AudioSource>().mute = true;

        Instantiate(DeathVFX, DeathFXPos.position, Quaternion.identity);

        if (!StaticData.selfDestructActivated_Static)
        {
            if (defaultPost.GetComponent<TitanPost>())
            {
                EnemyManager.instance.RemoveDeadRobot(gameObject);
                defaultPost.GetComponent<TitanPost>().SetIsOccupied(false);
                defaultPost.GetComponent<TitanPost>().AssignTitan(null);
                EnemyManager.instance.RemoveTitanFromRoster(gameObject);
            }

            if (IntruderAlertManager.instance.responseTeam.Contains(gameObject))
                IntruderAlertManager.instance.responseTeam.Remove(gameObject);
        }

        StartCoroutine(DespawnDeadRobot(gameObject));
    }

    protected override void playFootstepSound()
    {
        int playTrack = Random.Range(0, footsteps.Count);

        audioPlayer.PlayOneShot(footsteps[playTrack], 0.1f);
    }

    private void playShieldSwingSound()
    {
        audioPlayer.clip = shieldSwing;
        audioPlayer.Play();
    }

    //public float GetShieldDamageReduction() { return shieldDamageReduction; }
}
