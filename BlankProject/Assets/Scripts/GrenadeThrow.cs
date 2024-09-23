using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grenadeThrow : MonoBehaviour
{
    public Transform camPos;
    public Transform throwPos;

    int selectedGrenade;
    // Grenade array:
    [SerializeField] public List<grenadeStats> grenades;
    [SerializeField] public grenadeStats grenadeEMP;
    [SerializeField] public grenadeStats grenadeDecoy;
    [SerializeField] public DecoyPingScript decoyPings;

    // currently selected grenade:
    public grenadeStats currentGrenade;

    [Header("-----Settings-----")]
    [SerializeField] public float throwForce;
    [SerializeField] public float throwUpForce;
    [SerializeField] float grenadeCooldown;

    [SerializeField] public int empAmmo;
    [SerializeField] public int decoyAmmo;

    int maxThrows;
    bool readyForThrow;

    public bool targetHit;

    [Header("---Grenade Stats---")]
    public MeshRenderer grenadeMeshRend;
    public GameObject grenadeModel;
    public float fuseTime;
    public float areaOfEffect;
    public float aoeDamage;
    public float offsetRange;
    public bool isEMP;
    public bool isDecoy;

    Vector3 offset;
    public Rigidbody rb;

    [Header("---Explosion FX---")]
    public ParticleSystem explosionFX;

    [Header("---Sounds---")]
    public AudioClip effectSound;
    public float effectVol;

    [SerializeField] AudioClip throwSound;
    [SerializeField] float throwVol;

    // Start is called before the first frame update
    void Start()
    {
        readyForThrow = true;

        // Adding default grenades:
        grenadeEMP.ammoCount = empAmmo;
        grenadeDecoy.ammoCount = decoyAmmo;
        addGrenade(grenadeEMP);
        addGrenade(grenadeDecoy);
        
        selectedGrenade = 1;

        // Set grenades to max grenade count.
        maxThrows = grenades[selectedGrenade].ammoCount;

        // Update UI to display grenade count:
        GameManager.instance.grenadeCount.text = maxThrows.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        //Update UI:
        GameManager.instance.empImage.enabled = (isEMP);
        GameManager.instance.decoyImage.enabled = (isDecoy);

        if (Input.GetButtonDown("Throw") && readyForThrow && maxThrows > 0 && grenades.Count > 0)
        {
            throwGrenade();
        }

        if (Input.GetButtonDown("Swap Grenades") && grenades.Count > 1)
        {
            SwapGrenades();
        }
    }
    public void addGrenade(grenadeStats grenade)
    {
        currentGrenade = grenade;
        grenades.Add(grenade);
        SetGrenade(grenade);
    }

    private void SwapGrenades()
    {
        // Increase index for grenade array:
        selectedGrenade++;
        // if index exceeds number of available grenades, return to start of the list:
        if (selectedGrenade > grenades.Count - 1)
        {
            selectedGrenade = 0;
        }
        // Set current grenades to our selection:
        SetGrenade(grenades[selectedGrenade]);

        // Update UI to display grenade count:
        GameManager.instance.grenadeCount.text = maxThrows.ToString();
    }

    private void SetGrenade(grenadeStats selection)
    {
        maxThrows = selection.ammoCount;

        isEMP = selection.isEMP;
        isDecoy = selection.isDecoy;

        grenadeModel = selection.grenadeModel;
        grenadeModel.GetComponent<MeshRenderer>().sharedMaterial = selection.grenadeModel.GetComponent<MeshRenderer>().sharedMaterial;
        grenadeModel.GetComponent<MeshFilter>().sharedMesh = selection.grenadeModel.GetComponent<MeshFilter>().sharedMesh;

        fuseTime = selection.fuseTime;
        areaOfEffect = selection.areaOfEffect;
        aoeDamage = selection.aoeDamage;
        offsetRange = selection.offsetRange;

        explosionFX = selection.explosionFX;

        effectSound = selection.effectSound;
        effectVol = selection.effectVol;

        //offset = selection.offset;

        rb = selection.rb;

        currentGrenade = selection;
    }

    void throwGrenade()
    {
        readyForThrow = false;

        // Grenade is instantiated:
        GameObject throwable = Instantiate(grenadeModel, throwPos.position, camPos.rotation);
        // Play throw sound effect:
        GameManager.instance.playAud(throwSound, throwVol);
        // Get rigidbody and calculate throw force:
        Rigidbody throwableRB = throwable.GetComponent<Rigidbody>();
        Vector3 forceAdded = camPos.transform.forward * throwForce + transform.up * throwUpForce;

        throwableRB.AddForce(forceAdded, ForceMode.Impulse);

        // Counter for remaining throws lowers.
        maxThrows--;
        // Update grenade count.
        grenades[selectedGrenade].ammoCount = maxThrows;

        // Update UI to display grenade count:
        GameManager.instance.grenadeCount.text = maxThrows.ToString();

        // Cooldown grenade throwing:
        Invoke(nameof(resetGrenade), grenadeCooldown);
    }

    private void resetGrenade()
    {
        readyForThrow = true;
    }
}
