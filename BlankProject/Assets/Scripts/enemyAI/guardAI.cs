using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;

public class guardAI : enemyAI, IDamage
{

    // Start is called before the first frame update
    void Start()
    {
        HPOrig = HP;



        if (defaultPost != null)
        {
            if (defaultPost.GetComponent<GuardPost>())
            {
                EnemyManager.instance.AddRobotToGuardCount();
                EnemyManager.instance.AddGuardToRoster(gameObject);
                defaultPost.GetComponent<GuardPost>().SetIsOccupied(true);
                defaultPost.GetComponent<GuardPost>().AssignGuard(gameObject);
            }
            else if (Vector3.Distance(transform.position, LevelManager.instance.GetReinforcementSpawner().transform.position) < 0.5f)
                defaultPost = LevelManager.instance.GetReinforcementSpawner();
        }
        else
            EnemyManager.instance.AssignGuardPost(gameObject);

        HPOrig = HP;
        colorOrig = model.sharedMaterial.color;

    }

    protected override void Death()
    {
        DeathShared();
        
        if (defaultPost.GetComponent<GuardPost>())
        {
            EnemyManager.instance.RemoveDeadGuard(gameObject);
            defaultPost.GetComponent<GuardPost>().SetIsOccupied(false);
        }
    
        if (LevelManager.instance.responseTeam.Contains(gameObject))
            LevelManager.instance.responseTeam.Remove(gameObject);

        StartCoroutine(DespawnDeadRobot(gameObject));

        GetComponent<guardAI>().enabled = false;
    }
  
}
