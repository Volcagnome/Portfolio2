using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] int sens;
    [SerializeField] float lockVertMin, lockVertMax;
    [SerializeField] bool invertY;

    float rotX;

    // Start is called before the first frame update
    void Start()
    {
        // turns off cursor, since we're using a fps view
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Get input:
        float mouseY = Input.GetAxis("Mouse Y") * sens;
        float mouseX = Input.GetAxis("Mouse X") * sens;
        //float mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;
        //float mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;

        if (invertY)
            rotX += mouseY;
        else
            rotX -= mouseY;

        // Clamp the rotX on the x-axis:
        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);

        // Rotate the cam on the x-axis:
        transform.localEulerAngles = Vector3.right * rotX;
        //transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        // Rotate the player on the y-axis
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
