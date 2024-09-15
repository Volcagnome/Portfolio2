using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class pickupStats : ScriptableObject
{
    public enum pickupType { upgrade, weapon, commandCode, securityPassword}


    [Header("----Name----")]
    public string itemName;

    [Header("----Base Information----")]
    public pickupType type;
    public GameObject itemModel;
    public float modelScale;

    [Header("----Upgrade Stats----")]
    public float health;
    public float stamina;
    public int damageUP;
    public float batteryUP;
    public float speed;

    [Header("----Player Ability----")]
    public bool xrayAbilityUnlocked;

    [Header("----Weapon Stats----")]
    public bool shotgun;
    public float shootRate;
    public int shootDamage;
    public float shootDist;
    public int dmgMultiplier;
    public float currentHeat;
    public int maxHeat;
    public float heatPerShot;
    public float coolRate;
    public float coolWaitTime;
    public Vector3 modelRotationAxis;
    [Range(-180, 180)] public float rotationAngle;

    [Header("----Sounds----")]
    public AudioClip fireSound;
    public float fireVol;

}
