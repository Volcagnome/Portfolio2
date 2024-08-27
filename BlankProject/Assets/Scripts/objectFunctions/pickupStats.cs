using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class pickupStats : ScriptableObject
{
    public enum pickupType { health, stamina, damage, ammo, ammoUpgrade, speed, weapon }
    public pickupType type;
    public GameObject itemModel;
    public float nonWeaponStat;

    public float shootRate;
    public int shootDamage;
    public float shootDist;
    public int dmgMultiplier;
}
