using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    private static int baseColorId = Shader.PropertyToID("_BaseColor");
    private static MaterialPropertyBlock block;
    
    public Color baseColor = Color.white;


    private void OnValidate()
    {
        if (block == null)
            block = new MaterialPropertyBlock();
        
        block.SetColor(baseColorId, baseColor);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }

    // Start is called before the first frame update
    void Start()
    {
        OnValidate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
