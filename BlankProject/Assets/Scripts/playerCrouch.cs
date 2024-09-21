using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCrouch : MonoBehaviour
{
    [SerializeField] Camera mainCam;
    [SerializeField] CharacterController controller;

    float currentHeight;
    float targetHeight;
    [SerializeField] float standingHeight;
    [SerializeField] float crouchHeight;
    [SerializeField] float proneHeight;

    [SerializeField] float crouchSpeed = 10f;
    [SerializeField] float proneTime;

    float standMoveSpeed;

    //Xray ability varaibles
    [SerializeField] float xraydius;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float xrayEffectDuration;
    [SerializeField] float xrayAbilityCooldown;
    bool xrayAblityUnlocked;
    bool xrayInEffect;

    bool isCrouched;
    bool isProne;
    float holdCrouch;

    Vector3 initialCamPos;
    Vector3 newCamPos;

    private void Awake()
    {
        xrayAblityUnlocked = StaticData.playerXrayAbility;
    }
    // Start is called before the first frame update
    void Start()
    {
        initialCamPos = mainCam.transform.localPosition;
        standingHeight = GameManager.instance.playerScript.playerHeight;
        currentHeight = standingHeight;
        standMoveSpeed = GameManager.instance.playerScript.getPlayerSpeed();
        xrayInEffect = false;
    }

    // Update is called once per frame
    void Update()
    {
        crouchInput();
        crouch();

        if(xrayAblityUnlocked)
            xrayInput();
    }

    void crouchInput()
    {
        // On crouch key activation:
        if (Input.GetButtonDown("Crouch"))
        {
            // Resets hold counter to 0.
            holdCrouch = 0;
            // Toggles crouch bool and sets isProne to false.
            isCrouched = !isCrouched;
            isProne = false;
            Debug.Log("test");

            // If button was pressed and player is no longer crouched or prone by effect:
            if (!isCrouched && !isProne)
            {
                // Raises player speed:
                GameManager.instance.playerScript.setPlayerSpeed(GameManager.instance.playerScript.getPlayerSpeedOG());
            }
        }
        // If key continues to be held down:
        if (Input.GetButton("Crouch"))
        {
            // Count towards prone time requirement.
            holdCrouch += Time.deltaTime;
            Debug.Log("test2");


            // If met, enter prone mode:
            if (holdCrouch >= proneTime)
            {
                isProne = true;
                isCrouched = false;
                if (GameManager.instance.player.GetComponent<playerDamage>().getIsBurning()) GameManager.instance.player.GetComponent<playerDamage>().setIsBurning(false);
            }
        }
    }

    void crouch()
    {
        // Variable for our crouch transition speed.
        var crouchDelta = Time.deltaTime * crouchSpeed;

        if (isCrouched == true)
        {
            // On button press, lowers player height to crouch height.
            currentHeight = Mathf.Lerp(currentHeight, crouchHeight, crouchDelta);
            controller.height = currentHeight;

            targetHeight = crouchHeight;
            camPosition();

            // Lower player speed:
            GameManager.instance.playerScript.setPlayerSpeed(standMoveSpeed / 1.5f);

            // Turn on indicator. (crouch true, prone false)
            GameManager.instance.crouchWindow.SetActive(true);
            GameManager.instance.proneWindow.SetActive(false);

        }

        if (isCrouched == false)
        {
            // returns to regular height.
            currentHeight = Mathf.Lerp(currentHeight, standingHeight, crouchDelta);
            GameManager.instance.playerScript.playerHeight = currentHeight;

            targetHeight = standingHeight;
            camPosition();

            // Turn off indicator. (both false)
            GameManager.instance.crouchWindow.SetActive(false);
            GameManager.instance.proneWindow.SetActive(false);

        }

        if (isProne == true)
        {
            currentHeight = Mathf.Lerp(currentHeight, proneHeight, crouchDelta);
            GameManager.instance.playerScript.playerHeight = currentHeight;

            targetHeight = proneHeight;
            camPosition();

            // Lower player speed:
            GameManager.instance.playerScript.setPlayerSpeed(standMoveSpeed / 2);

            // Turn on indicator. (prone true, crouch false)
            GameManager.instance.proneWindow.SetActive(true);
            GameManager.instance.crouchWindow.SetActive(false);
        }
    }

    void camPosition()
    {
        Vector3 halfHeightDiff = new Vector3(0, (currentHeight - targetHeight) / 2, 0);
        if (targetHeight == standingHeight) 
        {
            newCamPos = initialCamPos + halfHeightDiff;
        }
        else
        {
            newCamPos = initialCamPos - halfHeightDiff;
            
        }
        mainCam.transform.localPosition = newCamPos;
    }

    void xrayInput()
    {
        Material xrayMaterial = null;
        Material materialOrig = null;

        // On crouch key activation:
        if (Input.GetButtonDown("Xray"))
        {

            Collider[] nearbyEnemies = Physics.OverlapSphere(gameObject.transform.position, xraydius, enemyLayer);

            if (nearbyEnemies.Length > 0)

            {
                foreach (Collider enemy in nearbyEnemies)
                { 

                    if (enemy.gameObject.GetComponent<SharedEnemyAI>())
                    {

                        if (enemy.GetComponent<SharedEnemyAI>().GetIsDead() == false)

                            StartCoroutine(XrayEffect(enemy.gameObject));

                    }
                }
            }
        }
    }



    IEnumerator XrayEffect(GameObject enemy)
    {
        xrayInEffect = true;

        if (enemy != null)
            enemy.GetComponent<SharedEnemyAI>().XrayEnemy(enemy, true);

        yield return new WaitForSeconds(xrayEffectDuration);

        if (enemy != null)
            enemy.GetComponent<SharedEnemyAI>().XrayEnemy(enemy, false);

        yield return new WaitForSeconds(xrayAbilityCooldown);

        xrayInEffect = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy") && other.isTrigger == false)
           other.gameObject.GetComponent<SharedEnemyAI>().SetInCrouchRadius(true);
        else
            return;

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy") && other.isTrigger == false)
            other.gameObject.GetComponent<SharedEnemyAI>().SetInCrouchRadius(false);
        else
            return;
    }


    public void UnlockXrayAbility() { xrayAblityUnlocked = true; }

    public bool GetXrayAbilityUnlocked() { return xrayAblityUnlocked; }

    public bool GetIsCrouched() { return isCrouched; }
}
