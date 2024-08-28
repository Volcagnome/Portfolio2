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

    void Start()
    {
        displayedItem.GetComponent<MeshRenderer>().sharedMaterial = item.itemModel.GetComponent<MeshRenderer>().sharedMaterial;
        displayedItem.GetComponent<MeshFilter>().sharedMesh = item.itemModel.GetComponent<MeshFilter>().sharedMesh;
    }

    // Update is called once per frame
    void Update()
    {
        displayedItem.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (displayedItem.active)
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
                other.gameObject.GetComponent<enemyAI>().SetHP(item.nonWeaponStat + other.gameObject.GetComponent<enemyAI>().GetHP());
                displayedItem.SetActive(false);
                break;
            }
                case (pickupStats.pickupType.weapon):
            {
                other.gameObject.GetComponent<enemyAI>().SetShootRate(item.shootRate + other.gameObject.GetComponent<enemyAI>().GetShootRate());
                displayedItem.SetActive(false);
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
                GameManager.instance.player.GetComponent<playerDamage>().addWeapon(item);
                displayedItem.SetActive(false);
                break;
            }
                case (pickupStats.pickupType.health):
            {
                GameManager.instance.player.GetComponent<playerDamage>().setMaxHP(GameManager.instance.player.GetComponent<playerDamage>().getMaxHP() + item.nonWeaponStat);
                displayedItem.SetActive(false);
                break;
            }
                case (pickupStats.pickupType.stamina):
            {
                GameManager.instance.player.GetComponent<playerMovement>().setMaxStamina(item.nonWeaponStat + GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina());
                GameManager.instance.player.GetComponent<playerMovement>().setStamina(GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina());
                displayedItem.SetActive(false);
                break;
            }
                case (pickupStats.pickupType.damage):
            {
                GameManager.instance.player.GetComponent<playerDamage>().setBulletUpgrades(GameManager.instance.player.GetComponent<playerDamage>().getBulletUpgrades() + (int)item.nonWeaponStat);
                displayedItem.SetActive(false);
                break;
            }
                case (pickupStats.pickupType.speed):
            {
                GameManager.instance.player.GetComponent<playerMovement>().setPlayerSpeed(GameManager.instance.player.GetComponent<playerMovement>().getPlayerSpeed() + item.nonWeaponStat);
                displayedItem.SetActive(false);
                break;
            }
                case (pickupStats.pickupType.ammo): //For later when ammo is fully implemented
            {
                break;
            }
                case (pickupStats.pickupType.ammoUpgrade):
            {
                GameManager.instance.player.GetComponent<playerDamage>().setAmmoMultiplier(GameManager.instance.player.GetComponent<playerDamage>().getAmmoMultiplier() + item.nonWeaponStat);
                displayedItem.SetActive(false);
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
