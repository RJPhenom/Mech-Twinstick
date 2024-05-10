using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assembler : MonoBehaviour
{
    public GameObject snapPoint;
    public GameObject snapView;

    public GameObject buttonRLEG;
    public GameObject buttonLLEG;
    public GameObject buttonRCHAS;
    public GameObject buttonLCHAS;

    public GameObject doneSign;

    public GameObject[] buttons;
    public bool isAssembling;

    // Start is called before the first frame update
    void Awake()
    {
        buttonRLEG.SetActive(false);
        buttonLLEG.SetActive(false);
        buttonRCHAS.SetActive(false);
        buttonLCHAS.SetActive(false);
        doneSign.SetActive(false);

        buttons = new GameObject[] {buttonLLEG, buttonRLEG, buttonLCHAS, buttonRCHAS};
    }

    public void ChangeStateAssembling(bool state)
    {
        isAssembling = state;
        buttonRLEG.SetActive(state);
        buttonLLEG.SetActive(state);
        buttonRCHAS.SetActive(state);
        buttonLCHAS.SetActive(state);
        doneSign.SetActive(state);
    }

    public int ClickButton(GameObject button)
    {
        return Array.IndexOf(buttons, button);
    }
}
