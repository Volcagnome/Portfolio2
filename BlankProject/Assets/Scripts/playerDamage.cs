using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerDamage : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreMask;

    [SerializeField] float HP;
    [SerializeField] float HPRegenRate;
    [SerializeField] float HPRegenWaitTime;

    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;
    [SerializeField] float interactDist;
    [SerializeField] int dmgMultiplier;

    Coroutine regenCoroutine;

    float hpOG;

    bool isShooting;
    bool isInteracting;
    bool isTakingDamage;


    // Start is called before the first frame update
    void Start()
    {
        // Sets original starting stats:
        hpOG = HP;
        adjustHPBar();
        spawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        aiming();

        if (isEnemyTarget())
        {
            adjustHPBar();

            if (!isTakingDamage)
                RegenerateHealth();
        }
    }

    void aiming()
    {
        //////////////////////
        // *** SHOOTING *** //
        //////////////////////

        if (Input.GetButton("Shoot") && !isShooting)
            StartCoroutine(shoot());

        /////////////////////////
        // *** INTERACTING *** //
        /////////////////////////

        if (Input.GetButtonDown("Interact"))
        {
            interact();
        }

    }


    ////////////////////////////////////////////////////
    // *** HEALTH, DAMAGE AND INTERACTING METHODS *** //
    ////////////////////////////////////////////////////

    IEnumerator shoot()
    {
        isShooting = true;

        RaycastHit hit;
        // Physics.Raycast (Origin, Direction, hit info, max distance)
        if (Physics.Raycast(Camera.main.transform.position,
            Camera.main.transform.forward, out hit, shootDist, ~ignoreMask))
        {
            Debug.Log(hit.collider.name);
            IDamage dmg = hit.collider.gameObject.GetComponentInParent<IDamage>();

            if (dmg != null)
            {
                if (hit.collider.CompareTag("WeakSpot"))
                {
                    dmg.criticalHit(shootDamage * dmgMultiplier);
                }

                else
                {
                    dmg.takeDamage(shootDamage);
                }
            }
        }

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    void interact()
    {
        RaycastHit hit;
        // Physics.Raycast (Origin, Direction, hit info, max distance)
        if (Physics.Raycast(Camera.main.transform.position,
            Camera.main.transform.forward, out hit, interactDist, ~ignoreMask))
        {
            Debug.Log(hit.collider.name);
            IInteract interactWith = hit.collider.GetComponent<IInteract>();

            if (interactWith != null)
            {
                interactWith.interact();
            }
        }

    }

    bool isEnemyTarget()
    {
        if (HP < hpOG)
            return true;
        return false;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        isTakingDamage = true;

        adjustHPBar();
        StartCoroutine(flashRed());

        //Im dead
        if (HP <= 0)
        {
            GameManager.instance.youLose();
        }

        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
        regenCoroutine = StartCoroutine(EnableHealthRegen());
    }

    void RegenerateHealth()
    {
        //Debug.Log("Regenerating player health: " + HPRegenRate * Time.deltaTime);
        HP += HPRegenRate * Time.deltaTime;

        if (HP > hpOG)
        {
            HP = hpOG;
        }
    }

    IEnumerator EnableHealthRegen()
    {
        yield return new WaitForSeconds(HPRegenWaitTime);
        isTakingDamage = false;
        regenCoroutine = null;
    }

    public void criticalHit(int amount)
    {

    }

    /////////////////////////
    // *** HUD METHODS *** //
    /////////////////////////

    void adjustHPBar()
    {
        GameManager.instance.healthbar.fillAmount = HP / hpOG;
    }

    IEnumerator flashRed()
    {
        GameManager.instance.redFlash.SetActive(true);
        yield return new WaitForSeconds(.1F);
        GameManager.instance.redFlash.SetActive(false);
    }

    public void spawnPlayer()
    {
        HP = hpOG;
        adjustHPBar();
        controller.enabled = false;
        transform.position = GameManager.instance.playerSpawn.transform.position;
        controller.enabled = true;
    }
}
