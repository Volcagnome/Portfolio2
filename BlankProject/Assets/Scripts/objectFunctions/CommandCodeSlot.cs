using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CommandCodeSlot : MonoBehaviour, IInteract
{
    [SerializeField] GameObject commandCode;
    [SerializeField] GameObject lightbulb;
    [SerializeField] Material lightBulbGlow;
    [SerializeField] AudioClip insertCode;
    [SerializeField] int slotNumber;

    [Header("----Interaction Shaders----")]
    [SerializeField] Material shader;
    [SerializeField] Material nonShaderPlaceholder;
    [SerializeField] List<MeshRenderer> shadedParts;

    bool slotFull;
    bool shaderApplied;

    // Start is called before the first frame update
    void Start()
    {
        slotFull = StaticData.commandCodeSlotFull[slotNumber];

        if (slotFull)
        {
            commandCode.SetActive(true);
            lightbulb.GetComponent<MeshRenderer>().material = lightBulbGlow;
        }

        List<Material> baseMaterials = new List<Material>();

        for (int i = 0; i < shadedParts.Count; i++)
        {
            baseMaterials = shadedParts[i].materials.ToList();
            baseMaterials.Add(nonShaderPlaceholder);
            shadedParts[i].SetMaterials(baseMaterials);
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

            if (shaderApplied) removeShader();
        }
        else
            return;
        

    }

    public void applyShader()
    {
        if (!slotFull)
        {
            if (shader != null)
            {

                List<Material> baseMaterials = gameObject.GetComponent<MeshRenderer>().materials.ToList();
                baseMaterials[baseMaterials.Count - 1] = shader;
                gameObject.GetComponent<MeshRenderer>().SetMaterials(baseMaterials);

                for (int i = 0; i < shadedParts.Count; i++)
                {
                    baseMaterials = shadedParts[i].materials.ToList();
                    baseMaterials[baseMaterials.Count - 1] = shader;
                    shadedParts[i].SetMaterials(baseMaterials);
                }

                shaderApplied = true;
            }
        }
    }

    public void removeShader()
    {
        if (shaderApplied)
        {
            List<Material> baseMaterials = gameObject.GetComponent<MeshRenderer>().materials.ToList();
            baseMaterials[baseMaterials.Count - 1] = nonShaderPlaceholder;
            gameObject.GetComponent<MeshRenderer>().SetMaterials(baseMaterials);

            for (int i = 0; i < shadedParts.Count; i++)
            {
                baseMaterials = shadedParts[i].materials.ToList();
                baseMaterials[baseMaterials.Count - 1] = nonShaderPlaceholder;
                shadedParts[i].SetMaterials(baseMaterials);
            }

            shaderApplied = false;
        }
    }
}
