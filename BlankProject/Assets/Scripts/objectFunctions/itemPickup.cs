using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemPickup : MonoBehaviour, IPickup
{
    [SerializeField] Vector3 startingAxes;
    [Range(-180, 180)][SerializeField] float startingAngle;
    [SerializeField] Vector3 rotationSpeed;
    [SerializeField] pickupStats item;
    [SerializeField] bool grabbableByEnemy;
    [SerializeField] bool grabbableByPlayer;
    
    GameObject displayedItem = null;

    [Header("----- Sounds -----")]
    [SerializeField] AudioClip pickupSound;
    [SerializeField] float pickupVol;

    void Start()
    {
        startingAxes.Normalize();
        Vector3 startingPosition = transform.position; startingPosition.y += 1.5F;
        displayedItem = Instantiate(item.itemModel, startingPosition, Quaternion.identity);
        displayedItem.transform.rotation = Quaternion.AngleAxis(startingAngle, startingAxes);
        displayedItem.GetComponent<Transform>().localScale *= item.modelScale;
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
                case (pickupStats.pickupType.upgrade):
            {
                other.gameObject.GetComponent<SharedEnemyAI>().SetHP(item.health + other.gameObject.GetComponent<SharedEnemyAI>().GetHP());
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
            // Play item pickup sound:
            GameManager.instance.playAud(pickupSound, pickupVol);

            switch (item.type)
            {
                case (pickupStats.pickupType.weapon):
            {
                GameManager.instance.player.GetComponent<playerDamage>().addWeapon(item);
                Destroy(displayedItem);
                displayedItem = null;
                break;
            }
                case (pickupStats.pickupType.upgrade):
            {
                float hpHolder = GameManager.instance.player.GetComponent<playerDamage>().getMaxHP();
                float staminaHolder = GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina();

                GameManager.instance.player.GetComponent<playerDamage>().setMaxHP(GameManager.instance.player.GetComponent<playerDamage>().getMaxHP() + item.health);
                if (hpHolder != GameManager.instance.player.GetComponent<playerDamage>().getMaxHP()) GameManager.instance.player.GetComponent<playerDamage>().setHP(GameManager.instance.player.GetComponent<playerDamage>().getMaxHP());

                GameManager.instance.player.GetComponent<playerMovement>().setMaxStamina(item.stamina + GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina());
                if (staminaHolder != GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina()) GameManager.instance.player.GetComponent<playerMovement>().setStamina(GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina());
                
                GameManager.instance.player.GetComponent<playerDamage>().setBulletUpgrades(GameManager.instance.player.GetComponent<playerDamage>().getBulletUpgrades() + item.damageUP);

                GameManager.instance.player.GetComponent<playerMovement>().setPlayerSpeed(GameManager.instance.player.GetComponent<playerMovement>().getPlayerSpeed() + item.speed);
                GameManager.instance.player.GetComponent<playerMovement>().SetPlayerSpeedOG(GameManager.instance.player.GetComponent<playerMovement>().getPlayerSpeedOG() + item.speed);

                GameManager.instance.player.GetComponent<playerDamage>().setAmmoMultiplier(GameManager.instance.player.GetComponent<playerDamage>().getAmmoMultiplier() + item.batteryUP);

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

                    //if (GameManager.instance.GetCurrentLevel() == 0)
                    //    GameManager.instance.SetSecurtyPasswordLevel1(item.passwordCombo);
                    //else if (GameManager.instance.GetCurrentLevel() == 1)
                    //    GameManager.instance.SetSecurtyPasswordLevel2(item.passwordCombo);



                    break;

                default:
            {
                break;
            }
            }
        }
  
    }

    
}
