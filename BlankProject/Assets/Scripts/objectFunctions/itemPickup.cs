using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemPickup : MonoBehaviour, IPickup
{
    [SerializeField] Vector3 rotationSpeed;
    [SerializeField] pickupStats item;
    [SerializeField] bool grabbableByEnemy;
    [SerializeField] bool grabbableByPlayer;
    GameObject displayedItem = null;

    void Start()
    {
        Vector3 startingPosition = transform.position; startingPosition.y += 1.5F;
        displayedItem = Instantiate(item.itemModel, startingPosition, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if (displayedItem != null) displayedItem.transform.Rotate(rotationSpeed * Time.deltaTime);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (displayedItem != null && other != displayedItem.GetComponent<Collider>())
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
                other.gameObject.GetComponent<SharedEnemyAI>().SetHP(item.nonWeaponStat + other.gameObject.GetComponent<SharedEnemyAI>().GetHP());
                Destroy(displayedItem);
                displayedItem=null;
                break;
            }
                case (pickupStats.pickupType.weapon):
            {
                other.gameObject.GetComponent<SharedEnemyAI>().SetShootRate(item.shootRate + other.gameObject.GetComponent<SharedEnemyAI>().GetShootRate());
                Destroy(displayedItem);
                displayedItem=null;
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
                Destroy(displayedItem);
                displayedItem = null;
                break;
            }
                case (pickupStats.pickupType.health):
            {
                GameManager.instance.player.GetComponent<playerDamage>().setMaxHP(GameManager.instance.player.GetComponent<playerDamage>().getMaxHP() + item.nonWeaponStat);
                Destroy(displayedItem);
                displayedItem = null;
                break;
            }
                case (pickupStats.pickupType.stamina):
            {
                GameManager.instance.player.GetComponent<playerMovement>().setMaxStamina(item.nonWeaponStat + GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina());
                GameManager.instance.player.GetComponent<playerMovement>().setStamina(GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina());
                Destroy(displayedItem);
                displayedItem = null;
                break;
            }
                case (pickupStats.pickupType.damage):
            {
                GameManager.instance.player.GetComponent<playerDamage>().setBulletUpgrades(GameManager.instance.player.GetComponent<playerDamage>().getBulletUpgrades() + (int)item.nonWeaponStat);
                Destroy(displayedItem);
                displayedItem = null;
                break;
            }
                case (pickupStats.pickupType.speed):
            {
                GameManager.instance.player.GetComponent<playerMovement>().setPlayerSpeed(GameManager.instance.player.GetComponent<playerMovement>().getPlayerSpeed() + item.nonWeaponStat);
                Destroy(displayedItem);
                displayedItem = null;
                break;
            }
                case (pickupStats.pickupType.ammo): //For later when ammo is fully implemented
            {
                break;
            }
                case (pickupStats.pickupType.ammoUpgrade):
            {
                GameManager.instance.player.GetComponent<playerDamage>().setAmmoMultiplier(GameManager.instance.player.GetComponent<playerDamage>().getAmmoMultiplier() + item.nonWeaponStat);
                Destroy(displayedItem);
                displayedItem = null;
                break;
            }
                case (pickupStats.pickupType.commandCode):
                    Destroy(displayedItem);
                    displayedItem = null;
                    GameManager.instance.PickedUpCommandCode();
                
                    break;

                case (pickupStats.pickupType.securityPassword):
                    Destroy(displayedItem);
                    displayedItem= null;

                    if (GameManager.instance.GetCurrentLevel() == 0)
                        GameManager.instance.SetSecurtyPasswordLevel1(item.passwordCombo);
                    else if (GameManager.instance.GetCurrentLevel() == 1)
                        GameManager.instance.SetSecurtyPasswordLevel2(item.passwordCombo);



                    break;

                default:
            {
                break;
            }
            }
        }
  
    }

    
}
