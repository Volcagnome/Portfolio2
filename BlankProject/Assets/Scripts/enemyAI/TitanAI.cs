using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanAI : SharedEnemyAI, IDamage
{
    [SerializeField] Collider shieldBashCollider;
    [SerializeField] int minTimeBetweenBashes;
    [SerializeField] float shieldDamageReduction;

    bool isBashing;


    // Start is called before the first frame update
    void Start()
    {
        if (defaultPost == null)
        {
            //if (Vector3.Distance(transform.position, LevelManager.instance.GetReinforcementSpawner().transform.position) < 0.5f)
            //    defaultPost = LevelManager.instance.GetReinforcementSpawner();
            //else
                EnemyManager.instance.AssignTitanPost(gameObject);
        }
        else if (defaultPost.GetComponent<TitanPost>())
        {
            EnemyManager.instance.AddRobotToTitanCount();
            EnemyManager.instance.AddTitanToRoster(gameObject);
            defaultPost.GetComponent<TitanPost>().SetIsOccupied(true);
            defaultPost.GetComponent<TitanPost>().AssignTitan(gameObject);
        }

        isBashing = false;

        HPOrig = HP;
        colorOrig = model.sharedMaterial.color;
    }

    protected override void FoundPlayer()
    {

        agent.SetDestination(GameManager.instance.player.transform.position);
        agent.stoppingDistance = combatStoppingDistance;

        weapon_R.transform.LookAt(GameManager.instance.player.transform.position + new Vector3(0, -90f, 0)) ;

        if (Vector3.Distance(transform.position, GameManager.instance.player.transform.position) > 5f &&!isShooting)
        {
            StartCoroutine(shoot());
        }
       
        if (Vector3.Distance(transform.position, GameManager.instance.player.transform.position) <= 5f && !isBashing)
            StartCoroutine(ShieldBash());

        if (LevelManager.instance.GetIntruderAlert())
            LevelManager.instance.FoundTheIntruder(lastKnownPlayerLocation);
    }

    private void ShieldColliderOn()
    {
        shieldBashCollider.enabled = true;
    }

    private void ShieldColliderOff()
    {
        shieldBashCollider.enabled = false;
    }

    IEnumerator ShieldBash()
    { 

        isBashing = true;

        anim.SetTrigger("Bash");

        yield return new WaitForSeconds(minTimeBetweenBashes);

        isBashing = false;
    }

    protected override void Death()
    {
        DeathShared();

        Instantiate(DeathVFX, DeathFXPos.position, Quaternion.identity);

        if (defaultPost.GetComponent<TitanPost>())
        {
            EnemyManager.instance.RemoveDeadTitan(gameObject);
            defaultPost.GetComponent<TitanPost>().SetIsOccupied(false);
            EnemyManager.instance.RemoveTitanFromRoster(gameObject);
        }

        if (LevelManager.instance.responseTeam.Contains(gameObject))
            LevelManager.instance.responseTeam.Remove(gameObject);

        StartCoroutine(DespawnDeadRobot(gameObject));
    }

    public float GetShieldDamageReduction() { return shieldDamageReduction; }
}
