using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Item : MonoBehaviour
{
    [SerializeField]
    private float[] itemStats = new float[]
        {
                0f, //mvmt
                0f, //sprnt (acts as coefficient on mvmt)
                0f, //hp
                0f, //shld
                0f, //nrgy
                0f, //dmg
                0f, //rof
                0f, //crtc
                0f, //crtd (acts as coefficient on dmg)
                0f  //cd
        };

    //For templating, individual items will override with their own stats
    public Dictionary<string, float> itemData;
    public float lifespan = 100;
    private float death;

    private void Awake()
    {
        itemData = new Dictionary<string, float>()
        {
            {"mvmt", itemStats[0]},
            {"sprnt", itemStats[1] },
            {"hp", itemStats[2] },
            {"shld", itemStats[3] },
            {"nrgy", itemStats[4] },
            {"dmg", itemStats[5] },
            {"rof", itemStats[6] },
            {"crtc", itemStats[7] },
            {"crtd", itemStats[8] },
            {"cd", itemStats[9] }
        };

    }
    private void Start()
    {
        Debug.Log("A " + gameObject.name + " has spawned.");
        death = Time.time + lifespan;
    }

    private void Update()
    {
        if (death <= Time.time) { Destroy(gameObject); }
    }
}
