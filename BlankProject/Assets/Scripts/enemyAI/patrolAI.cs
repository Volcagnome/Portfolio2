using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//Handles all behavior unique to patrol robots, everything else handled by SharedEnemyAI.

public class patrolAI : SharedEnemyAI,IDamage
{

    bool isWhistleBlower;
    [SerializeField] AudioClip raisingAlarmSound;


    // Start is called before the first frame update
    //If patrol has no default post, is passed to EnemyManager for patrol route assignment. Otherwise, if default post has been
    //manually assigned will add itself to the list of robots assigned to the route and pass itself to the enemy manager to add it
    //to the patrol robot count.
    void Start()
    {
        HPOrig = HP;

        if (defaultPost == null)
            EnemyManager.instance.AssignPatrolPost(gameObject);
        else
        {
            defaultPost.GetComponent<PatrolWaypoint>().AddRobotToRoute(gameObject);
            EnemyManager.instance.AddRobotToCount(gameObject);
        }

        colorOrig = model.sharedMaterial.color;

        enemyDetectionLevel = 0;

        readyToSpeak = true;
        currentIdleSoundCooldown = Random.Range(5, maxIdleSoundCooldown);

        isWhistleBlower = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            CallMovementAnimation();

            //If patrol is whistleblower, ignores all other AI until it sucessfully activates the Intruder Alert button, or
            //dies trying.
            if (!isWhistleBlower)
            {

                //If on duty, checks if it has arrived at it's current destination, if so, updates its current destination
                //to the next waypoint on the route and starts a path to it.
                if (onDuty)
                {
                    if (Vector3.Distance(transform.position, currentDestination.transform.position) < 0.5f)
                    {
                        currentDestination = currentDestination.GetComponent<PatrolWaypoint>().GetNextWaypoint();
                        agent.SetDestination(currentDestination.transform.position);
                    }
                    
                }

                //If player in patrol's field of view, changes its alert status and alerts nearby allies. If there is 
                //not currently an Intruder Alert in progress and no one is already running for an Intruder Alert button,
                //patrol will become the WhistleBlower and run to the nearest Intruder Alert button and then return to engage
                //with the player. Otherwise will engage with the player normally.
                if (playerInView)
                {
                    if(!isAlerted)
                        AlertEnemy();
                    AlertAllies();
                    playerInViewIndicator.SetActive(true);
                    if (!LevelManager.instance.GetIntruderAlert() && !LevelManager.instance.GetIsRaisingAlarm()
                        && LevelManager.instance.intruderAlertButtons.Count != 0)
                        RaiseAlarm();

                    else
                    {
                        FoundPlayer();
                        agent.stoppingDistance = combatStoppingDistance;
                    }

                    if (!playerSpotted)
                    {
                        audioPlayer.PlayOneShot(foundPlayer, 0.75f);
                        playerSpotted = true;

                    }
                }
                else
                    agent.stoppingDistance = idleStoppingDistance;


                //When alerted will rotate to player. If they are not in view, will move to the player's last known location. 
                //Otherwise will return to their post.
                if (isAlerted)
                {
                    if (!playerInView)
                        StartCoroutine(PursuePlayer());

                    if (playerInRange)
                        RotateToPlayer();

                }
                else if (!onDuty && defaultPost != null)
                {
                    if (readyToSpeak)
                        StartCoroutine(playIdleSound());

                    ReturnToPost();
                }

            }

            //If enemy has taken damage, makes its health bar visible and updates it to reflect health loss, otherwise hides it.
            if (isPlayerTarget())
            {
                UpdateEnemyUI();
            }
            else
                enemyHPBar.SetActive(false);


            //If player is within its outer range or is alerted, makes its detection meter visible and updates it appropriately,
            //otherwise hides it.
            if (playerInOuterRange || isAlerted)
                UpdateDetectionUI();
            else
            {
                playerDetectionCircle.SetActive(false);
            }
        }

    }

    //Informs the LevelManager they are raising the alarm and passes themselves to the LevelManager which returns the nearest
    //Intruder Alert button. Robot travels to the given Intruder Alert Button.
    public void RaiseAlarm()
    {
        isWhistleBlower = true;

        audioPlayer.PlayOneShot(raisingAlarmSound);

        GameObject nearestButton = LevelManager.instance.SetIsRaisingAlarm(gameObject);

        agent.stoppingDistance = 1f;
        agent.SetDestination(nearestButton.transform.position);

    }

    //Returns to their current destination (the point on their patrol route they were en route to when interrupted)
    protected override void ReturnToPost()
    {
        onDuty = true;

        if (currentDestination != null)
            agent.SetDestination(currentDestination.transform.position);
    }


    //Overrides the SharedEnemyAI playFootstepSound function in order to play it at a custom volume.
    protected override void playFootstepSound()
    {
        if(!isDead)
        { 
            int playTrack = Random.Range(0, footsteps.Count);

            audioPlayer.PlayOneShot(footsteps[playTrack], 0.3f);
        }
    }

    //Calls the DeathShared function to execute all common death operations. Passes itself to the EnemyManager to remove it
    //from the patrol robot count, and removes itself from it's patrol route. If robot was the WhistleBlower, informs the
    //LevelMangager. Starts the timer to despawn their corpse.
    protected override void Death()
    {
        DeathShared();

        EnemyManager.instance.RemoveDeadRobot(gameObject);
        defaultPost.GetComponent<PatrolWaypoint>().RemoveRobotFromRoute(gameObject);

        if (isWhistleBlower)
        {
            LevelManager.instance.WhistleBlowerKilled();
        }

        StartCoroutine(DespawnDeadRobot(gameObject));
    }


    public bool GetIsWhistleBlower() { return isWhistleBlower; }

   public void SetIsWhistleBlower(bool status) { isWhistleBlower = false; }

}
