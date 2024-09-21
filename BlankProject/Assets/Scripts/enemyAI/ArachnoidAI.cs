using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

//Handles all behavior unique to spider robots, everything else handled by SharedEnemyAI.
public class ArachnoidAI : SharedEnemyAI,IDamage
{
    //Web line renderer
    [SerializeField] LineRenderer web;
    [SerializeField] Material originalMaterial2;
    [SerializeField] Material xrayMaterial2;
  
    bool caughtPlayer;

    void Start()
    {
        if(loadedFromState == false)
            HP = HPOrig;

        caughtPlayer = false;

        colorOrig = model.sharedMaterial.color;
        web = GetComponent<LineRenderer>();

        readyToSpeak = true;
        playerSpotted = false;
        currentIdleSoundCooldown = Random.Range(5, maxIdleSoundCooldown);

    }


    void Update()
    {
        CallMovementAnimation();

        if (readyToSpeak)
            StartCoroutine(playIdleSound());

        //When player enters spider's field of view, changes alert status, alerts enemies within
        //its configured Ally Radius, and moves to engage with player
        if (playerInView)
        {
            if(!isAlerted)
                AlertEnemy();
            AlertAllies();
            FoundPlayer();
            agent.stoppingDistance = combatStoppingDistance;

            if (!playerSpotted)
            {
                playerSpotted = true;

                if (!audioPlayer.isPlaying)
                    audioPlayer.PlayOneShot(foundPlayer, 0.5f);     
            }

        }
        else
            agent.stoppingDistance = idleStoppingDistance;


        //If spider is alerted but can't see the player, moves to their last known location, if they are in range
        //rotates towards them. If they are in range, but not in view, moves to the player's position (otherwise they
        //would never leave their spider holes). If they lose track of the player, will default to their idle behavior
        //and return to their default post (their spider hole).
        if (isAlerted)
        {
            if (!playerInView && !playerInRange)
                StartCoroutine(PursuePlayer());

            else if (playerInRange)
            {
                RotateToPlayer();
            }
            else if (playerInRange && !playerInView)
                agent.SetDestination(GameManager.instance.player.transform.position);

            

        }
        else if (!onDuty)
            ReturnToPost();


        //If they have taken damage from the player, makes their health bar visible and adjusts it
        //to reflect any hp loss, otherwise hides it.
        if (isPlayerTarget())
        {
            UpdateEnemyUI();

        }
        else
            enemyHPBar.SetActive(false);


        //If player is hit by their web attack, enables their line renderer and sets begin point at their
        //shoot position, and the end point at the player's position. Applies the webbed overlay. If player
        //is dead, disables line renderer and reverses effects.
        if (caughtPlayer)
        {

            if (GameManager.instance.player.GetComponent<playerDamage>().getHP() <= 0 || !playerInView)
            {
                ReleaseFromWeb();
            }
            else
            {
                web.SetPosition(0, transform.InverseTransformPoint(shootPos.position));
                web.SetPosition(1, transform.InverseTransformPoint(GameManager.instance.player.transform.position - new Vector3(0f, -0.5f, 0f)));

                web.enabled = true;
                GameManager.instance.webbedOverlay.SetActive(true);
            }
        }
        

    }

    ////////////////////////////////////////
    ///        PLAYER DETECTION         ///
    ///////////////////////////////////////

    //Overrides Shared OnTriggerEnter function. When player enters their trigger sphere collider, starts looking for the player.
    //If the spider is in their spider hole (within 0.5f of their default post) changes their alert status and they will 
    //set a path to the player. 
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            StartCoroutine(FOVRoutine());

