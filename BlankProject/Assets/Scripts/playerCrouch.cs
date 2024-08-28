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

    bool isCrouched;
    bool isProne;
    float holdCrouch;

    Vector3 initialCamPos;
    Vector3 newCamPos;

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        initialCamPos = mainCam.transform.localPosition;
        standingHeight = GameManager.instance.playerScript.playerHeight;
    }

    // Update is called once per frame
    void Update()
    {
        crouchInput();
        crouch();
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
        }
        // If key continues to be held down:
        if (Input.GetButton("Crouch"))
        {
            // Count towards prone time requirement.
            holdCrouch += Time.deltaTime;

            // If met, enter prone mode:
            if (holdCrouch >= proneTime)
            {
                isProne = true;
                isCrouched = false;
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
}
