using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static enemyAI;

public class SharedEnemyAI : MonoBehaviour
{
    
    //Components
    [SerializeField] protected Transform headPos;
    [SerializeField] protected Renderer model;
    [SerializeField] protected GameObject defaultPost;
    [SerializeField] protected GameObject currentDestination;
    [SerializeField] protected NavMeshAgent agent;
    

    //PlayerDetection
    [SerializeField] protected float FOV_Angle;
    [SerializeField] protected LayerMask targetMask;
    [SerializeField] protected float rotationSpeed;
    protected Vector3 playerDirection;
    protected Vector3 lastKnownPlayerLocation;
    protected bool playerInRange;
    protected bool playerInView;


    //Current State
    protected bool isAlerted;
    protected bool onDuty;
    protected bool isShooting;
    protected Color colorOrig;

    //Stats

    [SerializeField] public enum behaviorType { none, guard, patrol, guardReinforcement };
    [SerializeField] public enum enemyType { none, Guard, Patrol, Titan, Turret, Boss };
    [SerializeField] protected behaviorType enemyBehavior;
    [SerializeField] protected enemyType enemy_Type;
    [SerializeField] protected float HP;
    [SerializeField] protected float HPRegenRate;
    [SerializeField] protected float HPRegenWaitTime;
    [SerializeField] protected float shootRate;
    [SerializeField] protected float combatSpeed;
    [SerializeField] protected float combatStoppingDistance;
    [SerializeField] protected float idleStoppingDistance;
    [SerializeField] protected float idleSpeed;

    //Ally Detection
    [SerializeField] protected int allyRadius;
    [SerializeField] protected LayerMask allyLayer;
    protected GameObject[] alliesInRange;




    // Start is called before the first frame update
    void Start()
    {
        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;
        isAlerted = false;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            StartCoroutine(FOVRoutine());
        }
        else
            return;
    }

    //When player exits detection range notes their last known location and toggles playerInRange and isAlerted bools
    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInView = false;

            if (isAlerted)
                lastKnownPlayerLocation = GameManager.instance.player.transform.position;
        }
        else
            return;
    }


    private IEnumerator FOVRoutine()
    {
        while (playerInRange)
        {
            yield return new WaitForSeconds(0.05f);
            FieldOfViewCheck();
        }
    }

    protected bool FieldOfViewCheck()
    {
        bool result;

        playerDirection = GameManager.instance.player.transform.position - headPos.position;
        float angleToPlayer = Vector3.Angle(playerDirection, transform.forward);
        if (playerInRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(headPos.position, playerDirection, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Player") && angleToPlayer <= FOV_Angle)
                    result = true;

                else result = false;

            }
            else result = false;
        }
        else result = false;

        playerInView = result;

        return result;
    }


    protected IEnumerator PursuePlayer()
    {
        agent.SetDestination(lastKnownPlayerLocation);

        if (agent.remainingDistance <= 0.3f || !agent.hasPath)
        {
            yield return new WaitForSeconds(1.5f);
            CalmEnemy();
        }
    }

    protected void RotateToPlayer()
    { 
        playerDirection = GameManager.instance.player.transform.position - transform.position;

        Quaternion rotationToPlayer = Quaternion.LookRotation(playerDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotationToPlayer, Time.deltaTime * rotationSpeed);

    }

    
    protected IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    protected IEnumerator flashYellow()
    {
        model.material.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }


    protected virtual void ReturnToPost()
    {
        onDuty = true;
        agent.SetDestination(defaultPost.transform.position);   
    }

    protected virtual void AlertEnemy()
    {
        isAlerted = true;
        lastKnownPlayerLocation = GameManager.instance.player.transform.position;
        agent.speed = combatSpeed;
        onDuty = false;
        transform.GetChild(2).tag = "Alerted";
    }

    public virtual void CalmEnemy()
    {
        isAlerted = false;
        ReturnToPost();
        agent.speed = idleSpeed;
        onDuty = true;
        transform.GetChild(2).tag = "Idle";
    }

    protected virtual void AlertAllies()
    {
        Collider[] alliesInRange = Physics.OverlapSphere(gameObject.transform.position, allyRadius, allyLayer);

        if (alliesInRange.Length > 0)
        {
            foreach (Collider ally in alliesInRange)
            {
                    ally.gameObject.GetComponent<SharedEnemyAI>().lastKnownPlayerLocation = lastKnownPlayerLocation;
                    ally.gameObject.GetComponent<SharedEnemyAI>().AlertEnemy();
            }
        }
    }
    public GameObject GetDefaultPost() { return defaultPost; }

    public enemyType GetEnemyType() { return enemy_Type; }

    public void SetDefaultPost(GameObject post) { defaultPost = post; }

    public bool CheckIfOnDuty() { return onDuty; }

    public void SetIsOnDuty(bool status) { onDuty = status; }

    public Vector3 GetLastKnownPlayerLocation() { return lastKnownPlayerLocation; }
    public void SetCurrentDestination(GameObject destination) { currentDestination = destination; }
    public GameObject GetCurrentDestination() { return currentDestination; }
}
