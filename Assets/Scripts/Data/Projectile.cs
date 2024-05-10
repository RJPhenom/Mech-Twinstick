using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region Properties

    public GameObject gfx;
    public int dmg;
    public int travelspeed;
    public float lifespan;
    private float death;

    bool isAOE;
    float aoeRadius;
    float aoeDmg;

    Rigidbody rb;
    Collider cd;

    #endregion Properties

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cd = GetComponent<Collider>();

        rb.isKinematic = true;
        cd.isTrigger = true;
    }

    private void Start()
    {
        death = Time.time + lifespan;
    }

    private void Update()
    {
        transform.position +=  transform.forward * travelspeed * Time.deltaTime;
        if (death <= Time.time) { Destroy(gameObject); }
    }
}
