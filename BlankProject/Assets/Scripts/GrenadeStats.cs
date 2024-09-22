using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class grenadeStats : ScriptableObject
{
    // This script is for creating scriptable grenade objects that can be added
    // to the player's grenade list.
    [Header("---Mesh Renderer---")]
    public MeshRenderer grenadeMeshRend;
    public GameObject grenadeModel;

    [Header("---Select (ONE) Grenade Type:---")]
    public bool isEMP;
    public bool isDecoy;

    [Header("---Grenade Stats---")]
    public float fuseTime;
    public float areaOfEffect;
    public float aoeDamage;
    public float offsetRange;
    public float stunTime;
    public int ammoCount;

    [Header("---Decoy Stats---")]
    public float startFuse;
    public float distractTime;

    [Header("---Explosion FX---")]
    public ParticleSystem explosionFX;

    [Header("---Sounds---")]
    public AudioClip effectSound;
    public float effectVol;

    Vector3 offset;

    public bool isLive;
    public Rigidbody rb;
}