using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{

    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreMask;

    [SerializeField] float stamina;
    [SerializeField] float staminaWait;
    [SerializeField] float staminaDecrease;
    [SerializeField] float staminaIncrease;

    [SerializeField] int speed;
    [SerializeField] int sprintMod;

    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;

    Vector3 move;
    Vector3 playerVel;

    int jumpCount;
    int speedOG;
    float staminaOG;

    bool hasStamina;
    bool isSprinting;

    // Start is called before the first frame update
    void Start()
    {
        staminaOG = stamina;
        speedOG = speed;
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
        staminaUsage();
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

        move = Input.GetAxis("Vertical") * transform.forward +
               Input.GetAxis("Horizontal") * transform.right;
        controller.Move(move * speed * Time.deltaTime);


        /////////////////////
        // *** JUMPING *** //
        /////////////////////

        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }

        controller.Move(playerVel * Time.deltaTime);
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
}