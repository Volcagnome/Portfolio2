using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using static UnityEngine.GraphicsBuffer;
//using UnityEditor.Experimental.GraphView;
//using UnityEditor.Search;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Data.Common;


//All enemy AI common to all enemies handled here.


public class SharedEnemyAI : MonoBehaviour
{ 
    //Components
    [SerializeField] protected Transform headPos;
    [SerializeField] protected Renderer model;
    [SerializeField] protected GameObject defaultPost;
    [SerializeField] protected GameObject currentDestination;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator anim;
    [SerializeField] protected ParticleSystem DeathVFX;
    [SerializeField] protected Transform DeathFXPos;
    [SerializeField] GameObject weakspot;
    [SerializeField] protected AudioSource audioPlayer;
    [SerializeField] protected Material xrayMaterial;
    [SerializeField] protected Material originalMaterial;

    [SerializeField] protected Transform shootPos;
    [SerializeField] protected GameObject weapon_R;
    [SerializeField] protected GameObject ammoType;
    [SerializeField] protected float aimOffset;

    //PlayerDetection
    [SerializeField] protected float FOV_Angle;
    [SerializeField] protected float rotationSpeed;
    protected Vector3 playerDirection;
    protected float angleToPlayer;
    protected float distanceToPlayer;
    protected Vector3 lastKnownPlayerLocation;
    protected bool playerInRange;
    protected bool playerInOuterRange;
    protected bool playerInView;
    protected float enemyDetectionLevel;
    float enemyDetectionLevelOG = 100f;
    [SerializeField] protected GameObject playerDetectionCircle;
    [SerializeField] protected Image playerDetectionCircleFill;
    [SerializeField] protected GameObject playerInViewIndicator;
    Coroutine FindIntruderCoroutine;
    Coroutine PursuePlayerCoroutine;




    //Current State
    protected bool isAlerted;
    protected bool onDuty;
    protected bool isShooting;
    protected bool isDead;
    protected Color colorOrig;
    protected bool hasPost;
    protected bool readyToSpeak;
    protected bool playerSpotted;
    protected bool isRespondingToAlert;
    protected bool isEndgameEnemy;
    protected bool isSearching;
    protected bool loadedFromState;
    protected bool inCrouchRadius;


    //Stats
    [SerializeField] public enum enemyType { none, Guard, Patrol, Titan, Turret, Boss, Arachnoid };
    [SerializeField] protected enemyType enemy_Type;
    [SerializeField] protected float HPOrig;
    [SerializeField] protected float shootRate;
    [SerializeField] protected float combatSpeed;
    [SerializeField] protected float combatStoppingDistance;
    [SerializeField] protected float idleStoppingDistance;
    [SerializeField] protected float idleSpeed;
    [SerializeField] float originalDetectionRadius;
    [SerializeField] float detectionRadiusPlayerCrouched;


    //Ally Detection
    [SerializeField] protected int allyRadius;
    [SerializeField] protected LayerMask allyLayer;
    protected GameObject[] alliesInRange;

    //Health Bar
    protected bool isTakingDamage;
    protected Coroutine regenCoroutine;
    protected float HP;
    public GameObject enemyHPBar;
    public Image enemyHPBarFill;
   [SerializeField] Vector3 HPBarPos;

    //Audio
    [SerializeField] protected float maxIdleSoundCooldown;
    [SerializeField] List<AudioClip> shootSounds;
    [SerializeField] protected List<AudioClip> footsteps;
    [SerializeField] protected List<AudioClip> idleSounds;
    [SerializeField] protected AudioClip foundPlayer; 
    protected float currentIdleSoundCooldown;



    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.player.GetComponent<Camera>().transform.position = GameManager.instance.player.GetComponent<Camera>().transform.position - new Vector3(0f, 1f, 0f);

        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;
        isAlerted = false;

