using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static enemyAI;

public class securityCamera : MonoBehaviour, IDamage, IToggle
{
    [SerializeField] GameObject cameraHead;
    [SerializeField] GameObject controller;
    [SerializeField] GameObject searchPoint;
    [SerializeField] float alarmRadius;
    [SerializeField] Camera referenceCamera;

    bool isActive;

    //Taken to potentially fix a bug
    private GameObject[] alliesInRange;

    //Fields lifted directly from Lynsey's enemyAI script
    [SerializeField] float FOV;
    [SerializeField] LayerMask enemyLayer;
    public bool playerInView;


    void Start()
    {
        if (controller != null) isActive = controller.GetComponent<ISendState>().getState();
    }

    
    void Update()
    {
        if (cameraHead != null)
        {
            if (controller != null && controller.GetComponent<ISendState>().getState() != isActive) toggle(controller.GetComponent<ISendState>().getState());

            if (playerInView) AlertEnemies();

            CheckAngle();
        }
        else if (playerInView) playerInView = false;
    }
    public void toggle(bool state)
    {
        isActive = state;
    }

    public void takeDamage(float dmg)
    {
        if (cameraHead != null) Destroy(cameraHead);
        playerInView = false;
    }
    public void criticalHit(float dmg)
    {
        takeDamage(dmg);
    }

    //Copied directly from another chunk of Lynsey's code becuase I'm too lazy to be original here
    public void AlertEnemies()
    {
        Collider[] alliesInRange = Physics.OverlapSphere(searchPoint.transform.position, alarmRadius, enemyLayer);

        if (alliesInRange.Length > 0)
        {
            foreach (Collider ally in alliesInRange)
            {
                ally.gameObject.GetComponent<SharedEnemyAI>().AlertEnemy();
            }
        }
    }
    //Taken from lecture code
    void CheckAngle()
    {
        Vector3 playerDirection = GameManager.instance.player.transform.position - searchPoint.transform.position;
        float angleToPlayer = Vector3.Angle(playerDirection, searchPoint.transform.forward);

        

        RaycastHit hit;
        if (Physics.Raycast(searchPoint.transform.position, playerDirection, out hit, alarmRadius, ~enemyLayer))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= FOV)
            {
                playerInView = true;
            }
            else playerInView = false;
        }
        else playerInView = false;
    }
}
