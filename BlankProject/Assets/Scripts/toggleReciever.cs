using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleReciever : MonoBehaviour, IToggle
{
    [SerializeField] GameObject toggleRecipientOn;
    [SerializeField] GameObject toggleRecipientOff;
    [SerializeField] togglingItem controller;
    private bool currentState;

    // Start is called before the first frame update
    void Start()
    {
        toggle(controller.itemState);
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.itemState != currentState) toggle(controller.itemState);
    }

    public void toggle(bool state)
    {
        currentState = state;

        toggleRecipientOn.SetActive(state);
        toggleRecipientOff.SetActive(!state);
    }
}
