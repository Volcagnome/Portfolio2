using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEditor;
using UnityEngine;


//Lights will change to red or flash during an intruder alert.


public class IntruderAlertLight : MonoBehaviour
{
    [SerializeField] Material idleMaterial;
    [SerializeField] Material alertMaterial;
    [SerializeField] Light lightComponent;


    [SerializeField] public enum LightType {ceiling, alert };
    [SerializeField] LightType type;
    Color colorOrig;
    Coroutine flashLightCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        colorOrig = lightComponent.color;
    }

    // Update is called once per frame
    //If Intruder Alert is currently active and object is an alert light, starts flashing. If object is a ceiling light,
    //just changes the light color and bulb color to red. When Intruder Alert ends and object is ceiling light,
    //changes light color and bulb color to original. If object is alert light, flashing coroutine will end naturally.
    void Update()
    {
        if (IntruderAlertManager.instance.GetIntruderAlert() || GameManager.instance.GetSelfDestructActivated())
        {
            if (flashLightCoroutine == null && type==LightType.alert)
                flashLightCoroutine = StartCoroutine(flashLight());
            else if(type == LightType.ceiling)  
            {
                gameObject.transform.GetChild(0).GetComponent<Renderer>().material = alertMaterial;
                lightComponent.color = Color.red;
            }
        } else if (type == LightType.ceiling)
        {
            gameObject.transform.GetChild(0).GetComponent<Renderer>().material = idleMaterial;
            lightComponent.color = colorOrig;
        }


    }

    //Changes color of bulb objects to the glowing alert material and turns on the light component, after a second returns
    //to idle color and turns light off. 
    IEnumerator flashLight()
    {
        yield return new WaitForSeconds(1f);

        gameObject.transform.GetChild(0).GetComponent<Renderer>().material = alertMaterial;
        gameObject.transform.GetChild(1).GetComponent<Renderer>().material = alertMaterial;
        lightComponent.gameObject.GetComponent<Light>().enabled = true;

        yield return new WaitForSeconds(1f);

        gameObject.transform.GetChild(0).GetComponent<Renderer>().material = idleMaterial;
        gameObject.transform.GetChild(1).GetComponent<Renderer>().material = idleMaterial;
        lightComponent.gameObject.GetComponent<Light>().enabled = false;

        flashLightCoroutine = null;
    }

}
