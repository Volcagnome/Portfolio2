using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemPickup : MonoBehaviour, IPickup
{
    [SerializeField] pickupStats item;
    [SerializeField] GameObject displayedItem;
    [SerializeField] int rotationSpeed;

    [SerializeField] bool grabbableByEnemy;
    [SerializeField] bool grabbableByPlayer;

    // Update is called once per frame
    void Update()
    {
        displayedItem.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (displayedItem != null)
        {
            pickup(other);
        }
    }


    public void pickup(Collider other)
    {
        if (grabbableByEnemy && other.gameObject.CompareTag("Enemy"))
        {
            switch (item.type)
            {
                case (pickupStats.pickupType.health):
            {
                other.GetComponent<enemyAI>().SetHP(item.nonWeaponStat + other.GetComponent<enemyAI>().GetHP());
                displayedItem.GetComponent<MeshRenderer>().enabled = false;
                break;
            }
                case (pickupStats.pickupType.weapon):
            {
                other.GetComponent<enemyAI>().SetShootRate(item.shootRate + other.GetComponent<enemyAI>().GetShootRate());
                displayedItem.GetComponent<MeshRenderer>().enabled = false;
                break;
            }
                default:
            {
                break;
            }
            }

            
        }
        else if (grabbableByPlayer && other.gameObject.CompareTag("Player"))
        {
            switch (item.type)
            {
                case (pickupStats.pickupType.weapon):
            {
                other.GetComponent<playerControl>().addWeapon(item);
                displayedItem.GetComponent<MeshRenderer>().enabled = false;
                break;
            }
                case (pickupStats.pickupType.health):
            {
                other.GetComponent<playerControl>().setMaxHP(item.nonWeaponStat + other.GetComponent<playerControl>().getMaxHP());
                other.GetComponent<playerControl>().setHP(other.GetComponent<playerControl>().getMaxHP());
                displayedItem.GetComponent<MeshRenderer>().enabled = false;
                break;
            }
                case (pickupStats.pickupType.stamina):
            {
                other.GetComponent<playerControl>().setMaxStamina(item.nonWeaponStat + other.GetComponent <playerControl>().getMaxStamina());
                other.GetComponent<playerControl>().setStamina(other.GetComponent<playerControl>().getMaxStamina());
                displayedItem.GetComponent<MeshRenderer>().enabled = false;
                break;
            }
                case (pickupStats.pickupType.damage):
            {
                other.GetComponent<playerControl>().setBulletUpgrades(other.GetComponent<playerControl>().getBulletUpgrades() + (int)item.nonWeaponStat);
                displayedItem.GetComponent<MeshRenderer>().enabled = false;
                break;
            }
                case (pickupStats.pickupType.speed):
            {
                other.GetComponent<playerControl>().setPlayerSpeed(other.GetComponent<playerControl>().getPlayerSpeed() + (int)item.nonWeaponStat);
                displayedItem.GetComponent<MeshRenderer>().enabled = false;
                break;
            }
                case (pickupStats.pickupType.ammo): //For later when ammo is fully implemented
            {
                break;
            }
                case (pickupStats.pickupType.ammoUpgrade):
            {
                other.GetComponent<playerControl>().setAmmoMultiplier(other.GetComponent<playerControl>().getAmmoMultiplier() + item.nonWeaponStat);
                displayedItem.GetComponent<MeshRenderer>().enabled = false;
                break;
            }
                default:
            {
                break;
            }
            }
        }
        
    }
}
