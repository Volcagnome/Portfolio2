using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class basicInteractive : MonoBehaviour, IInteract, ISendState
{
    bool isInteracted;

    [Header("----Interaction Shaders----")]
    [SerializeField] Material shader;
    [SerializeField] Material nonShaderPlaceholder;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void interact()
    {
        isInteracted = true;
    }
    public bool getState() 
    {
        bool holder = isInteracted;

        if (isInteracted) isInteracted = !isInteracted;

        return holder; 
    }

    public void applyShader()
    {
        if (shader != null)
        {

            List<Material> baseMaterials = gameObject.GetComponent<MeshRenderer>().materials.ToList();
            baseMaterials[baseMaterials.Count - 1] = shader;
            gameObject.GetComponent<MeshRenderer>().SetMaterials(baseMaterials);
        }
    }

    public void removeShader()
    {
        List<Material> baseMaterials = gameObject.GetComponent<MeshRenderer>().materials.ToList();
        baseMaterials[baseMaterials.Count - 1] = nonShaderPlaceholder;
        gameObject.GetComponent<MeshRenderer>().SetMaterials(baseMaterials);

    }
}
