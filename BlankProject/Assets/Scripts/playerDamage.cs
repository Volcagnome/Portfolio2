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
    [SerializeField] pickupStats defaultWeapon;
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
    public ParticleSystem overheatSmokeFX;
     ParticleSystem currentSmoke;
    [SerializeField] float glowIntesityMultiplier;

    [Header("----- Damage / Interacting -----")]
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] float shootDist;
    [SerializeField] int dmgMultiplier;
    [SerializeField] float interactDist;
    [SerializeField] int maxHeat;
    [SerializeField] float heatPerShot;
    [SerializeField] float coolRate;
    [SerializeField] float coolWaitTime;

    Coroutine cooldownCoroutine;
    [Header("----- Weapon Sounds -----")]
    [SerializeField] AudioClip pistolSound;
    [SerializeField] float PistolVol;

    [SerializeField] AudioClip rifleSound;
    [SerializeField] float rifleVol;

    [SerializeField] AudioClip shotgunSound;
    [SerializeField] float shotgunVol;

    [SerializeField] AudioClip switchGunSound;
    [SerializeField] float switchGunVol;

    [SerializeField] AudioClip overheatSound;
    [SerializeField] float overheatVol;

    [Header("----- Health Sounds -----")]
    [SerializeField] AudioClip audioLowHP;
    [SerializeField] float lowHPVol;

    Coroutine regenCoroutine;

    float hpOG;
    float currentHeat;

    int bulletUpgradeTotal;
    float maxAmmoMultiplier;

    bool isFlashOn;
    bool isShooting;
    bool isInteracting;
    bool isTakingDamage;
    bool canCool;
    bool usedToMax;
    bool isShotgun;
    bool isEmittingSmoke;
    bool isLowHp;

    // Start is called before the first frame update
    //Sets the player's health and max health stats pulled from the StaticPlayerData script.
    void Start()
    {
        //Sets original starting stats:
        //hpOG = HP;
        HP = StaticPlayerData.playerHealth;
        hpOG = StaticPlayerData.playerMaxHealth;
        isLowHp = false;

        adjustHPBar();
        adjustOverheatMeter();
        adjustGlow();
        if (weapons.Count == 0) addWeapon(defaultWeapon);
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
            if (cooldownCoroutine == null) coolWeapon();
            if (usedToMax && !isEmittingSmoke) StartCoroutine(emitSmoke());
        }
    }

    // Contains GetButton methods that may be called any update:
    void aiming()
    {
        // Listen for shooting, interacting or flashlight:

        if (Input.GetButton("Shoot") && weapons.Count > 0 && maxHeat >= currentHeat + heatPerShot && !usedToMax && !isShooting)
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
        if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
        StartCoroutine(flashMuzzle());
        cooldownCoroutine = StartCoroutine(enableCooling());
        // Spawns a tracer round (playerBullet prefab)
        Instantiate(bulletPrefab, shootPos.position, shootPos.transform.rotation);
        currentHeat += heatPerShot;
        

        if (maxHeat < currentHeat + heatPerShot)
        {
            usedToMax = true;
            GameManager.instance.playAud(overheatSound, overheatVol);
        }

        adjustOverheatMeter();
        adjustGlow();

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

        if (isShotgun)
        {
            for (int currentPellet = 0; currentPellet < 9; ++currentPellet)
            {
                Vector3 offset = Vector3.zero; offset.x = Random.Range(-.1F, .1F); offset.y = Random.Range(-.1F, .1F); offset.z = Random.Range(-.1F, .1F); 
                RaycastHit pellet;

                Vector3 linePoint = Camera.main.transform.position; linePoint.y += .01F;
                

                if (Physics.Raycast(Camera.main.transform.position,
                    Camera.main.transform.forward + offset, out pellet, shootDist, ~ignoreMask))
                {
                   
                    IDamage dmg = pellet.collider.gameObject.GetComponentInParent<IDamage>();

                    if (dmg != null)
                    {
                        if (pellet.collider.CompareTag("WeakSpot"))
                        {
                            dmg.criticalHit((shootDamage + bulletUpgradeTotal) * dmgMultiplier);
                            Instantiate(botCritFX, pellet.point, Quaternion.identity);
                        }

                        else
                        {
                            dmg.takeDamage(shootDamage);
                            Instantiate(botDmgFX, pellet.point, Quaternion.identity);
                        }
                    }

                    // This prevents our particles from instantiating on enemies, but putting the
                    // function outside the else statement will cause it to happen always.
                    else
                    {
                        // Instantiates a bullet hole effect when impacting a surface.
                        Instantiate(bulletHoleFX, pellet.point, Quaternion.identity);
                    }
                }
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
            // Play weapon switch sound:
            playSwitchGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedGun > 0)
        {
            selectedGun--;
            setWeapon(weapons[selectedGun]);
            // Play weapon switch sound:
            playSwitchGun();
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
                // If that index isn't already the selected gun:
                if (tempIndex-1 != selectedGun)
                {
                    // Update index:
                    selectedGun = tempIndex - 1;
                    // Set to that weapon.
                    setWeapon(weapons[selectedGun]);
                    // Play weapon switch sound:
                    playSwitchGun();
                }
            }
        }
    }

    // Sounds for weapon switching:
    void playSwitchGun()
    {
        GameManager.instance.playAud(switchGunSound, switchGunVol);
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

        // Play low HP alert if current HP is equal to:
        if (HP <= (hpOG / 3) && !isLowHp)
        {
            GameManager.instance.playAud(audioLowHP, lowHPVol);
            isLowHp = true;
        }
    }

    void RegenerateHealth()
    {
        //Debug.Log("Regenerating player health: " + HPRegenRate * Time.deltaTime);
        HP += HPRegenRate * Time.deltaTime;

        // If health goes above og hp value, reduce to the og hp value:
        if (HP > hpOG)
        {
            HP = hpOG;
        }

        // If hp is Higher than a third of the og hp, player is no longer at low health:
        if (HP > (hpOG/3))
        {
            isLowHp = false;
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
        float heatHolder = maxHeat;

        weapon.modelRotationAxis.Normalize();
        shootRate = weapon.shootRate;
        shootDamage = weapon.shootDamage;
        shootDist = weapon.shootDist;
        dmgMultiplier = weapon.dmgMultiplier;
        maxHeat = weapon.maxHeat;
        heatPerShot = weapon.heatPerShot;
        coolRate = weapon.coolRate;
        coolWaitTime = weapon.coolWaitTime;
        isShotgun = weapon.shotgun;

        currentHeat *= maxHeat / heatHolder;

        muzzleFlash.transform.SetParent(GameManager.instance.player.transform, true);
        flashlight.transform.SetParent(GameManager.instance.player.transform, true);

        //gunModel.transform.rotation = Quaternion.identity;
        gunModel.transform.localRotation = Quaternion.AngleAxis(weapon.rotationAngle, weapon.modelRotationAxis);
        gunModel.transform.localScale = Vector3.one * weapon.modelScale;

        muzzleFlash.transform.SetParent(gunModel.transform, true);
        flashlight.transform.SetParent(gunModel.transform, true);

        gunModel.GetComponent<MeshFilter>().sharedMesh = weapon.itemModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = weapon.itemModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

    public void addWeapon(pickupStats weapon)
    {
        currentHeat = 0;
        weapons.Add(weapon);
        setWeapon(weapon);
    }

    void coolWeapon()
    {
        currentHeat -= coolRate * Time.deltaTime;

        if (currentHeat < 0) { currentHeat = 0; }
        if (currentHeat == 0 && usedToMax) usedToMax = false;

        adjustGlow();
        adjustOverheatMeter();
    }

    IEnumerator enableCooling()
    {
        canCool = false;
        yield return new WaitForSeconds(coolWaitTime);
        canCool = true;
        cooldownCoroutine = null;
    }

    void adjustGlow()
    {
        controller.GetComponentInParent<Light>().intensity = glowIntesityMultiplier * currentHeat / maxHeat;

    }

    IEnumerator emitSmoke()
    {
        isEmittingSmoke = true;
        ParticleSystem currentSmoke = Instantiate(overheatSmokeFX, gunModel.transform.position + (Vector3.up * .25F), Quaternion.identity);
        yield return new WaitForSeconds(3);
        Destroy(currentSmoke);
        isEmittingSmoke = false; 
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

    void adjustOverheatMeter()
    {
        GameManager.instance.overheatMeter.fillAmount = currentHeat / maxHeat;
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
        //transform.position = GameManager.instance.playerSpawn.transform.position;
        transform.position = GameManager.instance.currentSpawn.transform.position;
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
