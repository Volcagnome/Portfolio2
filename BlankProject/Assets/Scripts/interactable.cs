using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactable : MonoBehaviour, IInteract
{

    [SerializeField] Renderer model;

    Color colorOrig;

    // Start is called before the first frame update
    void Start()
    {
        colorOrig = gameObject.GetComponentInChildren<Renderer>().sharedMaterial.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void interact()
    {
        // When interact is called on this object: (Example)
        StartCoroutine(flashGreen());
    }

    // Example method:
    IEnumerator flashGreen()
    {
        model.material.color = Color.green;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
}
