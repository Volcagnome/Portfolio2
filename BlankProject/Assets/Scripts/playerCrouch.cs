using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCrouch : MonoBehaviour
{
    [SerializeField] Camera mainCam;
    [SerializeField] CharacterController controller;

    [SerializeField] float standingHeight;
    [SerializeField] float crouchHeight;
    [SerializeField] float proneHeight;
    [SerializeField] float proneTime;

    float heightOG;
    bool isCrouched;
    float holdCrouch;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        crouch();
    }

    void crouch()
    {
        if (Input.GetButtonDown("Crouch") && isCrouched == false)
        {
            // On button press, lowers player height to crouch height.
            transform.localPosition = new Vector3(0, crouchHeight, 0);
            crouchCam();

            // Turn on indicator. (crouch true, prone false)
            GameManager.instance.crouchWindow.SetActive(true);
            GameManager.instance.proneWindow.SetActive(false);

            isCrouched = true;

        }

        else if (Input.GetButtonDown("Crouch") && isCrouched == true)
        {
            // On key up, returns to regular height.
            transform.localPosition = new Vector3(0, standingHeight, 0);
            standCam();

            // Turn off indicator. (both false)
            GameManager.instance.crouchWindow.SetActive(false);
            GameManager.instance.proneWindow.SetActive(false);

            isCrouched = false;
        }

        // *** PRONE *** //
        if (Input.GetButton("Crouch"))
        {
            // Count towards prone time requirement.
            holdCrouch += Time.deltaTime;
            // If met, enter prone mode:
            if (holdCrouch >= proneTime)
            {
                // Transform:
                transform.localPosition = new Vector3(0, proneHeight, 0);
                proneCam();

                holdCrouch = 0;

                // Turn on indicator. (prone true, crouch false)
                GameManager.instance.proneWindow.SetActive(true);
                GameManager.instance.crouchWindow.SetActive(false);

                isCrouched = true;
            }

            if (Input.GetButtonUp("Crouch"))
            {
                // If prone time is not enough and player lets go of key, reset count timer.
                holdCrouch = 0;
            }
        }
    }

    // Methods for changing the camera's height when crouching/standing.
    void standCam()
    {
        mainCam.transform.localPosition = new Vector3(0, standingHeight, 0);
    }

    void crouchCam()
    {
        mainCam.transform.localPosition = new Vector3(0, crouchHeight, 0);
    }

    void proneCam()
    {
        mainCam.transform.localPosition = new Vector3(0, proneHeight, 0);
    }
}