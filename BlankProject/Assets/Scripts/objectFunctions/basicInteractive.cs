using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basicInteractive : MonoBehaviour, IInteract, ISendState
{
    bool isInteracted;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void interact()
    {
        isInteracted = true;
    }
    public bool getState() 
    {
        bool holder = isInteracted;

        if (isInteracted) isInteracted = !isInteracted;

        return holder; 
    }  

    public void applyShader()
    {

    }
}
