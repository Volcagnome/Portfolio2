using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
    public enum tooltipType { commandCode, securityPassword, statUpgrade }
    string[] toolTips = { "Command Code:\r\nCollect the command codes hidden in each sector and insert them into the terminal in the central mainframe to activate the self destruct. ",
                           "Security Password:\r\nYour progress will be impeded by security checkpoints. Find the security passwords to gain access to the next sector.",
                            "Stat Pickups:\r\nCollect any bioengineering upgrades you can find to increase your chances of survival. You might also find useful robot tech worth salvaging. "};
    [SerializeField] tooltipType type;
    [SerializeField] GameObject tooltipWindow;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            tooltipWindow.GetComponentInChildren<TMP_Text>().text = toolTips[(int)type];
            tooltipWindow.SetActive(true);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            tooltipWindow.SetActive(false);
        }
    }
}
