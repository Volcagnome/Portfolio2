using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandCodeSlot : MonoBehaviour, IInteract
{
    [SerializeField] GameObject commandCode;
    [SerializeField] GameObject lightbulb;
    [SerializeField] Material lightBulbGlow;
    [SerializeField] AudioClip insertCode;
    [SerializeField] int slotNumber;

    bool slotFull;

    // Start is called before the first frame update
    void Start()
    {
        slotFull = StaticData.commandCodeSlotFull[slotNumber];

        if (slotFull)
        {
            commandCode.SetActive(true);
            lightbulb.GetComponent<MeshRenderer>().material = lightBulbGlow;
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void interact()
    {
        if (GameManager.instance.GetCommandCodesCollected() > 0 && !slotFull)
        {
            GameManager.instance.PlugInCode();
            commandCode.SetActive(true);
            lightbulb.GetComponent<MeshRenderer>().material = lightBulbGlow;
            GetComponent<AudioSource>().PlayOneShot(insertCode);
            slotFull = true;
            StaticData.commandCodeSlotFull[slotNumber] = true;

        }
        else
            return;
        

    }

    public void applyShader() { }
    public void removeShader() { }
}
