using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleReciever : MonoBehaviour, IToggle
{
    [SerializeField] GameObject toggleRecipientOn;
    [SerializeField] GameObject toggleRecipientOff;
    [SerializeField] GameObject controller;
    private bool currentState;

    [Header("----- Sounds -----")]
    [SerializeField] public AudioClip toggleSound;
    [SerializeField] public float toggleVol;

    // Start is called before the first frame update
    void Start()
    {
        toggle(controller.GetComponent<ISendState>().getState());
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.GetComponent<ISendState>().getState() != currentState)
        {
            toggle(controller.GetComponent<ISendState>().getState());

            // Play sound when toggled:
            GameManager.instance.playAud(toggleSound, toggleVol);
        }
    }

    public void toggle(bool state)
    {
        currentState = state;

        toggleRecipientOn.SetActive(state);
        toggleRecipientOff.SetActive(!state);
    }
}
