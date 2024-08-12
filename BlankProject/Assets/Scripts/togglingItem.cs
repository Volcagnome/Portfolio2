using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class togglingItem : MonoBehaviour
{
    [SerializeField] GameObject itemOnState;
    [SerializeField] GameObject itemOffState;

    public bool itemState;

    // Start is called before the first frame update
    void Start()
    {
        setState(itemState);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Shoot")) itemState = !itemState;

        setState(itemState);
    }

    void setState(bool state)
    {
        itemOnState.SetActive(state);
        itemOffState.SetActive(!state);
    }
}
