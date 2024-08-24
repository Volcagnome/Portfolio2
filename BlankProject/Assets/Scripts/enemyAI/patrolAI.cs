using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class patrolAI : enemyAI,IDamage
{

    bool isWhistleBlower;

    // Start is called before the first frame update
    void Start()
    {
        HPOrig = HP;



        if (defaultPost == null)
        {
            Debug.Log("assigning post");
            EnemyManager.instance.AssignPatrolPost(gameObject);
        }
        else
        {
            Debug.Log("updating post");
            defaultPost.GetComponent<PatrolWaypoint>().AddRobotToRoute(gameObject);
            EnemyManager.instance.AddRobotToPatrolCount();
        }

        colorOrig = model.sharedMaterial.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isWhistleBlower)
        {
            if (playerInView)
            {
                AlertEnemy();
                AlertAllies();

                if (!LevelManager.instance.GetIntruderAlert() && !LevelManager.instance.GetIsRaisingAlarm())
                {
                    RaiseAlarm();
                }
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
                if (!playerInView && !playerInRange)
                    StartCoroutine(PursuePlayer());

                else if (playerInRange)
                {
                    RotateToPlayer();

                    //Vector3 playerDirection = GameManager.instance.player.transform.position - transform.position;
                    //playerDirection.y = 0;
                    //transform.rotation = Quaternion.LookRotation(playerDirection);

                    if (!playerInView)
                        agent.SetDestination(GameManager.instance.player.transform.position);
                }
            }
            else if (!onDuty)
                ReturnToPost();

        }
        else if (LevelManager.instance.GetIntruderAlert())
        {
            isWhistleBlower = false;
        }

        if (isPlayerTarget())
        {
            UpdateEnemyUI();

            if (!isTakingDamage)
                RegenerateHealth();
        }
        else
            enemyHPBar.SetActive(false);
    }

    protected override void Death()
    {
        EnemyManager.instance.RemoveDeadPatrol(gameObject);
        defaultPost.GetComponent<PatrolWaypoint>().RemoveRobotFromRoute(gameObject);
        
        if (isWhistleBlower)
        {
            LevelManager.instance.WhistleBlowerKilled();
        }

        Destroy(gameObject);
    }

    public void OnPatrol()
    {
        agent.SetDestination(currentDestination.transform.position);
    }

    public void RaiseAlarm()
    {
        isWhistleBlower = true;

        GameObject nearestButton = LevelManager.instance.SetIsRaisingAlarm(gameObject);

        agent.stoppingDistance = idleStoppingDistance;
        agent.SetDestination(nearestButton.transform.position);

    }

    public bool GetIsWhistleBlower() { return isWhistleBlower; }

    //public void SetIsWhistleBlower(bool status) { isWhistleBlower = false; }

}
