using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using UnityEditor.Search;
using System.Threading;

public class GameManager : MonoBehaviour
{
    public int dimensions = 50;
    #region Singleton Instance

    private static GameManager instance;
    public static GameManager Instance 
    { 
        get 
        { 
            return instance; 
        } 
    }

    #endregion Singleton Instance
    #region Controller Objects

    public GameObject PlayerPrefab;
    public GameObject UIPrefab;

    public GameObject Player;
    public GameObject UI;

    public PlayerController PlayerController;
    public UIController UIController;

    #endregion Controller & Handler Objects
    #region Camera Properties

    private Vector3 cameraDefaultPosition = new Vector3 (0, 30, -30);

    #endregion Camera Properties
    #region Difficulty & Game Session Management

    public int score = 0;

    int zone = 0;

    int baseItemSpawns = 1;
    int baseEnemySpawns = 3;

    private float iTimer;
    private float eTimer;

    private float iDelay = 2f;
    private float eDelay = 1f;

    #endregion Difficulty & Game Session Management
    #region Resource Lists

    private GameObject[] items;
    private GameObject[] enemies;
    private GameObject[] weapons;

    #endregion Resources Lists

    void Awake()
    {
        #region Singleton Handling
        //Checks on Awake() if other instance exists and destroys itself if so,
        //since there should only ever be one instance of a singleton.

        if (instance != null && instance != this) {
            Destroy(this);
        }

        else { 
            instance = this; 
        }

        #endregion Singleton Handling
        #region Load in Controllers & Important Classes

        Camera.main.transform.position = cameraDefaultPosition;

        Player = Instantiate(PlayerPrefab);
        UI = Instantiate(UIPrefab);

        PlayerController = Player.GetComponent<PlayerController>();
        UIController = UI.GetComponent<UIController>();

        DontDestroyOnLoad(Player);
        DontDestroyOnLoad(UI);
        DontDestroyOnLoad(Camera.main.gameObject);
        DontDestroyOnLoad(this);

        #endregion Load in Controllers & Important Classes
        #region Resource Lists

        items = Resources.LoadAll<GameObject>("Items");
        enemies = Resources.LoadAll<GameObject>("Enemies");
        weapons = Resources.LoadAll<GameObject>("Weapons");

        #endregion Resource Lists
    }

    void Update()
    {
        if (zone != 0)
        {
            SessionManager(zone);
        }
    }

    public void StartPlayScene()
    {
        if (PlayerController.isAssembling)
        {
            PlayerController.ExitAssembly();
        }

        UIController.MainMenuLayer.SetActive(false);
        UIController.HUDLayer.SetActive(true);
        UIController.timeStart = Time.time;
        SceneManager.LoadSceneAsync("Play");
        Camera.main.transform.position = Player.transform.position + cameraDefaultPosition;
        Camera.main.transform.SetParent(Player.transform);
        StartZone(1);
    }

    public void EndPlayScene()
    {
        SceneManager.LoadSceneAsync("Main Menu");
        UIController.MainMenuLayer.SetActive(true);
        UIController.HUDLayer.SetActive(false);
        Camera.main.gameObject.transform.SetParent(null);
        zone = 0;
    }

    private void StartZone(int next_zone)
    {
        zone = next_zone;
        iTimer = Time.time;
    }

    private void SessionManager(int zone)
    {
        if (iTimer + iDelay < Time.time)
        {
            if (UnityEngine.Random.Range(-1f, 1f) >= 0f)
            {
                ItemSpawner(1 / zone * baseItemSpawns);

            }

            else
            {
                WeaponSpawner(1 / zone * baseItemSpawns);
            }
            iTimer = Time.time;
        }

        if (eTimer + eDelay < Time.time)
        {
            EnemySpawner(1 / zone * baseEnemySpawns);
            eTimer = Time.time;
        }
    }

    public void ItemSpawner(int numToSpawn)
    {
        for (int i = 0; i < numToSpawn; i++)
        {
            System.Random r = new System.Random();
            int rint = r.Next(items.Length);

            Vector3 pos = new Vector3(UnityEngine.Random.Range(0 - dimensions, dimensions), 0, UnityEngine.Random.Range(0 - dimensions, dimensions));

            GameObject item = Instantiate(items[rint]);
            item.transform.position = pos;
        }
    }
    public void WeaponSpawner(int numToSpawn)
    {
        for (int i = 0; i < numToSpawn; i++)
        {
            System.Random r = new System.Random();
            int rint = r.Next(weapons.Length);

            Vector3 pos = new Vector3(UnityEngine.Random.Range(0 - dimensions, dimensions), 0, UnityEngine.Random.Range(0 - dimensions, dimensions));

            GameObject weapon = Instantiate(weapons[rint]);
            weapon.transform.position = pos;
        }
    }

    public void EnemySpawner(int numToSpawn)
    {
        for (int i = 0; i < numToSpawn; i++)
        {
            System.Random r = new System.Random();
            int rint = r.Next(enemies.Length);

            Vector3 pos = new Vector3(UnityEngine.Random.Range(0 - dimensions, dimensions), 0, UnityEngine.Random.Range(0 - dimensions, dimensions));

            GameObject item = Instantiate(enemies[rint]);
            item.transform.position = pos;
        }
    }
}
