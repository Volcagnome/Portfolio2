using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerControl : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreMask;

    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;

    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;
    [SerializeField] float interactDist;
    [SerializeField] int dmgMultiplier;

    Vector3 move;
    Vector3 playerVel;

    int jumpCount;
    int hpOG;

    bool isSprinting;
    bool isShooting;
    bool isInteracting;

    // Start is called before the first frame update
    void Start()
    {
        hpOG = HP;
        adjustHPBar();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            // Jump count reset
            jumpCount = 0;
            // Fall velocity is 0, since player is grounded
            playerVel.y = 0;
        }

        else
        {
            // If not grounded, fall velocity increases
            playerVel.y -= gravity * Time.deltaTime;
        }

        move = Input.GetAxis("Vertical") * transform.forward +
               Input.GetAxis("Horizontal") * transform.right;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }

        controller.Move(playerVel * Time.deltaTime);

        if (Input.GetButton("Shoot") && !isShooting)
            StartCoroutine(shoot());

        if (Input.GetButtonDown("Interact"))
        {
            interact();
        }
            
    }

    void sprint()
    {
        // Sprint modifiers modify speed:
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }


    IEnumerator shoot()
    {
        isShooting = true;

        RaycastHit hit;
        // Physics.Raycast (Origin, Direction, hit info, max distance)
        if (Physics.Raycast(Camera.main.transform.position,
            Camera.main.transform.forward, out hit, shootDist, ~ignoreMask))
        {
            Debug.Log(hit.collider.name);
            IDamage dmg = hit.collider.gameObject.GetComponentInParent<IDamage>();

            if (dmg != null)
            {
                if (hit.collider.CompareTag("WeakSpot"))
                {
                    dmg.takeDamage(shootDamage * dmgMultiplier);
                }

                else
                {
                    dmg.takeDamage(shootDamage);
                }
            }
        }

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    void interact()
    {
        RaycastHit hit;
        // Physics.Raycast (Origin, Direction, hit info, max distance)
        if (Physics.Raycast(Camera.main.transform.position,
            Camera.main.transform.forward, out hit, interactDist, ~ignoreMask))
        {
            Debug.Log(hit.collider.name);
            IInteract interactWith = hit.collider.GetComponent<IInteract>();

            if (interactWith != null)
            {
                interactWith.interact();
            }
        }

    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        adjustHPBar();
         if (HP > 0)StartCoroutine(flashRed());

        //Im dead
        if (HP <= 0)
        {

            GameManager.instance.youLose();
        }
    }

    void adjustHPBar()
    {
        GameManager.instance.healthbar.fillAmount = (float)HP / hpOG;
    }

    IEnumerator flashRed()
    {
        GameManager.instance.redFlash.SetActive(true);
        yield return new WaitForSeconds(.1F);
        GameManager.instance.redFlash.SetActive(false);
    }
}
