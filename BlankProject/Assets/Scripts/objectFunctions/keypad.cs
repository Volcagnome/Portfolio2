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
        currentCombo = "";
        currentComboSize = currentCombo.Length;
        correct = false;
        updateDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        if (!correct && didInteract()) { updateDisplay(); }
    }

    bool didInteract()
    {
        bool somethingWasTouched = false;

        if (backspaceButton.GetComponent<ISendState>().getState())
        {
            string holder = "";

            for (int i = 0; i < currentComboSize - 1; ++i) holder += currentCombo[i];

            currentCombo = holder;
        }
        else if (confirmButton.GetComponent<ISendState>().getState())
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
        else if (currentCombo.Length < goalCombo.Length)
        {
            if (zeroButton.GetComponent<ISendState>().getState()) currentCombo += '0';
            else if (oneButton.GetComponent<ISendState>().getState()) currentCombo += '1';
            else if (twoButton.GetComponent<ISendState>().getState()) currentCombo += '2';
            else if (threeButton.GetComponent<ISendState>().getState()) currentCombo += '3';
            else if (fourButton.GetComponent<ISendState>().getState()) currentCombo += '4';
            else if (fiveButton.GetComponent<ISendState>().getState()) currentCombo += '5';
            else if (sixButton.GetComponent<ISendState>().getState()) currentCombo += '6';
            else if (sevenButton.GetComponent<ISendState>().getState()) currentCombo += '7';
            else if (eightButton.GetComponent<ISendState>().getState()) currentCombo += '8';
            else if (nineButton.GetComponent<ISendState>().getState()) currentCombo += '9';
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
        display.enabled = false;
        display.enabled = true; 
    }

    public bool getState()
    {
        return correct;
    }

}
