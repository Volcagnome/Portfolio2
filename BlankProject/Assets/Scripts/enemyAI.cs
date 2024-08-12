using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{

    //Basic Components
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;

    //Basic stats
    [SerializeField] int HP;
    [SerializeField] float shootRate;

    //Player detection
    [SerializeField] public float viewRadius;
    [SerializeField] public float FOV_Angle;
    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask obstructionMask;
    [SerializeField] Texture emissionOrig;
    [SerializeField] Texture emissionAlerted;
    public bool playerInView;
    private bool playerInRange;
    private bool isAlerted;
    
    private bool isShooting;
    

    //Original color to return to after flashing red
    Color colorOrig;

    // Start is called before the first frame update
    void Start()
    {
        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerInRange)
        {

            if (playerInView)
            {
                isAlerted = true;
            }
        }

        if(isAlerted)
        {
            var playerDirection = GameManager.instance.player.transform.position - transform.position;
            gameObject.GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_EmissionMap", emissionAlerted);
            playerDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(playerDirection);
            agent.SetDestination(GameManager.instance.player.transform.position);
            if (playerInView && !isShooting)
                StartCoroutine(shoot());
        }
        else
        {
            gameObject.GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_EmissionMap", emissionOrig);
        }
    }

    IEnumerator shoot()
    {
        isShooting = true;
        Instantiate(bullet, shootPos.position, transform.rotation);

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    //When enemy is damaged 
    public void takeDamage(int amount)
    {
        HP -= amount;

        isAlerted = true;
        StartCoroutine(flashRed());

        if (HP <= 0)
        {
            Destroy(gameObject);
        }
    }

    //When player enters detection range
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(FOVRoutine());

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    //When player exits detection range
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            isAlerted = false;
        }
    }

    //Enemy model flashes red when hit
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    //While player is in range, calls function to check if in line of sight
    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
            if (playerInRange == false)
                yield break;
        }
    }

    //Checks to see if player in field of view
    private void FieldOfViewCheck()
    {
        Vector3 directionToTarget = (GameManager.instance.player.transform.position - transform.position);

        if (playerInRange)
        {
            if (Vector3.Angle(transform.forward, directionToTarget) < FOV_Angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    playerInView = true;
                else
                    playerInView = false;
            }
            else
                playerInView = false;

        }
        else if (playerInView)
            playerInView = false;
    }

}
