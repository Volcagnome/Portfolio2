using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerDamage : MonoBehaviour, IDamage
{
    [SerializeField] LayerMask ignoreMask;
    [SerializeField] List<pickupStats> weapons;

    [SerializeField] float HP;
    [SerializeField] float HPRegenRate;
    [SerializeField] float HPRegenWaitTime;

    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] float shootDist;
    [SerializeField] float interactDist;
    [SerializeField] int dmgMultiplier;

    Coroutine regenCoroutine;

    float hpOG;

    int bulletUpgradeTotal;
    float maxAmmoMultiplier;

    bool isShooting;
    bool isInteracting;
    bool isTakingDamage;


    // Start is called before the first frame update
    void Start()
    {
        // Sets original starting stats:
        hpOG = HP;
        adjustHPBar();
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
                    dmg.criticalHit((shootDamage + bulletUpgradeTotal) * dmgMultiplier);
                }

                else
                {
                    dmg.takeDamage(shootDamage + bulletUpgradeTotal);
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

    public void setWeapon(pickupStats weapon)
    {
        shootRate = weapon.shootRate;
        shootDamage = weapon.shootDamage;
        shootDist = weapon.shootDist;
        dmgMultiplier = weapon.dmgMultiplier;
    }

    public void addWeapon(pickupStats weapon)
    {
        weapons.Add(weapon);
        setWeapon(weapon);
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

    //Getters and setters
    public float getHP() { return HP; }
    public void setHP(float value) { HP = value; }

    public float getMaxHP() { return hpOG; }
    public void setMaxHP(float value) { hpOG = value; }

    public float getStamina() { return stamina; }
    public void setStamina(float value) { stamina = value; }

    public float getMaxStamina() { return staminaOG; }
    public void setMaxStamina(float value) { staminaOG = value; }

    public int getBulletUpgrades() { return bulletUpgradeTotal; }
    public void setBulletUpgrades(int value) { bulletUpgradeTotal = value; }

    public float getAmmoMultiplier() { return maxAmmoMultiplier; }
    public void setAmmoMultiplier(float value) { maxAmmoMultiplier = value; }

    public int getPlayerSpeed() { return speed; }
    public void setPlayerSpeed(int value) { speed = value; }
}
