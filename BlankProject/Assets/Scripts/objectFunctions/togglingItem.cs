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
    [SerializeField] List<GameObject> shadedParts;
    bool shaderApplied;

    // Start is called before the first frame update
    void Start()
    {
        //applyShader();

        setState(itemState);
    }




    // Update is called once per frame
    public void interact()
    {
        itemState = !itemState;

        setState(itemState);

        //// Play interact sound:
        //GameManager.instance.playAud(interactSound, interactVol);

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
       if (shader != null && shaderApplied == false)
        {
            shaderApplied = true;
            List<Material> baseMaterials = gameObject.GetComponent<MeshRenderer>().materials.ToList();
            baseMaterials.Add(shader);
            gameObject.GetComponent<MeshRenderer>().SetMaterials(baseMaterials);

            for (int i = 0; i < shadedParts.Count; i++)
            {
                baseMaterials = shadedParts[i].GetComponent<MeshRenderer>().materials.ToList();
                baseMaterials.Add(shader);
                shadedParts[i].GetComponent<MeshRenderer>().SetMaterials(baseMaterials);
            }

            StartCoroutine(removeShader());
        }
    }

    IEnumerator removeShader()
    {
       List<Material> baseMaterials = gameObject.GetComponent<MeshRenderer>().materials.ToList();
        baseMaterials.Remove(shader);
        gameObject.GetComponent<MeshRenderer>().SetMaterials(baseMaterials);


        yield return new WaitForSeconds(1);

        for (int i = 0; i < shadedParts.Count; i++)
        {
            baseMaterials.Clear();

            for (int j = 0; j < shadedParts[i].GetComponent<MeshRenderer>().materials.ToList().Count() - 1; j++) baseMaterials.Add(shadedParts[i].GetComponent<MeshRenderer>().materials.ToList()[j]);

            shadedParts[i].GetComponent<MeshRenderer>().SetMaterials(baseMaterials);
        }


        shaderApplied = false;
    }
}