            if (Vector3.Distance(transform.position, defaultPost.transform.position) < 0.5f)
            {
                AlertEnemy();
                agent.SetDestination(GameManager.instance.player.transform.position);
            }
        }
        else
            return;
    }

    //Rotates the spider to track the player's movements.
    protected override void RotateToPlayer()
    {
        transform.LookAt(GameManager.instance.player.transform.position, transform.up);
    }



    ////////////////////////////////////////
    ///             COMBAT               ///
    ///////////////////////////////////////

    //When player enters view, sets a path to them and changes stopping distance to their configured combat
    //stopping distance. If they have not already webbed the player, will start shooting. If intruder alert in progress
    //will report the player's location to any responding guards.
    protected override void FoundPlayer()
    {
        agent.SetDestination(GameManager.instance.player.transform.position);
        agent.stoppingDistance = combatStoppingDistance;

        if (playerInView && !isShooting && !caughtPlayer)
            StartCoroutine(shoot(ammoType));

        if (IntruderAlertManager.instance.GetIntruderAlert())
            IntruderAlertManager.instance.FoundTheIntruder(lastKnownPlayerLocation);
    }

    //Shoots web projectile at player, on instantiation remembers spider who shot it so line rendererer can be enabled
    //if player is hit.
    protected override IEnumerator shoot(GameObject ammoType)
    {
        isShooting = true;

        anim.SetTrigger("Attack");

        GameObject projectile = Instantiate(ammoType, shootPos.position, transform.rotation);

        Vector3 offset = new Vector3(Random.Range(-aimOffset, aimOffset), 0f, Random.Range(aimOffset, aimOffset));
        playerDirection = GameManager.instance.player.transform.position  - shootPos.position;

        RaycastHit hit;
        if (Physics.Raycast(shootPos.position, playerDirection + offset, out hit))
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                caughtPlayer = true;
                GameManager.instance.player.GetComponent<playerMovement>().SetIsCaught(true);
            }

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }


    //On death, calls the SharedDeath function to execute common death operations. If player is currently webbed by
    //spider, reverses effects and disables line renderer. Finally removes spider from scene.
    protected override void Death()
    {
        DeathShared();

        if (caughtPlayer)
        {
            GameManager.instance.player.GetComponent<playerMovement>().SetIsCaught(false);
            GameManager.instance.player.GetComponent<playerMovement>().SetSpeed(GameManager.instance.player.GetComponent<playerMovement>().GetSpeedOG());
            GameManager.instance.webbedOverlay.SetActive(false);
        }

        Destroy(gameObject);
    }

    private void ReleaseFromWeb()
    {
        web.enabled = false;
        caughtPlayer = false;
        GameManager.instance.player.GetComponent<playerMovement>().SetIsCaught(false);
        GameManager.instance.player.GetComponent<playerMovement>().SetSpeed(GameManager.instance.player.GetComponent<playerMovement>().GetSpeedOG());
        GameManager.instance.webbedOverlay.SetActive(false);
    }

    protected override void playFootstepSound()
    {
        int playTrack = Random.Range(0, footsteps.Count);

        audioPlayer.PlayOneShot(footsteps[playTrack],3f);
    }

    public override void XrayEnemy(GameObject spider, bool xrayApplied)
    {
        //Material body1 = null;
        //Material body2 = null;


        //if (xrayApplied)
        //{
        //    body1 = xrayMaterial;
        //    body2 = xrayMaterial2;
        //}
        //else
        //{
        //     body1 = originalMaterial;
        //     body2 = originalMaterial2;
        //}

        //transform.GetChild(2).GetComponent<SkinnedMeshRenderer>().material = body2;

        //for(int bodyPart = 3; bodyPart < 11; bodyPart++) 
        //{
        //    if(bodyPart <7)
        //        transform.GetChild(bodyPart).GetComponent<SkinnedMeshRenderer>().material = body1;
        //    else
        //        transform.GetChild(bodyPart).GetComponent<SkinnedMeshRenderer>().material = body2;
        //}
    }


    ////////////////////////////////////////
    ///         GETTERS/SETTERS          ///
    ///////////////////////////////////////

    public void SetCaughtPlayer(bool status) { caughtPlayer = status;}
}
