using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingMaterial : MonoBehaviour
{
    //Movement and bouncing
    public float rotationSpeed = 100f;
    public float buoyancyCoeff = 1f;

    private float buoyancyOffset = 2.5f;

    //Texture scrolling
    public float scrollSpeed;
    float _uvFactor;
    Material _materialToScroll;

    void Awake()
    {
        _materialToScroll = GetComponent<MeshRenderer>().material;
        float n = _materialToScroll.GetTextureScale("_MainTex").y;
        float scale = transform.localScale.z;
        _uvFactor = n / scale;
    }

    void Update()
    {
        _materialToScroll.mainTextureOffset = new Vector2(_materialToScroll.mainTextureOffset.x, _materialToScroll.mainTextureOffset.y + Time.deltaTime * scrollSpeed * _uvFactor);

        gameObject.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        gameObject.transform.position = new Vector3(transform.position.x, buoyancyCoeff * Mathf.Sin(Time.time) + buoyancyOffset, transform.position.z);
    }
}
