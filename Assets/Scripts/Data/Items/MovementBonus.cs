using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBonus : Item
{
    private Dictionary<string, float> item1Stats = new Dictionary<string, float>()
        {
            { "mvmt", 10f},
        };

void Awake()
    {
        itemData = item1Stats;
    }
}
