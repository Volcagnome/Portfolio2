using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleLaserField : MonoBehaviour
{
    [SerializeField] GameObject toggler;
    [SerializeField] GameObject lever;

    [SerializeField] GameObject destroyedGameObject;

    bool isDestroyed;

    [Header("----- Sounds -----")]
    [SerializeField] AudioClip powerOffSound;
    [SerializeField] float powerOffVol;

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
            GameManager.instance.playAud(powerOffSound, powerOffVol);
        }
    }

    public void toggle()
    {
        isDestroyed = true;
    }
}
