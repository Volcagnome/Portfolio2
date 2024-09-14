using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class trashCompactor : MonoBehaviour
{
    [Range(-5, 5)][SerializeField] float movementSpeed;
    [Range(0, 10)][SerializeField] float playerDamage;
    [SerializeField] float stopTime;
    [SerializeField] GameObject leftCrusher;
    [SerializeField] GameObject rightCrusher;

    [Header("----Audio----")]
    [SerializeField] AudioSource crushSource;
    [SerializeField] AudioClip movementSound;
    [Range(0, 1)][SerializeField] float movementVolume;
    [SerializeField] AudioClip collisionSound;
    [Range(0, 1)][SerializeField] float collisionVolume;

    float leftCrusherStartPosition;
    float rightCrusherStartPosition;
    bool turningAround;
    bool stopped;

    // Start is called before the first frame update
    void Start()
    {
        leftCrusherStartPosition = leftCrusher.transform.localPosition.z;
        rightCrusherStartPosition = rightCrusher.transform.localPosition.z;
        stopped = false;
        turningAround = false;
        if (movementSpeed < 0) movementSpeed = 0 - movementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!stopped)
        {
            
            leftCrusher.transform.Translate(Vector3.back * movementSpeed * Time.deltaTime);
            rightCrusher.transform.Translate(Vector3.back * movementSpeed * Time.deltaTime);
        }
        if (!turningAround)
        {
            if (leftCrusherStartPosition <= leftCrusher.transform.localPosition.z || rightCrusherStartPosition >= rightCrusher.transform.localPosition.z)
            {
                StartCoroutine(turnAround());
            }
        }
    }

    

    public IEnumerator turnAround()
    {
        crushSource.PlayOneShot(collisionSound, collisionVolume);

        stopped = true;
        turningAround = true;
        movementSpeed = 0 - movementSpeed;

        yield return new WaitForSeconds(stopTime);

        stopped = false;
        crushSource.PlayOneShot(movementSound, movementVolume);

        yield return new WaitForSeconds(.1F);

        turningAround = false;

    }

    public bool getTurningAround() {  return turningAround; }
    public float getPlayerDamage() { return playerDamage; }
}
