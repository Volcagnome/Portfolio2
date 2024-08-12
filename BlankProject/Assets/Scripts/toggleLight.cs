using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleLight : MonoBehaviour, IToggle
{
    [SerializeField] Light toggledLight;
    [SerializeField] togglingItem controller;

    // Start is called before the first frame update
    void Start()
    {
        toggle(controller.itemState);
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.itemState != toggledLight.enabled) toggle(controller.itemState);
    }

    public void toggle(bool state)
    {
        toggledLight.enabled = state;
    }
}