        enemyDetectionLevel = 0;
        readyToSpeak = true;
        playerSpotted = false;
        currentIdleSoundCooldown = Random.Range(5, maxIdleSoundCooldown); 
        inCrouchRadius = false;
    }

    // Update is called once per frame
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
            if (EnemyManager.instance.GetIsBossFight() && !isSearching)
            {
                isAlerted = true;
                onDuty = false;
                agent.speed = combatSpeed;
                agent.stoppingDistance = combatStoppingDistance;
                agent.SetDestination(GameManager.instance.player.transform.position);
            }
        
            //If player is in view, notes their location, changes their alert status, alerts nearby allies and
            //begins engaging with the player. Otherwise reduces their stopping distance so they can reach their
            //destinations, stops aiming their weapon, and deactivates their playerInView indicator.
            if (playerInView)
            {
                lastKnownPlayerLocation = GameManager.instance.player.transform.position;

                if (!isAlerted)
                {
                    if(!audioPlayer.isPlaying)
                        audioPlayer.PlayOneShot(foundPlayer, 0.75f);
                    AlertEnemy();
                }

                AlertAllies();
                FoundPlayer();
                agent.stoppingDistance = combatStoppingDistance;
                playerInViewIndicator.SetActive(true);

            }
            else
            {
                if(isAlerted)
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
            if (isAlerted || playerInOuterRange)
                UpdateDetectionUI();
            else
            {
                playerDetectionCircle.SetActive(false);
            }
        }
    }


    ////////////////////////////////////////
    ///        PLAYER DETECTION         ///
    //////////////////////////////////////


    //Trigger colliders track if player is in detection range, if so starts the field of view coroutine to monitor
    //if player is in view.
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDead)
        {
            playerInRange = true;
            StartCoroutine(FOVRoutine());
        }
        else
            return;
    }

    //If player exits detection range, hides the stealth meter and resets their detection level. If they were
    //already alerted and player didn't leave detection sphere due to respawning, sets their lastKnownLocation to
    //the players position when they exited the sphere. If player exited because they respawned, returns to their
    //idle behavior.
    protected void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerDetectionCircle.SetActive(false);

            playerInRange = false;
            playerInView = false;
            enemyDetectionLevel = 0;

            if (isAlerted && !GameManager.instance.GetIsRespawning())
                lastKnownPlayerLocation = GameManager.instance.player.transform.position;

            else if (EnemyManager.instance.GetIsBossFight() && GameManager.instance.GetIsRespawning())
                    SearchArea(lastKnownPlayerLocation, 99);

            else if (isAlerted && GameManager.instance.GetIsRespawning())
                CalmEnemy();
        }
        else
            return;
    }

    //Calls the FieldOfViewCheck function at regular intervals, but not every single frame.
    protected IEnumerator FOVRoutine()
    {
        while (playerInRange)
        {
            yield return new WaitForSeconds(0.05f);
            playerInView = FieldOfViewCheck();
        }
    }

    //Sends a raycast from the enemy's head position in the direction of the player. If it hits the player, 
    //player is in view. 
    protected bool FieldOfViewCheck()
    {
        bool result;

        playerDirection = GameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDirection, transform.forward);

        if (playerInRange)
        {

            RaycastHit hit;
            if (Physics.Raycast(headPos.position, playerDirection, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Player") && angleToPlayer <= FOV_Angle)
                {
                    result = true;
                }

                else result = false;

            }
            else result = false;
        }
        else result = false;

        return result;
    }


    //Rotates the enemy to face the player.
    protected virtual void RotateToPlayer()
    {
        playerDirection = GameManager.instance.player.transform.position - transform.position;

        Quaternion rotationToPlayer = Quaternion.LookRotation(playerDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotationToPlayer, Time.deltaTime * rotationSpeed);
    }


    // if the player is in range and within 90 degrees of the enemy's forward midline, will increase the stealth
    //meter as they get closer to entering their field of view. Changes from blue to yellow, to red as the meter
    //fills, and remains red and filled if the player enters their field of view and enemy is alerted.
    //
    //If the player is in front of the enemy but not within their detection range (in outer detection range) and
    //is within their view angle, will also slowly fill the stealth meter the closer they get to coming within 
    //detection range.
    public void UpdateDetectionUI()
    {
        playerDetectionCircle.SetActive(true);

        float innerDetectionRadius = (gameObject.GetComponent<SphereCollider>().radius) * gameObject.transform.localScale.x;
        float outerDetectionRadius = (gameObject.transform.GetChild(1).GetComponent<SphereCollider>().radius) * gameObject.transform.localScale.x;

        playerDirection = GameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDirection, transform.forward);
        distanceToPlayer = Vector3.Distance(GameManager.instance.player.transform.position, transform.position);

        if (playerInRange)
        {
            if (angleToPlayer > 90f)
                enemyDetectionLevel = 0f;
            else if (angleToPlayer <= 90f && angleToPlayer >= 60f)
                enemyDetectionLevel = ((90f - angleToPlayer) / 30f) * 100f;
        }
        else if (playerInOuterRange && angleToPlayer < 60f)
        {

            if (distanceToPlayer >= innerDetectionRadius && distanceToPlayer < outerDetectionRadius)
                enemyDetectionLevel = (100f - (((distanceToPlayer - innerDetectionRadius) / (outerDetectionRadius - innerDetectionRadius)) * 100f));
        }
        else if (playerInOuterRange && angleToPlayer > 60f)
            enemyDetectionLevel = 0f;



        playerDetectionCircleFill.fillAmount = enemyDetectionLevel / enemyDetectionLevelOG;
        playerDetectionCircle.transform.parent.rotation = Camera.main.transform.rotation;

        if (enemyDetectionLevel > enemyDetectionLevelOG)
            enemyDetectionLevel = enemyDetectionLevelOG;

        if (enemyDetectionLevel <= 33f)
        {
            playerDetectionCircleFill.color = Color.cyan;

        }
        else if (enemyDetectionLevel > 33f && enemyDetectionLevel <= 66f)
        {
            playerDetectionCircleFill.color = Color.yellow;

        }
        else if (enemyDetectionLevel > 66f)
        {
            playerDetectionCircleFill.color = Color.red;

        }
    }


    ////////////////////////////////////////
    ///      SEARCHING FOR PLAYER       ///
    ///////////////////////////////////////

    //Sets animation speed variable to enemy's current velocity.
    protected void CallMovementAnimation()
    {
        if (!isDead)
            anim.SetFloat("Speed", agent.velocity.magnitude);
        else
            anim.SetFloat("Speed", 0f);
    }

    //Enemy will travel to the player's last known location. If they find the player, stops coroutine. Otherwise
    //if they reach the player's last known location, pauses for a second, then returns to their idle behavior.
    protected IEnumerator PursuePlayer()
    {

        agent.SetDestination(lastKnownPlayerLocation);

        if (!EnemyManager.instance.GetIsBossFight() && !isEndgameEnemy && agent.remainingDistance <= 0.3f || !agent.hasPath)
        {
            yield return new WaitForSeconds(1.5f);
            CalmEnemy();
        }
        else if (agent.remainingDistance <= 0.3f && isEndgameEnemy)
            StartCoroutine(SearchArea(lastKnownPlayerLocation, IntruderAlertManager.instance.GetMaxSearchAttempts()));
    }

    // If the FindIntruder coroutine is already in progress, stops it and restarts it with a new location to 
    // search.
    public void StartOrUpdateFindIntruder(Vector3 location)
    {
        if (FindIntruderCoroutine != null)
            StopCoroutine(FindIntruder(location));

        FindIntruderCoroutine = StartCoroutine(FindIntruder(location));
    }


    //Sets isRespondingToAlert bool to true, so they do not stop until they reach their destination. Updates their
    //alert status and their lastKnownPlayerLocation variable with the passed location. While en route, if they player
    //stops the coroutine and sets their isRespondingToAlert bool to false so they resume their normal behavior. If
    //they reach the intruder location, starts the SearchArea coroutine.
    public IEnumerator FindIntruder(Vector3 intruderLocation)
    {
        isRespondingToAlert = true;
        
        if(!isAlerted)
            AlertEnemy();
        lastKnownPlayerLocation = intruderLocation;

        agent.SetDestination(lastKnownPlayerLocation);

        while (true && !isDead)
        {

            yield return new WaitForSeconds(0.05f);

            if (playerInView)
            {
                FindIntruderCoroutine = null;
                isRespondingToAlert = false;
                break;
            }

            if (agent.remainingDistance <= 1f)
            {
                FindIntruderCoroutine = null;
                isRespondingToAlert = false;
                StartCoroutine(SearchArea(IntruderAlertManager.instance.GetIntruderLocation(),IntruderAlertManager.instance.GetMaxSearchAttempts()));
                break;
            }
        }
    }


    //Selects a random location within their configured search radius, travels to it and pauses for a moment before 
    //selecting a new random search location. If player not found after the configured max number of search attempts
    //will return to their idle behavior.

    public IEnumerator SearchArea(Vector3 location, int searchAttempts)
    {
        isSearching = true;

        int maxSearchAttempts = IntruderAlertManager.instance.GetSearchAttempts();
        float searchRadius = IntruderAlertManager.instance.GetSearchRadius();
        float searchTimer = IntruderAlertManager.instance.GetSearchTimer();
        bool playerFound = false;

        for (int attempts = 0; attempts < maxSearchAttempts; attempts++)
        {

            Vector3 randomDist = Random.insideUnitSphere * searchRadius;
            randomDist += location;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDist, out hit, searchRadius, 1);
            agent.SetDestination(hit.position);

            yield return new WaitForSeconds(searchTimer);

            if (playerInView)
            {
                isSearching = false;
                playerFound = true;
                yield break;
            }
        }

        if (!playerFound)
        {   isSearching = false;
            CalmEnemy();
        }
    }

    ////////////////////////////////////////
    ///             COMBAT               ///
    ///////////////////////////////////////

    //If player was recently in view but has taken cover (no longer in view) will continue firing in their direction, but will wait a configured
    //number of seconds before attempting to 


    //Flashes the enemy model red when weakspot is hit
    protected IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    //Flashes the enemy model yellow if hit anywhere besides the weakspot.
    protected IEnumerator flashYellow()
    {
        model.material.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    //Travels to the player's location up to their combat stopping distance. Aims their weapon at the player. Starts the
    //shoot coroutine. If there is an Intruder Alert currently in progress, alerts the guards on the response team of the 
    //player's location.
    protected virtual void FoundPlayer()
    {

        agent.SetDestination(lastKnownPlayerLocation);
        agent.stoppingDistance = combatStoppingDistance;

        anim.SetBool("Aiming", true);
        weapon_R.transform.LookAt(GameManager.instance.player.transform.position + new Vector3(0f, 1f, 0f));

        if (!isShooting && !isDead)
            StartCoroutine(shoot(ammoType));

        if (IntruderAlertManager.instance.GetIntruderAlert())
            IntruderAlertManager.instance.FoundTheIntruder(lastKnownPlayerLocation);
    }

    //Sets Shoot animation trigger and waits for the configured shootRate number of seconds.
    protected virtual IEnumerator shoot(GameObject ammoType)
    {
        anim.SetTrigger("Shoot");

        isShooting = true;

        Vector3 offset = new Vector3(Random.Range(-aimOffset, aimOffset), 0f, Random.Range(aimOffset, aimOffset));
        playerDirection = GameManager.instance.player.transform.position - shootPos.position;

        RaycastHit hit;
        if (Physics.Raycast(shootPos.position, playerDirection + offset, out hit))
            if (hit.collider.gameObject.CompareTag("Player"))
                GameManager.instance.player.GetComponent<IDamage>().takeDamage(ammoType.GetComponent<damage>().GetDamageAmount());
                
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    //Instantiates a bullet when called by the shooting animation event.
    private void CreateBullet()
    {
        Instantiate(ammoType, shootPos.position, transform.rotation);
    }

    //When enemy is shot, they note the player's current location, update their alert status,
    //alerts nearby allies, flash yellow, and applies the passed damage amount. If their health drops to or below 0,
    //calls the death function.
    public void takeDamage(float amount)
    {
        HP -= amount;
        isTakingDamage = true;

        if (HP <= 0 && !isDead)
        {
            isDead = true;

            Death();
        }
        else 
        {
            lastKnownPlayerLocation = GameManager.instance.player.transform.position;

            if (!isAlerted)
                AlertEnemy();
            AlertAllies();
            StartCoroutine(flashYellow());

            if (!playerSpotted && !isAlerted )
            {
                if(!audioPlayer.isPlaying)
                    audioPlayer.PlayOneShot(foundPlayer, 0.75f);
                playerSpotted = true;
            }
        }
    }

    


    //If the enemy is shot in their weakspot, passes the critical damage amount to the regular take damage function
    //and flashes the enemy model red.
    public void criticalHit(float amount)
    {
        takeDamage(amount);
        StartCoroutine(flashRed());
    }


    //Death operations common to all enemies when they die. Sets their isDead bool to true, stops their NavMeshAgent, sets all
    //player detection bools to false, deactivates their AlertStatus child object, health bar and stealth meter, and instantiates
    //their death particle effects.
    protected void DeathShared()
    {
        isDead = true;
        agent.isStopped = true;
        anim.SetBool("isDead", true);

        if (isEndgameEnemy)
            defaultPost.GetComponent<EndgameEnemySpawnPoint>().RemoveFromEnemySpawnedList(gameObject);

        playerInRange = false;
        playerInView = false;
        isAlerted = false;
        transform.GetChild(0).gameObject.SetActive(false);
        enemyHPBar.SetActive(false);
        playerDetectionCircle.SetActive(false);
        playerInViewIndicator.SetActive(false);
        GetComponent<CapsuleCollider>().enabled = false;
        weakspot.SetActive(false);

        Instantiate(DeathVFX, DeathFXPos.position, Quaternion.identity);
    }


    //Standard death function to despawn the enemy but is overriden by individual AI scripts.
    protected virtual void Death()
    {
        Destroy(gameObject);
    }


    //Starts timer before despawning enemy corpse so they don't disappear right away.
    protected IEnumerator DespawnDeadRobot(GameObject robot)
    {
        yield return new WaitForSeconds(20f);

        Destroy(robot);
    }


    //Checks if enemy has lost health.
    protected bool isPlayerTarget()
    {
        if (HP < HPOrig)
            return true;
        return false;
    }


    //Makes enemy health bar visible and adjusts it to reflect current health. Rotates health bar towards player.
    public void UpdateEnemyUI()
    {
        enemyHPBar.SetActive(true);
        enemyHPBarFill.fillAmount = HP / HPOrig;
        enemyHPBar.transform.parent.rotation = Camera.main.transform.rotation;
    }

    ////////////////////////////////////////
    ///          ALERT STATUS            ///
    ///////////////////////////////////////


    //Fills their stealth meter, changes the tag on their AlertStatus object to alerted to allow tracking by the LevelManager,
    //changes their speed to their configured combat speed, and toggles their onDuty boo
    public virtual void AlertEnemy()
    {
        RevertDetectionRadius();
        isAlerted = true;
        enemyDetectionLevel = enemyDetectionLevelOG;
        transform.GetChild(0).tag = "Alerted";
        agent.speed = combatSpeed;
        onDuty = false;
    }


    //Casts a sphere collider with their configured allyRadius ignoring anything that is not on their ally layer. For each ally
    //in the array, if they are not themselves and are not dead, passes them the player's location and calls their AlertFunciton.
    protected virtual void AlertAllies()
    {
        Collider[] alliesInRange = Physics.OverlapSphere(gameObject.transform.position, allyRadius, allyLayer);

        if (alliesInRange.Length > 0)
        {
            foreach (Collider ally in alliesInRange)
            {
                if (ally.gameObject.GetComponent<SharedEnemyAI>() != null)
                if (ally.GetComponent<SharedEnemyAI>().GetIsDead() == false && ally.gameObject != gameObject)
                {
                    ally.gameObject.GetComponent<SharedEnemyAI>().lastKnownPlayerLocation = lastKnownPlayerLocation;
                    
                    if(!ally.gameObject.GetComponent<SharedEnemyAI>().GetIsAlerted())
                        ally.gameObject.GetComponent<SharedEnemyAI>().AlertEnemy();
                }
            }
        }
    }

    //Returns all their detection variables to their idle state and returns to their post.
    public virtual void CalmEnemy()
    {
        enemyDetectionLevel = 0;
        isAlerted = false;
        transform.GetChild(0).tag = "Idle";
        ReturnToPost();
        agent.speed = idleSpeed;
        onDuty = true;
        currentIdleSoundCooldown = Random.Range(0, maxIdleSoundCooldown);
        playerSpotted = false;
    }

    //Travels to their default post.
    protected virtual void ReturnToPost()
    { 

        onDuty = true;

        if (defaultPost != null)
            agent.SetDestination(defaultPost.transform.position);
    }


    ////////////////////////////////////////
    ///              AUDIO               ///
    ///////////////////////////////////////


    //Randomly selects an audio clip from their list of shoot sounds and plays it from their weapon's audio source.
    protected virtual void playShootSound()
    {
        int playTrack = Random.Range(0,shootSounds.Count);

        weapon_R.GetComponent<AudioSource>().PlayOneShot(shootSounds[playTrack]);
    }


    //Randomly selects an audio clip from their list of footstep sounds and plays it from their audio source.
    protected virtual void playFootstepSound()
    {
        if (!isDead)
        {
            int playTrack = Random.Range(0, footsteps.Count);

            audioPlayer.PlayOneShot(footsteps[playTrack], 1f);
        }
    }

    protected IEnumerator playIdleSound()
    {
        readyToSpeak = false;

        if (currentIdleSoundCooldown > 0)
            yield return new WaitForSeconds(currentIdleSoundCooldown);

        currentIdleSoundCooldown = Random.Range(0, maxIdleSoundCooldown);

        int playTrack = Random.Range(0, idleSounds.Count);

        audioPlayer.PlayOneShot(idleSounds[playTrack]);

        yield return new WaitForSeconds(currentIdleSoundCooldown);

        //currentIdleSoundCooldown = 0;

        readyToSpeak = true;
    }


    public virtual void XrayEnemy(GameObject enemy,bool xrayApplied)
    {

        if(xrayApplied)
            enemy.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material = xrayMaterial;
        else
            enemy.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material = originalMaterial;

    }

    protected void SetPlayerCrouchedDetectionRadius()
    {
        GetComponent<SphereCollider>().radius = detectionRadiusPlayerCrouched;
    }

    protected void RevertDetectionRadius()
    {
        GetComponent<SphereCollider>().radius = originalDetectionRadius;
    }


    ////////////////////////////////////////
    ///          GETTERS/SETTERS         ///
    ///////////////////////////////////////

    public GameObject GetEnemyHealthBar() { return enemyHPBar; }

public GameObject GetDefaultPost() { return defaultPost; }

public enemyType GetEnemyType() { return enemy_Type; }

public bool GetIsDead() { return isDead; }

public void SetDefaultPost(GameObject post) { defaultPost = post; }

public bool CheckIfOnDuty() { return onDuty; }

public Vector3 GetLastKnownPlayerLocation() { return lastKnownPlayerLocation; }

public void SetLastKnownPlayerLocation(Vector3 location) { lastKnownPlayerLocation = location; }

public void SetCurrentDestination(GameObject destination) { currentDestination = destination; }

public GameObject GetCurrentDestination() { return currentDestination; }

public float GetHP() { return HP; }
public void SetHP(float value) { HP = value; }

public float GetMaxHP() { return HPOrig; }

public void SetMaxHP(float hp) { HPOrig = hp; }


public float GetShootRate() { return shootRate; }

public void SetShootRate(float value) { shootRate = value; }

public void SetInOuterRange(bool status) { playerInOuterRange = status; }

public float GetRotationSpeed() { return rotationSpeed; }

public bool GetIsAlerted() { return isAlerted; }    

public void SetIsEndgameEnemy(bool status) {  isEndgameEnemy = status; }

public bool GetIsRespondingToAlert() { return isRespondingToAlert; }

public void SetIsRespondingToAlert(bool status) { isRespondingToAlert = status; }

public bool GetIsSearching() { return isSearching; }

public void SetIsSearching(bool status) { isSearching = status; }

public bool GetLoadedFromState() { return loadedFromState; }

public void SetLoadedFromState(bool status) {  loadedFromState = status; }

public bool GetIsEndGameEnemy() { return isEndgameEnemy; } 

public void SetIsEndGameEnemy(bool status) { isEndgameEnemy= status; }

public Material GetXrayMaterial() { return xrayMaterial; }

public Material GetOriginalMaterial() { return originalMaterial; }

public void SetInCrouchRadius(bool status) { inCrouchRadius = status;  }


}
