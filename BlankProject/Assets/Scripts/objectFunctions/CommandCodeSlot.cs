using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandCodeSlot : MonoBehaviour, IInteract
{
    [SerializeField] GameObject commandCode;
    [SerializeField] GameObject lightbulb;
    [SerializeField] Material lightBulbGlow;

    // Start is called before the first frame update
    void Start()
    {
        commandCode.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void interact()
    {
        if (GameManager.instance.GetCommandCodesCollected() > 0)
        {
            GameManager.instance.PlugInCode();
            commandCode.SetActive(true);
            lightbulb.GetComponent<MeshRenderer>().material = lightBulbGlow;
        }
        else
            return;
        

    }

}
