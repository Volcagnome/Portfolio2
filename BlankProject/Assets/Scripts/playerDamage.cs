using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerDamage : MonoBehaviour, IDamage
{
    [Header("----- Controller -----")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreMask;

    [Header("----- Weapons -----")]
    [SerializeField] List<pickupStats> weapons;
    [SerializeField] GameObject gunModel;
    [SerializeField] GameObject flashlight;
    [SerializeField] GameObject muzzleFlash;

    [Header("----- Hit Points (HP) -----")]
    [SerializeField] float HP;
    [SerializeField] float HPRegenRate;
    [SerializeField] float HPRegenWaitTime;

    [Header("----- Particle Effects -----")]
    // Particle effects for dealing damage
    public ParticleSystem botDmgFX;
    public ParticleSystem botCritFX;
    public ParticleSystem bulletHoleFX;

    [Header("----- Damage / Interacting -----")]
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] float shootDist;
    [SerializeField] int dmgMultiplier;
    [SerializeField] float interactDist;

    Coroutine regenCoroutine;

    float hpOG;

    int bulletUpgradeTotal;
    float maxAmmoMultiplier;

    bool isFlashOn;
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

    // Contains GetButton methods that may be called any update:
    void aiming()
    {
        // Listen for shooting, interacting or flashlight:

        if (Input.GetButton("Shoot") && !isShooting)
            StartCoroutine(shoot());

        if (Input.GetButtonDown("Interact"))
        {
            interact();
        }

        useFlashlight();

    }

    ////////////////////////////////////////////////////
    // *** HEALTH, DAMAGE AND INTERACTING METHODS *** //
    ////////////////////////////////////////////////////

    // *** SHOOTING *** //
    IEnumerator shoot()
    {
        isShooting = true;
        StartCoroutine(flashMuzzle());

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
                    Instantiate(botCritFX, hit.point, Quaternion.identity);
                }

                else
                {
                    dmg.takeDamage(shootDamage);
                    Instantiate(botDmgFX, hit.point, Quaternion.identity);
                }
            }

            // This prevents our particles from instantiating on enemies, but putting the
            // function outside the else statement will cause it to happen always.
            else
            {
                // Instantiates a bullet hole effect when impacting a surface.
                Instantiate(bulletHoleFX, hit.point, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    IEnumerator flashMuzzle()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        muzzleFlash.SetActive(false);
    }

    // *** INTERACTING *** //
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


   // *** HUD METHODS *** //
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

    public int getBulletUpgrades() { return bulletUpgradeTotal; }
    public void setBulletUpgrades(int value) { bulletUpgradeTotal = value; }

    public float getAmmoMultiplier() { return maxAmmoMultiplier; }
    public void setAmmoMultiplier(float value) { maxAmmoMultiplier = value; }

    // *** SPAWN *** //
    public void spawnPlayer()
    {
        HP = hpOG;
        adjustHPBar();
        controller.enabled = false;
        transform.position = GameManager.instance.playerSpawn.transform.position;
        controller.enabled = true;
    }

    // *** FLASHLIGHT *** //
    void useFlashlight()
    {
        if (Input.GetButtonDown("Flashlight"))
        {
            // Toggles flashlight on and off.
            isFlashOn = !isFlashOn;
        }

        if (isFlashOn == true)
            flashlight.SetActive(true);

        else
            flashlight.SetActive(false);
    }
}
