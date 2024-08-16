using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class keypad : MonoBehaviour, ISendState
{
    [SerializeField] string goalCombo;

    [SerializeField] TMP_Text display;

    [SerializeField] GameObject zeroButton;
    [SerializeField] GameObject oneButton;
    [SerializeField] GameObject twoButton;
    [SerializeField] GameObject threeButton;
    [SerializeField] GameObject fourButton;
    [SerializeField] GameObject fiveButton;
    [SerializeField] GameObject sixButton;
    [SerializeField] GameObject sevenButton;
    [SerializeField] GameObject eightButton;
    [SerializeField] GameObject nineButton;
    [SerializeField] GameObject backspaceButton;
    [SerializeField] GameObject confirmButton;

    string currentCombo;
    int currentComboSize;
    bool correct;

    // Start is called before the first frame update
    void Start()
    {
        currentComboSize = currentCombo.Length;
    }

    // Update is called once per frame
    void Update()
    {
        if (!correct && didInteract()) { updateDisplay(); }
    }

    bool didInteract()
    {
        bool somethingWasTouched = false;

        if (backspaceButton.GetComponent<basicInteractive>().GetInteracted())
        {
            string holder = "";

            for (int i = 0; i < currentComboSize - 1; ++i) holder += currentCombo[i];

            currentCombo = holder;
        }
        else if (confirmButton.GetComponent<basicInteractive>().GetInteracted())
        {
            if (currentCombo == goalCombo)
            {
                correct = true;
                somethingWasTouched = true;
            }
            else
            {
                currentCombo = "";
            }
        }
        else if (currentComboSize < goalCombo.Length)
        {
            if (zeroButton.GetComponent<basicInteractive>().GetInteracted()) currentCombo += '0';
            else if (oneButton.GetComponent<basicInteractive>().GetInteracted()) currentCombo += '1';
            else if (twoButton.GetComponent<basicInteractive>().GetInteracted()) currentCombo += '2';
            else if (threeButton.GetComponent<basicInteractive>().GetInteracted()) currentCombo += '3';
            else if (fourButton.GetComponent<basicInteractive>().GetInteracted()) currentCombo += '4';
            else if (fiveButton.GetComponent<basicInteractive>().GetInteracted()) currentCombo += '5';
            else if (sixButton.GetComponent<basicInteractive>().GetInteracted()) currentCombo += '6';
            else if (sevenButton.GetComponent<basicInteractive>().GetInteracted()) currentCombo += '7';
            else if (eightButton.GetComponent<basicInteractive>().GetInteracted()) currentCombo += '8';
            else if (nineButton.GetComponent<basicInteractive>().GetInteracted()) currentCombo += '9';
        }

        if (currentComboSize != currentCombo.Length)
        {
            somethingWasTouched = true;
            currentComboSize = currentCombo.Length;
        }

        return somethingWasTouched;
    }

    void updateDisplay()
    {
        display.text = currentCombo;
    }

    public bool getState()
    {
        return correct;
    }
}
