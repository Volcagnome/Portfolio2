using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class patrolAI : SharedEnemyAI,IDamage
{

    bool isWhistleBlower;


    // Start is called before the first frame update
    void Start()
    {
        HPOrig = HP;

        if (defaultPost == null)
            EnemyManager.instance.AssignPatrolPost(gameObject);
        else
        {
            defaultPost.GetComponent<PatrolWaypoint>().AddRobotToRoute(gameObject);
            EnemyManager.instance.AddRobotToPatrolCount();
        }

        colorOrig = model.sharedMaterial.color;

        isWhistleBlower = false;

    }

    // Update is called once per frame
    void Update()
    {

        CallMovementAnimation();

        if (!isDead)
        {

            if (!isWhistleBlower)
            {
                if (onDuty)
                {
                    if (Vector3.Distance(transform.position, currentDestination.transform.position) < 0.5f)
                    {
                        currentDestination = currentDestination.GetComponent<PatrolWaypoint>().GetNextWaypoint();
                        agent.SetDestination(currentDestination.transform.position);
                    }
                    
                }

                if (playerInView)
                {
                    AlertEnemy();
                    AlertAllies();

                    if (!LevelManager.instance.GetIntruderAlert() && !LevelManager.instance.GetIsRaisingAlarm()
                        && LevelManager.instance.intruderAlertButtons.Count != 0)
                        RaiseAlarm();

                    else
                    {
                        FoundPlayer();
                        agent.stoppingDistance = combatStoppingDistance;
                    }
                }
                else
                    agent.stoppingDistance = idleStoppingDistance;

                if (isAlerted)
                {
                    if (!playerInView)
                        StartCoroutine(PursuePlayer());

                    else if (playerInRange)
                    {
                        RotateToPlayer();

                        //if (!playerInView)
                        //    agent.SetDestination(GameManager.instance.player.transform.position);
                    }
                }
                else if (!onDuty && defaultPost != null)
                    ReturnToPost();


                else if (LevelManager.instance.GetIntruderAlert())
                {
                    isWhistleBlower = false;
                }

                if (isPlayerTarget())
                {
                    UpdateEnemyUI();
                }
                else
                    enemyHPBar.SetActive(false);
            }
        }
    }

    protected override void Death()
    {
        DeathShared();

        EnemyManager.instance.RemoveDeadPatrol(gameObject);
        defaultPost.GetComponent<PatrolWaypoint>().RemoveRobotFromRoute(gameObject);

        if (isWhistleBlower)
        {
            LevelManager.instance.WhistleBlowerKilled();
        }

        StartCoroutine(DespawnDeadRobot(gameObject));
    }

    public void OnPatrol()
    {
        agent.SetDestination(currentDestination.transform.position);
    }

    protected override void ReturnToPost()
    {
        onDuty = true;

        if (currentDestination != null)
            agent.SetDestination(currentDestination.transform.position);
    }


    public void RaiseAlarm()
    {
        isWhistleBlower = true;

            GameObject nearestButton = LevelManager.instance.SetIsRaisingAlarm(gameObject);

            agent.stoppingDistance = 1f;
            agent.SetDestination(nearestButton.transform.position);

    }

    public bool GetIsWhistleBlower() { return isWhistleBlower; }

    public void SetIsWhistleBlower(bool status) { isWhistleBlower = false; }

    


}
