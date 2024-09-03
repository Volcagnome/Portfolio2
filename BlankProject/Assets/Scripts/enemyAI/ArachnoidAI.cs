using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class ArachnoidAI : SharedEnemyAI,IDamage
{

    [SerializeField] LineRenderer web;

    bool caughtPlayer;

    void Start()
    {
        caughtPlayer = false;
        HPOrig = HP;
        colorOrig = model.sharedMaterial.color;
        web = GetComponent<LineRenderer>();

    }

    void Update()
    {
        CallMovementAnimation();

        if (playerInView)
        {
            AlertEnemy();
            AlertAllies();
            FoundPlayer();
            agent.stoppingDistance = combatStoppingDistance;
        }
        else
            agent.stoppingDistance = idleStoppingDistance;

        if (isAlerted)
        {
            if (!playerInView && !playerInRange && !isRespondingToAlert)
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

        if (isPlayerTarget())
        {
            UpdateEnemyUI();

        }
        else
            enemyHPBar.SetActive(false);

        if(caughtPlayer)
        {
            web.enabled = true;

            web.SetPosition(0, transform.InverseTransformPoint(shootPos.position));
            web.SetPosition(1, transform.InverseTransformPoint(GameManager.instance.player.transform.position));
        }

    }

    protected override void RotateToPlayer()
    {
        transform.LookAt(GameManager.instance.player.transform.position, transform.up);
    }

    protected override IEnumerator shoot()
    {
        isShooting = true;

        anim.SetTrigger("Attack");

        GameObject projectile = Instantiate(ammoType, shootPos.position, transform.rotation);

        projectile.GetComponent<WebAttack>().SetShooter(gameObject);

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    protected override void FoundPlayer()
    {
        agent.SetDestination(GameManager.instance.player.transform.position);
        agent.stoppingDistance = combatStoppingDistance;

        if (playerInView && !isShooting && !caughtPlayer)
            StartCoroutine(shoot());

        if (LevelManager.instance.GetIntruderAlert())
            LevelManager.instance.FoundTheIntruder(lastKnownPlayerLocation);
    }

    public void SetCaughtPlayer(bool status) { caughtPlayer = status;}

    protected override void Death()
    {
        DeathShared();

        if (caughtPlayer)
        {
           GameManager.instance.playerScript.SetIsCaught(false);
            GameManager.instance.playerScript.SetSpeed(GameManager.instance.playerScript.GetSpeedOG());
        }

        Destroy(gameObject);
    }
    
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



}
