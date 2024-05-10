using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : Attachment
{
    #region Properties

    public GameObject projectile;
    public Transform muzzlePoint;
    public int ammunition;
    public float weaponDmg;
    public float weaponROF;

    #endregion Properties

    private float nextShot;

    private void Awake()
    {
        nextShot = 0.0f;
    }

    public void Fire()
    {
        if(ammunition > 0 && Time.time > nextShot)
        {
            nextShot = Time.time + weaponROF;
            GameObject shot = Instantiate(projectile);
            shot.transform.position = muzzlePoint.transform.position;
            shot.transform.rotation = muzzlePoint.transform.rotation;
            ammunition--;
        }
    }
}
