using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerDamage : MonoBehaviour, IDamage, IStatusEffect
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

    [Header("----- Status Effects -----")]
    [SerializeField] float burnDamage;
    [SerializeField] float burnTime;
    [SerializeField] float burnRate;
    bool isBurning;
    Coroutine burnCoroutine = null;

    [SerializeField] float bleedDamage;
    [SerializeField] float bleedTime;
    [SerializeField] float bleedRate;
    bool isBleeding;
    Coroutine bleedCoroutine = null;

    bool isShocked;

    [SerializeField] float stunTime;
    bool isStunned;
    Coroutine stunCoroutine = null;

    [Header("----- Particle Effects -----")]
    // Particle effects for dealing damage
    public ParticleSystem botDmgFX;
    public ParticleSystem botCritFX;
    public ParticleSystem bulletHoleFX;
    public ParticleSystem overheatSmokeFX;
     ParticleSystem currentSmoke;
    [SerializeField] Light weaponLight;
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
    [SerializeField] AudioClip switchGunSound;
    [SerializeField] float switchGunVol;

    [SerializeField] AudioClip overheatSound;
    [SerializeField] float overheatVol;

    [Header("----- Health Sounds -----")]
    [SerializeField] AudioClip audioLowHP;
    [SerializeField] float lowHPVol;

    Coroutine regenCoroutine;
    GameObject currentlyHighlighted;

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
        HP = StaticData.playerHealth;
        hpOG = StaticData.playerMaxHealth;
        isLowHp = false;

        adjustHPBar();
        adjustGlow();

        weapons = StaticData.playerWeaponsList;
        
        if (maxHeat <= 0) maxHeat = 1;

        if (weapons.Count == 0) addWeapon(defaultWeapon);

        selectedGun = StaticData.playerSelectedGun;
        setWeapon(weapons[selectedGun]);
        GameManager.instance.UpdateCurrentWeaponUI(selectedGun,weapons);
        GameManager.instance.UpdatePlayerStatsUI();

        currentlyHighlighted = null;

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

            if (isBurning) HP -= burnDamage * burnRate * Time.deltaTime;
            if (isBleeding) HP -= bleedDamage * bleedRate * Time.deltaTime;
        }
    }

    // Contains GetButton methods that may be called any update:
    void aiming()
    {
        // Listen for shooting, interacting or flashlight:
        if (!isStunned && !isShocked)
        {
            if (Input.GetButton("Shoot") && weapons.Count > 0 && maxHeat > currentHeat && !usedToMax && !isShooting)
                StartCoroutine(shoot());

            
            
            interact();
            

            useFlashlight();
        }
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
        // Play firing sound for selected weapon:
        GameManager.instance.playAud(weapons[selectedGun].fireSound, weapons[selectedGun].fireVol);

        currentHeat += heatPerShot;
        

        if (maxHeat <= currentHeat + heatPerShot)
        {
            usedToMax = true;
            currentHeat = maxHeat;
            GameManager.instance.playAud(overheatSound, overheatVol);
        }

        
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
                Quaternion tracerOffset = shootPos.transform.rotation; tracerOffset.x += offset.x; tracerOffset.y += offset.y; tracerOffset.z += offset.z;
                Instantiate(bulletPrefab, shootPos.position, tracerOffset);
                RaycastHit pellet;
                

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
            weapons[selectedGun].currentHeat = currentHeat;
            selectedGun++;
            setWeapon(weapons[selectedGun]);
            // Play weapon switch sound:
            playSwitchGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedGun > 0)
        {
            weapons[selectedGun].currentHeat = currentHeat;
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
                    weapons[selectedGun].currentHeat = currentHeat;
                    selectedGun = tempIndex - 1;
                    // Set to that weapon.
                    setWeapon(weapons[selectedGun]);
                    // Play weapon switch sound:
                    playSwitchGun();
                }
            }
        }

        GameManager.instance.UpdateCurrentWeaponUI(selectedGun, weapons);
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

            IInteract interactWith = hit.collider.GetComponent<IInteract>();

            if (interactWith != null)
            {
                if (hit.collider.gameObject != currentlyHighlighted)
                {
                    if (currentlyHighlighted != null) currentlyHighlighted.GetComponent<IInteract>().removeShader();

                    interactWith.applyShader();
                    currentlyHighlighted = hit.collider.gameObject;
                }

                if (Input.GetButtonDown("Interact"))
                    interactWith.interact();
            }
            else
            {
                if (currentlyHighlighted != null)
                {
                    currentlyHighlighted.GetComponent<IInteract>().removeShader();
                    currentlyHighlighted = null;
                }
            }
        }
        else
        {
            if (currentlyHighlighted != null)
            {
                currentlyHighlighted.GetComponent<IInteract>().removeShader();
                currentlyHighlighted = null;
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
        currentHeat = weapon.currentHeat;

        muzzleFlash.transform.SetParent(GameManager.instance.player.transform, true);
        flashlight.transform.SetParent(GameManager.instance.player.transform, true);
        shootPos.transform.SetParent(GameManager.instance.player.transform, true);

        gunModel.transform.localRotation = Quaternion.AngleAxis(weapon.rotationAngle, weapon.modelRotationAxis);
        gunModel.transform.localScale = Vector3.one * weapon.modelScale;

        muzzleFlash.transform.SetParent(gunModel.transform, true);
        flashlight.transform.SetParent(gunModel.transform, true);
        shootPos.transform.SetParent(gunModel.transform, true);

        gunModel.GetComponent<MeshFilter>().sharedMesh = weapon.itemModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = weapon.itemModel.GetComponent<MeshRenderer>().sharedMaterial;

        adjustGlow();
    }

    public void addWeapon(pickupStats weapon)
    {
        if (weapons.Count > 0) weapons[selectedGun].currentHeat = currentHeat;
        weapons.Add(weapon);
        setWeapon(weapon);
        selectedGun = weapons.IndexOf(weapon);
    }

    void coolWeapon()
    {
        currentHeat -= coolRate * Time.deltaTime;

        if (currentHeat < 0) { currentHeat = 0; }
        if (currentHeat == 0 && usedToMax) usedToMax = false;

        adjustGlow();
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
        weaponLight.intensity = glowIntesityMultiplier * currentHeat / maxHeat;
        gunModel.GetComponent<Light>().intensity = glowIntesityMultiplier * currentHeat / maxHeat;
        adjustOverheatMeter();
    }

    IEnumerator emitSmoke()
    {
        isEmittingSmoke = true;
        ParticleSystem currentSmoke = Instantiate(overheatSmokeFX, gunModel.transform.position + (Vector3.up * .25F), Quaternion.identity);
        yield return new WaitForSeconds(3);
        Destroy(currentSmoke);
        isEmittingSmoke = false; 
    }

    //Applying Status Effects
    public void bleed()
    {
        if (bleedCoroutine != null) StopCoroutine(bleedCoroutine);
        StartCoroutine(bleedRoutine());
    }

    IEnumerator bleedRoutine()
    {
        isBleeding = true;
        yield return new WaitForSeconds(bleedTime);
        isBleeding = false;
    }

    public void shock()
    {
        isShocked = true;
    }

    public void unshock()
    {
        isShocked = false;
    }

    public void burn()
    {
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
        StartCoroutine(burnRoutine());
    }

    IEnumerator burnRoutine()
    {
        isBurning = true;
        yield return new WaitForSeconds(burnTime);
        isBurning = false;
    }

    public void stun()
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        StartCoroutine(stunRoutine());
    }

    IEnumerator stunRoutine()
    {
        isStunned = true;
        float speedStorage = controller.gameObject.GetComponent<playerMovement>().getPlayerSpeedOG();
        controller.gameObject.GetComponent<playerMovement>().setPlayerSpeed(0);
        controller.gameObject.GetComponent<playerMovement>().SetPlayerSpeedOG(0);
        yield return new WaitForSeconds(stunTime);
        controller.gameObject.GetComponent<playerMovement>().SetPlayerSpeedOG(speedStorage);
        controller.gameObject.GetComponent<playerMovement>().setPlayerSpeed(speedStorage);
        isStunned = false;
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

    public bool getIsBurning() { return isBurning; }
    public void setIsBurning(bool value) { isBurning = value; }

    public int GetSelectedGun() { return selectedGun; } 

    public List<pickupStats> GetWeaponList() { return weapons; }



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
