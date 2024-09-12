using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{

    [SerializeField] CharacterController controller;

    [Header("-- Stamina, Speed, Jumping --")]
    [SerializeField] float stamina;
    [SerializeField] float staminaWait;
    [SerializeField] float staminaDecrease;
    [SerializeField] float staminaIncrease;

    [SerializeField] float speed;
    [SerializeField] int sprintMod;

    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;

    [Header("----- Sounds -----")]
    [SerializeField] AudioClip audioJump;
    [SerializeField] float audioJumpVol;

    [SerializeField] AudioClip[] audioHurt;
    [SerializeField] float audioHurtVol;

    [SerializeField] AudioClip[] audioSteps;
    [SerializeField] float audioStepsVol;

    Vector3 move;
    Vector3 playerVel;

    int jumpCount;
    float speedOG;
    float staminaOG;

    bool hasStamina;
    bool isSprinting;
    bool isPlayingSteps;
    bool isCaught;
    

    public float playerHeight
    {
        get => controller.height;
        set => controller.height = value;
    }

    // Start is called before the first frame update

    //Sets the player's stamina and speed stats pulled from the StaticPlayerData script.
    void Start()
    {
        isCaught = false;
        //staminaOG = stamina;
        //speedOG = speed;

        staminaOG = StaticPlayerData.playerMaxStamina;
        speedOG = StaticPlayerData.playerSpeedOG; 
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
        staminaUsage();


        //If player is currently caught by a spider web, debuffs speed per the serialized field in LevelManager
        if (isCaught)
            speed = speedOG * LevelManager.instance.GetWebSpeedDebuff();

    }

    void movement()
    {
        if (controller.isGrounded)
        {
            // Jump count reset
            jumpCount = 0;
            // Fall velocity is 0, since player is grounded
            playerVel.y = 0;
        }

        else
        {
            // If not grounded, fall velocity increases
            playerVel.y -= gravity * Time.deltaTime;
        }

        controller.enabled = true;
        move = Input.GetAxis("Vertical") * transform.forward +
               Input.GetAxis("Horizontal") * transform.right;
        controller.Move(move * speed * Time.deltaTime);


        /////////////////////
        // *** JUMPING *** //
        /////////////////////

        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            GameManager.instance.playAud(audioJump, audioJumpVol);
            jumpCount++;
            playerVel.y = jumpSpeed;
        }

        controller.Move(playerVel * Time.deltaTime);

        // Play steps when moving:
        if (controller.isGrounded && move.magnitude > 0.3f && !isPlayingSteps)
        {
            StartCoroutine(playSteps());
        }
    }

    ///////////////////////////////
    // *** SOUND METHODS *** //
    ///////////////////////////////

    IEnumerator playSteps()
    {
        isPlayingSteps = true;
        GameManager.instance.playAud(audioSteps[Random.Range(0, audioSteps.Length)], audioStepsVol);
        
        if (!isSprinting)
            yield return new WaitForSeconds(0.5f);
        else
            yield return new WaitForSeconds(0.3f);
        
        isPlayingSteps = false;
    }

    ///////////////////////////////
    // *** SPRINTING METHODS *** //
    ///////////////////////////////

    void sprint()
    {
        // Sprint modifiers modify speed:
        if (Input.GetButtonDown("Sprint") && hasStamina)
        {
            speed *= sprintMod;
            isSprinting = true;
        }


        else if (Input.GetButtonUp("Sprint") && hasStamina)
        {
            stopSprint();
        }
    }

    // Resets speed to original settings and stops sprinting:
    void stopSprint()
    {
        speed = speedOG;
        isSprinting = false;
    }

    void staminaUsage()
    {
        GameManager.instance.staminaBar.fillAmount = stamina / staminaOG;

        if (stamina > 0)
            hasStamina = true;

        if (isSprinting)
        {
            stamina -= staminaDecrease * Time.deltaTime;
            // Sets back to 0 if stamina goes below and starts staminaOut() :
            if (stamina <= 0)
            {
                StartCoroutine(staminaOut());
            }
        }

        if (!isSprinting && hasStamina)
        {
            regenStamina();
        }
    }

    // Recover stamina:
    void regenStamina()
    {
        stamina += staminaIncrease * Time.deltaTime;
        hasStamina = true;

        // Sets back to max stamina if value goes above:
        if (stamina > staminaOG)
        {
            stamina = staminaOG;
        }
    }

    // Stamina runs out:
    IEnumerator staminaOut()
    {
        stamina = 0;
        hasStamina = false;
        stopSprint();

        yield return new WaitForSeconds(staminaWait);

        regenStamina();
    }

    //Functions to allow the arachnoids to influence player's current speed when they are webbed. 
    public void SetIsCaught(bool status) { isCaught = status; }

    public void SetSpeed(float newSpeed) { speed = newSpeed; }

    public float GetSpeedOG() { return speedOG; }

    

    //Getters and Setters
    public float getStamina() { return stamina; }
    public void setStamina(float value) { stamina = value; }

    public float getMaxStamina() { return staminaOG; }
    public void setMaxStamina(float value) { staminaOG = value; }
    public float getPlayerSpeed() { return speed; }
    public void setPlayerSpeed(float value) { speed = value; }

    public void SetPlayerSpeedOG(float newSpeed) { speedOG = newSpeed; }
}