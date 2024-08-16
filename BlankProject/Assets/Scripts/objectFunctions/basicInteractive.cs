using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basicInteractive : MonoBehaviour, IInteract
{
    bool isInteracted;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isInteracted) isInteracted = false;
    }

    public void interact()
    {
        isInteracted = true;
    }

    public void SetInteracted(bool state) { isInteracted = state; }
    public bool GetInteracted() { return isInteracted; }  
}
