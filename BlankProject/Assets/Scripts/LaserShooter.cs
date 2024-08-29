using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LaserShooter : MonoBehaviour, IDamage
{
    
    [SerializeField] Renderer model;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;
    

    [SerializeField] int HP;
    [SerializeField] int viewAngle;
    [SerializeField] int facePlayerSpeed;


    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] float shootAngle;

    bool isShooting;
    bool playerInRange;
   
    float angleToPlayer;
    float stoppingDistanceOrig;

    Vector3 playerDir;
  

    Color colorOrig;

    // Start is called before the first frame update
    void Start()
    {
        colorOrig = model.material.color;
       
        
    }

    // Update is called once per frame
    void Update()
    {
        canSeePlayer();
    }


    bool canSeePlayer()
    {
        playerDir = GameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.Log(angleToPlayer);
        Debug.DrawRay(headPos.position, playerDir);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewAngle)
            {

                if (!isShooting && angleToPlayer <= shootAngle)
                {
                    facePlayer();
                    StartCoroutine(shoot());
                }
                return true;
            }
        }
        
        return false;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());

        if (HP <= 0)
        {
        
            Destroy(gameObject);
        }
    }


    void facePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * facePlayerSpeed);
    }
    

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    IEnumerator shoot()
    {
        isShooting = true;
        createBullet();
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    public void createBullet()
    {
        Instantiate(bullet, shootPos.position, transform.rotation);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
        }
    }

    public void criticalHit(int amount)
    {
        throw new System.NotImplementedException();
    }
}
