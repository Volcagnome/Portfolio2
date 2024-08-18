using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FieldOfViewDebug : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {   
        
        Handles.color = Color.white;
        Handles.DrawWireArc(gameObject.GetComponent<enemyAI>().transform.position, Vector3.up, Vector3.forward, 360, gameObject.GetComponent<SphereCollider>().radius);

        Vector3 viewAngle01 = DirectionFromAngle(gameObject.GetComponent<enemyAI>().transform.eulerAngles.y, -gameObject.GetComponent<enemyAI>().FOV_Angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(gameObject.GetComponent<enemyAI>().transform.eulerAngles.y, gameObject.GetComponent<enemyAI>().FOV_Angle / 2); 

        Handles.color = Color.yellow;
        Handles.DrawLine(gameObject.GetComponent<enemyAI>().transform.position, gameObject.GetComponent<enemyAI>().transform.position + viewAngle01 * gameObject.GetComponent<SphereCollider>().radius);
        Handles.DrawLine(gameObject.GetComponent<enemyAI>().transform.position, gameObject.GetComponent<enemyAI>().transform.position + viewAngle02 * gameObject.GetComponent<SphereCollider>().radius);

        if (gameObject.GetComponent<enemyAI>().playerInView)
        {
            Handles.color = Color.green;
            Handles.DrawLine(gameObject.GetComponent<enemyAI>().transform.position, GameManager.instance.player.transform.position);
        }

    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
