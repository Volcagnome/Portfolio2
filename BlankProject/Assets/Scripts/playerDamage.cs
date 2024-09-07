using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerDamage : MonoBehaviour, IDamage
{
    [Header("----- Controller -----")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreMask;

    [Header("----- Weapons -----")]
    int selectedGun;
    [SerializeField] List<pickupStats> weapons;
    [SerializeField] GameObject gunModel;
    [SerializeField] GameObject flashlight;
    [SerializeField] GameObject muzzleFlash;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform shootPos;

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
        if ( (!GameManager.instance.isPaused))
        {
            aiming();
            selectGun();

            if (isEnemyTarget())
            {
                adjustHPBar();

                if (!isTakingDamage)
                    RegenerateHealth();
            }
        }
    }

    // Contains GetButton methods that may be called any update:
    void aiming()
    {
        // Listen for shooting, interacting or flashlight:

        if (Input.GetButton("Shoot") && weapons.Count > 0 && !isShooting)
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
        // Spawns a tracer round (playerBullet prefab)
        Instantiate(bulletPrefab, shootPos.position, shootPos.transform.rotation);

        RaycastHit hit;
        // Physics.Raycast (Origin, Direction, hit info, max distance)
        if (Physics.Raycast(Camera.main.transform.position,
            Camera.main.transform.forward, out hit, shootDist, ~ignoreMask))
        {

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

    void selectGun()
    {
        // Mouse wheel switching:
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedGun < weapons.Count - 1)
        {
            selectedGun++;
            setWeapon(weapons[selectedGun]);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedGun > 0)
        {
            selectedGun--;
            setWeapon(weapons[selectedGun]);
        }

        // Number press targeted switching:
        // If a number is pressed (1-9), switch to the weapon in that slot in player's weapon list:
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3)
            || Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Alpha6)
            || Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Alpha9))
        {
            // Collect key press string:
            string pressedNum = Input.inputString;
            int tempIndex;
            // Parse to int into selectedGun (index):
            int.TryParse(pressedNum, out tempIndex);
            // If temp index is within the number of weapons in list...
            if (tempIndex <= weapons.Count)
            {
                // Update index:
                selectedGun = tempIndex-1;
                // Set to that weapon.
                setWeapon(weapons[selectedGun]);
            }
        }
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

    public void takeDamage(float amount)
    {
        HP -= amount;
        isTakingDamage = true;

        adjustHPBar();
        StartCoroutine(flashRed());

        // lose condition, player HP 0:
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

    public void criticalHit(float amount)
    {

    }

    public void setWeapon(pickupStats weapon)
    {
        shootRate = weapon.shootRate;
        shootDamage = weapon.shootDamage;
        shootDist = weapon.shootDist;
        dmgMultiplier = weapon.dmgMultiplier;
        gunModel.GetComponent<MeshFilter>().sharedMesh = weapon.itemModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial= weapon.itemModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

    public void addWeapon(pickupStats weapon)
    {
        weapons.Add(weapon);
        setWeapon(weapon);
    }


   // *** HUD METHODS *** //
    public void adjustHPBar()
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
