using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEditor;
using UnityEngine;

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
    void Update()
    {
        if (LevelManager.instance.GetIntruderAlert())
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
