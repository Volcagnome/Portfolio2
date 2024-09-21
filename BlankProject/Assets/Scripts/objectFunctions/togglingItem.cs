using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class togglingItem : MonoBehaviour, IInteract, ISendState
{
    [SerializeField] GameObject itemOnState;
    [SerializeField] GameObject itemOffState;
    [SerializeField] bool itemState;

    [Header("----- Sounds -----")]
    [SerializeField] AudioClip interactSound;
    [SerializeField] float interactVol;

    [Header("----Interaction Shaders----")]
    [SerializeField] Material shader;
    [SerializeField] Material nonShaderPlaceholder;
    [SerializeField] List<MeshRenderer> shadedParts;
    bool shaderApplied;
    bool hitFlag;

    // Start is called before the first frame update
    void Start()
    {
        setState(itemState);

        List<Material> baseMaterials = new List<Material>();
        
        for (int i = 0; i < shadedParts.Count; i++)
        {
            baseMaterials = shadedParts[i].materials.ToList();
            baseMaterials.Add(nonShaderPlaceholder);
            shadedParts[i].SetMaterials(baseMaterials);
        }
    }


    // Update is called once per frame
    public void interact()
    {
        itemState = !itemState;
        setState(itemState);
        

        //// Play interact sound:
        GetComponent<AudioSource>().PlayOneShot(interactSound, interactVol);

        // Action the toggled object's sound:
        //GameManager.instance.playAud(itemOnState.GetComponent<toggleReciever>().toggleSound, itemOnState.GetComponent<toggleReciever>().toggleVol);
    }

    void setState(bool state)
    {
        itemOnState.SetActive(state);
        
        itemOffState.SetActive(!state);

    }

    public bool getState()
    {
        return itemState;
    }

    public void applyShader()
    {
       if (shader != null)
        {

            List<Material> baseMaterials = new List<Material> ();

            for (int i = 0; i < shadedParts.Count; i++)
            {
                baseMaterials = shadedParts[i].materials.ToList();
                baseMaterials[baseMaterials.Count - 1] = shader;
                shadedParts[i].SetMaterials(baseMaterials);
            }

            
        }
    }

    public void removeShader()
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
    }
}
