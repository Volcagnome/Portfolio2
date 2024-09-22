using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


//Handles all behavior unique to guard robots, everything else handled by SharedEnemyAI.

public class guardAI : SharedEnemyAI, IDamage
{

    // Start is called before the first frame update
    //If robot was placed in scene and already has a manually assigned post, adds them to the guard count and roster, and
    //sets its assigned guard post as occupied. If they are within 0.5f of the reinforcement spawner, they are reinforcements
    //called to respond to an Intruder Alert and sets their default post as the reinforcement spawner. Otherwise, passes them
    //to the EnemyManager to assign them a guard post.
    void Start()
    {
        currentAmmo = ammoCapacity;

        enemyDetectionLevel = 0;
        currentDetectTime = detectTime;


        if (loadedFromState == false)
            HP = HPOrig;

        if (defaultPost != null && !GameManager.instance.GetSelfDestructActivated())
        {
            if (defaultPost.GetComponent<GuardPost>())
            {
                EnemyManager.instance.AddRobotToCount(gameObject);
                EnemyManager.instance.AddGuardToRoster(gameObject);
                defaultPost.GetComponent<GuardPost>().SetIsOccupied(true);
                defaultPost.GetComponent<GuardPost>().AssignGuard(gameObject);
            }
            else if (Vector3.Distance(transform.position, IntruderAlertManager.instance.GetReinforcementSpawner().transform.position) < 0.5f)
                defaultPost = IntruderAlertManager.instance.GetReinforcementSpawner();
        }
        else
            EnemyManager.instance.AssignGuardPost(gameObject);


        colorOrig = model.sharedMaterial.color;

        readyToSpeak = true;
        currentIdleSoundCooldown = 5f;

    }


    //On death calls the DeathShared functions to execute all the common death operations. If they were assigned to a guard post,
    //passes them to the EnemyManager to remove them from the guard count, removes them from the guard roster, and sets their
    //guard post as unoccupied. If they were on the response team for an Intruder Alert, removes them from the list. Disables
    //their guardAI and starts the timer to despawn their corpse.
    protected override void Death()
    {

        isDead = true;
        weapon_R.GetComponent<AudioSource>().mute = true;
        agent.isStopped = true;

        DeathShared();

        ChangeMaterial(originalMaterial);

        if (!StaticData.selfDestructActivated_Static)
        {
            if (defaultPost.GetComponent<GuardPost>())
            {
                EnemyManager.instance.RemoveDeadRobot(gameObject);
                defaultPost.GetComponent<GuardPost>().SetIsOccupied(false);
                defaultPost.GetComponent<GuardPost>().AssignGuard(null);
                EnemyManager.instance.RemoveGuardFromRoster(gameObject);
            }

            if (IntruderAlertManager.instance.responseTeam.Contains(gameObject))
                IntruderAlertManager.instance.responseTeam.Remove(gameObject);
        }

        GetComponent<guardAI>().enabled = false;
        StartCoroutine(DespawnDeadRobot(gameObject));
    }


    protected override void playFootstepSound()
    {
        if (!isDead)
        {
            int playTrack = Random.Range(0, footsteps.Count);

            audioPlayer.PlayOneShot(footsteps[playTrack], 1);
        }
    }

    public override void XrayEnemy(GameObject enemy, bool xrayApplied)
    {
        
        Material materialToApply = null;

        if (xrayApplied && !isAlerted && !isSearching && !isPursuing && !detecting)
        {
            isXrayed = true;
            materialToApply = passiveMaterial;
        }
        else if (!xrayApplied)
        {
            isXrayed = false;
        }

        for (int bodyPart = 1; bodyPart < 9; bodyPart++)
            enemy.transform.GetChild(bodyPart).GetComponent<SkinnedMeshRenderer>().material = materialToApply;

    }

    protected override void ChangeMaterial(Material material)
    {

        for (int bodyPart = 1; bodyPart < 9; bodyPart++)
            gameObject.transform.GetChild(bodyPart).GetComponentInChildren<SkinnedMeshRenderer>().material = material;
    }
}
