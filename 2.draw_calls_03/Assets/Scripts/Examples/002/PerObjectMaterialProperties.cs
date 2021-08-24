using System;
using UnityEngine;
using Random = System.Random;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour {
	static MaterialPropertyBlock block;
	
	static int baseColorId = Shader.PropertyToID("_BaseColor");
	static int cutoffId = Shader.PropertyToID("_Clipping");
	
    [SerializeField]
    Color baseColor = Color.white;

    [SerializeField] float cutoff = 0.5f;

    private void Awake()
    {
	    baseColor = UnityEngine.Random.ColorHSV();
	    cutoff = UnityEngine.Random.value;
	    OnValidate();
    }

    void OnValidate () {
	    if (block == null) {
		    block = new MaterialPropertyBlock();
	    }
	    block.SetColor(baseColorId, baseColor);
	    block.SetFloat(cutoffId, cutoff);
	    GetComponent<Renderer>().SetPropertyBlock(block);
    }
}