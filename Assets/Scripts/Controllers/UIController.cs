using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region Debug Layer

    [SerializeField]
    private GameObject DebugLayer;
    private bool debugMode = false;

    [SerializeField] 
    private Button rebuildDebugButton;

    #endregion Debug Layer
    #region Main Menu Layer

    public GameObject MainMenuLayer;

    [SerializeField]
    private Button 
        playButton, 
        buildButton, 
        achievementsButton, 
        settingsButton, 
        exitButton;

    [SerializeField]
    private GameObject 
        MenuAssembler, 
        Achievements, 
        Settings;

    #endregion Main Menu Layer
    #region HUD Layer

    public GameObject HUDLayer;

    //World State
    public GameObject timeTextObj;
    public float timeStart;

    //Player State
    public Image hpBar;
    public Image shieldBar;
    public Image energyBar;

    private float barDefaultWidth = 350;

    #endregion HUD Layer

    void Awake()
    {
        //I prefer to add the listeners through code when the UIController wakes,
        //since the UIController will always be instantiated after the UI canvas

        #region Debug Layer Listeners

        rebuildDebugButton.onClick.AddListener(delegate { GameManager.Instance.PlayerController.MechAssembler(); });

        #endregion Debug Layer Listeners
        #region Main Menu Layer Listeners

        playButton.onClick.AddListener(delegate { GameManager.Instance.StartPlayScene(); });
        exitButton.onClick.AddListener(Application.Quit);

        #endregion Main Menu Layer Listeners
        #region HUD Layer

        barDefaultWidth = hpBar.rectTransform.sizeDelta.x;

        #endregion HUD Layer
    }

    void Update()
    {
        //Debug Mode Toggle Key: '~'
        if (Input.GetKeyDown(KeyCode.BackQuote)) { DebugMode(!debugMode); }

        //Run persistent UI updates
        playerStatusBars();
        SetTime();
    }

    void DebugMode(bool mode)
    {
        debugMode = mode;
        DebugLayer.SetActive(mode);

        Debug.Log("Toggled Debug Mode to " + mode + " at " + Time.time.ToString() + ".");
    }

    void SetTime()
    {
        timeTextObj.GetComponent<TMP_Text>().text = TimeSpan.FromSeconds(Time.time - timeStart).ToString("mm\\:ss\\:ff") + "\n\n" + GameManager.Instance.score.ToString();
    }

    void playerStatusBars()
    {
        float hpPerc = GameManager.Instance.PlayerController.curhp / 100;
        float shldPerc = GameManager.Instance.PlayerController.curshld / 100;
        float nrgyPerc = GameManager.Instance.PlayerController.curnrgy / 100;

        hpBar.rectTransform.sizeDelta = new Vector2(barDefaultWidth * hpPerc, hpBar.rectTransform.sizeDelta.y);
        shieldBar.rectTransform.sizeDelta = new Vector2(barDefaultWidth * shldPerc, hpBar.rectTransform.sizeDelta.y);
        energyBar.rectTransform.sizeDelta = new Vector2(barDefaultWidth * nrgyPerc, hpBar.rectTransform.sizeDelta.y);
    }
}
