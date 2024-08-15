using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class togglingItem : MonoBehaviour, IInteract
{
    [SerializeField] GameObject itemOnState;
    [SerializeField] GameObject itemOffState;
    [SerializeField] bool vital;

    public bool itemState;

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
    }

    void setState(bool state)
    {
        itemOnState.SetActive(state);
        itemOffState.SetActive(!state);

        if (vital)
        {
            if (state) GameManager.instance.UpdateWinCondition(1);
            else GameManager.instance.UpdateWinCondition(-1);
        }
    }
}
