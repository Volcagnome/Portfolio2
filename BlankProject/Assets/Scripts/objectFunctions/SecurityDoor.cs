using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SecurityDoor : toggleReciever
{

    [SerializeField] GameObject leftDoor;
    [SerializeField] GameObject rightDoor;
    [SerializeField] GameObject secondaryControl;
    [SerializeField] AudioSource audioPlayer;
    [SerializeField] AudioClip doorOpenSound;
    [SerializeField] AudioClip doorCloseSound;

    Vector3 closedPositionRight;
    Vector3 closedPositionLeft;
    Vector3 openPositionRight;
    Vector3 openPositionLeft;
    Vector3 currentPositionRightDoor;
    Vector3 currentPositionLeftDoor;
    bool controllerState;

    // Start is called before the first frame update
    private void Start()
    {
        if (!controllerState)
        {
            closedPositionRight = rightDoor.transform.localPosition;
            closedPositionLeft = leftDoor.transform.localPosition;
            openPositionRight = new Vector3(closedPositionRight.x, closedPositionRight.y, closedPositionRight.z + 1.5f);
            openPositionLeft = new Vector3(closedPositionLeft.x, closedPositionLeft.y, closedPositionLeft.z - 1.5f);
        }

    }

    void Update()
    {
        if (secondaryControl != null)
        {
            if (secondaryControl != null && controller.GetComponent<ISendState>().getState() != controllerState)
            {
                controllerState = controller.GetComponent<togglingItem>().getState();
                secondaryControl.GetComponent<togglingItem>().interact();
            }
            else if (secondaryControl.GetComponent<ISendState>().getState() != controllerState)
            {
                controllerState = secondaryControl.GetComponent<togglingItem>().getState();
                controller.GetComponent<togglingItem>().interact();
            }
        }
        else
            controllerState = controller.GetComponent<ISendState>().getState();
        
            toggle(controllerState);
    }



    private void OpenDoors()
    {
        currentPositionRightDoor = rightDoor.transform.localPosition;
        currentPositionLeftDoor = leftDoor.transform.localPosition;

        if (Vector3.Distance(currentPositionRightDoor, openPositionRight) <= 0.05f)
        {
            currentState = true;
            return;
        }else
        {
            rightDoor.transform.localPosition = Vector3.MoveTowards(currentPositionRightDoor, openPositionRight, Time.deltaTime * 4f);
            leftDoor.transform.localPosition = Vector3.MoveTowards(currentPositionLeftDoor, openPositionLeft, Time.deltaTime * 4f);
        }
 
    }


    //Reverses the OpenDoor function.
    private void CloseDoors()
    {
        currentPositionRightDoor = rightDoor.transform.localPosition;
        currentPositionLeftDoor = leftDoor.transform.localPosition;

        if (Vector3.Distance(currentPositionRightDoor, closedPositionRight) <= 0.05f)
        {
            currentState = false;
            return;
        }
        else
        {
            rightDoor.transform.localPosition = Vector3.MoveTowards(currentPositionRightDoor, closedPositionRight, Time.deltaTime * 4f);
            leftDoor.transform.localPosition = Vector3.MoveTowards(currentPositionLeftDoor, closedPositionLeft, Time.deltaTime * 4f);
        }


    }

    public override void toggle(bool state)
    {
        if(state && !currentState)
        {
            OpenDoors();
            if (!audioPlayer.isPlaying)
                audioPlayer.PlayOneShot(doorOpenSound);
        }
        else if(!state && currentState)
        {
            CloseDoors();
           if (!audioPlayer.isPlaying)
                audioPlayer.PlayOneShot(doorCloseSound);
        }

    }
}
