using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleLaserField : MonoBehaviour
{
    [SerializeField] GameObject toggler;
    [SerializeField] GameObject lever;

    [SerializeField] GameObject destroyedGameObject;

    bool isDestroyed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDestroyed)
        {
            Destroy(destroyedGameObject);
        }
    }

    public void toggle()
    {
        isDestroyed = true;

    }
}
