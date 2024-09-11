using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Handles all behavior unique to Titans, everything else handled by SharedEnemyAI.

public class TitanAI : SharedEnemyAI, IDamage
{

    [SerializeField] Collider shieldBashCollider;
    [SerializeField] int minTimeBetweenBashes;
    [SerializeField] AudioClip shieldSwing;
    [SerializeField] public AudioClip shieldHit;

    //[SerializeField] float shieldDamageReduction;

    bool isBashing;

    // Start is called before the first frame update
    //On start if their default post is null and they are within 0.5f of a reinforcement spawner, they are responding to an
    //intruder alert and the reinforcement spawner is set as their default post. Otherwise they are sent to the
    //EnemyManager to be assigned a Titan post.
    //
    //If their default post is not null, they add themselves to the Titan count and Titan roster within the
    //EnemyManager, and they set their manually assigned default post to occupied.
    void Start()
    {
        if (defaultPost == null)
        {
            if (Vector3.Distance(transform.position, LevelManager.instance.GetReinforcementSpawner().transform.position) < 0.5f)
                defaultPost = LevelManager.instance.GetReinforcementSpawner();
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

        HPOrig = HP;
        colorOrig = model.sharedMaterial.color;

        enemyDetectionLevel = 0;
        readyToSpeak = true;
        playerSpotted = false;
        currentIdleSoundCooldown = Random.Range(5, maxIdleSoundCooldown);
    }


    //If the player is in view, they travel to their position up to their combat stopping distance and aim their weapon
    //at the player. If the player is within 5f, they will attempt to bash them with their shield. Otherwise they will 
    //shoot at them. If there is an IntruderAlert in progress, they will inform the response team of the player's location.
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
    }

    //Overrides the standard Death function in SharedEnemyAI. Calls the DeathShared function to execute all common death operations
    //If they were assigned to a Titan post, passes themselves to the EnemyManager to decrement the Titan count and remove them
    //from the Titan roster. Also sets their Titan post to unoccupied. If they were part of an Intruder Alert response team, removes
    //them from the response team list. Starts the coroutine to despawn their corpse.
    protected override void Death()
    {
        DeathShared();

        Instantiate(DeathVFX, DeathFXPos.position, Quaternion.identity);

        if (defaultPost.GetComponent<TitanPost>())
        {
            EnemyManager.instance.RemoveDeadRobot(gameObject);
            defaultPost.GetComponent<TitanPost>().SetIsOccupied(false);
            defaultPost.GetComponent<TitanPost>().AssignTitan(null);
            EnemyManager.instance.RemoveTitanFromRoster(gameObject);
        }

        if (LevelManager.instance.responseTeam.Contains(gameObject))
            LevelManager.instance.responseTeam.Remove(gameObject);

        StartCoroutine(DespawnDeadRobot(gameObject));
    }

    protected override void playFootstepSound()
    {
        int playTrack = Random.Range(0, footsteps.Count);

        audioPlayer.PlayOneShot(footsteps[playTrack], 0.1f);
    }

    private void playShieldSwingSound()
    {
        audioPlayer.PlayOneShot(shieldSwing);
    }

    //public float GetShieldDamageReduction() { return shieldDamageReduction; }
}
