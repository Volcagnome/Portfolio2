using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class pickupStats : ScriptableObject
{
    public enum pickupType { upgrade, weapon, commandCode,securityPassword}
    public pickupType type;
    public GameObject itemModel;
    public Vector3 modelRotationAxis;
    [Range(-180,180)]public float rotationAngle;
    public float health;
    public float stamina;
    public int damageUP;
    public float batteryUP;
    public float speed;
    public float shootRate;
    public int shootDamage;
    public float shootDist;
    public int dmgMultiplier;
    public string itemName;
    public int passwordCombo;
}
