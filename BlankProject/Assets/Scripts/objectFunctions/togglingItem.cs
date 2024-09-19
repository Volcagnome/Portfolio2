using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class togglingItem : MonoBehaviour, IInteract, ISendState
{
    [SerializeField] GameObject itemOnState;
    [SerializeField] GameObject itemOffState;
    [SerializeField] bool itemState;
    [SerializeField] bool vital;
  

    [Header("----- Sounds -----")]
    [SerializeField] AudioClip interactSound;
    [SerializeField] float interactVol;

    // Start is called before the first frame update
    void Start()
    {
        if (vital) 
        { 
            itemState = true;
        }

        setState(itemState);
    }

    // Update is called once per frame
    public void interact()
    {
        itemState = !itemState;
        setState(itemState);
        

        //// Play interact sound:
        GameManager.instance.playAud(interactSound, interactVol);

        // Action the toggled object's sound:
        //GameManager.instance.playAud(itemOnState.GetComponent<toggleReciever>().toggleSound, itemOnState.GetComponent<toggleReciever>().toggleVol);
    }

    void setState(bool state)
    {
        itemOnState.SetActive(state);
        
        itemOffState.SetActive(!state);

    }

    public bool getState()
    {
        return itemState;
    }
}
